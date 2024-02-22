using Microsoft.Xna.Framework;
using Prototype.GameEntity;
using Prototype.MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

    public static class CollisionChecker
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
                            if (colHitbox.Intersects(tileHit))
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
        
        public static bool CheckTilemapCollision(Entity e, Tileset tileset)
        {
            // Check if entity'll hit any tile in the dungeon
            for (int y = 0; y < tileset.Layout.GetLength(0); y++)
            {
                for (int x = 0; x < tileset.Layout.GetLength(1); x++)
                {
                    Tile curTile = tileset.Layout[y, x];

                    if (curTile.Collidable)
                    {
                        // Get tile's hitbox
                        Rectangle tileHit = new Rectangle(
                            (int)curTile.WorldPosition.X,
                            (int)curTile.WorldPosition.Y,
                            Game1.TILESIZE,
                            Game1.TILESIZE);

                        // Get final position of entity hitbox
                        Vector2 colPosition = e.WorldPosition + e.Velocity;
                        Rectangle colHitbox = new Rectangle(
                            (int)colPosition.X,
                            (int)colPosition.Y,
                            e.Hitbox.Width,
                            e.Hitbox.Height);

                        float numShifts = e.Velocity.Length();
                        bool intersects = false;
                        Rectangle temp = new Rectangle();
                        Vector2 tempPoint = Vector2.Zero;

                        if (numShifts > 1f)
                        {
                            Vector2 normVelo = e.Velocity;
                            normVelo.Normalize();

                            // Explain what this does
                            for (int i = 0; i < (int)numShifts; i++)
                            {
                                tempPoint = e.WorldPosition + normVelo * (i + 1);
                                temp = new Rectangle(
                                (int)MathF.Round(tempPoint.X),
                                (int)MathF.Round(tempPoint.Y),
                                e.Hitbox.Width,
                                e.Hitbox.Height);

                                intersects = temp.Intersects(tileHit);

                                if (intersects)
                                    break;
                            }
                            
                            // Check if final hitbox position overlaps
                            if (!intersects)
                            {
                                temp = colHitbox;
                                tempPoint = e.WorldPosition;
                                intersects = colHitbox.Intersects(tileHit);
                            }
                        }
                        else
                        {
                            // If change in dist isn't significant
                            // Just use final hitbox position to check overlap
                            temp = colHitbox;
                            tempPoint = e.WorldPosition;

                            intersects = colHitbox.Intersects(tileHit);
                        }
                        

                        // Check if entity will hit the tile
                        if (intersects)
                        {
                            Vector2 distFromTile = curTile.WorldPosition - tempPoint;

                            // Determine direction entity hit the tile

                            if (Math.Abs(distFromTile.X) > Math.Abs(distFromTile.Y))
                            {
                                e.OnHitTile(curTile, CollisionType.Horizontal);
                            }
                            else
                            {
                                e.OnHitTile(curTile, CollisionType.Vertical);
                            }
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool CheckEntityCollision(Entity e1, Entity e2)
        {
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
                    e1.OnHitEntity(e2, CollisionType.Horizontal, true);
                    e2.OnHitEntity(e1, CollisionType.Horizontal, false);
                }
                else
                {
                    e1.OnHitEntity(e2, CollisionType.Vertical, true);
                    e2.OnHitEntity(e1, CollisionType.Vertical, false);
                }
            }

            return collided;
        }
        
        public static CollisionType CheckEntityCollision(Rectangle checkRect, Entity e2)
        {   
            // Get where entity's hitbox will be
            Rectangle colRect2 = new Rectangle(
                e2.Hitbox.X + (int)e2.Velocity.X,
                e2.Hitbox.Y + (int)e2.Velocity.Y,
                e2.Hitbox.Width,
                e2.Hitbox.Height);

            // Check if entities will collide
            bool collided = checkRect.Intersects(colRect2);

            if (collided)
            {
                // Check which direction entity was hit from
                Vector2 distFromTile = new Vector2(checkRect.X, checkRect.Y) - e2.WorldPosition;

                // Tell entities they hit something
                // Tell how they were hit
                if (Math.Abs(distFromTile.X) > Math.Abs(distFromTile.Y))
                {
                    return CollisionType.Horizontal;
                }
                else
                {
                    return CollisionType.Vertical;
                }
            }

            return CollisionType.None;
        }

        public static bool CheckMapObjectCollision(Entity e1, MapOBJ obj)
        {
            // Get where first entity's hitbox will be
            Rectangle colRect1 = new Rectangle(
                e1.Hitbox.X + (int)e1.Velocity.X,
                e1.Hitbox.Y + (int)e1.Velocity.Y,
                e1.Hitbox.Width,
                e1.Hitbox.Height);

            // Check if entities will collide
            bool collided = colRect1.Intersects(obj.Hitbox);

            if (collided)
            {
                // Check which direction entity was hit from
                Vector2 distFromTile = e1.WorldPosition - obj.WorldPosition;

                // Tell entities they hit something
                // Tell how they were hit
                if (Math.Abs(distFromTile.X) > Math.Abs(distFromTile.Y))
                {
                    e1.OnHitObject(obj, CollisionType.Horizontal);
                    obj.OnHitEntity(e1, CollisionType.Horizontal);
                }
                else
                {
                    e1.OnHitObject(obj, CollisionType.Vertical);
                    obj.OnHitEntity(e1, CollisionType.Vertical);
                }
            }

            return collided;
        }
    }
}
