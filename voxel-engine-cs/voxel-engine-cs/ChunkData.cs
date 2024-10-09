using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace voxel_engine_cs {
    internal class ChunkData {
        public int[,,] blocks { get; set; }
        public VertexCustom[] vertices { get; set; }
        public int[] indices { get; set; }
        public int primitiveCount { get; set; }

        public ChunkData(int chunkSize) {
            blocks = new int[chunkSize, chunkSize, chunkSize];
            vertices = new VertexCustom[0];
            indices = new int[0];
            primitiveCount = 0;
        }
    }
}

/*
public struct ChunkData {
        public int[,,] blocks;
        public VertexCustom[] vertices;
        public int[] indices;
        public int primitiveCount { get; set; }

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
            Console.WriteLine(primitiveCount + "??????");
        }
    }

*/