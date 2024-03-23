using Final_Game.Pickups;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Final_Game.Entity;

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
            // Remove Collected Pickups
            for (int i = 0; i < Pickups.Count; i++)
            {
                if (!Pickups[i].Alive)
                {
                    Pickups.RemoveAt(i);

                    i--;
                }
            }
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
            Pickups.Add(new Pickup_Health(_gm.Content, new Vector2(100, 100)));

            return;
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
