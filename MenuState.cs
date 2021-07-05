using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GoGame
{
    public class MenuState : State
    {
        private Map _goBoard;
        private List<Component> _components;
        private Texture2D _mapTexture;
        private Texture2D _bigMapTexture;
        private Texture2D _buttonTexture;
        private GraphicsDevice _graphicsDevice;

        private Button _newGameButton;
        private Button _nineMapButton;
        private Button _nineteenMapButton;
        private Button _quitGameButton;
        public MenuState(Game1 game, GraphicsDeviceManager graphics, ContentManager content) : base(game, graphics, content)
        {
            _graphicsDevice = graphics.GraphicsDevice;
            _mapTexture = _content.Load<Texture2D>("GoBoard");
            _buttonTexture = _content.Load<Texture2D>("Button");
            var buttonFont = _content.Load<SpriteFont>("Font");
            _mapTexture = _content.Load<Texture2D>("GoBoard");
            _bigMapTexture = _content.Load<Texture2D>("BigGoBoard");
            _goBoard = new Map(_mapTexture, 9);

            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)_goBoard.Width;
            _graphics.PreferredBackBufferHeight = (int)_goBoard.Height + 50;
            _graphics.ApplyChanges();

            _newGameButton = new Button(_buttonTexture, buttonFont)
            {
                Position = new Vector2((_goBoard.Width / 2) - (_buttonTexture.Width), ((_goBoard.Height / 2) - 150) - _buttonTexture.Height),
                Text = "New Game"
            };

            _newGameButton.Click += NewGameButton_Click;

            _nineMapButton = new Button(_buttonTexture, buttonFont)
            {
                Position = new Vector2(_goBoard.Width / 2 - (_buttonTexture.Width), ((_goBoard.Height / 2) - 50) - _buttonTexture.Height),
                Text = "9x9 Map"
            };

            _nineMapButton.Click += NineMapButton_Click;

            _nineteenMapButton = new Button(_buttonTexture, buttonFont)
            {
                Position = new Vector2(_goBoard.Width / 2 - (_buttonTexture.Width), ((_goBoard.Height / 2) + 50) - _buttonTexture.Height),
                Text = "19x19 Map"
            };

            _nineteenMapButton.Click += NineteenMapButton_Click;

            _quitGameButton = new Button(_buttonTexture, buttonFont)
            {
                Position = new Vector2(_goBoard.Width / 2 - (_buttonTexture.Width), ((_goBoard.Height / 2) + 150) - _buttonTexture.Height),
                Text = "Quit Game"
            };

            _quitGameButton.Click += QuitGameButton_Click;

            _components = new List<Component>()
            {
               _newGameButton,
               _nineMapButton,
               _nineteenMapButton,
               _quitGameButton,
            };


        }

        private void NineteenMapButton_Click(object sender, EventArgs e)
        {
            _goBoard = new Map(_bigMapTexture, 19);
            _graphics.PreferredBackBufferWidth = (int)_goBoard.Width;
            _graphics.PreferredBackBufferHeight = (int)_goBoard.Height + 50;
            _graphics.ApplyChanges();
            UpdateButtonPosition();
        }

        private void NineMapButton_Click(object sender, EventArgs e)
        {
            _goBoard = new Map(_mapTexture, 9);
            _graphics.PreferredBackBufferWidth = (int)_goBoard.Width;
            _graphics.PreferredBackBufferHeight = (int)_goBoard.Height + 50;
            _graphics.ApplyChanges();
            UpdateButtonPosition();
        }

        private void UpdateButtonPosition()
        {
            _newGameButton.Position =
                new Vector2((_goBoard.Width / 2) - (_buttonTexture.Width), ((_goBoard.Height / 2) - 150) - _buttonTexture.Height);
            _nineMapButton.Position =
                new Vector2(_goBoard.Width / 2 - (_buttonTexture.Width), ((_goBoard.Height / 2) - 50) - _buttonTexture.Height);
            _nineteenMapButton.Position =
                new Vector2(_goBoard.Width / 2 - (_buttonTexture.Width), ((_goBoard.Height / 2) + 50) - _buttonTexture.Height);
            _quitGameButton.Position =
                new Vector2(_goBoard.Width / 2 - (_buttonTexture.Width), ((_goBoard.Height / 2) + 150) - _buttonTexture.Height);
        }

        private void QuitGameButton_Click(object sender, EventArgs e)
        {
            _game.Exit();
        }

        // wechselt von dem einen State in den Nächsten (z.B. GameState)
        private void NewGameButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new GameState(_game, _graphics, _content, _goBoard));
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            _goBoard.Draw(gameTime, spriteBatch);
            foreach (var component in _components)
            {
                component.Draw(gameTime, spriteBatch);
            }

            spriteBatch.End();
        }

        public override void PostUpdate(GameTime gameTime)
        {
            //remove Sprites if they're not needed
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Update(gameTime);
            }
        }
    }
}
