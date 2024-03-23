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
        // Fields
        private Game1 _gm;

        // Properties
        public List<Entity.Entity> Pickups { get; private set; }

        public PickupManager(Game1 gm)
        {
            _gm = gm;

            Pickups = new List<Entity.Entity>();

            CreateTestPickups();
        }

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

        public void CreateTestPickups()
        {
            //Rectangle roomBounds = new Rectangle(
            //    0,
            //    0,
            //    Game1.TestLevel.CurrentRoom.Tileset.Width,
            //    Game1.TestLevel.CurrentRoom.Tileset.Height);

            // Health Pickup
            Pickups.Add(new Pickup_Health(_gm.Content, new Vector2(900, 900)));

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
    }
}
