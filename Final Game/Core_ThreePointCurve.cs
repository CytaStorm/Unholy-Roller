using Final_Game.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
    public class Core_ThreePointCurve : Core
    {
        #region Fields

        private float _curveCompletion = 1f;
        private float _speedModifier = 2f;
        private Vector2 nextCurvePoint;

        private Sprite _trajectoryPointImage;

        #endregion

        #region Properties
        public Vector2 CurvePosOne { get; private set; }
        public Vector2 CurvePosTwo { get; private set; }
        public Vector2 CurvePosThree { get; private set; }
        public override bool IsCurving => _curveCompletion < 1f;

        #endregion

        #region Constructors

        public Core_ThreePointCurve(ContentManager content) : base (content)
        {
            UsesCurve = true;

            Texture2D iconTexture = content.Load<Texture2D>("Sprites/CurveCoreIcon");
            Icon = new Sprite(
                iconTexture,
                iconTexture.Bounds,
                new Rectangle(
                    0, 0,
                    Game1.TileSize * 3 / 2, Game1.TileSize * 3 / 2));
            Icon.ObeyCamera = false;

            Name = "Curve";
        }

        #endregion

        public override void Update(GameTime gameTime)
        {
            if (!IsCurving) return;

            CalculateVelocity(gameTime);

            base.Update(gameTime);
        }

        public override void DrawTrajectoryHint(SpriteBatch sb)
        {
            return;
        }

        public override void DrawTrajectoryHint()
        {
            if (!Game1.Player.Controllable ||
               (!Game1.Player.LaunchPrimed && !Game1.Player.CurCore.IsCurving)) return;

            int numPoints = 10;
            float curveDivision = 1f / numPoints;


            for (int i = 0; i < numPoints; i++)
            {
                float curveCompletion = curveDivision * i;

                Vector2 curvePointPos =
                    MathF.Pow(1 - curveCompletion, 2) * CurvePosOne +
                    2 * (1 - curveCompletion) * curveCompletion * CurvePosTwo +
                    MathF.Pow(curveCompletion, 2) * CurvePosThree;

                ShapeBatch.Circle(
                    curvePointPos + Game1.MainCamera.WorldToScreenOffset,
                    25f * Game1.MainCamera.Zoom,
                    Color.White * 0.4f);
            }

            ShapeBatch.Circle(
                    CurvePosThree + Game1.MainCamera.WorldToScreenOffset,
                    35f * Game1.MainCamera.Zoom,
                    Color.White * 0.8f);
        }

        public override void CalculateTrajectory()
        {
            // Calculate curve
            CurvePosOne = Game1.Player.CenterPosition;

            CurvePosThree = Game1.CurMouse.Position.ToVector2() - Game1.MainCamera.WorldToScreenOffset;

            Vector2 threeMinusOne = (CurvePosThree - CurvePosOne);

            float destinationAngle = MathF.Atan2(threeMinusOne.Y, threeMinusOne.X);

            Vector2 perpNorm = threeMinusOne;
            perpNorm.Normalize();

            perpNorm = new Vector2(perpNorm.Y, -perpNorm.X);

            Vector2 midPoint = CurvePosOne + threeMinusOne / 2;

            // Quad III and IV Smile
            if ((destinationAngle >= 0 && destinationAngle < MathF.PI / 2) ||
                (destinationAngle < -MathF.PI / 2 && destinationAngle >= -MathF.PI))
            {
                CurvePosTwo = midPoint + perpNorm * 200f;
            }
            // Quad I and II Frown
            else if ((destinationAngle >= MathF.PI / 2 && destinationAngle < MathF.PI) ||
                (destinationAngle < 0 && destinationAngle >= -MathF.PI / 2))
            {
                CurvePosTwo = midPoint - perpNorm * 200f;
            }

        }

        /// <summary>
        /// Calculates the velocity to the next curve point in the path
        /// </summary>
        /// <param name="gameTime"></param>
        public override void CalculateVelocity(GameTime gameTime)
        {
            if (_curveCompletion >= 1f) return;

            // 3-Point Bezier Curve:
            // P = (1−t)^2P1 + 2(1−t)tP2 + t^2P3
            nextCurvePoint =
                MathF.Pow(1 - _curveCompletion, 2) * CurvePosOne +
                2 * (1 - _curveCompletion) * _curveCompletion * CurvePosTwo +
                MathF.Pow(_curveCompletion, 2) * CurvePosThree;

            Velocity = nextCurvePoint - Game1.Player.CenterPosition;

            // Adjust velocity at beginning of curve
            if (_curveCompletion == 0)
            {
                Velocity = nextCurvePoint - Game1.Player.CenterPosition;
            }

            _curveCompletion +=
                (float)(gameTime.ElapsedGameTime.TotalSeconds * _speedModifier
                * Player.BulletTimeMultiplier);

            // Adjust velocity at end of curve
            if (_curveCompletion >= 1f)
            {
                Velocity = nextCurvePoint - Game1.Player.CenterPosition;
                Velocity /= Velocity.Length();
                Velocity *= Game1.Player.Speed;
            }


        }

        public override void Launch(GameTime gameTime)
        {
            // Start curving
            _curveCompletion = 0f;
            
            // Calculate trajectory
            CalculateTrajectory();
        }

        public override void StopCurving()
        {
            if (!IsCurving) return;

            _curveCompletion = 1f;

            Velocity = nextCurvePoint - Game1.Player.CenterPosition;
            Velocity *= Game1.Player.Speed / Velocity.Length();
        }
    }
}
