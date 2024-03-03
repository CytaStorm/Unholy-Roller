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
using System.Diagnostics;

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
        public const int DEFAULT_SPRITE_HEIGHT = 140;

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
        private Rectangle _attackHitbox;

        private float _chaseRange;
        private float _aggroRange;

        private Texture2D _gloveSpriteSheet;
        private int _gloveFrameWidth;

        // Animation
        private double _walkAnimTimeCounter;
        private double _walkAnimSecondsPerFrame = 0.12;
        private int _walkAnimCurrentFrame;

        bool _hitPlayer;

        // Properties
        public bool IsKO { get => _koTimer > 0; }
        public EnemyState ActionState { get; private set; }

        // Constructors

        public Enemy(Texture2D spriteSheet, Texture2D gloveSprites, Vector2 position, GraphicsDeviceManager gdManager)
        {
            // Set Enemy Image
            Image = new Sprite(spriteSheet, 
                new Rectangle(
                    DEFAULT_SPRITE_X, 
                    DEFAULT_SPRITE_Y, 
                    DEFAULT_SPRITE_WIDTH, 
                    DEFAULT_SPRITE_HEIGHT),
                new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    (int)(Game1.TILESIZE * 1.5f),
                    (int)(Game1.TILESIZE * 1.5f * 
                    DEFAULT_SPRITE_HEIGHT / DEFAULT_SPRITE_WIDTH)));


            // Position
            WorldPosition = position;

            // Hitbox
            Hitbox = new Rectangle(
                (int)WorldPosition.X + Image.DestinationRect.Width / 2 - 50, 
                (int)WorldPosition.Y + Image.DestinationRect.Height - 100, 
                100, 
                100);

            // Default Speed
            _speed = 5f;

            // Set Vitality
            MaxHealth = 5;
            CurHealth = MaxHealth;
            _iDuration = 0.5;
            _iTimer = _iDuration;

            // Attacking
            _attackForce = 15f;
            _attackDuration = 0.1d;
            _attackDurationTimer = 0d;
            _attackRadius = Game1.TILESIZE;
            _attackRange = Game1.TILESIZE;
            _attackDelay = 0.25d;
            _attackDelayTimer = _attackDelay;

            _gloveSpriteSheet = gloveSprites;
            
            // Set type
            Type = EntityType.Enemy;

            // Set state
            _chaseRange = Game1.TILESIZE * 4;
            _aggroRange = Game1.TILESIZE * 2;

            ActionState = EnemyState.Chase;

            // Check if on screen
            _gdManager = gdManager;

            // Animation
            _gloveFrameWidth = _gloveSpriteSheet.Width / 3;
        }


        // Methods
        public override void Update(GameTime gameTime)
        {
            TickInvincibility(gameTime);

            TickKnockout();

            //CollisionChecker.CheckTilemapCollision(this, Game1.TEST_ROOM.Floor);

            Vector2 distanceFromPlayer = Game1.Player1.CenterPosition - CenterPosition;
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
                    _attackDelayTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

                    // If not currently attacking or winding up
                    // Use attack 
                    if (_attackDurationTimer <= 0 && _attackDelayTimer <= 0) 
                        _attackDurationTimer = _attackDuration;

                    // Currently Attacking
                    if (_attackDurationTimer > 0)
                    {
                        Attack();
                        _attackDurationTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

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

            Move(Velocity * Player.BulletTimeMultiplier);

            // Update animations
     
            UpdateWalkAnimation(gameTime);
        }

        private void UpdateWalkAnimation(GameTime gameTime)
        {
            // Add to the time counter (need TOTALSECONDS here)
            _walkAnimTimeCounter += gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

            // Has enough time gone by to actually flip frames?
            if (_walkAnimTimeCounter >= _walkAnimSecondsPerFrame)
            {
                // Update the frame and wrap
                _walkAnimCurrentFrame++;
                if (_walkAnimCurrentFrame > 4) _walkAnimCurrentFrame = 1;

                // Remove one "frame" worth of time
                _walkAnimTimeCounter -= _walkAnimSecondsPerFrame;
            }
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
                _iTimer = _iDuration;

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
                _iTimer = _iDuration;
            }
            
        }

        public void Attack()
        {
            // Get direction from self to player
            Vector2 directionToPlayer = Game1.Player1.CenterPosition - CenterPosition;

            // Apply attack range
            directionToPlayer.Normalize();
            directionToPlayer *= _attackRange;


            // Cast the damage box
            Rectangle damageBox = new Rectangle(
                (int)(CenterPosition.X + directionToPlayer.X - _attackRadius/2),
                (int)(CenterPosition.Y + directionToPlayer.Y - _attackRadius/2),
                (int)_attackRadius,
                (int)_attackRadius);

            // Store damage box
            _attackHitbox = damageBox;

            // Check if player is in damage box
            bool hitPlayerDir = damageBox.Intersects(Game1.Player1.Hitbox);
                //CollisionChecker.CheckEntityCollision(damageBox, Game1.Player1);

            if (hitPlayerDir)
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
                // Stop attacking if player moves out of range
                _attackDurationTimer = 0;
            }
            else if (playerDist < _chaseRange && playerDist > _aggroRange)
            {
                ActionState = EnemyState.Chase;

                _attackDelayTimer = _attackDelay;
                // Stop attacking if player moves out of range
                _attackDurationTimer = 0;
            }
            else if (playerDist < _aggroRange)
            {
                ActionState = EnemyState.Attack;
            }
        }

        private void DrawWalking(SpriteBatch sb, Vector2 screenPos)
        {

            switch (_walkAnimCurrentFrame)
            {
                case 1:
                    Vector2 origin = new Vector2(
                                Image.SourceRect.X,
                                Image.SourceRect.Center.Y - 15);


                    sb.Draw(
                        Image.Texture,
                        screenPos,
                        Image.SourceRect,
                        Color.White,
                        MathF.PI / 6,
                        origin,
                        (float)Image.DestinationRect.Width / Image.SourceRect.Width,
                        SpriteEffects.None,
                        1f
                        );
                    break;

                case 2:
                    Image.Draw(sb, screenPos);
                    break;

                case 3:

  
                    sb.Draw(
                        Image.Texture,
                        screenPos,
                        Image.SourceRect,
                        Color.White,
                        MathF.PI / 6f * -1,
                        new Vector2(
                                Image.SourceRect.Center.X,
                                Image.SourceRect.Y),
                        (float)Image.DestinationRect.Width / Image.SourceRect.Width,
                        SpriteEffects.None,
                        1f
                        );
                    break;

                case 4:
                    Image.Draw(sb, screenPos);
                    break;
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
                Image.DestinationRect.Width,
                Image.DestinationRect.Height);

            bool onScreen = screenBounds.Right >= screenHit.X &&
               screenBounds.X <= screenHit.Right &&
               screenBounds.Bottom >= screenHit.Y &&
               screenBounds.Y <= screenHit.Bottom;

            if (onScreen)
            {
                if (_hitPlayer && !IsKO)
                {
                    Image.TintColor = Color.Red;
                    _hitPlayer = false;
                }
                else if (IsKO)
                {
                    Image.TintColor = Color.Blue;
                }
                else
                {
                    Image.TintColor = Color.White;
                }

                //Image.Draw(spriteBatch, screenPos);

                switch (ActionState)
                {
                    case EnemyState.Idle:
                        Image.Draw(spriteBatch, screenPos);
                        break;

                    case EnemyState.Chase:
                        DrawWalking(spriteBatch, screenPos);
                        break;

                    case EnemyState.Attack:
                        Image.Draw(spriteBatch, screenPos);
                        
                        DrawAttacking(spriteBatch, screenPos, distFromPlayer);
                        break;
                }

                
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

        private void DrawAttacking(SpriteBatch sb, Vector2 screenPos, Vector2 distFromPlayer)
        {
            // Draw Attack Windup
            if (_attackDelayTimer < _attackDelay && _attackDurationTimer <= 0d)
            {
                // Get vector pointing away from the player
                // with a length of attack radius
                Vector2 windupPosShift = distFromPlayer;
                windupPosShift.Normalize();
                windupPosShift *= -_attackRange;

                Rectangle drawnWindupHit = new Rectangle(
                    (int)(screenPos.X - windupPosShift.X),
                    (int)(screenPos.Y - windupPosShift.Y),
                    _attackHitbox.Width,
                    _attackHitbox.Height);

                sb.Draw(
                    _gloveSpriteSheet,
                    drawnWindupHit,
                    new Rectangle(0, 0, _gloveFrameWidth, _gloveFrameWidth),
                    Color.Red);

                Image.TintColor = Color.Orange;
            }

            // Draw actively attacking
            if (_attackDurationTimer > 0)
            {
                int atkHitDistX = (int)(_attackHitbox.X - WorldPosition.X);
                int atkHitDistY = (int)(_attackHitbox.Y - WorldPosition.Y);

                Rectangle drawnAttackHit = new Rectangle(
                    (int)(screenPos.X + atkHitDistX),
                    (int)(screenPos.Y + atkHitDistY),
                    _attackHitbox.Width,
                    _attackHitbox.Height);

                sb.Draw(
                    _gloveSpriteSheet,
                    drawnAttackHit,
                    new Rectangle(_gloveFrameWidth * 2, 0, _gloveFrameWidth, _gloveFrameWidth),
                    Color.White);

            }
        }

        public void DrawGizmos()
        {
            // Draw Enemy relative to the player
            Vector2 distFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = Game1.Player1.ScreenPosition + distFromPlayer;

            // Show attack windup 
            float attackReadiness = (float)((_attackDelay - _attackDelayTimer) / _attackDelay);

            float boxFill = Image.DestinationRect.Height * attackReadiness;
            int boxFillInt = (int)MathF.Round(boxFill);

            Rectangle readyBox = new Rectangle(
                (int)screenPos.X,
                (int)screenPos.Y + Image.DestinationRect.Height - boxFillInt,
                Image.DestinationRect.Width,
                boxFillInt);

            Color fadedYellow = new Color(0.5f, 0.5f, 0.5f, 0.4f);

            ShapeBatch.Box(readyBox, fadedYellow);

            // Draw Hitbox
            int hitDistX = (int)WorldPosition.X - Hitbox.X;
            int hitDistY = (int)WorldPosition.Y - Hitbox.Y;

            Rectangle drawnHit = new Rectangle(
                (int)(screenPos.X - hitDistX),
                (int)(screenPos.Y - hitDistY),
                Hitbox.Width,
                Hitbox.Height);

            Color fadedRed = new Color(1f, 0f, 0f, 0.4f);

            ShapeBatch.Box(drawnHit, fadedRed);

            // Attacking
            if (_attackDurationTimer > 0)
            {
                // Draw Hitbox
                int atkHitDistX = (int)WorldPosition.X - _attackHitbox.X;
                int atkHitDistY = (int)WorldPosition.Y - _attackHitbox.Y;

                Rectangle drawnAttackHit = new Rectangle(
                    (int)(screenPos.X - atkHitDistX),
                    (int)(screenPos.Y - atkHitDistY),
                    _attackHitbox.Width,
                    _attackHitbox.Height);

                Color fadedGreen = new Color(0f, 0f, 1f, 0.4f);

                ShapeBatch.Box(drawnAttackHit, fadedGreen);
            }
        }
    }
}
