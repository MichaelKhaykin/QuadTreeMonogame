using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MichaelLibrary;
using System.Collections.Generic;
using System.Reflection;

namespace QuadTreeMonogame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Dictionary<Screens, Screen> ScreenManager = new Dictionary<Screens, Screen>();
        public static Screens CurrentScreen = Screens.QuadTreeCollision;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();

            IsMouseVisible = true;

            base.Initialize();
        }
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ScreenManager.Add(Screens.Main, new Main(GraphicsDevice, Content));
            ScreenManager.Add(Screens.QuadTreeCollision, new QuadTreeHitCollisionDemo(GraphicsDevice, Content));
           // ScreenManager.Add(Screens.QuadTreeImage, new QuadTreeImageDemo(GraphicsDevice, Content));

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            InputManager.Mouse = Mouse.GetState();


            ScreenManager[CurrentScreen].Update(gameTime);

            InputManager.OldMouse = InputManager.Mouse;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            ScreenManager[CurrentScreen].Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
