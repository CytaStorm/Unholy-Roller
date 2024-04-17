using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
    public class Dummy : Enemy
    {
        public Dummy(Game1 gm, Vector2 position) : base(gm, position)
        {
            Texture2D dummyImage = gm.Content.Load<Texture2D>("Sprites/BasicEnemy");
            Image = new Sprite(
                dummyImage,
                dummyImage.Bounds,
                new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    (int)(Game1.TileSize * 1.5f),
                    (int)(Game1.TileSize * 1.5f *
                    140 / 120)));

            Hitbox = new Rectangle(
                (int)WorldPosition.X + Image.DestinationRect.Width / 2 - 50,
                (int)WorldPosition.Y + Image.DestinationRect.Height - 100,
                100,
                100);

            MaxHealth = 3;
            CurHealth = MaxHealth;

            InvDuration = 0.5;

            // Extra Aesthetics
            Texture2D _koStarSpritesheet =
                gm.Content.Load<Texture2D>("Sprites/KO_StarsSpritesheet");
            _knockoutStars = new Sprite(
                _koStarSpritesheet,
                new Rectangle(
                    0, 0,
                    _koStarSpritesheet.Bounds.Width / 4,
                    _koStarSpritesheet.Bounds.Height),
                new Rectangle(
                    0, 0,
                    (int)(Game1.TileSize * 1.5),
                    (int)(Game1.TileSize * 1.5)));

            _koStarAnimDuration = 0.5;
        }

        public override void Update(GameTime gameTime)
        {
            TickKnockout(gameTime);
            TickInvincibility(gameTime);

            DetermineState(0f);

            UpdateKOAnimation(gameTime);
        }
        
        public override void Draw(SpriteBatch sb)
        {
            Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;

            switch (ActionState)
            {
                case EnemyState.KO:
                    Vector2 drawPos =
                                screenPos +
                                new Vector2(Image.DestinationRect.Width, 0f);

                    Image.Draw(
                        sb,
                        drawPos,
                        MathHelper.PiOver2,
                        Vector2.Zero);

                    Vector2 starDrawPos =
                        screenPos -
                        new Vector2(0f, _knockoutStars.DestinationRect.Height / 2);

                    _knockoutStars.Draw(
                        sb,
                        starDrawPos,
                        0f,
                        Vector2.Zero);

                    break;

                case EnemyState.Idle:
                    Image.Draw(sb, screenPos, 0f, Vector2.Zero);
                    break;
            }

        }


        protected override void Attack()
        {
            throw new NotImplementedException();
        }

        protected override void EndAttack(bool stoppedEarly)
        {
            throw new NotImplementedException();
        }

        protected override void DetermineState(float playerDist)
        {
            if (IsKO)
            {
                ActionState = EnemyState.KO;
                CollisionOn = false;
            }
            else
            {
                ActionState = EnemyState.Idle;
                CollisionOn = true;
            }
        }

        private void UpdateKOAnimation(GameTime gameTime)
        {
            int numKoStars = 4;

            // Current frame is (passedTime / (seconds per frame))
            int curFrame =
                (int)(_koStarAnimTimeCounter /
                (_koStarAnimDuration / (numKoStars - 1)));

            // Get corresponding image
            _knockoutStars.SourceRect = new Rectangle(
                _knockoutStars.SourceRect.Width * curFrame,
                0,
                _knockoutStars.SourceRect.Width,
                _knockoutStars.SourceRect.Height);

            // Move animation forward
            _koStarAnimTimeCounter +=
                gameTime.ElapsedGameTime.TotalSeconds *
                Player.BulletTimeMultiplier;

            // Reset animation once it reaches or exceeds its duration
            if (_koStarAnimTimeCounter >= _koStarAnimDuration)
                _koStarAnimTimeCounter -= _koStarAnimDuration;
        }
    }
}
