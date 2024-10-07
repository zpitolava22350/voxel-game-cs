using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace voxel_engine_cs {

    public static class ResourceManager {
        public static GraphicsDevice GraphicsDevice { get; set; }
    }

    public class Game1: Game {

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;

        World world = new World();

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            AllocConsole();

            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 144.0);
            IsFixedTimeStep = true;

            _graphics.IsFullScreen = false;

            //_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize() {
            // TODO: Add your initialization logic here
            base.Initialize();
            ResourceManager.GraphicsDevice = GraphicsDevice;

            for(int x = -3; x < 4; x++) {
                for (int y = -3; y < 4; y++) {
                    for (int z = -3; z < 4; z++) {
                        //world.chunkGenerateQueue.Enqueue(new Vector3(x, y, z));
                    }
                }
            }
            world.chunkGenerateQueue.Enqueue(new Vector3(0, -1, 0));
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("ArialFont");
            world.lighting = Content.Load<Effect>("Lighting");
            world.fogEnabled = true;
            world.fogNear = 50;
            world.fogFar = 550;

            world.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(world.player.fieldOfView, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);

            world.texture = Content.Load<Texture2D>("texsheet");

            world.texture.GraphicsDevice.SamplerStates[0] = new SamplerState { Filter = TextureFilter.Point, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap };

            world.lighting.Parameters[$"Texture"].SetValue(world.texture);

            //world.lighting.Parameters[$"LightDirection"].SetValue(new Microsoft.Xna.Framework.Vector4(0, 0, 0, 0));
            //world.lighting.Parameters[$"AmbientColor"].SetValue(new Microsoft.Xna.Framework.Vector4(1, 1, 1, 1));

            //world.regenerate();
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Tab))
                Exit();

            world.updateChunks();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {

            //GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.Clear(Color.SkyBlue);

            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(world.player.r, world.player.t, 0);
            Vector3 lookDirection = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Vector3 upDirection = Vector3.Transform(Vector3.Up, rotationMatrix);
            Matrix viewMatrix = Matrix.CreateLookAt(world.player.cameraPosition, world.player.cameraPosition + lookDirection, upDirection);

            //world.lighting.Parameters["WorldViewProjection"].SetValue(Matrix.CreateTranslation(0, 0, 0) * viewMatrix * world.projectionMatrix);

            world.lighting.Parameters["World"].SetValue(Matrix.CreateTranslation(0, 0, 0));
            world.lighting.Parameters["playerPos"].SetValue(world.player.position);

            //world.lighting.Parameters["AmbientColor"].SetValue(new Microsoft.Xna.Framework.Vector4(1f, 1f, 1f, 1f));
            //world.lighting.Parameters["AmbientIntensity"].SetValue(1f);

            world.lighting.Parameters["View"].SetValue(viewMatrix);
            world.lighting.Parameters["Projection"].SetValue(Matrix.CreatePerspectiveFieldOfView(world.player.fieldOfView, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f));

            world.render();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, "FPS: " + (1 / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString("0.0"), new Vector2(10, 10), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
