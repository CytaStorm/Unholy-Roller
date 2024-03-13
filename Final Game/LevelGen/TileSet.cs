using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Final_Game.LevelGen
{
	internal class Tileset : IGameObject
	{
		// Fields
		public bool devRendering = false;
		private static Random _random = new Random();

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

		public Tileset(Point origin)
		{

		}

		/// <summary>
		/// Creates a square tileset from the integer values in a text file
		/// </summary>
		/// <param name="filename"> the file to read </param>
		public Tileset(string floorFilename, string enemyPosFileName,
			string obstaclePosFileName)
		{
			//Read in floor data from file.
			string[] floorRows = File.ReadAllLines(floorFilename);
			//X
			int height = floorRows.Length;
			//Y
			int width = floorRows[0].Split(' ').Length;
			Layout = new Tile[height, width];

			//Parse room floor.
			for (int row = 0; row < height; row++)
			{
				string[] rowData = floorRows[height].Split(' ');
				for (int col = 0; col < width; col++)
				{
					Layout[row, col] = ParseRoomFloor(rowData[col], row, col);
				}
			}

			//Read in enemy and obstacle position data from file.
			string[] allEnemyPos = File.ReadAllLines(enemyPosFileName);
			string[] allObstaclePos = File.ReadAllLines(obstaclePosFileName);

			List<Point> EnemyPos;
			List<Point> ObstaclePos;

			//Select Enemy and Obstacle positions, ensure no overlap, and that
			//they fit in the room.
			bool invalidEnemyObstacleCombo = false;
			bool obstaclesFit = true;
			bool enemiesFit = true;
			do
			{
				EnemyPos = ConvertPositions(selectLine(allEnemyPos));
				ObstaclePos = ConvertPositions(selectLine(allObstaclePos));

				if (ObstaclePos.Count == 0 || EnemyPos.Count == 0)
				{
					continue;
				}
                invalidEnemyObstacleCombo = EnsureNoOverlap(EnemyPos, ObstaclePos);
				obstaclesFit = PositionsFit(ObstaclePos);
				enemiesFit = PositionsFit(EnemyPos);
			} while (invalidEnemyObstacleCombo && obstaclesFit && enemiesFit);

			//Add positions to Layout
			if (EnemyPos != null)
			{
			    // Set spawners
			    foreach (Point enemyPos in EnemyPos)
			    {
			        Layout[enemyPos.X, enemyPos.Y].IsEnemySpawner = true;
			        Spawners.Add(Layout[enemyPos.X, enemyPos.Y]);
			    }
			}

			if (ObstaclePos != null)
			{
			    // Set obstacles
			    foreach (Point obstaclePos in ObstaclePos)
			    {
			        Layout[obstaclePos.X, obstaclePos.Y] = TileMaker.SetTile(
			            TileType.Spike, new Vector2(
			                obstaclePos.X * Game1.TILESIZE,
			                obstaclePos.Y * Game1.TILESIZE),
			                "");
			    }
			}

		}

		#region Methods
		

		private Tile ParseRoomFloor(string tileString, int row, int col)
		{
			Tile result;
			int tileIdInt;
			string orientation = string.Empty;
			//Special tile Handling
			if (!int.TryParse(tileString, out tileIdInt))
			{
				string[] sValues = tileString.Split('.');
				tileIdInt = int.Parse(sValues[0]);
				orientation = sValues[1];
			}
		    result = TileMaker.SetTile(
				(TileType)tileIdInt,
				new Vector2(row * Game1.TILESIZE, col * Game1.TILESIZE),
				orientation);
			return result;
		}

		private string selectLine(string[] pool)
		{
			string result;
			do
			{
				result = pool[_random.Next(pool.Length)];
			} while (!result.StartsWith("//"));

			return result;
		}

		private bool EnsureNoOverlap(List<Point> EnemyPos, List<Point> ObstaclePos)
		{
			foreach (Point obstacle in ObstaclePos)
			{
				if (EnemyPos.IndexOf(obstacle) != -1)
				{
					return false;
				}
			}
			return true;
		}

		private bool PositionsFit(List<Point> positionsToCheck)
		{
			foreach (Point position in positionsToCheck)
			{
				if (position.X > Layout.GetLength(1) ||
					position.X < 0 ||
					position.Y > Layout.GetLength(0) ||
					position.Y < 0)
				{
					return false;
				}
			}
			return true;
		}
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
					Vector2 distanceFromPlayer = curTile.WorldPosition - Game1.Player.WorldPosition;
					Vector2 screenPos = distanceFromPlayer + Game1.Player.ScreenPosition;

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

					Layout[x, y].Draw(spriteBatch, screenPos);

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
			for (int i = 0; i < positions.Length; i++)
			{
				string[] pointXY = positions[i].Split(',');

				foreach (string x in pointXY)
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
	#endregion
	}
}
