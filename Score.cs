using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoGame
{
    // write this as singleton pattern?
    // TODO Event for UpdateScore();
    public class Score : Component
    {
        Map _goBoard;
        private string _winner;
        private float _finalScore;
        private float _whiteScore = 5.5f;
        private float _blackScore = 0f;
        ColorTaken _colorTurn;
        private bool _visible = true;
        private bool _enabled = true;

        SpriteFont _scoreFont;

        public float WhiteScore { get { return _whiteScore; } }
        public float BlackScore { get { return _blackScore; } }
        public override int UpdateOrder => 1;

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

        public Score(SpriteFont scoreFont, ColorTaken colorTurn, Map goBoard)
        {
            _goBoard = goBoard;
            _colorTurn = colorTurn;
            _scoreFont = scoreFont;

            _goBoard.SquareKilled += OnSquareKilled;
        }

        public ColorTaken ColorTurn
        {
            set { _colorTurn = value; }
        }

        public void AddPointsToBlack(int points)
        {
            _blackScore += points;
        }

        public void AddPointsToWhite(int points)
        {
            _whiteScore += points;
        }

        private void OnSquareKilled(object sender, KilledEventArgs e)
        {
            UpdateScore();
        }

        private void UpdateScore()
        {
            if (_colorTurn == ColorTaken.White)
            {
                _whiteScore += 1;
            }
            else
            {
                _blackScore += 1;
            }
        }

        private void GetFinalScore()
        {
            float pointDifference = _whiteScore - _blackScore;
            if (pointDifference < 0)
            {
                _winner = "Schwarz gewinnt mit \n";
            }
            else
            {
                _winner = "Weiss gewinnt mit \n";
            }
            _finalScore = Math.Abs(pointDifference);
        }


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            string winScore = _winner + _finalScore.ToString() + "\nPunkten!";
            string whiteScore = "Weiss hat " + _whiteScore.ToString() + " Punkte!";
            string blackScore = "Schwarz hat " + _blackScore.ToString() + " Punkte!";
            spriteBatch.DrawString(_scoreFont, winScore, 
                new Vector2((_goBoard.Size.X / 2) - (_scoreFont.MeasureString(winScore).X / 2), _goBoard.Size.Y / 2 - 100 - (_scoreFont.MeasureString(winScore).Y / 2)) , Color.SlateGray);
            spriteBatch.DrawString(_scoreFont, whiteScore, 
                new Vector2((_goBoard.Size.X / 2) - (_scoreFont.MeasureString(whiteScore).X / 2), _goBoard.Size.Y / 2 + 0 - (_scoreFont.MeasureString(whiteScore).Y / 2)), Color.SlateGray);
            spriteBatch.DrawString(_scoreFont, blackScore, 
                new Vector2((_goBoard.Size.X / 2) - (_scoreFont.MeasureString(blackScore).X / 2), _goBoard.Size.Y / 2 + 100 - (_scoreFont.MeasureString(blackScore).Y / 2)), Color.SlateGray);
        }

        public override void Update(GameTime gameTime)
        {
            GetFinalScore();
        }
    }
}
