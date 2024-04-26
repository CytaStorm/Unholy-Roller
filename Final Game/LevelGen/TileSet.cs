using Final_Game.Pickups;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Final_Game.LevelGen
{
	public class Tileset : IGameObject
	{
		#region Fields

		/// <summary>
		/// Random used in-class.
		/// </summary>

		private static Random _random = new Random();

		#endregion

		#region Properties
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
		public int Width { get => Columns * Game1.TileSize; }

		/// <summary>
		/// Height of the tileset
		/// </summary>
		public int Height { get => Rows * Game1.TileSize; }

		/// <summary>
		/// Key: Direction
		/// Value: Door tile
		/// </summary>
		public Dictionary<string, Tile> Doors { get ; private set; } =
			new Dictionary<string, Tile>();

		public List<Tile> Spawners { get; private set; } = new List<Tile>();

		public int RoomFloorLayout;
		public int EnemyPositionLayout;
		public int ObstaclePositionLayout;
		#endregion

		#region Constructors(s)
		/// <summary>
		/// Creates a square tileset with randomly selected layout.
		/// </summary>
		/// <param name="filename"> the file to read </param>
		public Tileset()
		{
			//Read in floor data from file, and select layout.
			string allFloors = File.ReadAllText("Content/Room Layouts/roomLayouts.txt");
			string[] possibleFloors = allFloors.Split('|');
			RoomFloorLayout = _random.Next(possibleFloors.Length);
			string selectedFloor = possibleFloors[RoomFloorLayout];
			string[] floorRows = selectedFloor.Split('\n');
			//foreach(string str in floorRows)
			//{
			//	Debug.WriteLine(str + "poo poo");
			//}

			//X
			int height = floorRows.Length - 1;
			//Y
			int width = floorRows[0].Split(' ').Length;
			Layout = new Tile[height, width];

			//Parse room floor.
			for (int row = 0; row < height; row++)
			{
				string[] rowData = floorRows[row].Split(' ');
				for (int col = 0; col < width; col++)
				{
					Layout[row, col] = ParseRoomFloor(rowData[col], row, col);
				}
			}

			//Read in enemy and obstacle position data from file.
			string[] allEnemyPos = File.ReadAllLines(
				"Content/Room Layouts/enemyLayouts.txt");
			string[] allObstaclePos = File.ReadAllLines(
				"Content/Room Layouts/obstacleLayouts.txt");

			List<Point> EnemyPos;
			Dictionary<Point, char> ObstaclePos;

			//Select Enemy and Obstacle positions, ensure no overlap, and that
			//they fit in the room.
			bool invalidEnemyObstacleCombo = false;
			bool obstaclesFit = false;
			bool enemiesFit = false;
			do
			{
				EnemyPos = ConvertPositions(selectLine(allEnemyPos));
				ObstaclePos = ReadObstacles(selectLine(allObstaclePos));

				//If one of them is empty, then there will be no overlap.
				if (ObstaclePos.Count == 0 || EnemyPos.Count == 0)
				{
					//invalidEnemyObstacleCombo = false;
					//obstaclesFit = true;
					//enemiesFit = true;
					//continue;
					break;
				}
				invalidEnemyObstacleCombo = EnsureNoOverlap(EnemyPos, ObstaclePos);
				obstaclesFit = PositionsFit(ObstaclePos);
				enemiesFit = PositionsFit(EnemyPos);
			} while (!invalidEnemyObstacleCombo && obstaclesFit && enemiesFit);

			//Add positions to Layout
			
   			// Set spawners
   			foreach (Point enemyPos in EnemyPos)
   			{
   				Layout[enemyPos.Y, enemyPos.X].IsEnemySpawner = true;
   				Spawners.Add(Layout[enemyPos.Y, enemyPos.X]);
   			}

			// Set obstacles
			foreach (KeyValuePair<Point, char> obstaclePos in ObstaclePos)
			{
				//If no enemies, ignore spikes.
				if (EnemyPos.Count == 0 && obstaclePos.Value == 's')
				{
					continue;
				}
				CreateObstacle(obstaclePos);
			}
			//Debug.WriteLine("Here " + EnemyPos.Count);
			return;
		}

		public Tileset(string obstacleData, string enemySpawnData)
		{
            //Read in floor data from file, and select layout.
            string allFloors = File.ReadAllText("Content/Room Layouts/roomLayouts.txt");
            string[] possibleFloors = allFloors.Split('|');
            RoomFloorLayout = _random.Next(possibleFloors.Length);
            string selectedFloor = possibleFloors[RoomFloorLayout];
            string[] floorRows = selectedFloor.Split('\n');
            //foreach(string str in floorRows)
            //{
            //	Debug.WriteLine(str + "poo poo");
            //}

            //X
            int height = floorRows.Length - 1;
            //Y
            int width = floorRows[0].Split(' ').Length;
            Layout = new Tile[height, width];

            //Parse room floor.
            for (int row = 0; row < height; row++)
            {
                string[] rowData = floorRows[row].Split(' ');
                for (int col = 0; col < width; col++)
                {
                    Layout[row, col] = ParseRoomFloor(rowData[col], row, col);
                }
            }

            // Read obstacles
			if (obstacleData != null)
			{
				string[] obstacles = obstacleData.Split("|");

				for (int i = 0; i < obstacles.Length; i++)
				{
					// Get obstacle details (type, posX, posY)
					string[] obstacleSpecs = obstacles[i].Split(',');

					char obsType = char.Parse(obstacleSpecs[0]);

					int row = int.Parse(obstacleSpecs[1]);
					int col = int.Parse(obstacleSpecs[2]);


                    if (obsType == 'w')
					{
						// Create Wall Obstacle
						Layout[row, col] = TileMaker.SetTile(
							TileType.Wall,
							new Vector2(Game1.TileSize * col, Game1.TileSize * row),
                            "OBSTACLEWALL");
					}
					else if (obsType == 's')
					{
                        // Create Spike Obstacle
                        Layout[row, col] = TileMaker.SetTile(
                            TileType.Spike,
                            new Vector2(Game1.TileSize * col, Game1.TileSize * row),
                            "");
                    }
				}
			}

			// Read enemy positions 
			if (enemySpawnData != null)
			{
				string[] enemySpawners = enemySpawnData.Split('|');

				for (int i = 0; i < enemySpawners.Length; i++)
				{
					string[] spawnPosition = enemySpawners[i].Split(',');

					int row = int.Parse(spawnPosition[0]);
					int col = int.Parse(spawnPosition[1]);

                    Layout[row, col].IsEnemySpawner = true;

					Spawners.Add(Layout[row, col]);
				}
			}

		}



		#endregion

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

				//Ensures compatibility with CRLF/LF roomLayout etc files.
				if (orientation[orientation.Length - 1] == '\r')
				{
					orientation = orientation.Substring(0, orientation.Length - 1);
				}
			}
			
			result = TileMaker.SetTile(
				(TileType)tileIdInt,
				new Vector2(col * Game1.TileSize, row * Game1.TileSize),
				orientation);
			return result;
		}

		private string selectLine(string[] pool)
		{
			string result;
			int i;
			do
			{
				i = _random.Next(pool.Length);
				result = pool[i];
			} while (result.StartsWith("//"));

			return result;
		}

		private bool EnsureNoOverlap(List<Point> EnemyPos, Dictionary<Point, char> ObstaclePos)
		{
			foreach (KeyValuePair<Point, char> pair in ObstaclePos)
			{
				if (EnemyPos.IndexOf(pair.Key) != -1)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks if the List of enemy positions fits on the map.
		/// </summary>
		/// <param name="positionsToCheck">List of positions to check.</param>
		/// <returns>If the positions fit on the map.</returns>
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
		
		/// <summary>
		/// Checks if Dictionary of obstacle positions fits within the map.
		/// </summary>
		/// <param name="positionsToCheck"></param>
		/// <returns>If the positions fit on the map.</returns>
		private bool PositionsFit(Dictionary<Point, char> positionsToCheck)
		{
			foreach (KeyValuePair<Point, char> position in positionsToCheck)
			{
				if (position.Key.X > Layout.GetLength(1) ||
					position.Key.X < 0 ||
					position.Key.Y > Layout.GetLength(0) ||
					position.Key.Y < 0)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Creates closed doors based on the Room's connections list.
		/// </summary>
		/// <param name="connections">ActualConnections dictionary that specifies 
		/// which cardinal directions have a door.</param>
		public void CreateClosedDoors(Room[] connections)
		{
			if (connections[(int)Directions.North] != null)
			{
				Layout[0, (Columns - 1) / 2] =
					TileMaker.SetTile(
						TileType.ClosedDoor,
						new Vector2((Columns - 1) / 2 * Game1.TileSize, 0),
						"U");
				Doors.Add("North", Layout[0, (Columns - 1) / 2]);
			}
			if (connections[(int)Directions.South] != null)
			{
				Layout[Rows - 1, (Columns - 1) / 2] =
					TileMaker.SetTile(
						TileType.ClosedDoor,
						new Vector2((Columns - 1) / 2 * Game1.TileSize, (Rows - 1) * Game1.TileSize),
						"B");
				Doors.Add("South", Layout[Rows - 1, (Columns - 1) / 2]);
			}
			if (connections[(int)Directions.East] != null)
			{
				Layout[(Rows - 1) / 2, Columns - 1] =
					TileMaker.SetTile(
						TileType.ClosedDoor,
						new Vector2((Columns - 1) * Game1.TileSize, (Rows - 1) / 2 * Game1.TileSize),
						"R");
				Doors.Add("East", Layout[(Rows - 1) / 2, Columns - 1]);
			}
			if (connections[(int)Directions.West] != null)
			{
				Layout[(Rows - 1) / 2, 0] =
					TileMaker.SetTile(
						TileType.ClosedDoor,
						new Vector2(0, (Rows - 1) / 2 * Game1.TileSize),
						"L");
				Doors.Add("West", Layout[(Rows - 1) / 2, 0]);
			}
		}

		/// <summary>
		/// Creates open doors based on existing closed doors.
		/// </summary>
		public void CreateOpenDoors(Room[] connections)
		{
			if (connections[(int)Directions.North] != null)
			{
				Layout[0, (Columns - 1) / 2] =
					TileMaker.SetTile(
						TileType.OpenDoor,
						new Vector2((Columns - 1) / 2 * Game1.TileSize, 0),
						"U");
			}
			if (connections[(int)Directions.South] != null)
			{
				Layout[Rows - 1, (Columns - 1) / 2] =
					TileMaker.SetTile(
						TileType.OpenDoor,
						new Vector2((Columns - 1) / 2 * Game1.TileSize, (Rows - 1) * Game1.TileSize),
						"B");
			}
			if (connections[(int)Directions.East] != null)
			{
				Layout[(Rows - 1) / 2, Columns - 1] =
					TileMaker.SetTile(
						TileType.OpenDoor,
						new Vector2((Columns - 1) * Game1.TileSize, (Rows - 1) / 2 * Game1.TileSize),
						"R");
			}
			if (connections[(int)Directions.West] != null)
			{
				Layout[(Rows - 1) / 2, 0] =
					TileMaker.SetTile(
						TileType.OpenDoor,
						new Vector2(0, (Rows - 1) / 2 * Game1.TileSize),
						"L");
			}
		}

		

		public void Update(GameTime gameTime)
		{
			// TODO: Check for events
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach(Tile tile in Layout)
			{
				Vector2 screenPos = 
					tile.WorldPosition + Game1.MainCamera.WorldToScreenOffset;
				if (Game1.DebugOn)
				{
					if (tile.IsDoor)
					{
						tile.TileSprite.TintColor = Color.Magenta;
					}
					if (tile.IsEnemySpawner)
					{
						tile.TileSprite.TintColor = Color.IndianRed;
					}
				}
				tile.Draw(spriteBatch, screenPos);

				tile.TileSprite.TintColor = Color.White;
			}
		}

		/// <summary>
		/// Takes in a string array of positions and converts them to
		/// a list of Points. Each position is a delimited by '|'.
		/// </summary>
		/// <param name="allPositions">String array of positions.</param>
		/// <returns>List of points, converted from string array of positions.</returns>
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

				//foreach (string x in pointXY)
				//{
				//	Debug.WriteLine(x);
				//}

				int X = int.Parse(pointXY[0]);
				int Y = int.Parse(pointXY[1]);
				result.Add(new Point(X, Y));
			}
			return result;
		}

		/// <summary>
		/// Takes in a string array of osbtacle positions and if they are a wall or
		/// a spike, and converts it to a dictionary of points and chars, indicating
		/// if it is a wall or a spike.
		/// </summary>
		/// <param name="allPositions">Obstacle position and type data.</param>
		/// <returns></returns>
		public Dictionary<Point,char> ReadObstacles(string allPositions)
		{
			if (allPositions.Length == 0)
			{
				return new Dictionary<Point, char>();
			}

			string[] positions = allPositions.Split('|');
			Dictionary<Point, char> result = new Dictionary<Point, char>(positions.Length);

			for (int i = 0; i < positions.Length; i++)
			{
				string[] posData = positions[i].Split(',');

				char wallOrSpike = posData[0][0];
				//Matrix shenanigans
				int X = int.Parse(posData[2]);
				int Y = int.Parse(posData[1]);
				result.Add(new Point(X, Y), wallOrSpike);

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

		/// <summary>
		/// Creates obstacles based on the keyvalue pair.
		/// </summary>
		/// <param name="pair">Keyvalue pair containing position and type of obstacle.</param>
		private void CreateObstacle(KeyValuePair<Point, char> pair)
		{
			//Wall obstacle
			if (pair.Value == 'w')
			{
				Layout[pair.Key.Y, pair.Key.X] = TileMaker.SetTile(
					TileType.Wall, new Vector2(
						pair.Key.X * Game1.TileSize,
						pair.Key.Y * Game1.TileSize),
						"OBSTACLEWALL");
				return;
			}

			//Spike obstacle
			Layout[pair.Key.Y, pair.Key.X] = TileMaker.SetTile(
				TileType.Spike, new Vector2(
					pair.Key.X * Game1.TileSize,
					pair.Key.Y * Game1.TileSize),
					"");
			return;
		}

		/// <summary>
		/// Returns a random floor tile.
		/// </summary>
		/// <returns></returns>
		public Tile FindRandomFloorTile()
		{
			List<Tile> tileOptions = new List<Tile>();
			foreach (Tile tile in  Layout)
			{
				if (tile.Type == TileType.Floor)
				{
					tileOptions.Add(tile);
				}
			}
			return tileOptions[_random.Next(tileOptions.Count)];
		}

		/// <summary>
		/// Removes pickup flag at tile.
		/// </summary>
		/// <param name="worldposition">World position
		/// of tile to remove pickup flag at.</param>
		public void CollectedPickup(Vector2 worldposition)
		{
			//Debug.WriteLine("removed health pickup");
			Tile selectedTile = Layout[
				(int)worldposition.X / Game1.TileSize,
				(int)worldposition.Y / Game1.TileSize];
			//Debug.WriteLine(selectedTile.WorldPosition);
			selectedTile.HasHealthPickup = false;
		}

		/// <summary>
		/// Removes enemies near a door tile..
		/// </summary>
		/// <param name="tile">Door tile to remove enemies around.</param>
		public void RemoveEnemiesNearDoor(Tile door)
		{
			for (int i = 0; i < Spawners.Count; i++)
			{
				Tile tile = Spawners[i];
				//Remove spawners within 50 units of the door.
				Vector2 distance = door.WorldPosition - tile.WorldPosition;
				//Debug.WriteLine(distance.Length());
				if (distance.Length() < 500)
				{
					//Debug.WriteLine("entered loop");
					Spawners.Remove(tile);
					tile.IsEnemySpawner = false;	
				}
			}
		}

		/// <summary>
		/// Removes all enemies in room.
		/// </summary>
		public void ClearEnemies()
		{
			Spawners.Clear();
		}

		/// <summary>
		/// Clears item flags in the layout.
		/// </summary>
		public void ClearItemFlags()
		{
			Debug.WriteLine("cleared item flags");
			for (int i = 0; i < Layout.GetLength(0); i++)
			{
				for (int j = 0; j < Layout.GetLength(1); j++)
				{
					Layout[i, j].HasHealthPickup = false;
				}
			}
			Game1.PManager.ClearPickups();
		}

		#endregion
	}
}
