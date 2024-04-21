using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Final_Game.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Runtime.CompilerServices;

namespace Final_Game.Managers
{
    public enum Cutscene
    {
        Tutorial,
        GameOver,
        None
    }

    public class CutsceneManager
    {
        private Game1 gm;

        public int PhaseNum { get; private set; }
        public Cutscene Scene { get; private set; } = Cutscene.None;

        #region Tutorial Vars

        private bool _hasPrimedLaunch;
        private bool _hasLaunched;
        private bool _hasRedirected;
        private bool _hasUsedBrake;
        private bool _hasWalked;

        private string _launchInstructions;
        private string _redirectInstructions;
        private string _brakeInstructions;
        private string _walkInstructions;

        private string _tutorialEndMessage;

        private bool _shieldUsed;

        private Sprite _mouseImage;
        private Texture2D _arrowTexture;
        private Texture2D _wasdTexture;

        #endregion

        #region GameOver Vars

        private Texture2D _blankPanel;

        private double _backgroundFadeDuration;
        private double _backgroundFadeTimeCounter;

        #endregion

        #region General Fields
        // Text scrolling
        private string _curText;
        private int _writeLength;

        private double _waitToIncrementCharTime = 0.02;
        private double _incrementCharTimeCounter;

        // Phase transfer
        private double _phaseTransferDuration = 5;
        private double _phaseTransferTimer;

        // UI control
        private bool _isPausable;
        #endregion

        // Constructors
        public CutsceneManager(Game1 gm)
        {
            this.gm = gm;

            _blankPanel = gm.Content.Load<Texture2D>("BlankPanel");

            // Write tutorial
            _walkInstructions = "You can walk if you're not rolling";

            _launchInstructions = "You will only be able to roll through enemies if you launch.";

            _redirectInstructions =
                UI.GetWrappedText("You can redirect a few times per launch", 60);

            _brakeInstructions =
                UI.GetWrappedText("Slow down significantly to regain redirects.", 60);

            _tutorialEndMessage =
                UI.GetWrappedText("You've finished the tutorial. " +
                "NOW GO EVISCERATE THOSE PINHEADS!!! " +
                "Press 'Esc'", 60);

            Texture2D mouseTexture = gm.Content.Load<Texture2D>("UI Images/MousePressSpritesheet");
            _mouseImage = new Sprite(
                mouseTexture,
                new Rectangle(0, 0, mouseTexture.Width / 2, mouseTexture.Height),
                new Rectangle(0, 0, 200, 200));
            _mouseImage.Columns = 2;
            _mouseImage.FrameBounds = _mouseImage.SourceRect;
            _mouseImage.ObeyCamera = false;

            _wasdTexture = gm.Content.Load<Texture2D>("UI Images/WASD_Keys");
            _arrowTexture = gm.Content.Load<Texture2D>("UI Images/Arrow_Keys");

            _isPausable = true;
        }

