using MichaelLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuadTreeMonogame
{
    public class WrapperRectangle : IHasRectangle
    {
        public Rectangle Rectangle { get; set; }

        public static implicit operator Rectangle(WrapperRectangle r) => r.Rectangle;
        public static implicit operator WrapperRectangle(Rectangle r) => new WrapperRectangle() { Rectangle = r };
    }
    public class QuadTreeHitCollisionDemo : Screen
    {
        List<Sprite> circles = new List<Sprite>();
        Texture2D circleTexture;

        QuadTree<WrapperRectangle> qTree;

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

        bool isDragging = false;
        int indexOfDraggedCircle = 0;
        public QuadTreeHitCollisionDemo(GraphicsDevice graphics, ContentManager content) 
            : base(graphics, content)
        {
            var height = graphics.Viewport.Height;
            var width = graphics.Viewport.Width;

            whiteBackGround = new Sprite(Content.Load<Texture2D>("whitebackground"), new Vector2(gap / 2, height / 2), Color.White, Vector2.One);

            mainView = new Viewport(0, 0, width - gap, height);
            uiView = new Viewport(width - gap, 0, width, height);

            var font = Content.Load<SpriteFont>("Font");

            radiusLabel = new TextLabel(new Vector2(gap / 2 - 30, 30), Color.Black, "Radius:", font);

            radiusTextBox = new TextBox(graphics, new Rectangle(gap / 2 - 50, 50, 100, 18), font,
                                        Color.Black, Color.White, Color.Black,
                                        false, false)
            {
                IsNumbersOnly = true
            };

            radiusTextBox.Add('5');

            var properties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var prop in properties)
            {
                var value = (Color)prop.GetValue(null);
                if (value == Color.Transparent || value == Color.Black)
                {
                    continue;
                }

                Colors.Add(value);
            }

            pixel = new Texture2D(graphics, 1, 1);
            pixel.SetData(new[] { Color.White });

            circles = new List<Sprite>();
            circleTexture = Content.Load<Texture2D>("circley");

            qTree = new QuadTree<WrapperRectangle>(new Rectangle(0, 0, width - gap, graphics.Viewport.Height));
        }

        public override void Update(GameTime gameTime)
        {
            //This doesn't have to be done, but it allows for moving the circles and rebuilding
            //the tree on the fly as well as the re-splits 
            qTree.Clear();
            Splits.Clear();

            var radiusValue = 5;
            float ogSize = circleTexture.Width / 2;
            if (radiusTextBox.Text.Length > 0)
            {
                radiusValue = int.Parse(radiusTextBox.Text);
                radiusValue = MathHelper.Clamp(radiusValue, 5, 10);
            }
            for (int i = 0; i < circles.Count; i++)
            {
                var circle = circles[i];

                circle.Color = Color.Red;
                circle.Scale = Vector2.One * (radiusValue / ogSize);

                if (isDragging && i == indexOfDraggedCircle)
                {
                    circle.Position = InputManager.Mouse.Position.ToVector2();
                }

                qTree.Add(circle.HitBox);
            }

            if (mainView.Bounds.Contains(InputManager.Mouse.Position))
            {
                if (InputManager.Mouse.LeftButton == ButtonState.Released)
                {
                    isDragging = false;
                }

                for (int i = 0; i < circles.Count; i++)
                {
                    var circle = circles[i];

                    if (!circle.HitBox.Contains(InputManager.Mouse.Position)) continue;

                    if (InputManager.Mouse.LeftButton == ButtonState.Pressed)
                    {
                        isDragging = true;
                        indexOfDraggedCircle = i;
                    }

                    var resultingObjects = qTree.Retrieve(new List<WrapperRectangle>(), circle.HitBox);
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


                if (InputManager.Mouse.LeftButton == ButtonState.Pressed && InputManager.OldMouse.LeftButton == ButtonState.Released &&
                    !isDragging)
                {
                    var circle = new Sprite(circleTexture, InputManager.Mouse.Position.ToVector2(), Color.Red, Vector2.One * (radiusValue / ogSize));
                    circles.Add(circle);

                    qTree.Add(circle.HitBox);
                }
            }
            else
            {
                var localMousePos = new Vector2(InputManager.Mouse.X - (Graphics.Viewport.Width - gap), InputManager.Mouse.Y);
                radiusTextBox.Update(gameTime, localMousePos);
            }


            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            #region MainQuadViewport

            spriteBatch.End();

            Graphics.Viewport = mainView;
            
            spriteBatch.Begin();

            // TODO: Add your drawing code here

            foreach (var circle in circles)
            {
                circle.Draw(spriteBatch);
            }

            currentIndex = 0;

            foreach (var rect in Splits)
            {
                var currColor = Colors[currentIndex];
                currentIndex++;

                var verticalLine = new Line(new Vector2(rect.X + rect.Width / 2, rect.Y), new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height));
                var horizontalLine = new Line(new Vector2(rect.X, rect.Y + rect.Height / 2), new Vector2(rect.X + rect.Width, rect.Y + rect.Height / 2));

                int thickness = 1;

                spriteBatch.DrawLine(pixel, currColor, verticalLine, thickness);
                spriteBatch.DrawLine(pixel, currColor, horizontalLine, thickness);
            }

            #endregion

            #region UI

            
            spriteBatch.End();
            Graphics.Viewport = uiView;
            spriteBatch.Begin();


            whiteBackGround.Draw(spriteBatch);
            radiusTextBox.Draw(spriteBatch);
            radiusLabel.Draw(spriteBatch);

            #endregion

            base.Draw(spriteBatch);
        }
    }
}
