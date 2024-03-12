using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Prototype.GameEntity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Prototype.MapGeneration
{
    public class Tileset : IGameObject
    {
        // Fields
        public bool devRendering = false;

        // Properties

        /// <summary>
        /// Set of individual tiles comprising this tileset
        /// </summary>
        public Tile[,] Layout { get; private set; }

        /// <summary>
        /// The number of vertical columns in this tileset
        /// </summary>
        public int Columns { get => Layout.GetLength(1); }

        /// <summary>
        /// The number of horizontal rows in this tileset
        /// </summary>
        public int Rows { get => Layout.GetLength(0); }

        /// <summary>
        /// Width of the tileset
        /// </summary>
        public int Width { get => Columns * Game1.TILESIZE; }

        /// <summary>
        /// Height of the tileset
        /// </summary>
        public int Height { get => Rows * Game1.TILESIZE; }

        /// <summary>
        /// The doors or bridge-points of this tileset
        /// </summary>
        public List<Tile> Doors { get; private set; } = new List<Tile>();

        public List<Tile> Spawners { get; private set; } = new List<Tile>();

        // Constructors

        /*
        public TileSet(string filename, int rows, int columns, Point origin)
        {
            // Open the file
            StreamReader input = new StreamReader(filename);

            // Create tile grid
            Layout = new Tile[rows, columns];

            // Convert file values to tiles
            int y = 0;
            string line = "";
            while ((line = input.ReadLine()!) != null)
            {
                // Split line into tile ids
                string[] tileValues = line.Split(" ");
                
                // Add tiles to tile grid
                for (int x = 0; x < columns; x++)
                {
                    int tileId = -1;

                    // Unparsable tile values are probably doors (denoted by '*')
                    bool isDoor = !int.TryParse(tileValues[x], out tileId) && tileValues[x].Length > 1;

                    Layout[y, x] = TileMaker.SetTile(
                        tileId,
                        new Vector2(x * Game1.TILESIZE, y * Game1.TILESIZE));

                    Layout[y, x].IsDoor = isDoor;
                }

                // Move to next row
                y++;
            }

            input.Close();
        }
        */

        /// <summary>
        /// Creates a square tileset from the integer values in a text file
        /// </summary>
        /// <param name="filename"> the file to read </param>
        public Tileset(string floorFilename, string enemyPosFileName,
            string obstaclePosFileName, Point origin)
        {
            // Open the file that contains the floor tiles.
            StreamReader input = new StreamReader(floorFilename);

            // Convert file values to tiles
            int y = 0;
            string line = "";
            while ((line = input.ReadLine()!) != null)
            {
                // Split line into tile ids
                string[] tileValues = line.Split(" ");

                // Set layout size based off of line width
                if (Layout == null)
                {
                    Layout = new Tile[tileValues.Length, tileValues.Length];
                }
                
                // Add tiles to tile grid
                for (int x = 0; x < Layout.GetLength(0); x++)
                {
                    int tileIdInt;

                    bool isDoor = false;
                    string orientation = "";

                    // Unparsable tile values are probably special tiles
                    if (!int.TryParse(tileValues[x], out tileIdInt))
                    {
                        string[] sValues = tileValues[x].Split('.');

                        // Read tile type 
                        tileIdInt = int.Parse(sValues[0]);

                        if (sValues.Length == 2)
                        {
                            // Read tile specification
                            switch (sValues[1])
                            {
                                case "*":
                                    isDoor = true;
                                    break;

                                default:
                                    orientation = sValues[1];
                                    break;
                                    
                            }
                        }

                    }

                    // Create and add tile
                    Layout[y, x] = TileMaker.SetTile(
                        (TileType) tileIdInt,
                        new Vector2(origin.X + x*Game1.TILESIZE, origin.Y + y*Game1.TILESIZE), 
                        orientation);

                    // Set door status
                    Layout[y, x].IsDoor = isDoor;
                    if (isDoor)
                    {
                        Layout[y, x].Doorientaiton = GetDoorientation(x, y);
                        
                        // Store door
                        Doors.Add(Layout[y, x]);
                    }
                }

                // Move to next row
                y++;
            }

            input.Close();

            //Read in enemy positions and obstacle positions
            string[] allPossibleEnemyPos = File.ReadAllLines(enemyPosFileName);
            string[] allPossibleObstaclePos = File.ReadAllLines(obstaclePosFileName);

            //Select enemy and obstacle positions, and ensure that they do not overlap.
            Random random = new Random();
            List<Point> EnemyPos = null;
            List<Point> ObstaclePos = null;
            bool invalidEnemyObstacleCombo = false;
            do
            {
                if (allPossibleEnemyPos.Length > 0)
                {
                    EnemyPos = ConvertPositions(
                        allPossibleEnemyPos[random.Next(allPossibleEnemyPos.Length)]); 
                }

                if (allPossibleObstaclePos.Length > 0)
                {
                    ObstaclePos = ConvertPositions(
                        allPossibleObstaclePos[random.Next(allPossibleObstaclePos.Length)]); 
                }

                if (ObstaclePos != null && EnemyPos != null)
                {
                    foreach(Point obstacle in ObstaclePos)
                    {
                        if (EnemyPos.IndexOf(obstacle) != -1)
                        {
                            invalidEnemyObstacleCombo = true; 
                        }
                    }
                }

            }
            while (invalidEnemyObstacleCombo);

            if (EnemyPos != null)
            {
                // Set spawners
                foreach(Point enemyPos in EnemyPos)
                {
                    Layout[enemyPos.X, enemyPos.Y].IsEnemySpawner = true;
                    Spawners.Add(Layout[enemyPos.X, enemyPos.Y]);
                }
            }

            if (ObstaclePos != null)
            {
                // Set obstacles
                foreach(Point obstaclePos in ObstaclePos)
                {
                    Layout[obstaclePos.Y, obstaclePos.X] = TileMaker.SetTile(
                        TileType.Spike, new Vector2(
                            origin.X + obstaclePos.X*Game1.TILESIZE,
                            origin.Y + obstaclePos.Y*Game1.TILESIZE), 
                            "");
                }
            }

        }

        // Methods

        public void Update(GameTime gameTime)
        {
            // TODO: Check for events
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {


            // Draw all tiles
            for (int y = 0; y < Layout.GetLength(1); y++)
            {
                for (int x = 0; x < Layout.GetLength(0); x++)
                {
                    Tile curTile = Layout[x, y];

                    // Find screen position
                    Vector2 distanceFromPlayer = curTile.WorldPosition - Game1.Player1.WorldPosition;
                    Vector2 screenPos = distanceFromPlayer + Game1.Player1.ScreenPosition;

                    if (devRendering)
                    {
                        if (curTile.IsDoor)
                        {
                            curTile.TileSprite.TintColor = Color.Pink;
                        }
                        if (curTile.IsEnemySpawner)
                        {
                            curTile.TileSprite.TintColor = Color.IndianRed;
                        }
                    }

                    Layout[x, y].Draw(spriteBatch, gameTime, screenPos);

                    curTile.TileSprite.TintColor = Color.White;
                }
            }
        }

        /// <summary>
        /// Takes in a string array of positions and converts them to
        /// a list of Points. Each position is a delimited by '|'.
        /// </summary>
        /// <param name="allPositions">String array of positions.</param>
        /// <returns></returns>
        public List<Point> ConvertPositions(string allPositions)
        {
            //If allPositions is empty (no enemies/obstacles, return early
            //with empty list of Points.
            if (allPositions.Length == 0)
            {
                return new List<Point>();
            }
            string[] positions = allPositions.Split('|');
            List<Point> result = new List<Point>(positions.Length);
            for(int i = 0; i < positions.Length; i++)
            {
                string[] pointXY = positions[i].Split(',');

                foreach(string x in pointXY)
                {
                    Debug.WriteLine(x);
                }
                
                int X = int.Parse(pointXY[0]);
                int Y = int.Parse(pointXY[1]);
                result.Add(new Point(X, Y));
            }
            return result;
        }

        public string GetDoorientation(int x, int y)
        {
            if (x == 0) return "left";
            else if (x == Columns - 1) return "right";
            else if (y == 0) return "top";
            else if (y == Rows - 1) return "bottom";
            else throw new Exception("Tile is not in border");
        }
    }
}
