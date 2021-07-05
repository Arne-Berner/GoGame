using System;
using System.Collections.Generic;

namespace GoGame
{
    public class Chain : IDisposable
    {
        #region Fields
        List<Square> _rectangles;
        ColorTaken _color;
        int _liberties;
        int _chainId;
        #endregion

        #region Constructor
        public Chain(Square rectangle, int liberties, int chainId, ColorTaken color)
        {
            _rectangles = new List<Square>();
            _rectangles.Add(rectangle);
            _liberties = liberties;
            _chainId = chainId;
            _color = color;
        }
        #endregion

        #region Properties
        public ColorTaken Color
        {
            get { return _color; }
        }
        public int ID
        {
            get { return _chainId; }
        }
        public int Liberties
        {
            get { return _liberties; }
            set { _liberties = value; }
        }
        public List<Square> Rectangles
        {
            get { return _rectangles; }
        }
        #endregion

        #region Methods
        public void AddRectangle(Square rectangle)
        {
            _rectangles.Add(rectangle);
            this.Liberties += rectangle.Liberties;
            rectangle.Chain = this;
        }
        public void JoinChain(Chain otherChain)
        {
            List<Square> otherChainRectangles = otherChain.Rectangles;
            _rectangles.AddRange(otherChainRectangles);
            foreach (Square rect in otherChainRectangles)
            {
                rect.Chain = this;
            }
            this.Liberties += otherChain.Liberties;
        }

        public void Dispose()
        {
            foreach (Square rect in _rectangles)
            {
                rect.Color = ColorTaken.Liberty;
                rect.Chain = null;
                rect.Free = true;
            }
            _rectangles.Clear();
            _liberties = 0;
        }

        public void Remove(Square rect)
        {
            _rectangles.Remove(rect);
        }
        #endregion
    }
}
