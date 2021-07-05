using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GoGame
{
    public enum GamePhase
    {
        PlacingTokens,
        RemovingTokens,
        CalculatingScore,
        FinalScreen
    }
    public class GameState : State
    {
        #region Fields
        private Map _goBoard;
        private Score score;
        private Dictionary<int, Token> _tokens;
        private List<Component> _gameComponents;
        private Stack<float> _recentScoreChangesBlack;
        private Stack<float> _recentScoreChangesWhite;
        private Queue<int[]> _turnCoordinates;
        private Stack<ColorTaken[,]> _undoStack;
        private ColorTaken[,] _lastTurn;

        private ColorTaken _colorTurn = ColorTaken.Black;
        private bool _whitesTurn = false;
        private bool _mousePressed = false;
        private bool _canClickAgain = true;
        private bool _canPressEnter = true;
        private bool _canPressLeft = true;
        private bool _isSuicide = false;
        private bool _isKo = false;
        private GamePhase _gamePhase = GamePhase.PlacingTokens;
        private int _tokenID = 0;
        private string _explanationText;

        private State _currentState;
        private State _nextState;
        private Button _removeTokensButton;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private SpriteFont _scoreFont;
        private Texture2D _blackTokenTexture;
        private Texture2D _whiteTokenTexture;
        private Texture2D _buttonTexture;
        #endregion

        public GameState(Game1 game, GraphicsDeviceManager graphics, ContentManager content, Map goBoard) : base(game, graphics, content)
        {
            _goBoard = goBoard;
            _graphicsDevice = _graphics.GraphicsDevice;
            LoadContent();
        }

        #region MainMethods

        public override void Update(GameTime gameTime)
        {


            if (_gamePhase == GamePhase.CalculatingScore)
            {
                ShowFinalScore();
            }

            if (_gamePhase == GamePhase.FinalScreen)
            {
                CreateRestartButton();
            }

            OnEnterChangePhase();

            OnLeftUndo();

            UpdateComponents(gameTime);

            CheckForMouseClicked();

            //TODO Sound implementieren
            //TODO Vorteil implementieren
            //TODO Stellungen implementieren vielleicht mit sgf datein arbeiten?
            if (_mousePressed)
            {
                _mousePressed = false;
                Vector2 mousePosition = GetMousePosition();

                CheckForTurn();

                int[] chosenCoordinates = GetGridCoordinates(mousePosition);

                if (chosenCoordinates != null)
                {

                    float recentBlackScore = score.BlackScore;
                    float recentWhiteScore = score.WhiteScore;
                    Square currentRect = _goBoard.GetRectangle(chosenCoordinates);

                    if (currentRect.Free && _gamePhase == GamePhase.PlacingTokens)
                    {
                        List<Chain> surroundingChains = _goBoard.GetSurroundingChains(chosenCoordinates);
                        List<Chain> currentColorChains = GetCurrentColorChains(_colorTurn, surroundingChains);
                        List<Chain> otherColorChains = GetOtherColorChains(_colorTurn, surroundingChains);
                        currentRect.Liberties = _goBoard.CheckForLiberties(chosenCoordinates);
                        _isSuicide = CheckForSuicide(currentRect, currentColorChains, otherColorChains);
                        _isKo = CheckForKo(chosenCoordinates);
                        if (!_isSuicide && !_isKo)
                        {
                            _undoStack.Push(_goBoard.CreateSquareColorMap());
                            SetUpRectangle(currentRect);
                            UpdateAllChains();

                            FormCurrentChain(currentRect, currentColorChains);

                            _goBoard.RemoveKilledChains(currentRect.Chain);

                            UpdateChain(currentRect.Chain);

                            if (_whitesTurn)
                            {
                                PlaceToken(currentRect, _whiteTokenTexture);
                            }
                            else
                            {
                                PlaceToken(currentRect, _blackTokenTexture);
                            }

                            if (_turnCoordinates.Count > 1)
                            {
                                _turnCoordinates.Dequeue();
                            }
                            _turnCoordinates.Enqueue(chosenCoordinates);


                            SwitchTurns();

                            surroundingChains.Clear();
                            currentColorChains.Clear();
                            _recentScoreChangesBlack.Push(score.BlackScore - recentBlackScore);
                            _recentScoreChangesWhite.Push(score.WhiteScore - recentWhiteScore);
                        }
                    }
                    if (!currentRect.Free && _gamePhase == GamePhase.RemovingTokens)
                    {
                        _tokens.Remove(currentRect.TokenID);
                        currentRect.Reset();
                    }
                }
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch = _spriteBatch;
            spriteBatch.Begin();
            DrawComponents(gameTime);
            spriteBatch.End();
        }

        #endregion

        #region HelperMethods

        private void OnLeftUndo()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && _canPressLeft)
            {
                if (_undoStack.Count != 0)
                {
                    Undo();
                    RecalculateScore();

                    _canPressLeft = false;
                }
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Left))
            {
                _canPressLeft = true;
            }
        }

        private void OnEnterChangePhase()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && _canPressEnter)
            {
                if (_gamePhase == GamePhase.RemovingTokens)
                {
                    _explanationText = "Punktestand wird berechnet";
                    _gamePhase = GamePhase.CalculatingScore;
                }
                else
                {
                    _explanationText = "Klicke auf verlorene Tokens, um sie zu entfernen. Druecke Enter fuer den Punktestand.";
                    _gamePhase = GamePhase.RemovingTokens;
                }

                _canPressEnter = false;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Enter))
            {
                _canPressEnter = true;
            }
        }

        private void CreateRestartButton()
        {
            var restartButton = new Button(_buttonTexture, _font)
            {
                Position = new Vector2(_goBoard.Width / 2 - (_buttonTexture.Width), ((_goBoard.Height / 2)) - _buttonTexture.Height),
                Text = "Restart?"
            };

            restartButton.Click += RestartButton_Click;
            _explanationText = "Druecke Enter, um Neuzustarten.";
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new MenuState(_game, _graphics, _content));
        }

        private void RecalculateScore()
        {
            score.AddPointsToBlack((int)_recentScoreChangesBlack.Pop() * -1);
            score.AddPointsToWhite((int)_recentScoreChangesWhite.Pop() * -1);
        }
        private void Undo()
        {
            _lastTurn = _undoStack.Pop();
            for (int i = 0; i < _goBoard.GridSize; i++)
            {
                for (int j = 0; j < _goBoard.GridSize; j++)
                {
                    _goBoard.Grid[j, i].Color = _lastTurn[j, i];
                    if (_lastTurn[j, i] != ColorTaken.Liberty)
                    {
                        _goBoard.Grid[j, i].Free = false;
                    }
                    else
                    {
                        _goBoard.Grid[j, i].Free = true;
                    }
                }
            }

            _goBoard.Chains.Clear();
            _tokens.Clear();
            foreach (Square rect in _goBoard.Grid)
            {
                if (rect.Color != ColorTaken.Liberty)
                {
                    List<Chain> surroundingChains = _goBoard.GetSurroundingChains(rect.Coordinates);
                    List<Chain> currentColorChains = GetCurrentColorChains(rect.Color, surroundingChains);
                    FormCurrentChain(rect, currentColorChains);
                    if (rect.Color == ColorTaken.Black)
                    {
                        PlaceToken(rect, _blackTokenTexture);
                    }
                    else
                    {
                        PlaceToken(rect, _whiteTokenTexture);
                    }
                }
            }
            SwitchTurns();
        }
        private void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphicsDevice);


            LoadTextures();

            _goBoard.SquareKilled += OnSquareKilled;


            score = new Score(_scoreFont, _colorTurn, _goBoard);
            _gameComponents = new List<Component>()
            {
                _goBoard
            };
            _tokens = new Dictionary<int, Token>();
            _turnCoordinates = new Queue<int[]>();
            _recentScoreChangesBlack = new Stack<float>();
            _recentScoreChangesWhite = new Stack<float>();
            _undoStack = new Stack<ColorTaken[,]>();


            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)_goBoard.Width;
            _graphics.PreferredBackBufferHeight = (int)_goBoard.Height + 50;
            _graphics.ApplyChanges();
        }
        private void LoadTextures()
        {
            _blackTokenTexture = _content.Load<Texture2D>("GoBlackToken");
            _whiteTokenTexture = _content.Load<Texture2D>("GoWhiteToken");
            _font = _content.Load<SpriteFont>("Score");
            _scoreFont = _content.Load<SpriteFont>("RealScoreFont");
            _buttonTexture = _content.Load<Texture2D>("Button");
        }

        public void ChangeState(State state)
        {
            _nextState = state;
        }

        private void UpdateComponents(GameTime gameTime)
        {
            foreach (var component in _gameComponents)
            {
                if (component.Enabled)
                {
                    component.Update(gameTime);
                }
            }
            score.Update(gameTime);
        }
        private bool CheckSurroundingColorsMixed(Chain currentChain, out ColorTaken color)
        {
            List<Square> rectanglesInCurrentChain = currentChain.Rectangles;
            bool mixedColor = false;
            color = ColorTaken.Liberty;
            foreach (Square rect in rectanglesInCurrentChain)
            {
                List<Square> surroundingRects = _goBoard.GetSurroundingRectangles(rect.Coordinates);
                foreach (Square sqr in surroundingRects)
                {
                    if (sqr.Color == ColorTaken.Black)
                    {
                        if (color == ColorTaken.White)
                        {
                            mixedColor = true;
                        }

                        color = ColorTaken.Black;
                    }
                    if (sqr.Color == ColorTaken.White)
                    {
                        if (color == ColorTaken.Black)
                        {
                            mixedColor = true;
                        }

                        color = ColorTaken.White;
                    }

                }

            }
            return mixedColor;
        }

        private void ShowFinalScore()
        {
            foreach (Square currentRect in _goBoard.Grid)
            {
                if (currentRect.Color == ColorTaken.Liberty)
                {
                    int[] chosenCoordinates = currentRect.Coordinates;
                    List<Chain> surroundingChains = _goBoard.GetSurroundingChains(chosenCoordinates);
                    List<Chain> currentColorChains = GetCurrentColorChains(ColorTaken.Liberty, surroundingChains);
                    FormCurrentChain(currentRect, currentColorChains);
                }
            }
            foreach (var KVPair in _goBoard.Chains)
            {
                Chain currentChain = KVPair.Value;
                if (currentChain.Color == ColorTaken.Liberty)
                {
                    ColorTaken color;
                    bool mixedColor = CheckSurroundingColorsMixed(currentChain, out color);
                    if (color == ColorTaken.Black && !mixedColor)
                    {
                        score.AddPointsToBlack(currentChain.Rectangles.Count);
                    }
                    else if (color == ColorTaken.White && !mixedColor)
                    {
                        score.AddPointsToWhite(currentChain.Rectangles.Count);
                    }
                }

            }
            _gamePhase = GamePhase.FinalScreen;
        }
        private void SetUpRectangle(Square currentRect)
        {
            currentRect.Liberties = _goBoard.CheckForLiberties(currentRect.Coordinates);
            currentRect.Color = _colorTurn;
            currentRect.Free = false;
        }

        private List<Chain> GetCurrentColorChains(ColorTaken color, List<Chain> surroundingChains)
        {
            List<Chain> currentColorChains = new List<Chain>();

            foreach (Chain chain in surroundingChains)
            {
                if (color == chain.Color)
                {
                    currentColorChains.Add(chain);
                }
            }

            return currentColorChains;
        }

        private List<Chain> GetOtherColorChains(ColorTaken color, List<Chain> surroundingChains)
        {
            List<Chain> otherColorChains = new List<Chain>();

            foreach (Chain chain in surroundingChains)
            {
                if (color != chain.Color && chain.Color != ColorTaken.Liberty)
                {
                    otherColorChains.Add(chain);
                }
            }

            return otherColorChains;
        }
        private void FormCurrentChain(Square currentRect, List<Chain> currentColorChains)
        {

            if (currentColorChains.Count == 0)
            {
                _goBoard.CreateNewChain(currentRect);
            }
            else
            {
                Chain currentChain = JoinChains(currentColorChains);
                currentChain.AddRectangle(currentRect);
            }

        }

        private Chain JoinChains(List<Chain> currentColorChains)
        {
            foreach (Chain chain in currentColorChains)
            {
                if (chain != currentColorChains[0])
                {
                    currentColorChains[0].JoinChain(chain);
                    _goBoard.Chains.Remove(chain.ID);
                }
            }
            return currentColorChains[0];
        }

        private void UpdateAllChains()
        {
            foreach (Chain chain in _goBoard.Chains.Values)
            {
                UpdateChain(chain);
            }
        }

        private void UpdateChain(Chain chain)
        {
            Dictionary<int, Square> liberties = new Dictionary<int, Square>();
            foreach (Square rect in chain.Rectangles)
            {
                int[] coordinates = rect.Coordinates;
                List<Square> surroundingRectangles = _goBoard.GetSurroundingRectangles(coordinates);
                foreach (Square surroundingRect in surroundingRectangles)
                {
                    if (surroundingRect.Color == ColorTaken.Liberty && !liberties.ContainsKey(surroundingRect.ID))
                    {
                        liberties.Add(surroundingRect.ID, surroundingRect);
                    }
                }
            }
            chain.Liberties = liberties.Count;
            liberties.Clear();
        }


        private void OnSquareKilled(object sender, KilledEventArgs e)
        {
            _tokens.Remove(e.TokenID);
        }



        private bool CheckForKo(int[] currentCoordinate)
        {
            bool isKo = false;
            if (_turnCoordinates.Count > 1)
            {
                int[] koCoordinate = _turnCoordinates.Peek();
                if (koCoordinate[0] == currentCoordinate[0] && koCoordinate[1] == currentCoordinate[1])
                {
                    isKo = true;
                }
            }
            return isKo;
        }
        private bool CheckForSuicide(Square currentRect, List<Chain> currentColorChains, List<Chain> otherColorChains)
        {
            foreach (Chain chain in otherColorChains)
            {
                if (chain.Liberties <= 1)
                {
                    return false;
                }
            }
            if (currentRect.Liberties == 0 && currentColorChains.Count == 0)
            {
                return true;
            }
            else if (currentRect.Liberties == 0)
            {
                foreach (Chain chain in currentColorChains)
                {
                    if (chain.Liberties > 1)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private int[] GetGridCoordinates(Vector2 mousePosition)
        {
            int rectX;
            int rectY;
            if (_goBoard.RectSize.X != 0 && mousePosition.X < _goBoard.Width && mousePosition.X > 0 &&
                _goBoard.RectSize.Y != 0 && mousePosition.Y < _goBoard.Height && mousePosition.Y > 0)
            {
                rectX = (int)(mousePosition.X / _goBoard.RectSize.X);
                rectY = (int)(mousePosition.Y / _goBoard.RectSize.Y);
                int[] rectXY = new int[] { rectX, rectY };
                return rectXY;
            }
            else
            {
                return null;
            }
        }

        private Vector2 GetMousePosition()
        {
            Vector2 mousePosition = new Vector2(0, 0);
            mousePosition.X = Mouse.GetState().X;
            mousePosition.Y = Mouse.GetState().Y;
            return mousePosition;
        }

        private void PlaceToken(Square currentRect, Texture2D tokenTexture)
        {
            currentRect.TokenID = _tokenID;

            Token token = new Token(currentRect.MidPoint, tokenTexture, _tokenID);
            _tokens.Add(_tokenID, token);

            _tokenID++;
        }

        private void SwitchTurns()
        {
            if (_whitesTurn)
            {
                _whitesTurn = false;
            }
            else
            {
                _whitesTurn = true;
            }
        }

        private void CheckForTurn()
        {
            if (_whitesTurn)
            {
                _colorTurn = ColorTaken.White;
                score.ColorTurn = _colorTurn;
            }
            else
            {
                _colorTurn = ColorTaken.Black;
                score.ColorTurn = _colorTurn;
            }
        }
        //TODO später Eventbezogen machen
        private void CheckForMouseClicked()
        {
            if (_canClickAgain == true)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    _mousePressed = true;
                    _canClickAgain = false;
                }
            }
            if (Mouse.GetState().LeftButton == ButtonState.Released)
            {
                _canClickAgain = true;
            }
        }


        private void DrawComponents(GameTime gameTime)
        {
            foreach (var components in _gameComponents)
            {
                if (components.Visible == true)
                {
                    components.Draw(gameTime, _spriteBatch);
                }
            }
            foreach (KeyValuePair<int, Token> keyValuePair in _tokens)
            {
                if (keyValuePair.Value.Visible == true)
                {
                    keyValuePair.Value.Draw(gameTime, _spriteBatch);
                }
            }
            if (_explanationText == null)
            {
                _explanationText = "Druecke Enter um die Partie zu beenden.";
            }
            _spriteBatch.DrawString(_font, _explanationText, Vector2.Zero, Color.Black);
            if (_gamePhase == GamePhase.FinalScreen)
            {
                score.Draw(gameTime, _spriteBatch);
            }

            _removeTokensButton?.Draw(gameTime, _spriteBatch);
        }

        #endregion
    }
}
