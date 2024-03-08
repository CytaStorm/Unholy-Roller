// Parent class for all interactable map objects

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
using System;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;

namespace Prototype.MapGeneration
{
    public enum MapObJType
    {
        Door,
        TransferTile,
        Health
    }

    public abstract class MapOBJ
    {
        // Fields

        // Properties
        public bool Alive { get; set; } = true;
        public Rectangle Hitbox { get; protected set; }
        public Sprite Image { get; protected set; }
        public Vector2 WorldPosition { get; set; }
        public MapObJType Type { get; protected set; }

        public Room Parent { get; protected set; }
        public Room RoomPointer { get; protected set; }

        // Methods

        public virtual void OnHitEntity(Entity e, CollisionType colType)
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
