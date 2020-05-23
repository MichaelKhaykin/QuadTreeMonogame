using MichaelLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace QuadTreeMonogame
{
    public class Pixel : IHasPixel
    {
        public Rectangle Rectangle { get; set; }

        public Color Data { get; set; }

        public Pixel(Rectangle rect, Color color)
        {
            Rectangle = rect;
            Data = color;
        }
    }

    public class QuadTreeImageDemo : Screen
    {
        Sprite image;

        ImageManipulationQuadTree<Pixel> pixels;

        Sprite quadTreeRender;
        Texture2D quadTreeTexture;


        private Texture2D MakeTexture(ImageManipulationQuadTree<Pixel> qTree)
        {
            var minX = int.MaxValue;
            var maxX = 0;
            var minY = int.MaxValue;
            var maxY = 0;
            foreach (var item in qTree.RectanglesInBounds)
            {
                if (item.Rectangle.X < minX)
                {
                    minX = item.Rectangle.X;
                }
                if (item.Rectangle.X > maxX)
                {
                    maxX = item.Rectangle.X;
                }
                if (item.Rectangle.Y < minY)
                {
                    minY = item.Rectangle.Y;
                }
                if (item.Rectangle.Y > maxY)
                {
                    maxY = item.Rectangle.Y;
                }
            }

            var testBitMap = new System.Drawing.Bitmap(maxX - minX + 1, maxY - minY + 1);
            for (int i = 0; i < qTree.RectanglesInBounds.Count; i++)
            {
                var item = qTree.RectanglesInBounds[i].Rectangle;
                var data = qTree.RectanglesInBounds[i].Data;
                var color = System.Drawing.Color.FromArgb(data.A, data.R, data.G, data.B);
                testBitMap.SetPixel(item.X - minX, item.Y - minY, color);
            }
            MemoryStream yeet = new MemoryStream();
            testBitMap.Save(yeet, ImageFormat.Png);
            var testTexture = Texture2D.FromStream(Graphics, yeet);

            return testTexture;
        }
        public QuadTreeImageDemo(GraphicsDevice graphics, ContentManager content)
            : base(graphics, content)
        {
            var texture = Content.Load<Texture2D>("quadTreeImage");
            image = new Sprite(texture, new Vector2(texture.Width / 2, texture.Height / 2), Color.White, Vector2.One);

            pixels = new ImageManipulationQuadTree<Pixel>(texture.Bounds);
            Color[] array = new Color[image.Texture.Width * image.Texture.Height];
            image.Texture.GetData(array);

            for (int i = 0; i < array.Length; i++)
            {
                var y = i / image.Texture.Width;
                var x = i % image.Texture.Width;

                var rect = new Microsoft.Xna.Framework.Rectangle(x, y, 1, 1);
                var color = array[i];
                pixels.Add(new Pixel(rect, color));
            }


            var test = pixels.Retrieve(new List<ImageManipulationQuadTree<Pixel>>(), 7);
            var useful = test.Where(m => m.RectanglesInBounds.Count > 2900);

            List<Texture2D> textures = new List<Texture2D>();

            foreach (var item in useful)
            {
                textures.Add(MakeTexture(item));
            }

            var allData = pixels.Retrieve(new List<Pixel>(), 7);

            quadTreeTexture = new Texture2D(Graphics, texture.Width, texture.Height);
            var stream = new MemoryStream();
            quadTreeTexture.SaveAsPng(stream, texture.Width, texture.Height);
            System.Drawing.Bitmap map = new System.Drawing.Bitmap(stream);

            for (int i = 0; i < allData.Count; i++)
            {
                var convertedColor = System.Drawing.Color.FromArgb(allData[i].Data.A, allData[i].Data.R, allData[i].Data.G, allData[i].Data.B);
                map.SetPixel(allData[i].Rectangle.X, allData[i].Rectangle.Y, convertedColor);
            }

            var newStream = new MemoryStream();
            map.Save(newStream, ImageFormat.Png);

            quadTreeTexture = Texture2D.FromStream(Graphics, newStream);

            Vector2 position = new Vector2(texture.Width * 3 / 2, texture.Height / 2);
            quadTreeRender = new Sprite(quadTreeTexture, new Vector2(700, 200), Color.White, Vector2.One);

            List<Sprite> sprites = new List<Sprite>();
            var mx = Graphics.Viewport.Width / 2 + textures[0].Width / 2;
            var my = textures[0].Height / 2;
            var largestHeight = 0;
            for (int i = 0; i < textures.Count; i++)
            {
                var item = textures[i];
                if (item.Height > largestHeight)
                {
                    largestHeight = item.Height;
                }

                var sprite = new Sprite(item, new Vector2(mx, my), Color.White, Vector2.One);
                sprites.Add(sprite);
                if (i + 1 >= textures.Count) break;

                if (mx + item.Width / 2 + textures[i + 1].Width / 2 > Graphics.Viewport.Width)
                {
                    mx = Graphics.Viewport.Width / 2 + textures[i + 1].Width / 2;
                    my += largestHeight;
                }
                else
                {
                    mx += item.Width / 2 + textures[i + 1].Width / 2;
                }
            }


            Sprites.Add(image);
            Sprites.AddRange(sprites);
            //Sprites.Add(quadTreeRender);
        }
    }
}