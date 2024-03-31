
﻿using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	public static class CollisionChecker
	{
		//public static CollisionType CheckTileCollision(Entity.Entity e)
		//{
		//    if (Game1._roomManager == null)
		//    {
		//        throw new Exception("There are no tiles to check.");
		//    }

		//    for (int i = 0; i < Game1._roomManager.Rooms.Count; i++)
		//    {
		//        Room curRoom = Game1._roomManager.Rooms[i];

		//        // Check if entity'll hit any tile in the dungeon
		//        for (int y = 0; y < curRoom.Floor.Layout.GetLength(0); y++)
		//        {
		//            for (int x = 0; x < curRoom.Floor.Layout.GetLength(1); x++)
		//            {
		//                Tile curTile = curRoom.Floor.Layout[y, x];

		//                if (curTile.Collidable)
		//                {
		//                    // Get tile's hitbox
		//                    Rectangle curTile.Hitbox = new Rectangle(
		//                        (int)curTile.WorldPosition.X,
		//                        (int)curTile.WorldPosition.Y,
		//                        Game1.TILESIZE,
		//                        Game1.TILESIZE);

		//                    // Get where the entity will be
		//                    Vector2 colPosition = e.WorldPosition + e.Velocity;
		//                    Rectangle colHitbox = new Rectangle(
		//                        (int)colPosition.X,
		//                        (int)colPosition.Y,
		//                        e.Hitbox.Width,
		//                        e.Hitbox.Height);

		//                    // Check if entity will hit the tile
		//                    if (colHitbox.Intersects(curTile.Hitbox))
		//                    {
		//                        Vector2 distFromTile = curTile.WorldPosition - e.WorldPosition;

		//                        // Check which of the entity was hit
		//                        if (Math.Abs(distFromTile.X) > Math.Abs(distFromTile.Y))
		//                        {
		//                            return CollisionType.Horizontal;
		//                        }
		//                        else
		//                        {
		//                            return CollisionType.Vertical;
		//                        }
		//                    }
		//                }
		//            }
		//        }
		//    }

		//    return CollisionType.None;
		//}
    
		public static bool CheckTilemapCollision(Entity.Entity e, Tileset tileset)
		{
			//Point holds position of collided tile in room tileset,
			//Rectangle holds the hitbox of player upon intersect.
			Dictionary<Point, Rectangle> collisions = new Dictionary<Point, Rectangle>();

			//Find all collisions on the map.
			for (int row = 0; row < tileset.Layout.GetLength(0); row++)
			{
				for (int col = 0; col < tileset.Layout.GetLength(1); col++)
				{
					CheckTileCollision(row, col, collisions, e, tileset);
				}
			}
			//Choose which collision to process.
			ProcessCollisions(e, tileset, collisions);

			return false;
		}

		/// <summary>
		/// Determines if the tile is intersecting with the player,
		/// if it is, adds it to the dictionary supplied.
		/// </summary>
		/// <param name="row">Row in 2D array of tiles to check.</param>
		/// <param name="col">Column in 2D array of tiles to check.</param>
		/// <param name="collisionsList">List of collisions.</param>
		/// <param name="e">Entity to check collisions with.</param>
		/// <param name="collisions">Dictionary containing the tiles that entity
		/// has collided with.</param>
		private static void CheckTileCollision(
			int row, int col, Dictionary<Point, Rectangle> collisionsList,
			Entity.Entity e, Tileset tileset)
		{
			Tile curTile = tileset.Layout[row, col];

			//If there is no tile collision.
			if (!curTile.CollisionOn)
			{
				return;
			}

			#region Tunneling
			// Get final position of entity hitbox
			Vector2 collisionPosition = e.WorldPosition + e.Velocity;
			Rectangle finalHitbox = new Rectangle(
				(int)collisionPosition.X,
				(int)collisionPosition.Y,
				(int)(e.Hitbox.Width),
				(int)(e.Hitbox.Height));

			int numShifts = (int) e.Velocity.Length();
			bool intersects = false;

			//Check if need to calculate tunneling

			//No need to calculate tunnelling.
			if (numShifts <= 1f)
			{
				// If change in dist isn't significant
				// Just use final hitbox position to check overlap
				if (finalHitbox.Intersects(curTile.Hitbox))
				{
					collisionsList.Add(new Point(row, col), finalHitbox);
				}
				return;
			}

			Vector2 normVelo = e.Velocity;
			normVelo.Normalize();

			//Checks if entity will hit a map tile along its travel path.
			Rectangle shiftedHitbox;
			for (int i = 0; i < numShifts; i++)
			{
				Vector2 shiftedPosition = e.WorldPosition + normVelo * (i + 1);
				shiftedHitbox = new Rectangle(
				(int)MathF.Round(shiftedPosition.X),
				(int)MathF.Round(shiftedPosition.Y),
				(int)(e.Hitbox.Width),
				(int)(e.Hitbox.Height));

				intersects = shiftedHitbox.Intersects(curTile.Hitbox);
				if (intersects)
				{
					collisionsList.Add(new Point(row, col), shiftedHitbox);
					return;
				}
			}
			//Check if entity will hit a map tile at its final position
			if (!intersects) 
			{ 
				intersects = finalHitbox.Intersects(curTile.Hitbox);
				if (intersects)
				{
					collisionsList.Add(new Point(row, col), finalHitbox);
					return;
				}
			}
			#endregion
			return;
		}

		/// <summary>
		/// Decide which collision to process.
		/// </summary>
		/// <param name="e">Entity that collides with tile.</param>
		/// <param name="tileset">Tileset that to calculate collisions with.</param>
		/// <param name="collisions">Dictionary containing the tiles that entity
		/// has collided with.</param>
		private static void ProcessCollisions(
			Entity.Entity e, Tileset tileset, Dictionary<Point, Rectangle> collisions)
		{
			Rectangle largestOverlap = new Rectangle();
			Point largestOverlapTilePos = new Point();
			if (collisions.Count == 0) return;

			//Determine which tile to calculate collisions off of.
			//Largest intersection area wins.
			foreach (KeyValuePair<Point, Rectangle> pair in collisions)
			{
				Rectangle overlap = Rectangle.Intersect(
					pair.Value, tileset.Layout[pair.Key.X, pair.Key.Y].Hitbox);
				if (largestOverlap.Width * largestOverlap.Height <=
					overlap.Width * overlap.Height) 
				{
					largestOverlap = overlap;
					largestOverlapTilePos = pair.Key;
				}
			}
			// Determine direction entity hit the tile
			Tile curTile = tileset.Layout[largestOverlapTilePos.X, largestOverlapTilePos.Y];

			if (largestOverlap.Height >= largestOverlap.Width)
			{
				e.OnHitTile(curTile, CollisionDirection.Horizontal);
				return;
			}
			e.OnHitTile(curTile, CollisionDirection.Vertical);
			return;
		}

		public static bool CheckEntityCollision(Entity.Entity e1, Entity.Entity e2)
		{
			// Don't check collisions if hitbox is off
			if (!e1.CollisionOn || !e2.CollisionOn)
				return false;

			// Get where first entity's hitbox will be
			Rectangle colRect1 = new Rectangle(
				e1.Hitbox.X + (int)e1.Velocity.X,
				e1.Hitbox.Y + (int)e1.Velocity.Y,
				e1.Hitbox.Width,
				e1.Hitbox.Height);

			// Get where second entity's hitbox will be
			Rectangle colRect2 = new Rectangle(
				e2.Hitbox.X + (int)e2.Velocity.X,
				e2.Hitbox.Y + (int)e2.Velocity.Y,
				e2.Hitbox.Width,
				e2.Hitbox.Height);

			// Check if entities will collide
			bool collided = colRect1.Intersects(colRect2);

			if (collided)
			{
				// Check which direction entity was hit from
				Vector2 distFromTile = e1.WorldPosition - e2.WorldPosition;

				// Tell entities they hit something
				// Tell how they were hit
				if (Math.Abs(distFromTile.X) > Math.Abs(distFromTile.Y))
				{
					e1.OnHitEntity(e2, CollisionDirection.Horizontal);
					e2.OnHitEntity(e1, CollisionDirection.Horizontal);
				}
				else
				{
					e1.OnHitEntity(e2, CollisionDirection.Vertical);
					e2.OnHitEntity(e1, CollisionDirection.Vertical);
				}
			}

			return collided;
		}

		//public static CollisionType CheckEntityCollision(Rectangle checkRect, Entity e2)
		//{
		//    // Get where entity's hitbox will be
		//    Rectangle colRect2 = new Rectangle(
		//        e2.Hitbox.X + (int)e2.Velocity.X,
		//        e2.Hitbox.Y + (int)e2.Velocity.Y,
		//        e2.Hitbox.Width,
		//        e2.Hitbox.Height);

		//    // Check if entities will collide
		//    bool collided = checkRect.Intersects(colRect2);

		//    if (collided)
		//    {
		//        // Check which direction entity was hit from
		//        Vector2 distFromTile = new Vector2(checkRect.X, checkRect.Y) - e2.WorldPosition;

		//        // Tell entities they hit something
		//        // Tell how they were hit
		//        if (Math.Abs(distFromTile.X) > Math.Abs(distFromTile.Y))
		//        {
		//            return CollisionType.Horizontal;
		//        }
		//        else
		//        {
		//            return CollisionType.Vertical;
		//        }
		//    }

		//    return CollisionType.None;
		//}

		//public static bool CheckMapObjectCollision(Entity e1, MapOBJ obj)
		//{
		//    // Get where first entity's hitbox will be
		//    Rectangle colRect1 = new Rectangle(
		//        e1.Hitbox.X + (int)e1.Velocity.X,
		//        e1.Hitbox.Y + (int)e1.Velocity.Y,
		//        e1.Hitbox.Width,
		//        e1.Hitbox.Height);

		//    // Check if entities will collide
		//    bool collided = colRect1.Intersects(obj.Hitbox);

		//    if (collided)
		//    {
		//        // Check which direction entity was hit from
		//        Vector2 distFromTile = e1.WorldPosition - obj.WorldPosition;

		//        // Tell entities they hit something
		//        // Tell how they were hit
		//        if (Math.Abs(distFromTile.X) > Math.Abs(distFromTile.Y))
		//        {
		//            e1.OnHitObject(obj, CollisionType.Horizontal);
		//            obj.OnHitEntity(e1, CollisionType.Horizontal);
		//        }
		//        else
		//        {
		//            e1.OnHitObject(obj, CollisionType.Vertical);
		//            obj.OnHitEntity(e1, CollisionType.Vertical);
		//        }
		//    }

		//    return collided;
		//}
	}
}



		