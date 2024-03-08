using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    internal class TransferTileOBJ : MapOBJ
    {
        public Tile DoorPointer { get; set; }
        public Vector2 Destination { get; set; }

        public TransferTileOBJ(Game1 gm, Vector2 worldPos, Room parent, Room roomPointer, Tile doorPointer)
        {
            WorldPosition = worldPos;

            Texture2D transferTileTexture = gm.Content.Load<Texture2D>("Cthulu_Muggles");
            Image = new Sprite(
                transferTileTexture,
                transferTileTexture.Bounds,
                new Rectangle(0, 0, Game1.TILESIZE, Game1.TILESIZE));

            // Make image invisible
            Image.TintColor = Color.Transparent;

            Hitbox = new Rectangle(
                (int)worldPos.X,
                (int)worldPos.Y,
                transferTileTexture.Width,
                transferTileTexture.Height);

            Parent = parent;

            DoorPointer = doorPointer;

            Destination = GetDestination();

            RoomPointer = roomPointer;

            Type = MapObJType.TransferTile;
        }

        public override void OnHitEntity(Entity e, CollisionType colType)
        {
            switch (e.Type)
            {
                case EntityType.Player:
                    // Unsubscribe from EManager
                    Game1.EManager.OnLastEnemyKilled -= Parent.ReactToEnemyClear;

                    // Set up next room
                    RoomPointer.PlayerEntered();
                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            Vector2 distFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = Game1.Player1.ScreenPosition + distFromPlayer;
            Image.Draw(sb, screenPos);
        }

        public Vector2 GetDestination()
        {
            Vector2 result = Vector2.Zero;

            switch (DoorPointer.Doorientaiton)
            {
                case "top":
                    result = DoorPointer.WorldPosition + new Vector2(0f, Game1.TILESIZE);
                    break;

                case "bottom":
                    result = DoorPointer.WorldPosition - new Vector2(0f, Game1.TILESIZE);
                    break;

                case "left":
                    result = DoorPointer.WorldPosition + new Vector2(Game1.TILESIZE, 0f);
                    break;

                case "right":
                    result = DoorPointer.WorldPosition - new Vector2(Game1.TILESIZE, 0f);
                    break;
            }

            return result;
        }
    }
}
