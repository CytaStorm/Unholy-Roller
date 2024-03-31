using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
    public class Camera : IMovable
    {
        #region Properties

        public Vector2 WorldPosition { get; private set; }
        public Vector2 ScreenPosition { get; private set; }
        public Vector2 WorldToScreenOffset { get; private set; }


        public Vector2 Velocity { get; private set; }
        public Vector2 Acceleration { get; private set; }
        public float Speed { get; private set; }

        public float Zoom { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new camera at the specified world-space
        /// position that uses the specified zoom
        /// </summary>
        /// <param name="worldPosition"> position in world-space </param>
        /// <param name="zoomPercent"> zoom percent (> 0.0) </param>
        public Camera(Vector2 worldPosition, float zoomPercent)
        {
            WorldPosition = worldPosition;

            ScreenPosition = Game1.ScreenCenter;

            WorldToScreenOffset = ScreenPosition - WorldPosition;

            if (zoomPercent < 0.1f)
                throw new Exception("Camera zoom cannot be less than 0.1");

            Zoom = zoomPercent;
        }
        #endregion

        #region Methods
        public void Update(GameTime gameTime)
        {
            FollowPlayer(gameTime);

            WorldToScreenOffset = ScreenPosition - WorldPosition;

            Zoom = 20 / Game1.Player.Velocity.Length();

            Zoom = MathHelper.Clamp(Zoom, 0.6f, 1f);
        }

        public void Move(Vector2 distance)
        {
            return;
        }

        public void FollowPlayer(GameTime gameTime)
        {
            WorldPosition = Game1.Player.WorldPosition;

            //if (Game1.IsMouseButtonPressed(1))
            //{
            //    WorldPosition =
            //        Vector2.Lerp(WorldPosition, Game1.Player.WorldPosition,
            //        (float)gameTime.ElapsedGameTime.TotalSeconds * 20);
            //}
            //else
            //{
            //    WorldPosition =
            //        Vector2.Lerp(WorldPosition, Game1.Player.WorldPosition,
            //        (float)gameTime.ElapsedGameTime.TotalSeconds * 10);
            //}
        }

        #endregion

        /// <summary>
        /// Gets specified coordinate's position in an orthographic perspective 
        /// that's vanishing point lies in the center of the screen
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        public Vector2 GetPerspectivePosition(Vector2 screenPosition)
        {
            return screenPosition * Zoom + ScreenPosition * (1 - Zoom);
        }
    }
}
