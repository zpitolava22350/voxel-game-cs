using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace voxel_engine_cs {
    internal class Player {

        private Vector3 pos;
        private Vector3 vel;
        private Vector2 rot;
        private Vector3 prevVel;
        private Vector3 nextVel;
        public float deltaTime { get; private set; }

        private float fov;

        public float mouseSensitivity { get; set; }
        public float movementSpeed { get; set; }
        public bool onGround { get; set; }
        public bool mouseLock { get; set; }
        public bool canMove { get; set; }
        public float gravity { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public float halfWidth { get; set; }
        public float halfHeight { get; set; }
        public float stepHeight { get; set; }
        public bool debugCanStepX { get; set; }
        public bool debugCanStepZ { get; set; }
        public float jumpHeight { get; set; }
        public float damping { get; set; }
        public Player(Microsoft.Xna.Framework.Vector3 position, float moveSpeed) {
            this.pos = position;
            this.movementSpeed = moveSpeed;
            this.mouseSensitivity = 0.002f;
            this.mouseLock = true;
            this.canMove = true;
            this.rot = new Microsoft.Xna.Framework.Vector2(0, 0);
            this.fov = 90f;
            this.gravity = 15f;
            this.width = 0.6f;
            this.height = 1.8f;
            this.halfWidth = this.width / 2;
            this.halfHeight = this.height / 2;
            this.stepHeight = 0.6f;
            this.jumpHeight = 6f;
            this.damping = 10f;
            this.onGround = false;
            this.debugCanStepX = false;
            this.debugCanStepZ = false;
        }

        public void bruh(float buh) {
            deltaTime = buh;
        }

        public Microsoft.Xna.Framework.Vector3 cameraPosition {
            get { return position + new Microsoft.Xna.Framework.Vector3(0, halfHeight * 0.75f, 0); }
        }

        public Microsoft.Xna.Framework.Vector3 position {
            get { return pos; }
            set { pos = value; }
        }
        public float x {
            get { return pos.X; }
            set { pos.X = value; }
        }
        public float y {
            get { return pos.Y; }
            set { pos.Y = value; }
        }
        public float z {
            get { return pos.Z; }
            set { pos.Z = value; }
        }

        public Microsoft.Xna.Framework.Vector3 velocity {
            get { return vel; }
            set { vel = value; }
        }
        public float xVel {
            get { return vel.X; }
            set { vel.X = value; }
        }
        public float yVel {
            get { return vel.Y; }
            set { vel.Y = value; }
        }
        public float zVel {
            get { return vel.Z; }
            set { vel.Z = value; }
        }

        public Microsoft.Xna.Framework.Vector3 prevVelocity {
            get { return prevVel; }
            set { prevVel = value; }
        }
        public float prevxVel {
            get { return prevVel.X; }
            set { prevVel.X = value; }
        }
        public float prevyVel {
            get { return prevVel.Y; }
            set { prevVel.Y = value; }
        }
        public float prevzVel {
            get { return prevVel.Z; }
            set { prevVel.Z = value; }
        }

        public Microsoft.Xna.Framework.Vector3 nextVelocity {
            get { return nextVel; }
            set { nextVel = value; }
        }
        public float nextxVel {
            get { return nextVel.X; }
            set { nextVel.X = value; }
        }
        public float nextyVel {
            get { return nextVel.Y; }
            set { nextVel.Y = value; }
        }
        public float nextzVel {
            get { return nextVel.Z; }
            set { nextVel.Z = value; }
        }

        public float xMax {
            get { return pos.X + halfWidth; }
        }
        public float xMin {
            get { return pos.X - halfWidth; }
        }
        public float yMax {
            get { return pos.Y + halfHeight; }
        }
        public float yMin {
            get { return pos.Y - halfHeight; }
        }
        public float zMax {
            get { return pos.Z + halfWidth; }
        }
        public float zMin {
            get { return pos.Z - halfWidth; }
        }

        public float xMaxNext {
            get { return pos.X + halfWidth + (vel.X * deltaTime); }
        }
        public float xMinNext {
            get { return pos.X - halfWidth + (vel.X * deltaTime); }
        }
        public float yMaxNext {
            get { return pos.Y + halfHeight + (vel.Y * deltaTime); }
        }
        public float yMinNext {
            get { return pos.Y - halfHeight + (vel.Y * deltaTime); }
        }
        public float zMaxNext {
            get { return pos.Z + halfWidth + (vel.Z * deltaTime); }
        }
        public float zMinNext {
            get { return pos.Z - halfWidth + (vel.Z * deltaTime); }
        }

        public float xMaxNextPrev {
            get { return pos.X + halfWidth + (prevVel.X * deltaTime); }
        }
        public float xMinNextPrev {
            get { return pos.X - halfWidth + (prevVel.X * deltaTime); }
        }
        public float yMaxNextPrev {
            get { return pos.Y + halfHeight + (prevVel.Y * deltaTime); }
        }
        public float yMinNextPrev {
            get { return pos.Y - halfHeight + (prevVel.Y * deltaTime); }
        }
        public float zMaxNextPrev {
            get { return pos.Z + halfWidth + (prevVel.Z * deltaTime); }
        }
        public float zMinNextPrev {
            get { return pos.Z - halfWidth + (prevVel.Z * deltaTime); }
        }

        public Microsoft.Xna.Framework.Vector2 rotation {
            get { return rot; }
            set { rot = value; }
        }
        public float r {
            get { return rot.Y; }
            set { rot.Y = value; }
        }
        public float t {
            get { return rot.X; }
            set { rot.X = value; }
        }

        public float fieldOfView {
            get { return fov * (MathHelper.Pi / 180f); }
            set { fov = value; }
        }

}
}
