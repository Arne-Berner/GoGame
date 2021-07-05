using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GoGame
{
    public enum ColorTaken
    {
        Black,
        White,
        Liberty
    }

    public class Map : Component

    {
        #region Fields
        private Texture2D _mapTexture;
        private float _gridSize;
        private Vector2 _rectSize;
        private Vector2 _mapSize;
        private Square[,] _grid;
        private Dictionary<int, Chain> _chains;
        private int _chainId;
        public event EventHandler<KilledEventArgs> SquareKilled;
        private bool _visible = true;
        private bool _enabled = true;
        #endregion

        #region Constructor
        public Map(Texture2D mapTexture, float gridSize)
        {
            _chains = new Dictionary<int, Chain>();
            _mapTexture = mapTexture;
            _gridSize = gridSize;
            _mapSize = CalculateMapSize();
            _rectSize = CalculateRectSize(_gridSize, _mapSize.X, _mapSize.Y);
            _grid = CreateGrid(gridSize, _rectSize);
        }
        #endregion

        #region Properties

        public Dictionary<int, Chain> Chains
        {
            get { return _chains; }
        }
        public Texture2D Texture
        {
            get { return _mapTexture; }
        }
        public Square[,] Grid
        {
            get { return _grid; }
        }
        public float GridSize
        {
            get { return _gridSize; }
        }
        public Vector2 Size
        {
            get { return _mapSize; }
        }
        public Vector2 RectSize
        {
            get { return _rectSize; }
        }
        public float Width
        {
            get { return _mapSize.X; }
        }
        public float Height
        {
            get { return _mapSize.Y; }
        }

            public override int UpdateOrder => 1;

        public override int DrawOrder => 0;

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
        #endregion

        #region Methods

        public Square GetRectangle(int[] coordinates)
        {
            return _grid[coordinates[0], coordinates[1]];
        }
        public List<Square> GetSurroundingRectangles(int[] coordinates)
        {
            List<Square> surroundingRects = new List<Square>();
            if (coordinates[0] > 0)
            {
                surroundingRects.Add(Grid[coordinates[0] - 1, coordinates[1]]);
            }
            if (coordinates[0] + 1 < GridSize)
            {
                surroundingRects.Add(Grid[coordinates[0] + 1, coordinates[1]]);
            }
            if (coordinates[1] > 0)
            {
                surroundingRects.Add(Grid[coordinates[0], coordinates[1] - 1]);
            }
            if (coordinates[1] + 1 < GridSize)
            {
                surroundingRects.Add(Grid[coordinates[0], coordinates[1] + 1]);
            }
            return surroundingRects;
        }

        public List<Chain> GetSurroundingChains(int[] chosenCoordinates)
        {
            List<Square> surroundingRects = GetSurroundingRectangles(chosenCoordinates);
            List<Chain> surroundingChains = new List<Chain>();
            foreach (Square rect in surroundingRects)
            {
                if (rect.Chain != null)
                {
                    surroundingChains.Add(rect.Chain);
                }
            }
            return surroundingChains;
        }

        public int CheckForLiberties(int[] coordinates)
        {
            int liberties = 0;
            List<Square> surroundingSquares = GetSurroundingRectangles(coordinates);
            foreach (Square sqr in surroundingSquares)
            {
                if (sqr.Color == ColorTaken.Liberty)
                {
                    liberties++;
                }
            }
            return liberties;
        }
        public void CreateNewChain(Square rect)
        {
            Chain chain = new Chain(rect, rect.Liberties, _chainId, rect.Color);
            _chains.Add(_chainId, chain);
            rect.Chain = chain;
            _chainId++;
        }

        public void RemoveChain(int chainId)
        {
            if (_chains.TryGetValue(chainId, out Chain chain))
            {
                chain.Dispose();
            }
            _chains.Remove(chainId);
        }

        public void RemoveKilledChains(Chain currentChain)
        {
            List<int> toBeDisposed = GetChainKeysToBeDisposed(currentChain);
            SquareKilledInList(toBeDisposed);
            DisposeOfChain(toBeDisposed);
        }

        private List<int> GetChainKeysToBeDisposed(Chain currentChain)
        {
            List<int> toBeDisposed = new List<int>();
            foreach (KeyValuePair<int, Chain> entry in Chains)
            {
                if (entry.Value.Liberties == 0 && entry.Value != currentChain)
                {
                    toBeDisposed.Add(entry.Key);
                }
            }
            return toBeDisposed;
        }

        private void SquareKilledInList(List<int> toBeDisposed)
        {
            foreach (int i in toBeDisposed)
            {
                foreach (Square rect in Chains[i].Rectangles)
                {
                    SquareKilled?.Invoke(this, new KilledEventArgs(rect.TokenID));
                }
            }
        }

        private void DisposeOfChain(List<int> toBeDisposed)
        {
            foreach (int i in toBeDisposed)
            {
                Chains[i].Dispose();
                Chains.Remove(i);
            }
        }
        public void AddToChain(Square rectangle, Chain chain)
        {
            chain.AddRectangle(rectangle);
            rectangle.Chain = chain;
            chain.Liberties += rectangle.Liberties;
        }

        private Vector2 CalculateMapSize()
        {
            Vector2 mapSize = new Vector2(_mapTexture.Width, _mapTexture.Height);
            return mapSize;
        }

        private Vector2 CalculateRectSize(float gridSize, float mapWidth, float mapHeight)
        {

            Vector2 rectSize = new Vector2((float)mapWidth / (float)gridSize, (float)mapHeight / (float)gridSize);
            return rectSize;
        }

        public Square[,] CreateGrid(float gridSize, Vector2 rectSize)
        {
            Vector2 rectPos = new Vector2(0, 0);
            float nextRectStepX = rectSize.X;
            float nextRectStepY = rectSize.Y;
            Square[,] grid = new Square[(int)gridSize, (int)gridSize];
            int rectID = 0;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int[] coordinates = { j, i };
                    Square rect = new Square(rectPos, rectSize, coordinates, rectID);
                    grid[j, i] = rect;
                    rectPos = new Vector2(rectPos.X + nextRectStepX, rectPos.Y);
                    rectID++;
                }
                rectPos = new Vector2(0, rectPos.Y + nextRectStepY);
            }

            return grid;
        }
                        
        public int CountTerritory()
        {
            int territoryCount = 0;
            foreach(Square rect in Grid)
            {
                int[] currentCoordinates = rect.Coordinates;
                
                
            }

            return territoryCount;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                Texture,
                position: Vector2.Zero,
                color: Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            //
        }

        public ColorTaken[,] CreateSquareColorMap()
        {
            ColorTaken[,] squareColors = new ColorTaken[(int)GridSize, (int)GridSize]; ;
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    squareColors[j, i] = Grid[j, i].Color;
                }
            }
            return squareColors;
        }

        #endregion
    }
}
