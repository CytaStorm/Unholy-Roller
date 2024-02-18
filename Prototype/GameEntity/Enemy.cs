using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototype.MapGeneration;
using ShapeUtils;

namespace Prototype.GameEntity
{
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack
    }

    public class Enemy : Entity
    {
        // Fields
        public const int DEFAULT_SPRITE_X = 0;
        public const int DEFAULT_SPRITE_Y = 0;
        public const int DEFAULT_SPRITE_WIDTH = 120;
        public const int DEFAULT_SPRITE_HEIGHT = 120;

        private GraphicsDeviceManager _gdManager;

        private int _koTime = 180; // Frames
        private int _koTimer;

        private float _attackForce;
        private double _attackDuration;
        private double _attackDurationTimer;
        private float _attackRadius;
        private float _attackRange;
        private double _attackDelay;
        private double _attackDelayTimer;
        private bool _attackLandedOnce;

        private float _chaseRange;
        private float _aggroRange;

        bool _hitPlayer;

        // Properties
        public bool IsKO { get => _koTimer > 0; }
        public EnemyState ActionState { get; private set; }
        
        // Constructors

        public Enemy(Texture2D spriteSheet, Vector2 position, GraphicsDeviceManager gdManager)
        {
            // Set Player Image
            Image = new Sprite(spriteSheet, DEFAULT_SPRITE_X, DEFAULT_SPRITE_Y,
                DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT, new Vector2(50, 50));

            // Hitbox
            Hitbox = new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT);

            // Position
            WorldPosition = position;

            // Graphics Manager -> Screen Collision
            _gdManager = gdManager;

            // Default Speed
            _speed = 3f;

            // Set Vitality
            MaxHealth = 5;
            CurHealth = MaxHealth;
            _iFrames = 30;
            _iTimer = _iFrames;

            // Attacking
            _attackForce = 15f;
            _attackDuration = 0.1;
            _attackRadius = Game1.TILESIZE;
            _attackRange = Game1.TILESIZE;
            _attackDelay = 0.25;
            _attackDelayTimer = _attackDelay;

            // Set type
            Type = EntityType.Enemy;

            // Set state
            _chaseRange = Game1.TILESIZE * 4;
            _aggroRange = Game1.TILESIZE * 2;

            ActionState = EnemyState.Chase;
        }


        // Methods
        public override void Update(GameTime gameTime)
        {
            TickInvincibility();

            TickKnockout();

            //CollisionChecker.CheckTilemapCollision(this, Game1.TEST_ROOM.Floor);


            Point eMinusP = Game1.Player1.Hitbox.Center - Hitbox.Center;
            Vector2 distanceFromPlayer = new Vector2(eMinusP.X, eMinusP.Y);
            float playerDist = distanceFromPlayer.Length();

            DetermineState(playerDist);

            switch (ActionState)
            {
                case EnemyState.Idle:
                    Velocity = Vector2.Zero;
                    break;

                case EnemyState.Chase:
                    TargetPlayer();
                    break;

                case EnemyState.Attack:
                    Velocity = Vector2.Zero;

                    // Charge attack
                    _attackDelayTimer -= gameTime.ElapsedGameTime.TotalSeconds;

                    // If not currently attacking or winding up
                    // Use attack 
                    if (_attackDurationTimer <= 0 && _attackDelayTimer <= 0) 
                        _attackDurationTimer = _attackDuration;

                    // Currently Attacking
                    if (_attackDurationTimer > 0)
                    {
                        Attack();
                        _attackDurationTimer -= gameTime.ElapsedGameTime.TotalSeconds;

                        if (_attackDurationTimer <= 0)
                        {
                            // Reset attack delay
                            _attackDelayTimer = _attackDelay;

                            // No new attack has happened
                            _attackLandedOnce = false;
                        }
                    }

                    break;
            }

            CheckEnemyCollisions();
            
            Move();
        }

        protected void TargetPlayer()
        {
            // Get direction from self to player
            Point eMinusP = Game1.Player1.Hitbox.Center - Hitbox.Center;
            Vector2 directionToPlayer = new Vector2(eMinusP.X, eMinusP.Y);

            // Aim enemy toward player at their speed
            directionToPlayer.Normalize();
            Velocity = new Vector2(directionToPlayer.X * _speed, directionToPlayer.Y * _speed);
        }

        public void CheckEnemyCollisions()
        {
            for (int i = 0; i < Game1.EManager.Dummies.Count; i++)
            {
                Enemy curEnemy = Game1.EManager.Dummies[i];
                if (curEnemy == this) continue;

                if (CollisionChecker.CheckEntityCollision(this, curEnemy))
                {
                    Image.TintColor = Color.LightGoldenrodYellow;
                }
            }
        }

        public override void OnHitEntity(Entity entityThatWasHit, CollisionType colType,
            bool causedCollision)
        {
            switch (entityThatWasHit.Type)
            {
                case EntityType.Player:
                    _hitPlayer = true;

                    // Cancel out attack charge
                    _attackDelayTimer = _attackDelay;

                    break;

                case EntityType.Enemy:
                    Velocity = Vector2.Zero;
                    break;
            }

            base.OnHitEntity(entityThatWasHit, colType, causedCollision);
        }

        public override void OnHitTile(Tile tile, CollisionType colType)
        {
            switch (tile.Type)
            {
                case TileType.Wall:

                    if (colType == CollisionType.Horizontal)
                        Velocity = new Vector2(Velocity.X * -1, Velocity.Y);
                    else
                        Velocity = new Vector2(Velocity.X, Velocity.Y * -1);

                    break;
            }
        }

        private void TickKnockout()
        {
            if (_koTimer > 0)
            {
                _koTimer--;
            }
        }

        public override void TakeDamage(int damage)
        {
            // Take damage if not invincible
            if (_iTimer <= 0 && !IsKO)
            {
                CurHealth -= damage;

                // Temporarily become invincible
                _iTimer = _iFrames;

                // Handle low health
                if (CurHealth <= 0)
                {
                    // Enemy is temporarily knocked out
                    _koTimer = _koTime;
                }
            }
            // Keep resetting invincibility until not hit
            else if (_iTimer > 0)
            {
                _iTimer = _iFrames;
            }
            
        }

        public void Attack()
        {
            // Get direction from self to player
            Vector2 directionToPlayer = Game1.Player1.WorldPosition - WorldPosition;

            // Apply attack range
            directionToPlayer.Normalize();
            directionToPlayer *= _attackRange;

            
            // Cast the damage box
            Rectangle damageBox = new Rectangle(
                (int)(WorldPosition.X + directionToPlayer.X / 2),
                (int)(WorldPosition.Y + directionToPlayer.Y / 2),
                (int)_attackRadius,
                (int)_attackRadius);

            // Check if player is in damage box
            CollisionType hitPlayerDir = 
                CollisionChecker.CheckEntityCollision(damageBox, Game1.Player1);

            if (hitPlayerDir != CollisionType.None)
            {
                directionToPlayer.Normalize();
                directionToPlayer *= _attackForce;

                Game1.Player1.TakeDamage(1);

                // Keep player from ricocheting endlessly
                // while in attack area
                if (!_attackLandedOnce)
                {
                    Game1.Player1.Ricochet(directionToPlayer);
                    _attackLandedOnce = true;
                }
            }
        }

        public void DetermineState(float playerDist)
        {
            if (IsKO || playerDist > _chaseRange)
            {
                ActionState = EnemyState.Idle;
                _attackDelayTimer = _attackDelay;
            }
            else if (playerDist < _chaseRange && playerDist > _aggroRange)
            {
                ActionState = EnemyState.Chase;
                _attackDelayTimer = _attackDelay;
            }
            else if (playerDist < _aggroRange)
            {
                ActionState = EnemyState.Attack;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Draw Enemy relative to the player
            Vector2 distFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = Game1.Player1.ScreenPosition + distFromPlayer;

            // Only draw enemy if they are on screen
            Rectangle screenBounds = new Rectangle(
                0, 0,
                _gdManager.PreferredBackBufferWidth,
                _gdManager.PreferredBackBufferHeight);

            Rectangle screenHit = new Rectangle(
                (int)screenPos.X,
                (int)screenPos.Y,
                Hitbox.Width,
                Hitbox.Height);

            bool onScreen = screenBounds.Right >= screenHit.X &&
               screenBounds.X <= screenHit.Right &&
               screenBounds.Bottom >= screenHit.Y &&
               screenBounds.Y <= screenHit.Bottom;

            if (onScreen)
            {
                if (_hitPlayer && !IsKO)
                {
                    Image.TintColor = Color.Wheat;
                    _hitPlayer = false;
                }
                else if (IsKO)
                {
                    Image.TintColor = Color.Blue;
                }
                else
                {
                    Image.TintColor = Color.Red;
                }

                Image.Draw(spriteBatch, screenPos);
            }

            // Display current health
            //spriteBatch.DrawString(Game1.ARIAL32, $"Hp: {CurHealth}", screenPos, Color.White);

            // Display attack delay
            //spriteBatch.DrawString(
            //    Game1.ARIAL32,
            //    $"ATK_D: {_attackDelayTimer:0.00}",
            //    screenPos,
            //    Color.White);

        }

        public void DrawGizmos()
        {
            // Draw Enemy relative to the player
            Vector2 distFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = Game1.Player1.ScreenPosition + distFromPlayer;

            // Show attack windup 
            float attackReadiness = (float)(_attackDelayTimer/_attackDelay);

            float boxFill = DEFAULT_SPRITE_HEIGHT * attackReadiness;

            Rectangle readyBox = new Rectangle(
                (int)screenPos.X,
                (int)screenPos.Y + DEFAULT_SPRITE_HEIGHT - (int)MathF.Round(boxFill),
                DEFAULT_SPRITE_WIDTH,
                (int)MathF.Round(boxFill));

            Color fadedYellow = new Color(0.5f, 0.5f, 0.5f, 0.4f);

            ShapeBatch.Box(readyBox, fadedYellow);
        }
    }
}
