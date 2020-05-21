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

        List<Sprite> circles = new List<Sprite>();
        Texture2D circleTexture;

        MouseState oldMouse;

        QuadTree qTree;

        public static List<Rectangle> Splits = new List<Rectangle>();

        Texture2D pixel;

        List<Color> Colors = new List<Color>();
        int currentIndex = 0;

        Viewport mainView;
        Viewport uiView;

        Sprite whiteBackGround;

        TextLabel radiusLabel;
        TextBox radiusTextBox;

        int gap = 200;
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

            var height = GraphicsDevice.Viewport.Height;
            var width = GraphicsDevice.Viewport.Width;

            whiteBackGround = new Sprite(Content.Load<Texture2D>("whitebackground"), new Vector2(gap / 2, height / 2), Color.White, Vector2.One);

            mainView = new Viewport(0, 0, width - gap, height);
            uiView = new Viewport(width - gap, 0, width, height);

            var font = Content.Load<SpriteFont>("Font");


            radiusTextBox = new TextBox(GraphicsDevice, new Rectangle(gap / 2 - 50, 50, 100, 18), font,
                                        Color.Black, Color.White, Color.Black,
                                        false, false)
            {
                IsNumbersOnly = true
            };

            radiusTextBox.Add('5');

            var properties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach(var prop in properties)
            {
                var value = (Color)prop.GetValue(null);
                if(value == Color.Transparent || value == Color.Black)
                {
                    continue;
                }

                Colors.Add(value);
            }

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            circles = new List<Sprite>();
            circleTexture = Content.Load<Texture2D>("circley");

            qTree = new QuadTree(0, new Rectangle(0, 0, width - gap, GraphicsDevice.Viewport.Height));

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouse = Mouse.GetState();

            Window.Title = "Epstein didn't kill himself";


            var radiusValue = 5;
            float ogSize = circleTexture.Width / 2;
            if (radiusTextBox.Text.Length > 0)
            {
                radiusValue = int.Parse(radiusTextBox.Text);
                radiusValue = MathHelper.Clamp(radiusValue, 5, 10);
            }
            foreach (var circle in circles)
            {
                circle.Color = Color.Red;
                circle.Scale = Vector2.One * (radiusValue / ogSize);
            }

            if (mainView.Bounds.Contains(mouse.Position))
            {
                if (mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released)
                {
                    var circle = new Sprite(circleTexture, mouse.Position.ToVector2(), Color.Red, Vector2.One * (radiusValue / ogSize));
                    circles.Add(circle);

                    qTree.Add(circle.HitBox);
                }

                foreach (var circle in circles)
                {
                    if (!circle.HitBox.Contains(mouse.Position)) continue;

                    var resultingObjects = qTree.Retrieve(new List<Rectangle>(), circle.HitBox);
                    foreach (var rect in resultingObjects)
                    {
                        foreach (var circlex in circles)
                        {
                            if (circlex.HitBox == rect)
                            {
                                circlex.Color = Color.Blue;
                            }
                        }
                    }
                    break;
                }

                oldMouse = mouse;
            }
            else
            {
                var localMousePos = new Vector2(mouse.X - (GraphicsDevice.Viewport.Width - gap), mouse.Y);
                radiusTextBox.Update(gameTime, localMousePos);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            #region MainQuadViewport
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.Viewport = mainView;
            // TODO: Add your drawing code here

            spriteBatch.Begin();

            foreach(var circle in circles)
            {
                circle.Draw(spriteBatch);
            }

            currentIndex = 0;

            foreach(var rect in Splits)
            {
                var currColor = Colors[currentIndex];
                currentIndex++;

                var verticalLine = new Line(new Vector2(rect.X + rect.Width / 2, rect.Y), new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height));
                var horizontalLine = new Line(new Vector2(rect.X, rect.Y + rect.Height / 2), new Vector2(rect.X + rect.Width, rect.Y + rect.Height / 2));

                int thickness = 1;

                spriteBatch.DrawLine(pixel, currColor, verticalLine, thickness);
                spriteBatch.DrawLine(pixel, currColor, horizontalLine, thickness);
            }

            spriteBatch.End();

            base.Draw(gameTime);

            #endregion

            #region UI

            GraphicsDevice.Viewport = uiView;

            spriteBatch.Begin();

            whiteBackGround.Draw(spriteBatch);
            radiusTextBox.Draw(spriteBatch);

            spriteBatch.End();

            #endregion
        }
    }
}
