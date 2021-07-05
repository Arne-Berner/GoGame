using Microsoft.Xna.Framework;

namespace GoGame
{
    public class Square
    {
        #region Fields
        Vector2 _position;
        Vector2 _size;
        ColorTaken _color;
        int _liberties;
        int _tokenID;
        int _rectID;
        Chain _currentChain = null;
        bool _free;
        int[] _coordinates;

        #endregion

        #region Constructor
        public Square(Vector2 position, Vector2 size, int[] coordinates, int rectID, ColorTaken color = ColorTaken.Liberty)
        {
            _rectID = rectID;
            _position = position;
            _coordinates = coordinates;
            _size = size;
            _color = color;
            _liberties = 0;
            _free = true;

        }
        public Square(float positionX, float positionY, float sizeY, float sizeX, int[] coordinates, int rectID, ColorTaken color = ColorTaken.Liberty)
        {
            _rectID = rectID;
            _position.X = positionX;
            _position.Y = positionY;
            _coordinates = coordinates;
            _size.X = sizeX;
            _size.Y = sizeY;
            _color = color;
            _liberties = 0;
            _free = true;
        }

        #endregion

        #region Properties

        public Vector2 MidPoint
        {
            get { return new Vector2(X + Width / 2, Y + Height / 2); }
        }
        public int ID
        {
            get { return _rectID; }
        }
        public int[] Coordinates
        {
            get { return _coordinates; }
        }
        public bool Free
        {
            get { return _free; }
            set { _free = value; }
        }
        public int TokenID
        {
            get { return _tokenID; }
            set { _tokenID = value; }
        }
        public Chain Chain
        {
            get { return _currentChain; }
            set
            {
                _currentChain = value;
            }
        }
        public int Liberties
        {
            get { return _liberties; }
            set { _liberties = value; }
        }
        public ColorTaken Color
        {
            get { return _color; }
            set { _color = value; }
        }
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public Vector2 Size
        {
            get { return _size; }
            set { _size = value; }
        }
        public float X
        {
            get { return _position.X; }
            set { _position.X = value; }
        }
        public float Y
        {
            get { return _position.Y; }
            set { _position.Y = value; }
        }
        public float Width
        {
            get { return _size.X; }
            set { _size.X = value; }
        }
        public float Height
        {
            get { return _size.X; }
            set { _size.X = value; }
        }
        #endregion
        public void Reset()
        {
            _free = true;
            _color = ColorTaken.Liberty;
            _liberties = 0;
            _tokenID = 0;
            _currentChain = null;
        }
    }
}
