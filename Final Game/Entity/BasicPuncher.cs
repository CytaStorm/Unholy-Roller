using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
    public class BasicPuncher : Enemy
    {
        // Constructors
        public BasicPuncher(Game1 gm, Vector2 position) 
            : base(gm, position)
        {
            // Set Enemy Image
            Texture2D puncherSpritesheet = 
                gm.Content.Load<Texture2D>("Sprites/BasicEnemy");

            Image = new Sprite(
                puncherSpritesheet,
                new Rectangle(
                    0, 0,
                    120, 140),
                new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    (int)(Game1.TileSize * 1.5f),
                    (int)(Game1.TileSize * 1.5f *
                    140 / 120)));

            // Position
            WorldPosition = position;

            // Hitbox
            Hitbox = new Rectangle(
                (int)WorldPosition.X + Image.DestinationRect.Width / 2 - 50,
                (int)WorldPosition.Y + Image.DestinationRect.Height - 100,
                100,
                100);

            // Default Speed
            Speed = 5f;

            // Set Vitality
            MaxHealth = 3;
            CurHealth = MaxHealth;
            InvDuration = 0.5;
            InvTimer = InvDuration;

            // Attacking
            _attackForce = 15f;
            _attackDuration = 0.2;
            _attackDurationTimer = 0.0;
            _attackRadius = Game1.TileSize;
            _attackRange = Game1.TileSize;
            _attackWindupDuration = 0.25;
            _attackWindupTimer = _attackWindupDuration;

            _attackCooldown = 0.8;
            _attackCooldownTimer = 0.0;

            _gloveSpriteSheet = gm.Content.Load<Texture2D>("Sprites/PinPunchSpritesheet");

            // Set type
            Type = EntityType.Enemy;

            // Set state
            _chaseRange = Game1.TileSize * 5;
            _aggroRange = Game1.TileSize * 3;

            ActionState = EnemyState.Chase;

            // Animation
            _gloveFrameWidth = _gloveSpriteSheet.Width / 3;
            _walkAnimSecondsPerFrame = 0.12;
        }

        // Methods

        public override void Update(GameTime gameTime)
        {
            TickInvincibility(gameTime);

            TickKnockout(gameTime);

            //CollisionChecker.CheckTilemapCollision(this, Game1.TEST_ROOM.Floor);

            Vector2 distanceFromPlayer = Game1.Player.CenterPosition - CenterPosition;
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

                    // Apply attack cooldown
                    if (_attackCooldownTimer > 0)
                    {
                        _attackCooldownTimer -=
                            gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
                    }
                    else
                    {
                        // Choose a direction to attack in
                        if (!_attackDirChosen)
                        {
                            // Get direction from self to player
                            _attackDirection = Game1.Player.CenterPosition - CenterPosition;

                            // Apply attack range
                            _attackDirection.Normalize();
                            _attackDirection *= _attackRange;

                            _attackDirChosen = true;
                        }

                        // Charge attack
                        _attackWindupTimer -=
                            gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

                        // If not currently attacking or winding up
                        // Use attack 
                        if (_attackDurationTimer <= 0 && _attackWindupTimer <= 0)
                            _attackDurationTimer = _attackDuration;

                        // Currently Attacking
                        if (_attackDurationTimer > 0)
                        {
                            Attack();
                            _attackDurationTimer -=
                                gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

                            if (_attackDurationTimer <= 0)
                            {
                                EndAttack(false);
                            }
                        }
                    }

                    break;
            }

            CheckEnemyCollisions();

            CollisionChecker.CheckTilemapCollision(this, Game1.TestLevel.CurrentRoom.RoomFloor);

            Move(Velocity * Player.BulletTimeMultiplier);

            // Update animations

            UpdateWalkAnimation(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw Enemy relative to the player
            Vector2 distFromPlayer = WorldPosition - Game1.Player.WorldPosition;
            Vector2 screenPos = Game1.Player.ScreenPosition + distFromPlayer;

            // Only draw enemy if they are on screen
            Rectangle screenHit = new Rectangle(
                (int)screenPos.X,
                (int)screenPos.Y,
                Image.DestinationRect.Width,
                Image.DestinationRect.Height);

            bool onScreen = screenHit.Intersects(Game1.ScreenBounds);

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
                        if (!IsKO)
                            Image.Draw(spriteBatch, screenPos);
                        else
                        {
                            DrawKoed(spriteBatch, screenPos);
                        }

                        break;

                    case EnemyState.Chase:
                        DrawWalking(spriteBatch, screenPos);
                        break;

                    case EnemyState.Attack:
                        if (_attackWindupTimer < _attackWindupDuration && _attackDurationTimer <= 0.0)
                            Image.TintColor = Color.Orange;
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

        #region Attack Methods

        protected override void TargetPlayer()
        {
            // Get direction from self to player
            Point eMinusP = Game1.Player.Hitbox.Center - Hitbox.Center;
            Vector2 directionToPlayer = new Vector2(eMinusP.X, eMinusP.Y);

            // Aim enemy toward player at their speed
            directionToPlayer.Normalize();
            directionToPlayer *= Speed;

            Vector2 positionAfterMoving = WorldPosition + directionToPlayer;

            // Stop if get too close to another enemy
            bool shouldStop = false;
            float minDistanceFromEnemies = Game1.TileSize * 3;
            foreach (Enemy e in Game1.EManager.Enemies)
            {
                Vector2 distFromEnemyAfterMoving = (e.WorldPosition - positionAfterMoving);
                if (e != this &&
                    distFromEnemyAfterMoving.LengthSquared() <= minDistanceFromEnemies * minDistanceFromEnemies)
                {
                    shouldStop = true;
                    break;
                }
            }

            if (shouldStop)
            {
                Velocity = Vector2.Zero;
            }
            else
            {
                Velocity = directionToPlayer;
            }
        }

        protected override void Attack()
        {
            Vector2 directionToPlayer = _attackDirection;

            // Cast the damage box
            Rectangle damageBox = new Rectangle(
                (int)(CenterPosition.X + directionToPlayer.X - _attackRadius / 2),
                (int)(CenterPosition.Y + directionToPlayer.Y - _attackRadius / 2),
                (int)_attackRadius,
                (int)_attackRadius);

            // Store damage box
            _attackHitbox = damageBox;

            // Check if player is in damage box
            bool hitPlayerDir = damageBox.Intersects(Game1.Player.Hitbox);
            //CollisionChecker.CheckEntityCollision(damageBox, Game1.Player);

            if (hitPlayerDir)
            {
                directionToPlayer.Normalize();
                directionToPlayer *= _attackForce;

                Game1.Player.TakeDamage(1);

                // Keep player from ricocheting endlessly
                // while in attack area
                if (!_attackLandedOnce)
                {
                    Game1.Player.Ricochet(directionToPlayer);
                    _attackLandedOnce = true;
                }
            }

            return;
        }

        protected override void EndAttack(bool stoppedEarly)
        {
            // Reset windup
            _attackWindupTimer = _attackWindupDuration;

            if (stoppedEarly)
                _attackCooldown = 0;
            else
                _attackCooldownTimer = _attackCooldown;

            // Enemy is not attacking
            _attackDurationTimer = 0;

            // No new attack has happened
            _attackLandedOnce = false;
            _attackDirChosen = false;
        }

        #endregion

        #region Drawing Helper Methods

        private void UpdateWalkAnimation(GameTime gameTime)
        {
            // Add to the time counter (need TOTALSECONDS here)
            _walkAnimTimeCounter += gameTime.ElapsedGameTime.TotalSeconds 
                * Player.BulletTimeMultiplier;

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
                        1f);
                    break;

                case 4:
                    Image.Draw(sb, screenPos);
                    break;
            }
        }

        private void DrawAttacking(SpriteBatch sb, Vector2 screenPos, Vector2 distFromPlayer)
        {
            // Get vector pointing away from the player
            // with a length of attack radius
            //Vector2 windupPosShift = distFromPlayer;
            //windupPosShift.Normalize();
            //windupPosShift *= -_attackRange;

            Vector2 windupPosShift = _attackDirection;

            // Draw Attack Windup
            if (_attackWindupTimer < _attackWindupDuration && _attackDurationTimer <= 0d)
            {
                Vector2 screenWindupPos = screenPos - windupPosShift;

                Rectangle drawnWindupHit = new Rectangle(
                    (int)screenWindupPos.X,
                    (int)screenWindupPos.Y,
                    _attackHitbox.Width,
                    _attackHitbox.Height);

                sb.Draw(
                    _gloveSpriteSheet,
                    drawnWindupHit,
                    new Rectangle(0, 0, _gloveFrameWidth, _gloveFrameWidth),
                    Color.White);
            }

            // Draw actively attacking
            if (_attackDurationTimer > 0)
            {

                // Rotate fist so knuckles face player
                float dirAngle = MathF.Atan2(_attackDirection.Y, _attackDirection.X);

                Rectangle attackSourceRect = new Rectangle(
                    _gloveFrameWidth * 2,
                    0,
                    _gloveFrameWidth,
                    _gloveFrameWidth);

                Vector2 centerScreenPos =
                    screenPos + (CenterPosition - WorldPosition);

                sb.Draw(
                    _gloveSpriteSheet,
                    centerScreenPos + _attackDirection * 3.5f,
                    attackSourceRect,
                    Color.White,
                    dirAngle,
                    new Vector2(
                        attackSourceRect.Center.X,
                        attackSourceRect.Center.Y),
                    1f,
                    SpriteEffects.None,
                    0f);
            }
        }

        #endregion
    }
}
