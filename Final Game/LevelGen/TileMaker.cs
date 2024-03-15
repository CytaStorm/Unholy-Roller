using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Final_Game.LevelGen
{
	/// <summary>
	/// What types of tiles there are.
	/// </summary>
	public enum TileType
	{
		Grass,
		Wall,
		Spike,
		Placeholder,
		ClosedDoor,
		OpenDoor
	}

	internal class TileMaker
	{
		#region Fields
		private static Texture2D[] _tileTextures;

		// The uniform dimensions of all map tiles
		private const int sourceTilesize = 60;

		#endregion

		#region Constructor(s)
		public TileMaker(ContentManager cm)
		{
			// Load all tile spritesheets
			_tileTextures = new Texture2D[7];
			_tileTextures[0] = cm.Load<Texture2D>("Sprites/PlaceholderTile");
			_tileTextures[1] = cm.Load<Texture2D>("Sprites/PurpleTile");
			_tileTextures[2] = cm.Load<Texture2D>("Sprites/GutterTileAtlas");
			_tileTextures[3] = cm.Load<Texture2D>("Sprites/SpikeTile");
			_tileTextures[4] = cm.Load<Texture2D>("Sprites/WallSheet");
			_tileTextures[5] = cm.Load<Texture2D>("Sprites/ClosedDoorAtlas");
			_tileTextures[6] = cm.Load<Texture2D>("Sprites/OpenDoorAtlas");
		}
		#endregion

		#region Methods
		/// <summary>
		/// Creates a tile of the specified type
		/// </summary>
		/// <param name="tileType"> type of tile to create </param>
		/// <returns> created tile </returns>
		public static Tile SetTile(TileType tileType)
		{
			return SetTile(tileType, new Vector2(0f, 0f), "");
		}

		/// <summary>
		/// Creates a tile of the specified type
		/// </summary>
		/// <param name="tileType"> type of tile to create </param>
		/// <param name="position"> position of the tile </param>
		/// <returns> created tile </returns>
		public static Tile SetTile(TileType tileType, Vector2 position)
		{
			return SetTile(tileType, position, "");
		}

		/// <summary>
		/// Creates a tile of the specified type
		/// </summary>
		/// <param name="tileType"> type of tile to create </param>
		/// <param name="position"> position of the tile </param>
		/// /// <param name="orientation"> rotation of the tile </param>
		/// <returns> created tile </returns>
		public static Tile SetTile(TileType tileType, Vector2 position, string orientation)
		{
			Tile result = new Tile();
			result.Type = tileType;

			result.WorldPosition = position;


			switch (tileType)
			{
				case TileType.Grass:
					result.Collidable = false;
					result.TileSprite = new Sprite(_tileTextures[1], GetOrientationSource(""),
						new Rectangle(
							(int)position.X,
							(int)position.Y,
							Game1.TileSize,
							Game1.TileSize));
					break;

				case TileType.Wall:
					result.Collidable = true;
					result.TileSprite = new Sprite(_tileTextures[2], GetOrientationSource(orientation),
						new Rectangle(
							(int)position.X,
							(int)position.Y,
							Game1.TileSize,
							Game1.TileSize));
					break;

				case TileType.ClosedDoor:
					result.Collidable = true;
					result.TileSprite = new Sprite(_tileTextures[5], GetOrientationSource(orientation),
						new Rectangle(
							(int)position.X,
							(int)position.Y,
							Game1.TileSize,
							Game1.TileSize));
					break;

				case TileType.OpenDoor:
					result.Collidable = true;
					result.TileSprite = new Sprite(_tileTextures[6], GetOrientationSource(orientation),
						new Rectangle(
							(int)position.X,
							(int)position.Y,
							Game1.TileSize,
							Game1.TileSize));
					break;
					
				case TileType.Spike:
					result.Collidable = true;

					result.TileSprite = new Sprite(_tileTextures[3], GetOrientationSource(""),
						new Rectangle(
							(int)position.X,
							(int)position.Y,
							Game1.TileSize,
							Game1.TileSize));
					break;
				default:
					result.Collidable = false;
					result.TileSprite = new Sprite(_tileTextures[0], GetOrientationSource(""),
						new Rectangle(
							(int)position.X,
							(int)position.Y,
							Game1.TileSize,
							Game1.TileSize));
					break;
			}
			return result;
		}

		/// <summary>
		/// Returns proper orientation of wall tiles.
		/// </summary>
		/// <param name="orientation">Desired orientation</param>
		/// <returns>Rectangle of the desired wall.</returns>
		private static Rectangle GetOrientationSource(string orientation)
		{
			if (string.IsNullOrEmpty(orientation))
			{
				return new Rectangle(0, 0, sourceTilesize, sourceTilesize);
			}

			switch (orientation)
			{
				case "UR":
					return new Rectangle(sourceTilesize * 2, 0, sourceTilesize, sourceTilesize);
				case "U":
					return new Rectangle(sourceTilesize, 0, sourceTilesize, sourceTilesize);
				case "UL":
					return new Rectangle(0, 0, sourceTilesize, sourceTilesize);
				case "L":
					return new Rectangle(0, sourceTilesize, sourceTilesize, sourceTilesize);
				case "R":
					return new Rectangle(sourceTilesize * 2, sourceTilesize, sourceTilesize, sourceTilesize);
				case "BL":
					return new Rectangle(0, sourceTilesize * 2, sourceTilesize, sourceTilesize);
				case "B":
					return new Rectangle(sourceTilesize, sourceTilesize * 2, sourceTilesize, sourceTilesize);
				case "BR":
					return new Rectangle(sourceTilesize * 2, sourceTilesize * 2, sourceTilesize, sourceTilesize);
			}
			return new Rectangle(0, 0, sourceTilesize, sourceTilesize);
		}
	}
	#endregion
}
