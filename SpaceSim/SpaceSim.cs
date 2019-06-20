#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace SpaceSim
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpaceSim : Game
    {
        GraphicsDeviceManager graphDev;
        Color background = new Color(2, 0, 6);
        public static SpaceSim World;
        Vector3 cameraPosition = new Vector3(0f, 30f, 80f);
        Vector3 cameraLookAt = new Vector3(0f, 0f, 0f);
        Matrix cameraOrientationMatrix = Matrix.Identity;
        public Matrix View;
        public Matrix Projection;
        public static GraphicsDevice Graphics;
		Random random = new Random();

        List<Sphere> spheres;

        Sphere sun;
		Sphere earth;
		Sphere mars;
		Sphere jupiter;
		Sphere saturn;
		Sphere uranus;
        Sphere moon;

        Spaceship spaceship;
        Vector3 spaceshipPosition = new Vector3(0f, 28f, 77f);
        Matrix spaceshipOrientationMatrix = Matrix.CreateFromYawPitchRoll(0f, -0.17f, 0f);
        Vector3 spaceshipFollowPoint = new Vector3(0f, 0.09f, 0.2f);
        Vector3 spaceshipLookAtPoint = new Vector3(0f, 0.05f, 0f);
        Vector3 bulletSpawnPosition = new Vector3(0f, 0f, -0.1f);

        Skybox skybox;

        SpriteBatch spriteBatch;
        Texture2D reticle, controls;
        Point mousePosition;
        bool wKeyDown, aKeyDown, sKeyDown, dKeyDown;
        bool mouseButton, mouseDown, lastMouseButton;
        float reticleHalfWidth, reticleHalfHeight;

        Vector2 screenCenter;

        Matrix transformRotatingSphere;
        Matrix sphereRotationY;

		float moonRotation;

        public SpaceSim()
            : base()
        {
            Content.RootDirectory = "Content";

            World = this;
            graphDev = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            Graphics = GraphicsDevice;

#if DEBUG
            graphDev.PreferredBackBufferWidth = 1600;
            graphDev.PreferredBackBufferHeight = 900;
            graphDev.IsFullScreen = false;
#else
            graphDev.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphDev.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphDev.IsFullScreen = true;
#endif
            graphDev.ApplyChanges();

            SetupCamera(true);
            Window.Title = "HvA - Simulation & Physics - Opdracht 6 - SpaceSim";
            spriteBatch = new SpriteBatch(Graphics);

            spheres = new List<Sphere>();

            spheres.Add(sun = new Sphere(Matrix.CreateTranslation(0, 0, 0), Color.Yellow, 30, 2, 0));
			spheres.Add(earth = new Sphere(Matrix.CreateTranslation(16, 0, 0), Color.DeepSkyBlue, 30, 1, 0.31f));
			spheres.Add(mars = new Sphere(Matrix.CreateTranslation(21, 0, 0), Color.Red, 30, 0.6f, 0.39f));
			spheres.Add(jupiter = new Sphere(Matrix.CreateTranslation(27, 0, 0), Color.Orange, 30, 1.7f, 0.15f));
			spheres.Add(saturn = new Sphere(Matrix.CreateTranslation(36, 0, 0), Color.Khaki, 30, 1.6f, 0.5f));
			spheres.Add(uranus = new Sphere(Matrix.CreateTranslation(43, 0, 0), Color.Cyan, 30, 1.5f, 0.21f));
            spheres.Add(moon = new Sphere(Matrix.CreateTranslation(0.5f, 0.5f, 0.5f) * Matrix.CreateTranslation(earth.transform.Translation) * Matrix.CreateTranslation(2, 0, 0), Color.LightGray, 30, 0.5f, 1.5f));

            //moon.transform *= Matrix.CreateTranslation(-earth.transform.Translation);

            Random rand = new Random();

            foreach (Sphere rotatingSphere in spheres) {
                if (rotatingSphere.color != Color.Yellow)
                {
                    rotatingSphere.transform *= Matrix.CreateRotationY((float)(rand.NextDouble() * 2.0 * Math.PI));
                }
            }
			//moon.transform *= Matrix.CreateRotationX(1.570796f);
			//moon.transform *= Matrix.CreateRotationY(-1.570796f);
			//moon.transform *= Matrix.CreateTranslation(earth.transform.Translation);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            spaceship = new Spaceship(spaceshipOrientationMatrix * Matrix.CreateTranslation(spaceshipPosition), Content);
            skybox = new Skybox(Matrix.CreateScale(1000f) * Matrix.CreateTranslation(cameraPosition), Content);
            reticle = Content.Load<Texture2D>("Reticle");
            reticleHalfWidth = reticle.Width / 2f;
            reticleHalfHeight = reticle.Height / 2f;
            controls = Content.Load<Texture2D>("Controls");

            IsMouseVisible = false;
        }

        private void SetupCamera(bool initialize = false)
        {
            View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, cameraOrientationMatrix.Up);
            if (initialize) Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SpaceSim.World.GraphicsDevice.Viewport.AspectRatio, 0.1f, 2000.0f);
        }

        int i = 0;
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(background);

            SetupCamera();

            skybox.Draw();

            foreach (Sphere sphere in spheres)
            {
                sphere.Draw();
            }



            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(reticle, new Vector2(mousePosition.X - reticleHalfWidth, mousePosition.Y - reticleHalfHeight), Color.White);
            spriteBatch.Draw(controls, new Vector2(10f, 10f), Color.White);
            spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)
        {
            TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
            //moon.transform *= Matrix.CreateTranslation(-earth.transform.Translation);
            foreach (Sphere rotatingSphere in spheres)
            {
                if (rotatingSphere.color != Color.Yellow)
                {
                    transformRotatingSphere = rotatingSphere.transform;
                    sphereRotationY = Matrix.CreateRotationY((float)elapsedGameTime.TotalSeconds * rotatingSphere.rotatingSpeed);
                    rotatingSphere.transform = transformRotatingSphere * sphereRotationY;
                }
            }


			//moon.transform = Matrix.CreateScale(0.5f);
			moon.transform = Matrix.CreateTranslation(2f, 0.0f, 0.0f);
			moon.transform *= Matrix.CreateRotationY(moonRotation += 1.5f * (float)gameTime.ElapsedGameTime.TotalSeconds);
			moon.transform *= Matrix.CreateRotationX(MathHelper.PiOver2);
			//moon.transform *= Matrix.CreateTranslation(Vector3.Transform(Vector3.Zero, earth.transform));
			//moon.transform *= Matrix.CreateTranslation(earth.transform.Translation);
			moon.transform *= earth.transform;

			cameraPosition = Vector3.Transform(spaceshipFollowPoint, spaceship.Transform);
            cameraLookAt = Vector3.Transform(spaceshipLookAtPoint, spaceship.Transform);    
            cameraOrientationMatrix = spaceshipOrientationMatrix;

            // Helpers for input
            KeyboardState keyboard = Keyboard.GetState();
            wKeyDown = keyboard.IsKeyDown(Keys.W);
            aKeyDown = keyboard.IsKeyDown(Keys.A);
            sKeyDown = keyboard.IsKeyDown(Keys.S);
            dKeyDown = keyboard.IsKeyDown(Keys.D);
            if (keyboard.IsKeyDown(Keys.Escape)) Exit();
            MouseState mouse = Mouse.GetState();
            mousePosition = mouse.Position;
            mouseButton = mouse.LeftButton == ButtonState.Pressed;
            mouseDown = mouseButton && !lastMouseButton;
            lastMouseButton = mouseButton;

            skybox.Transform = Matrix.CreateScale(1000f) * Matrix.CreateTranslation(cameraPosition);



            base.Update(gameTime);
        }

        static void RotateOrientationMatrixByYawPitchRoll(ref Matrix matrix, float yawChange, float pitchChange, float rollChange)
        {
            if (rollChange != 0f || yawChange != 0f || pitchChange != 0f)
            {
                Vector3 pitch = matrix.Right * pitchChange;
                Vector3 yaw = matrix.Up * yawChange;
                Vector3 roll = matrix.Forward * rollChange;

                Vector3 overallOrientationChange = pitch + yaw + roll;
                float overallAngularChange = overallOrientationChange.Length();
                Vector3 overallRotationAxis = Vector3.Normalize(overallOrientationChange);
                Matrix orientationChange = Matrix.CreateFromAxisAngle(overallRotationAxis, overallAngularChange);
                matrix *= orientationChange;
            }
        }
    }
}
