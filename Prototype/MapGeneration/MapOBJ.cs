// Parent class for all interactable map objects

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
using System;
using System.Net.Http;

namespace Prototype.MapGeneration
{
    public enum MapObJType
    {
        Door
    }

    public class MapOBJ
    {
        // Fields

        // Properties
        public bool Active { get; set; }
        public Rectangle Hitbox { get; protected set; }
        public Sprite Image { get; protected set; }
        public Vector2 WorldPosition { get; set; }
        public MapObJType Type { get; protected set; }

        // Constructors
        public MapOBJ(Vector2 worldPos, Texture2D image, MapObJType type)
        {
            WorldPosition = worldPos;
            Image = new Sprite(image, image.Bounds, image.Bounds);
            Hitbox = new Rectangle(
                (int)worldPos.X,
                (int)worldPos.Y,
                image.Width,
                image.Height);
            Active = true;
            Type = type;
        }

        // Methods

        public void OnHitEntity(Entity e, CollisionType colType)
        {
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Get position relative to the player
            Vector2 distFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = Game1.Player1.ScreenPosition + distFromPlayer;

            Image.Draw(spriteBatch, screenPos);
        }
    }   
}
