using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace voxel_engine_cs {

    public struct ChunkData {
        public int[,,] blocks;
        public VertexCustom[] vertices;
        public int[] indices;
        public int primitiveCount;

        public ChunkData(int chunkSize) {
            blocks = new int[chunkSize, chunkSize, chunkSize];
            vertices = new VertexCustom[0];
            indices = new int[0];
            primitiveCount = 0;
        }

        public void setBlocks(int[,,] blockArr) {
            int chunkSize = blockArr.GetLength(0);
            for (int x2 = 0; x2 < chunkSize; x2++) {
                for (int y2 = 0; y2 < chunkSize; y2++) {
                    for (int z2 = 0; z2 < chunkSize; z2++) {
                        blocks[x2, y2, z2] = blockArr[x2, y2, z2];
                    }
                }
            }
        }

        public void setVert(VertexCustom[] vert) {
            vertices = new VertexCustom[vert.Length];
            for (int v = 0; v < vert.Length; v++) {
                vertices[v] = vert[v];
            }
        }

        public void setInd(int[] ind) {
            indices = new int[ind.Length];
            for (int v = 0; v < ind.Length; v++) {
                indices[v] = ind[v];
            }
        }

        public void setPrimCount(int count) {
            primitiveCount = count;
        }
    }

    internal class World {

        private Random rnd;
        public Player player;

        private Dictionary<int, Dictionary<int, Dictionary<int, ChunkData>>> chunk = new Dictionary<int, Dictionary<int, Dictionary<int, ChunkData>>>();

        public Effect lighting { get; set; }
        public Matrix projectionMatrix { get; set; }
        public Texture2D texture { get; set; }
        public bool fogEnabled;
        public float fogNear;
        public float fogFar;

        private ChunkBuilder builder;

        private int chunkSize = 16;

        private Task[] generationThreads;
        private Task[] compileThreads;
        public Queue<Vector3> chunkGenerateQueue;
        public Queue<Vector3> chunkCompileQueue;

        public World() {

            builder = new ChunkBuilder();
            rnd = new Random();
            player = new Player(new Vector3(0, 3, 0), 60f);
            chunkGenerateQueue = new Queue<Vector3>();
            chunkCompileQueue = new Queue<Vector3>();
            generationThreads = new Task[3];
            compileThreads = new Task[3];
            for (int i = 0; i < generationThreads.Length; i++) {
                generationThreads[i] = Task.CompletedTask;
            }
            for (int i = 0; i < compileThreads.Length; i++) {
                compileThreads[i] = Task.CompletedTask;
            }

            builder.generateCallback += genCallback;
            builder.compileCallback += compCallback;

        }

        public void updateChunks() {
            for (int t = 0; t < generationThreads.Length; t++) {
                if (chunkGenerateQueue.Count >= 1 && generationThreads[t].Status != TaskStatus.Running) {
                    Vector3 currentChunk = chunkGenerateQueue.Dequeue();
                    ensureChunkExists((int)Math.Round(currentChunk.X), (int)Math.Round(currentChunk.Y), (int)Math.Round(currentChunk.Z));
                    generationThreads[t] = Task.Run(() => builder.generateChunk((int)Math.Round(currentChunk.X), (int)Math.Round(currentChunk.Y), (int)Math.Round(currentChunk.Z), chunkSize));
                }
            }
            for (int t = 0; t < compileThreads.Length; t++) {
                if (chunkCompileQueue.Count >= 1 && compileThreads[t].Status != TaskStatus.Running) {
                    Vector3 currentChunk = chunkCompileQueue.Dequeue();
                    ensureChunkExists((int)Math.Round(currentChunk.X), (int)Math.Round(currentChunk.Y), (int)Math.Round(currentChunk.Z));
                    compileThreads[t] = Task.Run(() => builder.compile(chunk[(int)Math.Round(currentChunk.X)][(int)Math.Round(currentChunk.Y)][(int)Math.Round(currentChunk.Z)].blocks, (int)Math.Round(currentChunk.X), (int)Math.Round(currentChunk.Y), (int)Math.Round(currentChunk.Z)));
                }
            }
        }

        public void render() {

            ResourceManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ResourceManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ResourceManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Point screenCenter = new Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);

            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(player.r, player.t, 0);
            Vector3 lookDirection = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Vector3 upDirection = Vector3.Transform(Vector3.Up, rotationMatrix);
            Matrix viewMatrix = Matrix.CreateLookAt(player.cameraPosition, player.cameraPosition + lookDirection, upDirection);

            List<int[]> tmp = new List<int[]>();

            foreach (int x in chunk.Keys) {
                foreach (int y in chunk[x].Keys) {
                    foreach (int z in chunk[x][y].Keys) {
                        if (chunk[x][y][z].primitiveCount >= 1) {
                            tmp.Add(new int[] { x, y, z });
                        }
                    }
                }
            }

            ResourceManager.GraphicsDevice.SamplerStates[0] = new SamplerState { Filter = TextureFilter.Point, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap };

            foreach (var pass in lighting.CurrentTechnique.Passes) {
                pass.Apply();
                for (int i = 0; i < tmp.Count; i++) {
                    ResourceManager.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, chunk[tmp[i][0]][tmp[i][1]][tmp[i][2]].vertices, 0, chunk[tmp[i][0]][tmp[i][1]][tmp[i][2]].vertices.Length, chunk[tmp[i][0]][tmp[i][1]][tmp[i][2]].indices, 0, (chunk[tmp[i][0]][tmp[i][1]][tmp[i][2]].indices.Length) / 3, VertexCustom.VertexDeclaration);
                }
            }

            ResourceManager.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            ResourceManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            ResourceManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        }

        public void updatePlayer(float deltaTime) {

            Point screenCenter = new Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);

            player.bruh(deltaTime);

            if (player.canMove) {
                if (player.mouseLock) {
                    // Get the current mouse state
                    var mouseState = Mouse.GetState();

                    // Calculate the difference between the current mouse position and the center of the screen
                    int deltaX = mouseState.X - screenCenter.X;
                    int deltaY = mouseState.Y - screenCenter.Y;

                    Mouse.SetPosition(screenCenter.X, screenCenter.Y);

                    // Update yaw and pitch based on mouse movement
                    player.r -= deltaX * player.mouseSensitivity;
                    player.t -= deltaY * player.mouseSensitivity;

                    // Clamp the pitch to prevent flipping
                    player.t = MathHelper.Clamp(player.t, -MathHelper.PiOver2, MathHelper.PiOver2);
                }

                // Get keyboard state
                var kstate = Keyboard.GetState();

                // Move player
                if (kstate.IsKeyDown(Keys.W)) {
                    player.x += (float)Math.Sin(player.r) * -player.movementSpeed * deltaTime;
                    player.z += (float)Math.Cos(player.r) * -player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.A)) {
                    player.x += (float)Math.Sin(player.r + MathHelper.PiOver2) * -player.movementSpeed * deltaTime;
                    player.z += (float)Math.Cos(player.r + MathHelper.PiOver2) * -player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.S)) {
                    player.x += (float)Math.Sin(player.r + MathHelper.Pi) * -player.movementSpeed * deltaTime;
                    player.z += (float)Math.Cos(player.r + MathHelper.Pi) * -player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.D)) {
                    player.x += (float)Math.Sin(player.r - MathHelper.PiOver2) * -player.movementSpeed * deltaTime;
                    player.z += (float)Math.Cos(player.r - MathHelper.PiOver2) * -player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.Space)) {
                    player.y += player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.LeftShift)) {
                    player.y -= player.movementSpeed * deltaTime;
                }
            }

            player.velocity = player.nextVelocity;

            player.position += player.velocity * deltaTime;

            player.xVel = MathHelper.LerpPrecise(player.xVel, 0, Math.Min(player.damping * deltaTime, 1));
            player.zVel = MathHelper.LerpPrecise(player.zVel, 0, Math.Min(player.damping * deltaTime, 1));

            player.prevVelocity = player.velocity;

        }

        private void ensureChunkExists(int x, int y, int z) {
            if (!chunk.ContainsKey(x)) {
                chunk[x] = new Dictionary<int, Dictionary<int, ChunkData>>();
            }

            if (!chunk[x].ContainsKey(y)) {
                chunk[x][y] = new Dictionary<int, ChunkData>();
            }

            if (!chunk[x][y].ContainsKey(z)) {
                chunk[x][y][z] = new ChunkData(chunkSize);
            }
        }
        private void ensureChunkExists(int x, int y) {
            if (!chunk.ContainsKey(x)) {
                chunk[x] = new Dictionary<int, Dictionary<int, ChunkData>>();
            }

            if (!chunk[x].ContainsKey(y)) {
                chunk[x][y] = new Dictionary<int, ChunkData>();
            }
        }
        private void ensureChunkExists(int x) {
            if (!chunk.ContainsKey(x)) {
                chunk[x] = new Dictionary<int, Dictionary<int, ChunkData>>();
            }
        }

        public bool doesChunkExist(int x, int y, int z) {
            if (chunk.ContainsKey(x)) {
                if (chunk[x].ContainsKey(y)) {
                    if (chunk[x][y].ContainsKey(z)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private void genCallback(int[,,] data, int x, int y, int z) {
            ensureChunkExists(x, y, z);
            chunk[x][y][z].setBlocks(data);
            chunkCompileQueue.Enqueue(new Vector3(x, y, z));
        }

        private void compCallback(VertexCustom[] vert, int[] ind, int primCount, int x, int y, int z) {
            ensureChunkExists(x, y, z);
            chunk[x][y][z].setPrimCount(primCount);
            chunk[x][y][z].setVert(vert);
            chunk[x][y][z].setInd(ind);
        }

    }
}
