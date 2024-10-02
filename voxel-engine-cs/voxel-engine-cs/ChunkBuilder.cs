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
        public Dictionary<int, Dictionary<int, Dictionary<int, VertexCustom[]>>> vertices = new Dictionary<int, Dictionary<int, Dictionary<int, VertexCustom[]>>>();
        public Dictionary<int, Dictionary<int, Dictionary<int, int[]>>> indices = new Dictionary<int, Dictionary<int, Dictionary<int, int[]>>>();
        public Dictionary<int, Dictionary<int, Dictionary<int, int>>> primitiveCount = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();

        public void generateChunk(int x, int y, int z) {
            bool alreadyExists = false;
            if (chunk.ContainsKey(x)) {
                if (chunk[x].ContainsKey(y)) {
                    if (chunk[x][y].ContainsKey(z)) {
                        alreadyExists = true;
                    }
                }
            }

            if (!alreadyExists) {
                if (!chunk.ContainsKey(x)) {
                    chunk[x] = new Dictionary<int, Dictionary<int, Block[,,]>>();
                }

                if (!chunk[x].ContainsKey(y)) {
                    chunk[x][y] = new Dictionary<int, Block[,,]>();
                }

                if (!chunk[x][y].ContainsKey(z)) {
                    chunk[x][y][z] = new Block[chunkSize, chunkSize, chunkSize];
                }

                if (!emptyChunk.ContainsKey(x)) {
                    emptyChunk[x] = new Dictionary<int, Dictionary<int, bool>>();
                }

                if (!emptyChunk[x].ContainsKey(y)) {
                    emptyChunk[x][y] = new Dictionary<int, bool>();
                }

                if (!emptyChunk[x][y].ContainsKey(z)) {
                    emptyChunk[x][y][z] = false;
                }
                int totalBlocks = 0;
                for (int cX = x * chunkSize; cX < (x * chunkSize) + chunkSize; cX++) {
                    for (int cY = y * chunkSize; cY < (y * chunkSize) + chunkSize; cY++) {
                        for (int cZ = z * chunkSize; cZ < (z * chunkSize) + chunkSize; cZ++) {

                            int aX = cX - (x * chunkSize);
                            int aY = cY - (y * chunkSize);
                            int aZ = cZ - (z * chunkSize);

                            float boise = (int)Math.Round(noise.GetNoise(cX * noiseScale, cZ * noiseScale) * vertScale);
                            float coise = (int)Math.Round(noise.GetNoise(cX * noiseScale, cY * noiseScale, cZ * noiseScale));

                            if (cY <= boise && coise > threshold) {
                                if (cY == boise) {
                                    chunk[x][y][z][aX, aY, aZ] = new Block(cX, cY, cZ, 0);
                                } else if (cY < boise && cY > boise - 5) {
                                    chunk[x][y][z][aX, aY, aZ] = new Block(cX, cY, cZ, 1);
                                } else if (cY <= boise - 5) {
                                    chunk[x][y][z][aX, aY, aZ] = new Block(cX, cY, cZ, 2);
                                }
                                totalBlocks++;
                            } else {
                                chunk[x][y][z][aX, aY, aZ] = new Block(cX, cY, cZ, -1);
                            }

                        }
                    }
                }
                if (totalBlocks == 0) {
                    emptyChunk[x][y][z] = true;
                }
                //regenerateChunk(x, y, z);
            }
        }


        public void compile(int x, int y, int z) {

            if (!primitiveCount.ContainsKey(x)) {
                primitiveCount[x] = new Dictionary<int, Dictionary<int, int>>();
            }
            if (!primitiveCount[x].ContainsKey(y)) {
                primitiveCount[x][y] = new Dictionary<int, int>();
            }

            primitiveCount[x][y][z] = 0;

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();
            List<float> AO = new List<float>();
            List<int> listIndices = new List<int>();
            int totalIndices = 0;

            for (int b = 0; b < chunkSize; b++) {
                for (int c = 0; c < chunkSize; c++) {
                    for (int d = 0; d < chunkSize; d++) {

                        if (chunk[x][y][z][b, c, d].tex >= 0) {

                            int cX = b + (x * chunkSize);
                            int cY = c + (y * chunkSize);
                            int cZ = d + (z * chunkSize);
                            float cLX = chunk[x][y][z][b, c, d].lx;
                            float cHX = chunk[x][y][z][b, c, d].hx;
                            float cLY = chunk[x][y][z][b, c, d].ly;
                            float cHY = chunk[x][y][z][b, c, d].hy;

                            bool[,,] around = new bool[3, 3, 3];

                            if (occlusionEnabled) {
                                for (int ax = 0; ax < 3; ax++) {
                                    for (int ay = 0; ay < 3; ay++) {
                                        for (int az = 0; az < 3; az++) {
                                            if (!(ax == 0 && ay == 0 && az == 0)) {
                                                around[ax, ay, az] = checkForBlock(cX + (ax - 1), cY + (ay - 1), cZ + (az - 1));
                                            }
                                        }
                                    }
                                }
                            } else {
                                around[0, 1, 1] = checkForBlock(cX - 1, cY, cZ);
                                around[1, 0, 1] = checkForBlock(cX, cY - 1, cZ);
                                around[1, 1, 0] = checkForBlock(cX, cY, cZ - 1);
                                around[2, 1, 1] = checkForBlock(cX + 1, cY, cZ);
                                around[1, 2, 1] = checkForBlock(cX, cY + 1, cZ);
                                around[1, 1, 2] = checkForBlock(cX, cY, cZ + 1);
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
                                if (occlusionEnabled) {
                                    AO.Add(around[0, 0, 0] || around[1, 0, 0] || around[0, 1, 0] ? 1 : 0);
                                    AO.Add(around[0, 2, 0] || around[1, 2, 0] || around[0, 1, 0] ? 1 : 0);
                                    AO.Add(around[2, 2, 0] || around[1, 2, 0] || around[2, 1, 0] ? 1 : 0);
                                    AO.Add(around[2, 0, 0] || around[1, 0, 0] || around[2, 1, 0] ? 1 : 0);
                                } else {
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                }


                                int[] addedIndices = new int[]{
                                    0 + totalIndices, 2 + totalIndices, 1 + totalIndices, 0 + totalIndices, 3 + totalIndices, 2 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount[x][y][z] += 2;
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
                                if (occlusionEnabled) {
                                    AO.Add(around[0, 0, 2] || around[1, 0, 2] || around[0, 1, 2] ? 1 : 0);
                                    AO.Add(around[0, 2, 2] || around[1, 2, 2] || around[0, 1, 2] ? 1 : 0);
                                    AO.Add(around[2, 2, 2] || around[1, 2, 2] || around[2, 1, 2] ? 1 : 0);
                                    AO.Add(around[2, 0, 2] || around[1, 0, 2] || around[2, 1, 2] ? 1 : 0);
                                } else {
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                }

                                int[] addedIndices = new int[]{
                                    0 + totalIndices, 1 + totalIndices, 2 + totalIndices, 0 + totalIndices, 2 + totalIndices, 3 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount[x][y][z] += 2;
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
                                if (occlusionEnabled) {
                                    AO.Add(around[0, 2, 2] || around[0, 1, 2] || around[0, 2, 1] ? 1 : 0);
                                    AO.Add(around[0, 0, 2] || around[0, 1, 2] || around[0, 0, 1] ? 1 : 0);
                                    AO.Add(around[0, 0, 0] || around[0, 1, 0] || around[0, 0, 1] ? 1 : 0);
                                    AO.Add(around[0, 2, 0] || around[0, 1, 0] || around[0, 2, 1] ? 1 : 0);
                                } else {
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                }

                                int[] addedIndices = new int[]{
                                    2 + totalIndices, 3 + totalIndices, 0 + totalIndices, 2 + totalIndices, 0 + totalIndices, 1 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount[x][y][z] += 2;
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
                                if (occlusionEnabled) {
                                    AO.Add(around[2, 2, 0] || around[2, 1, 0] || around[2, 2, 1] ? 1 : 0);
                                    AO.Add(around[2, 0, 0] || around[2, 1, 0] || around[2, 0, 1] ? 1 : 0);
                                    AO.Add(around[2, 0, 2] || around[2, 1, 2] || around[2, 0, 1] ? 1 : 0);
                                    AO.Add(around[2, 2, 2] || around[2, 1, 2] || around[2, 2, 1] ? 1 : 0);
                                } else {
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                }

                                int[] addedIndices = new int[]{
                                    0 + totalIndices, 1 + totalIndices, 2 + totalIndices, 0 + totalIndices, 2 + totalIndices, 3 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount[x][y][z] += 2;
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
                                if (occlusionEnabled) {
                                    AO.Add(around[0, 0, 0] || around[1, 0, 0] || around[0, 0, 1] ? 1 : 0);
                                    AO.Add(around[0, 0, 2] || around[1, 0, 2] || around[0, 0, 1] ? 1 : 0);
                                    AO.Add(around[2, 0, 2] || around[1, 0, 2] || around[2, 0, 1] ? 1 : 0);
                                    AO.Add(around[2, 0, 0] || around[1, 0, 0] || around[2, 0, 1] ? 1 : 0);
                                } else {
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                }

                                int[] addedIndices = new int[]{
                                    1 + totalIndices, 2 + totalIndices, 3 + totalIndices, 1 + totalIndices, 3 + totalIndices, 0 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount[x][y][z] += 2;
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
                                if (occlusionEnabled) {
                                    AO.Add(around[0, 2, 2] || around[1, 2, 2] || around[0, 2, 1] ? 1 : 0);
                                    AO.Add(around[0, 2, 0] || around[1, 2, 0] || around[0, 2, 1] ? 1 : 0);
                                    AO.Add(around[2, 2, 0] || around[1, 2, 0] || around[2, 2, 1] ? 1 : 0);
                                    AO.Add(around[2, 2, 2] || around[1, 2, 2] || around[2, 2, 1] ? 1 : 0);
                                } else {
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                    AO.Add(0);
                                }

                                int[] addedIndices = new int[]{
                                    0 + totalIndices, 2 + totalIndices, 3 + totalIndices, 0 + totalIndices, 1 + totalIndices, 2 + totalIndices
                                };

                                listIndices.AddRange(addedIndices);

                                totalIndices += 4;
                                primitiveCount[x][y][z] += 2;
                            }
                        }
                    }
                }
            }

            if (!vertices.ContainsKey(x)) {
                vertices[x] = new Dictionary<int, Dictionary<int, VertexCustom[]>>();
            }
            if (!vertices[x].ContainsKey(y)) {
                vertices[x][y] = new Dictionary<int, VertexCustom[]>();
            }

            if (!indices.ContainsKey(x)) {
                indices[x] = new Dictionary<int, Dictionary<int, int[]>>();
            }
            if (!indices[x].ContainsKey(y)) {
                indices[x][y] = new Dictionary<int, int[]>();
            }

            vertices[x][y][z] = new VertexCustom[positions.Count];
            indices[x][y][z] = new int[listIndices.Count];

            for (int i = 0; i < positions.Count; i++) {
                vertices[x][y][z][i] = new VertexCustom(new Vector3(positions[i].X, positions[i].Y, positions[i].Z), new Vector3(normals[i / 4].X, normals[i / 4].Y, normals[i / 4].Z), new Vector2(UVs[i].X, UVs[i].Y), AO[i]);
            }

            for (int i = 0; i < listIndices.Count; i++) {
                indices[x][y][z][i] = listIndices[i];
            }


        }


    }

}