using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace QuadTreeMonogame
{
    public class QuadTree<T> where T : IHasRectangle
    {
        public Rectangle Bounds;

        protected QuadTree<T>[] ChildNodes;

        protected List<T> RectanglesInBounds;

        protected const int MaxObjectsInAZone = 4;

        protected int level;
        public QuadTree(int pLevel, Rectangle initalBounds)
        {
            level = pLevel;

            Bounds = initalBounds;

            RectanglesInBounds = new List<T>();

            ChildNodes = new QuadTree<T>[4];
        }

        public QuadTree(Rectangle initalBounds)
            : this(pLevel: 0, initalBounds)
        {

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
        private void Split()
        {
            QuadTreeHitCollisionDemo.Splits.Add(Bounds);

            var x = Bounds.X;
            var y = Bounds.Y;
            var halfWidth = Bounds.Width / 2;
            var halfHeight = Bounds.Height / 2;

            ChildNodes[0] = new QuadTree<T>(level + 1, new Rectangle(x + halfWidth, y, halfWidth, halfHeight));
            ChildNodes[1] = new QuadTree<T>(level + 1, new Rectangle(x, y, halfWidth, halfHeight));
            ChildNodes[2] = new QuadTree<T>(level + 1, new Rectangle(x, y + halfHeight, halfWidth, halfHeight));
            ChildNodes[3] = new QuadTree<T>(level + 1, new Rectangle(x + halfWidth, y + halfHeight, halfWidth, halfHeight));
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

            if (RectanglesInBounds.Count >= MaxObjectsInAZone)
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
        public List<T> Retrieve(List<T> returnObjs, T rect)
        {
            var index = GetIndex(rect.Rectangle);
            if (index != -1 && ChildNodes[0] != null)
            {
                ChildNodes[index].Retrieve(returnObjs, rect);
            }

            returnObjs.AddRange(RectanglesInBounds);

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
    }
}
