using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GoGame
{
    public abstract class Component
    {
        public abstract bool Enabled { get; set; }

        public abstract int UpdateOrder { get; }

        public abstract int DrawOrder { get; }

        public abstract bool Visible { get; set; }


        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public abstract void Update(GameTime gameTime);
    }
}
