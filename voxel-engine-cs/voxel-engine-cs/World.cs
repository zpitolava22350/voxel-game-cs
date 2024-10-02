using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace voxel_engine_cs {
    internal class World {

        private Random rnd;
        public Player player;

        private Task<int[,,]>[] generationThreads;
        private Queue<Vector4> chunkUpdateQueue;

        /*
            Task.Run(() => 
            {
                myObject.Method1();  // First method
                myObject.Method2();  // Second method
            });
        */

        public World() {
            rnd = new Random();
            player = new Player();
            generationThreads = new Task<int[,,]>[3];
        }

        public void updateChunks() {
            for(int t=0; t<generationThreads.Length; t++) {
                if (generationThreads[t].Status == TaskStatus.WaitingToRun && chunkUpdateQueue.Count >= 1) {
                    Vector4 currentChunk = chunkUpdateQueue.Dequeue();
                }
            }
        }

    }
}