        // Methods
        public void Update(GameTime gameTime)
        {
            // Add text scroll
            if (_curText != null &&
                _writeLength < _curText.Length &&
                _incrementCharTimeCounter < _waitToIncrementCharTime)
            {
                _incrementCharTimeCounter += gameTime.ElapsedGameTime.TotalSeconds;

                if (_incrementCharTimeCounter >= _waitToIncrementCharTime)
                {
                    // Increment character
                    _writeLength++;

                    // Reset counter
                    _incrementCharTimeCounter -= _waitToIncrementCharTime;
                }
            }

            // Transfer phases
            if (_phaseTransferTimer > 0)
            {
                _phaseTransferTimer -= gameTime.ElapsedGameTime.TotalSeconds
                    * Player.BulletTimeMultiplier;

                if (_phaseTransferTimer <= 0) OnPhaseTransfer();
            }

            // Pause game if not paused and vice versa
            if (_isPausable && Game1.SingleKeyPress(Keys.Escape))
                gm.PauseGame(gm.State != GameState.Pause);

            // Update current cutscene
            switch (Scene)
            {
                case Cutscene.Tutorial:
                    RunTutorialCutscene(gameTime);

                    break;

                case Cutscene.GameOver:
                    RunGameOverCutscene(gameTime);

                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            // Draw current cutscene
            switch (Scene)
            {
                case Cutscene.Tutorial:
                    DrawTutorialCutscene(sb);

                    break;

                case Cutscene.GameOver:
                    DrawGameOverCutscene(sb);
                    break;
            }
        }

        public void DrawSimpleShapes()
        {
            switch (Scene)
            {
                case Cutscene.Tutorial:

                    if (PhaseNum == 7)
                    {
                        ShapeBatch.BoxOutline(
                            new Rectangle(
                                100, 400,
                                Game1.TileSize,
                                Game1.TileSize),
                            Color.White);
                    }

                    break;
            }
        }

        public void StartCutscene(Cutscene scene)
        {
            gm.State = GameState.Cutscene;

            _isPausable = true;

            _curText = null;

            _writeLength = 0;

            _phaseTransferTimer = 0;

            switch (scene)
            {
                case Cutscene.Tutorial:

                    _hasPrimedLaunch = false;
                    _hasLaunched = false;
                    _hasRedirected = false;
                    _hasUsedBrake = false;
                    _hasWalked = false;
                    _shieldUsed = false;

                    _curText = _walkInstructions;

                    gm.CreateTutorialLevel();

                    // Lock first room doors
                    // by spawning an invisible enemy
                    Game1.EManager.Enemies.Add(new Dummy(gm, Vector2.Zero, true));

                    // Entering the dummy room will cause
                    // an immediate phase transfer
                    Game1.TutorialLevel.Map[0, 2]
                        .OnRoomEntered += OnPhaseTransfer;

                    // Entering the final room will cause
                    // an immediate phase transfer
                    Game1.TutorialLevel.Map[1, 2]
                        .OnRoomEntered += OnPhaseTransfer;

                    Scene = Cutscene.Tutorial;

                    PhaseNum = 1;
                    break;

                case Cutscene.GameOver:
                    _backgroundFadeDuration = 2;
                    _backgroundFadeTimeCounter = 0;

                    Scene = Cutscene.GameOver;

                    PhaseNum = 1;

                    _isPausable = false;
                    break;
            }
        }

        private void OnPhaseTransfer()
        {
            // Set up things for next phase
            switch (Scene)
            {
                case Cutscene.Tutorial:

                    if (PhaseNum == 1) _curText = _launchInstructions;
                    else if (PhaseNum == 2)
                    {
                        _hasPrimedLaunch = false;
                        _hasLaunched = false;

                        _curText = _redirectInstructions;
                    }
                    else if (PhaseNum == 3) _curText = _brakeInstructions;
                    else if (PhaseNum == 4)
                    {
                        _curText = "";

                        // Clear invisible enemy so doors open
                        Game1.EManager.Clear();

                        Game1.EManager.OnLastEnemyKilled += ResetText;
                    }
                    else if (PhaseNum == 5)
                    {
                        _curText =
                            "Knock all enemies down to progress.\n" +
                            "Beware they don't stay down for long.";

                        Game1.TutorialLevel.Map[0, 2]
                        .OnRoomEntered -= OnPhaseTransfer;
                    }
                    else if (PhaseNum == 6)
                    {
                        _curText =
                            "Your combo increases each time you hit an enemy.\n" +
                            "Take a punch when you smile (10 combo)";

                        Game1.TutorialLevel.Map[1, 2]
                        .OnRoomEntered -= OnPhaseTransfer;

                        Game1.EManager.OnLastEnemyKilled -= ResetText;

                        Game1.Player.OnPlayerHit += CheckShieldAbilityUsed;   
                    }
                    else if (PhaseNum == 7)
                    {
                        _curText = _tutorialEndMessage;
                    }
                    

                    _writeLength = 0;
                    _incrementCharTimeCounter = 0;
                    break;
            }

            // Move to next phase
            PhaseNum++;
        }

        public void EndCurrentScene()
        {
            switch (Scene)
            {
                case Cutscene.GameOver:
                    gm.State = GameState.GameOver;
                    break;
            }

            // End Scene
            PhaseNum = 0;
            Scene = Cutscene.None;
        }

        #region Cutscene-Specific Helper Methods

        private void RunTutorialCutscene(GameTime gameTime)
        {
            // Simulate the Play State
            Game1.Player.Update(gameTime);

            Game1.EManager.Update(gameTime);

            Game1.FXManager.Update(gameTime);

            Game1.MainCamera.Update(gameTime);

            if (PhaseNum != 7)
                Game1.TutorialLevel.CurrentRoom.Update(gameTime);

            gm.UIManager.UpdateSpeedometerShake(gameTime);

            gm.UpdatePlayCursor();

            switch (PhaseNum)
            {
                case 1:
                    // Check if player walks

                    if (Game1.Player.State == PlayerState.Walking &&
                        Game1.Player.Velocity.LengthSquared() > 0)
                    {
                        _hasWalked = true;
                    }

                    if (_hasWalked && _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }
                    break;

                case 2:
                    // Check if player launches
                    if (Game1.Player.State == PlayerState.Walking)
                    {
                        if (Game1.IsMouseButtonPressed(Game1.Player.LaunchButton))
                        {
                            _hasPrimedLaunch = true;
                        }
                    }

                    if (_hasPrimedLaunch && Game1.IsMouseButtonReleased(Game1.Player.LaunchButton))
                    {
                        _hasLaunched = true;
                    }

                    if (_hasLaunched && _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }

                    break;

                case 3:
                    // Check if player redirects
                    if (Game1.Player.State == PlayerState.Rolling)
                    {
                        if (Game1.IsMouseButtonPressed(Game1.Player.LaunchButton))
                        {
                            _hasPrimedLaunch = true;
                        }

                        if (_hasPrimedLaunch && Game1.IsMouseButtonReleased(Game1.Player.LaunchButton))
                        {
                            _hasLaunched = true;
                            _hasRedirected = true;
                        }
                    }

                    // Make sure player has used all of their redirects
                    if (_hasRedirected && 
                        Game1.Player.NumRedirects == 0 && 
                        _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }

                    break;

                case 4:
                    // Check if player brakes
                    if (Game1.Player.State == PlayerState.Rolling)
                    {
                        if (Game1.IsMouseButtonPressed(Game1.Player.BrakeButton))
                        {
                            _hasUsedBrake = true;
                        }
                    }

                    if (_hasUsedBrake && _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }

                    break;

                case 7:

                    // Ensure player uses ability before tutorial ends
                    if (Game1.EManager.Enemies.Count < 1)
                    {
                        BasicPuncher extraEnemy =
                            new BasicPuncher(gm, new Vector2());

                        extraEnemy.MoveToRoomCenter(Game1.CurrentLevel.CurrentRoom);

                        Game1.EManager.Enemies.Add(extraEnemy);
                    }

                    if (_shieldUsed && _phaseTransferTimer <= 0)
                    {
                        _phaseTransferTimer = 1;
                    }

                    break;

            }
        }
        private void DrawTutorialCutscene(SpriteBatch sb)
        {
            Game1.TutorialLevel.CurrentRoom.Draw(sb);

            Game1.Player.Draw(sb);

            Game1.FXManager.Draw(sb);

            Game1.EManager.Draw(sb);

            gm.UIManager.DrawPlayerHealth();
            gm.UIManager.DrawPlayerSpeedometer();

            if (PhaseNum >= 6)
            {
                gm.UIManager.DrawPlayerCombo();
            }

            switch (PhaseNum){
                case 1:
                    sb.Draw(
                    _wasdTexture,
                    new Rectangle(
                        Game1.ScreenBounds.Width / 2 - 300, 700,
                        200, 200),
                    Color.White);


                    sb.DrawString(
                        UI.MediumArial,
                        "OR",
                        UI.GetCenteredTextPos(
                            "OR",
                            UI.MediumArial,
                            new Vector2(Game1.ScreenCenter.X, Game1.ScreenCenter.Y + 250f)),
                        Color.White);

                    sb.Draw(
                        _arrowTexture,
                        new Rectangle(
                            Game1.ScreenBounds.Width / 2 + 100, 700,
                            200, 200),
                        Color.White);

                    break;

                case 2:
                    _mouseImage.SetSourceToFrame(Game1.Player.LaunchButton);

                    _mouseImage.Draw(sb, new Vector2(735, 700), 0f, Vector2.Zero);

                    sb.DrawString(
                        UI.MediumArial,
                        "Press to Slow Time\nRelease to Launch",
                        new Vector2(Game1.ScreenCenter.X, Game1.ScreenCenter.Y + 210),
                        Color.White);
                    break;

                case 3:
                    _mouseImage.SetSourceToFrame(Game1.Player.LaunchButton);

                    _mouseImage.Draw(sb, new Vector2(735, 700), 0f, Vector2.Zero);

                    sb.DrawString(
                        UI.MediumArial,
                        "Press to Slow Time\nRelease to Launch",
                        new Vector2(Game1.ScreenCenter.X, Game1.ScreenCenter.Y + 210),
                        Color.White);

                    break;

                case 4:
                    _mouseImage.SetSourceToFrame(Game1.Player.BrakeButton);

                    _mouseImage.Draw(sb, new Vector2(735, 700), 0f, Vector2.Zero);

                    sb.DrawString(
                        UI.MediumArial,
                        "Hold to Decelerate",
                        new Vector2(Game1.ScreenCenter.X, Game1.ScreenCenter.Y + 210),
                        Color.White);

                    break;
            }
            

            string tempText = _curText.Substring(0, _writeLength);

            // Choose where text should be drawn
            Vector2 textCenterPos =
                new Vector2(
                    Game1.ScreenCenter.X,
                    Game1.ScreenCenter.Y + 100f);

            Vector2 textMeasurements = UI.MediumArial.MeasureString(_curText);

            // Get horizontally centered text position
            Vector2 textPosition =
                new Vector2(
                    textCenterPos.X - textMeasurements.X / 2,
                    textCenterPos.Y);

            // Draw text
            sb.DrawString(
                UI.MediumArial,
                tempText,
                textPosition,
                Color.White);

            
        }

        private void RunGameOverCutscene(GameTime gameTime)
        {
            switch (PhaseNum)
            {
                case 1:
                    if (_backgroundFadeTimeCounter < _backgroundFadeDuration)
                    {
                        // Fade in a black screen
                        _backgroundFadeTimeCounter +=
                            gameTime.ElapsedGameTime.TotalSeconds;

                        Game1.Player.Update(gameTime);

                        Game1.MainCamera.Update(gameTime);

                        Game1.EManager.Update(gameTime);
                    }
                    else
                    {
                        EndCurrentScene();
                    }

                    break;
            }
        }
        private void DrawGameOverCutscene(SpriteBatch sb)
        {
            Game1.CurrentLevel.CurrentRoom.Draw(sb);

            Game1.Player.Draw(sb);

            Game1.EManager.Draw(sb);

            sb.Draw(
                _blankPanel,
                Game1.ScreenBounds,
                Color.White * (float)(_backgroundFadeTimeCounter / _backgroundFadeDuration));
        }

        private void CheckShieldAbilityUsed(int nothing)
        {
            if (Game1.Player.ComboReward)
            {
                // Shield ability was used
                _shieldUsed = true;

                // No need to check anymore
                Game1.Player.OnPlayerHit -= CheckShieldAbilityUsed;
            }
        }

        private void ResetText()
        {
            _curText = "";
            _writeLength = 0;
        }

        #endregion
    }
}
