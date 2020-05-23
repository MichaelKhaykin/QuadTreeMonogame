using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace QuadTreeMonogame
{
    public class ImageManipulationQuadTree<T>
        where T : IHasPixel
    {
        public Rectangle Bounds;

        protected ImageManipulationQuadTree<T>[] ChildNodes;

        public List<T> RectanglesInBounds;

        protected int level;

        public ImageManipulationQuadTree(Rectangle initalBounds)
            :this(0, initalBounds)
        {

        }

        public ImageManipulationQuadTree(int pLevel, Rectangle initalBounds)
        {
            level = pLevel;

            Bounds = initalBounds;

            RectanglesInBounds = new List<T>();

            ChildNodes = new ImageManipulationQuadTree<T>[4];
        }
        private int GetIndex(Rectangle rectangleToSearchFor)
        {
            int index = -1;

            var horizontalMidPoint = Bounds.X + Bounds.Width / 2;
            var verticalMidPoint = Bounds.Y + Bounds.Height / 2;

            bool isInTopQuad = rectangleToSearchFor.Y + rectangleToSearchFor.Height < verticalMidPoint;
            bool isInBotQuad = rectangleToSearchFor.Y > verticalMidPoint;

            bool isInLeftSide = rectangleToSearchFor.X + rectangleToSearchFor.Width < horizontalMidPoint;
            bool isInRightSide = rectangleToSearchFor.X > horizontalMidPoint;

            if (isInLeftSide)
            {
                if (isInTopQuad)
                {
                    index = 1;
                }
                else if (isInBotQuad)
                {
                    index = 2;
                }
            }
            else if (isInRightSide)
            {
                if (isInTopQuad)
                {
                    index = 0;
                }
                else if (isInBotQuad)
                {
                    index = 3;
                }
            }

            return index;
        }
        public void Clear()
        {
            RectanglesInBounds.Clear();
            for (int i = 0; i < ChildNodes.Length; i++)
            {
                if (ChildNodes[i] != null)
                {
                    ChildNodes[i].Clear();
                    ChildNodes[i] = null;
                }
            }
        }
        public void Add(T rect)
        {
            if (ChildNodes[0] != null)
            {
                var index = GetIndex(rect.Rectangle);

                if (index != -1)
                {
                    ChildNodes[index].Add(rect);
                    return;
                }
            }

            RectanglesInBounds.Add(rect);

            int lengthToLoop = RectanglesInBounds.Count < 100 ? RectanglesInBounds.Count : 100;
            
            double avgColor = 0;
            for(int i = 0; i < lengthToLoop; i++)
            {
                var pixel = RectanglesInBounds[i];

                avgColor += (0.11) * pixel.Data.B + (0.59) * pixel.Data.G + (0.30) * pixel.Data.R;
            }
            avgColor /= RectanglesInBounds.Count;

            var myColor = (0.11) * rect.Data.B + (0.59) * rect.Data.G + (0.30) * rect.Data.R;

            var difference = Math.Abs(myColor - avgColor) * 100 / 255;

            if (difference > 85)
            {
                if (ChildNodes[0] == null)
                {
                    Split();
                }

                //move any objects that are in the parent list into the child nodes
                //if possible
                for (int i = 0; i < RectanglesInBounds.Count; i++)
                {
                    var index = GetIndex(RectanglesInBounds[i].Rectangle);
                    //meaning that this rectangle could actually be stored in a child node
                    if (index != -1)
                    {
                        ChildNodes[index].Add(RectanglesInBounds[i]);
                        RectanglesInBounds.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        public List<T> Retrieve(List<T> returnObjs, Rectangle rect)
        {
            var index = GetIndex(rect);
            if (index != -1 && ChildNodes[0] != null)
            {
                ChildNodes[index].Retrieve(returnObjs, rect);
            }

            returnObjs.AddRange(RectanglesInBounds);

            return returnObjs;
        }

        public List<ImageManipulationQuadTree<T>> Retrieve(List<ImageManipulationQuadTree<T>> returnObjs, int levelDepth = -1)
        {
            if (level == levelDepth)
            {
                return returnObjs;
            }

            returnObjs.Add(this);
            if (ChildNodes[0] != null)
            {
                foreach (var node in ChildNodes)
                {
                    node.Retrieve(returnObjs, levelDepth);
                }
            }

            return returnObjs;
        }
     
        public List<T> Retrieve(List<T> returnObjs, int levelDepth = -1)
        {
            if (level == levelDepth)
            {
                return returnObjs;
            }

            returnObjs.AddRange(RectanglesInBounds);
            if (ChildNodes[0] != null)
            {
                foreach (var node in ChildNodes)
                {
                    node.Retrieve(returnObjs, levelDepth);
                }
            }

            return returnObjs;
        }
        private void Split()
        {
            var x = Bounds.X;
            var y = Bounds.Y;
            var halfWidth = Bounds.Width / 2;
            var halfHeight = Bounds.Height / 2;

            ChildNodes[0] = new ImageManipulationQuadTree<T>(level + 1, new Rectangle(x + halfWidth, y, halfWidth, halfHeight));
            ChildNodes[1] = new ImageManipulationQuadTree<T>(level + 1, new Rectangle(x, y, halfWidth, halfHeight));
            ChildNodes[2] = new ImageManipulationQuadTree<T>(level + 1, new Rectangle(x, y + halfHeight, halfWidth, halfHeight));
            ChildNodes[3] = new ImageManipulationQuadTree<T>(level + 1, new Rectangle(x + halfWidth, y + halfHeight, halfWidth, halfHeight));
        }
    }
}
