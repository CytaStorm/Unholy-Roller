using Final_Game.Pickups;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Final_Game.Entity;
using Final_Game.LevelGen;
using System.Diagnostics;

namespace Final_Game
{
    public class PickupManager
    {
        #region Fields
        private Game1 _gm;
        #endregion

        #region Properties
        public List<Entity.Entity> Pickups { get; private set; }
        #endregion

        #region Constructor(s)
        public PickupManager(Game1 gm)
        {
            _gm = gm;

            Pickups = new List<Entity.Entity>();

            //CreateTestPickups();
        }
        #endregion

        #region Method(s)
        public void Update(GameTime gameTime)
        {
            //// Remove Collected Pickups
            //for (int i = 0; i < Pickups.Count; i++)
            //{
            //    if (!Pickups[i].Alive)
            //    {
            //        Pickups.RemoveAt(i);
            //        i--;
            //    }
            //}
        }

        public void Draw(SpriteBatch sb)
        {
            // Draw Pickups
            foreach (Entity.Entity p in Pickups)
            {
                p.Draw(sb);
            }
        }

        /// <summary>
        /// Creates one of every type of pickup in the room
        /// the player is currently in
        /// </summary>
        public void CreateTestPickups()
        {
            //Rectangle roomBounds = new Rectangle(
            //    Game1.TileSize,
            //    Game1.TileSize,
            //    Game1.TestLevel.CurrentRoom.Tileset.Width - Game1.TileSize,
            //    Game1.TestLevel.CurrentRoom.Tileset.Height - Game1.TileSize);

            // Health Pickup
            //Pickups.Add(new Pickup_Health(_gm.Content, new Vector2(100, 100)));

            return;
        }

        /// <summary>
        /// Creates a health pickup.
        /// </summary>
        /// <param name="tile">Tile to create health pickup at.</param>
        public void CreateHealthPickup(Tile tile)
        {
            Debug.WriteLine("Created health pickup");
			Pickups.Add(new Pickup_Health(_gm.Content, new Vector2(
				tile.WorldPosition.X, tile.WorldPosition.Y)));
		}

        public void PlayerCollided(Entity.Entity entity)
        {
            Game1.CurrentLevel.CurrentRoom.Tileset.CollectedPickup(
                entity.WorldPosition);
            Pickups.Remove(entity);
        }
        /// <summary>
        /// Clears all pickups in the room.
        /// </summary>
        public void ClearPickups()
        {
            Pickups.Clear();
        }

        public void DrawGizmos()
        {
            foreach(Entity.Entity p in Pickups)
            {
                p.DrawGizmos();
            }

            return;
        }

        #endregion
    }
}
