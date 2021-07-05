using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoGame
{
    public class Button : Component, IDisposable
    {
        #region Fields

        private MouseState _currentMouse;
        private MouseState _previousMouse;
        private SpriteFont _font;
        private bool _isHovering;
        private Texture2D _texture;
        private bool _visible = true;
        private bool _enabled = true;

        #endregion

        #region Properties

        public event EventHandler Click;
        public bool Clicked { get; set; }
        public Color PenColor { get; set; }
        public Vector2 Position { get; set; }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width * 2, _texture.Height * 2);
            }
        }
        public string Text { get; set; }

        public override int DrawOrder => 3;

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

        public override int UpdateOrder => 1;

        #endregion

        #region Methods

        public Button(Texture2D texture, SpriteFont font)
        {
            _texture = texture;

            _font = font;

            PenColor = Color.Black;
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var color = Color.White;

            if (_isHovering)
            {
                color = Color.Gray;
            }

            spriteBatch.Draw(_texture, Rectangle, color);

            if (!string.IsNullOrEmpty(Text))
            {
                var x = (Rectangle.X + (Rectangle.Width / 2)) - (_font.MeasureString(Text).X / 2);
                var y = (Rectangle.Y + (Rectangle.Height / 2)) - (_font.MeasureString(Text).Y / 2);

                spriteBatch.DrawString(_font, Text, new Vector2(x, y), PenColor);
            }
        }

        public override void Update(GameTime gameTime)
        {
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();

            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);

            _isHovering = false;

            if (mouseRectangle.Intersects(Rectangle))
            {
                _isHovering = true;

                if (_currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed)
                {
                    Click?.Invoke(this, new EventArgs());
                }
            }

        }

        public void Dispose()
        {
            Enabled = false;
            Visible = false;
        }
        #endregion
    }
}
