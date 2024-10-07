using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace voxel_engine_cs {

    public struct VertexCustom {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        //public float Occlusion;

        public VertexCustom(Vector3 position, Vector3 normal, Vector2 texCoord) {
            Position = position;
            Normal = normal;
            TextureCoordinate = texCoord;
            //Occlusion = occlusion;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        //new VertexElement(sizeof(float) * 8, VertexElementFormat.Single, VertexElementUsage.Color, 0)
        );
    }

    internal class ChunkBuilder {

        public event Action<int[,,], int, int, int> generateCallback;
        public event Action<VertexCustom[], int[], int, int, int, int> compileCallback;

        public void generateChunk(int x, int y, int z, int chunkSize) {

            int[,,] tempchunk = new int[chunkSize, chunkSize, chunkSize];

            FastNoiseLite noise = new FastNoiseLite();
            float noiseScale = 1.0f;
            float vertScale = 1.0f;
            float threshold = 0.5f;

            int totalBlocks = 0;
            for (int cX = x * chunkSize; cX < (x * chunkSize) + chunkSize; cX++) {
                for (int cY = y * chunkSize; cY < (y * chunkSize) + chunkSize; cY++) {
                    for (int cZ = z * chunkSize; cZ < (z * chunkSize) + chunkSize; cZ++) {

                        int aX = cX - (x * chunkSize);
                        int aY = cY - (y * chunkSize);
                        int aZ = cZ - (z * chunkSize);

                        float floorNoise = (int)Math.Round(noise.GetNoise(cX * noiseScale, cZ * noiseScale) * vertScale);
                        float caveNoise = (int)Math.Round(noise.GetNoise(cX * noiseScale, cY * noiseScale, cZ * noiseScale));

                        floorNoise = 0;
                        caveNoise = 100;

                        if (cY <= floorNoise && caveNoise > threshold) {
                            if (cY == floorNoise) {
                                tempchunk[aX, aY, aZ] = 0;
                            } else if (cY < floorNoise && cY > floorNoise - 5) {
                                tempchunk[aX, aY, aZ] = 1;
                            } else if (cY <= floorNoise - 5) {
                                tempchunk[aX, aY, aZ] = 2;
                            }
                            totalBlocks++;
                        } else {
                            tempchunk[aX, aY, aZ] = -1;
                        }

                    }
                }
            }
            generateCallback?.Invoke(tempchunk, x, y, z);
            //return chunk;
        }


        public void compile(int[,,] tempchunk, int x, int y, int z) {

            int primitiveCount = 0;
            int chunkSize = tempchunk.GetLength(0);

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();
            List<int> listIndices = new List<int>();
            int totalIndices = 0;

            for (int b = 0; b < chunkSize; b++) {
                for (int c = 0; c < chunkSize; c++) {
                    for (int d = 0; d < chunkSize; d++) {

                        if (tempchunk[b, c, d] >= 0) {

                            int cX = b + (x * chunkSize);
                            int cY = c + (y * chunkSize);
                            int cZ = d + (z * chunkSize);
                            float cLX = 0;
                            float cHX = 0;
                            float cLY = 0;
                            float cHY = 0;

                            switch (tempchunk[b, c, d]) {
                                case 0:
                                    cLX = 0.0f;
                                    cLY = 0.0f;
                                    cHX = 0.1f;
                                    cHY = 0.1f;
                                    break;
                                case 1:
                                    cLX = 0.1f;
                                    cLY = 0.0f;
                                    cHX = 0.2f;
                                    cHY = 0.1f;
                                    break;
                                case 2:
                                    cLX = 0.2f;
                                    cLY = 0.0f;
                                    cHX = 0.3f;
                                    cHY = 0.1f;
                                    break;
                            }

                            bool[,,] around = new bool[3, 3, 3];
                            Array.Clear(around, 0, 3);

                            if (b >= 1) {
                                around[0, 1, 1] = tempchunk[b - 1, c, d] >= 1;
                            }
                            if (c >= 1) {
                                around[1, 0, 1] = tempchunk[b, c - 1, d] >= 1;
                            }
                            if (d >= 1) {
                                around[1, 1, 0] = tempchunk[b, c, d - 1] >= 1;
                            }
                            if (b < 2) {
                                around[2, 1, 1] = tempchunk[b + 1, c, d] >= 1;
                            }
                            if (c < 2) {
                                around[1, 2, 1] = tempchunk[b, c + 1, d] >= 1;
                            }
                            if (d < 2) {
                                around[1, 1, 2] = tempchunk[b, c, d + 1] >= 1;
                            }

                            // Back (z-)
                            if (!around[1, 1, 0]) {
                                positions.Add(new Vector3(cX - 0.5f, cY - 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX - 0.5f, cY + 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY + 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY - 0.5f, cZ - 0.5f));
                                normals.Add(new Vector3(0, 0, -1));
                                UVs.Add(new Vector2(cLX, cLY));
                                UVs.Add(new Vector2(cLX, cHY));
                                UVs.Add(new Vector2(cHX, cHY));
                                UVs.Add(new Vector2(cHX, cLY));

                                int[] addedIndices = new int[]{
                                    0 + totalIndices, 2 + totalIndices, 1 + totalIndices, 0 + totalIndices, 3 + totalIndices, 2 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount += 2;
                            }

                            // Front (z+)
                            if (!around[1, 1, 2]) {
                                positions.Add(new Vector3(cX - 0.5f, cY - 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX - 0.5f, cY + 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY + 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY - 0.5f, cZ + 0.5f));
                                normals.Add(new Vector3(0, 0, 1));
                                UVs.Add(new Vector2(cLX, cLY));
                                UVs.Add(new Vector2(cLX, cHY));
                                UVs.Add(new Vector2(cHX, cHY));
                                UVs.Add(new Vector2(cHX, cLY));

                                int[] addedIndices = new int[]{
                                    0 + totalIndices, 1 + totalIndices, 2 + totalIndices, 0 + totalIndices, 2 + totalIndices, 3 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount += 2;
                            }

                            // Left (x-)
                            if (!around[0, 1, 1]) {
                                positions.Add(new Vector3(cX - 0.5f, cY + 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX - 0.5f, cY - 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX - 0.5f, cY - 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX - 0.5f, cY + 0.5f, cZ - 0.5f));
                                normals.Add(new Vector3(-1, 0, 0));
                                UVs.Add(new Vector2(cHX, cHY));
                                UVs.Add(new Vector2(cLX, cHY));
                                UVs.Add(new Vector2(cLX, cLY));
                                UVs.Add(new Vector2(cHX, cLY));

                                int[] addedIndices = new int[]{
                                    2 + totalIndices, 3 + totalIndices, 0 + totalIndices, 2 + totalIndices, 0 + totalIndices, 1 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount += 2;
                            }

                            // Right (x+)
                            if (!around[2, 1, 1]) {
                                positions.Add(new Vector3(cX + 0.5f, cY + 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY - 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY - 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY + 0.5f, cZ + 0.5f));
                                normals.Add(new Vector3(1, 0, 0));
                                UVs.Add(new Vector2(cHX, cHY));
                                UVs.Add(new Vector2(cLX, cHY));
                                UVs.Add(new Vector2(cLX, cLY));
                                UVs.Add(new Vector2(cHX, cLY));

                                int[] addedIndices = new int[]{
                                    0 + totalIndices, 1 + totalIndices, 2 + totalIndices, 0 + totalIndices, 2 + totalIndices, 3 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount += 2;
                            }

                            // Bottom (y-)
                            if (!around[1, 0, 1]) {
                                positions.Add(new Vector3(cX - 0.5f, cY - 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX - 0.5f, cY - 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY - 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY - 0.5f, cZ - 0.5f));
                                normals.Add(new Vector3(0, -1, 0));
                                UVs.Add(new Vector2(cLX, cLY));
                                UVs.Add(new Vector2(cLX, cHY));
                                UVs.Add(new Vector2(cHX, cHY));
                                UVs.Add(new Vector2(cHX, cLY));

                                int[] addedIndices = new int[]{
                                    1 + totalIndices, 2 + totalIndices, 3 + totalIndices, 1 + totalIndices, 3 + totalIndices, 0 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount += 2;
                            }

                            // Top (y+)
                            if (!around[1, 2, 1]) {
                                positions.Add(new Vector3(cX - 0.5f, cY + 0.5f, cZ + 0.5f));
                                positions.Add(new Vector3(cX - 0.5f, cY + 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY + 0.5f, cZ - 0.5f));
                                positions.Add(new Vector3(cX + 0.5f, cY + 0.5f, cZ + 0.5f));
                                normals.Add(new Vector3(0, 1, 0));
                                UVs.Add(new Vector2(cLX, cHY));
                                UVs.Add(new Vector2(cLX, cLY));
                                UVs.Add(new Vector2(cHX, cLY));
                                UVs.Add(new Vector2(cHX, cHY));

                                int[] addedIndices = new int[]{
                                    0 + totalIndices, 2 + totalIndices, 3 + totalIndices, 0 + totalIndices, 1 + totalIndices, 2 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount += 2;
                            }
                        }
                    }
                }
            }

            VertexCustom[] vertices = new VertexCustom[positions.Count];
            int[] indices = new int[listIndices.Count];

            for (int i = 0; i < positions.Count; i++) {
                vertices[i] = new VertexCustom(new Vector3(positions[i].X, positions[i].Y, positions[i].Z), new Vector3(normals[i / 4].X, normals[i / 4].Y, normals[i / 4].Z), new Vector2(UVs[i].X, UVs[i].Y));
            }

            for (int i = 0; i < listIndices.Count; i++) {
                indices[i] = listIndices[i];
            }

            compileCallback?.Invoke(vertices, indices, primitiveCount, x, y, z);

        }


    }

}