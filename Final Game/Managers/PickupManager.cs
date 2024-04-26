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

namespace Final_Game.Managers
{
    public enum PickupType
    {
        Health,
        CurveCore
    }

    public class PickupManager
    {
        #region Fields
        private Game1 _gm;
        #endregion

        #region Properties
        public List<Entity.Entity> Pickups => Game1.CurrentLevel.CurrentRoom.Pickups;
        #endregion

        #region Constructor(s)
        public PickupManager(Game1 gm)
        {
            _gm = gm;
        }
        #endregion

        #region Method(s)


        /// <summary>
        /// Updates the pickups of the room the player is currently in
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            foreach (Entity.Entity p in Pickups)
            {
                p.Update(gameTime);
            }
        }

        /// <summary>
        /// Draws the pickups of the room the player is currently in
        /// </summary>
        /// <param name="sb"></param>
        public void Draw(SpriteBatch sb)
        {
            foreach (Entity.Entity p in Pickups)
            {
                p.Draw(sb);
            }
        }

        /// <summary>
        /// Creates a pickup of the specified type.
        /// </summary>
        /// <param name="tile"> Tile to place pickup on. </param>
        public void CreatePickup(Tile tile, PickupType pickupType)
        {
            CreatePickup(tile.WorldPosition, pickupType);
        }

        /// <summary>
        /// Creates a pickup of the specified type.
        /// </summary>
        /// <param name="position"> Position of new pickup </param>
        public void CreatePickup(Vector2 position, PickupType pickupType)
        {
            Entity.Entity pickupToCreate = null;
            switch (pickupType)
            {
                case PickupType.Health:
                    pickupToCreate = new Pickup_Health(
                        _gm.Content, 
                        position);
                    break;

                case PickupType.CurveCore:
                    pickupToCreate = new Pickup_Core(
                        _gm.Content,
                        position,
                        "Curve");
                    break;
            }

            //Debug.WriteLine("Created pickup");
            Pickups.Add(pickupToCreate);
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
            foreach (Entity.Entity p in Pickups)
            {
                p.DrawGizmos();
            }

            return;
        }

        #endregion
    }
}
