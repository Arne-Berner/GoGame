using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GoGame
{
    public class Token : Component, IDisposable
    {
        Vector2 _pos;
        Texture2D _tokenTexture;
        int _tokenID;
        private bool _visible = true;
        private bool _enabled = true;

        public Token(Vector2 pos, Texture2D tokenTexture, int tokenID)
        {
            _pos = pos;
            _tokenTexture = tokenTexture;
            _tokenID = tokenID;
        }


        public int TokenID
        {
            get { return _tokenID; }
        }
        public Vector2 Position
        {
            get { return _pos; }
            set { _pos = value; }
        }
        public Texture2D Texture
        {
            get { return _tokenTexture; }
            set { _tokenTexture = value; }
        }

        public override int UpdateOrder => 1;

        public override int DrawOrder => 2;

        public override bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }


        public override bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public void Dispose()
        {
            _pos = Vector2.Zero;
            _tokenTexture = null;
            _tokenID = -1;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture,
                        Position,
                        null,
                        Color.White,
                        rotation: 0f,
                        origin: new Vector2(Texture.Width / 2, Texture.Height / 2),
                        Vector2.One * 1,
                        SpriteEffects.None,
                        0f);
        }


        public override void Update(GameTime gameTime)
        {
        }
    }
}
