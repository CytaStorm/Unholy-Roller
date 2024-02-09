using Microsoft.Xna.Framework;
using Prototype.GameEntity;
using Prototype.MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public enum CollisionType
    {
        Horizontal,
        Vertical,
        None
    }

    internal class CollisionChecker
    {
        public static CollisionType CheckTileCollision(Entity e)
        {
            if (Game1._roomManager == null)
            {
                throw new Exception("There are no tiles to check.");
            }

            for (int i = 0; i < Game1._roomManager.Rooms.Count; i++)
            {
                Room curRoom = Game1._roomManager.Rooms[i];

                // Check if entity'll hit any tile in the dungeon
                for (int y = 0; y < curRoom.Floor.Layout.GetLength(0); y++)
                {
                    for (int x = 0; x < curRoom.Floor.Layout.GetLength(1); x++)
                    {
                        Tile curTile = curRoom.Floor.Layout[y, x];

                        if (curTile.Collidable)
                        {
                            // Get tile's hitbox
                            Rectangle tileHit = new Rectangle(
                                (int)curTile.WorldPosition.X,
                                (int)curTile.WorldPosition.Y,
                                Game1.TILESIZE,
                                Game1.TILESIZE);

                            // Get where the entity will be
                            Vector2 colPosition = e.WorldPosition + e.Velocity;
                            Rectangle colHitbox = new Rectangle(
                                (int)colPosition.X,
                                (int)colPosition.Y,
                                e.Hitbox.Width,
                                e.Hitbox.Height);

                            // Check if entity will hit the tile
                            if (colHitbox.Right >= tileHit.X &&
                                colHitbox.X <= tileHit.Right &&
                                colHitbox.Bottom >= tileHit.Y &&
                                colHitbox.Y <= tileHit.Bottom)
                            {
                                Vector2 distFromTile = curTile.WorldPosition - e.WorldPosition;

                                // Check which of the entity was hit
                                if (Math.Abs(distFromTile.X) > Math.Abs(distFromTile.Y))
                                {
                                    return CollisionType.Horizontal;
                                }
                                else
                                {
                                    return CollisionType.Vertical;
                                }
                            }
                            
                        }
                    }
                }
            }

            return CollisionType.None;
        }
    }
}
