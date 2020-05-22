using MichaelLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace QuadTreeMonogame
{
    public class Main : Screen
    {
        public Main(GraphicsDevice graphics, ContentManager content) 
            : base(graphics, content)
        {

        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState kbs = Keyboard.GetState();
            if(kbs.IsKeyDown(Keys.C))
            {
                Game1.CurrentScreen = Screens.QuadTreeCollision;
            }
            else if(kbs.IsKeyDown(Keys.I))
            {
                Game1.CurrentScreen = Screens.QuadTreeImage;
            }

            base.Update(gameTime);
        }
    }
}