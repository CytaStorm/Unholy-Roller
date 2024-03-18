﻿using Microsoft.Xna.Framework;
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

namespace Final_Game
{
    public enum Cutscene
    {
        Tutorial,
        None
    }

    internal class CutsceneManager
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

            // Write tutorial
            _walkInstructions =
                UI.GetWrappedText("If your speed is zero, use W A S D to walk around", 30);

            _launchInstructions =
                UI.GetWrappedText("You will only be able to roll through enemies if you launch. " +
                "Click mouse left in any direction to launch. " +
                "Time will slow as long as you hold mouse left", 60);

            _redirectInstructions =
                UI.GetWrappedText("If you launch while rolling, you will redirect. " +
                "You get a limited number of these per launch so use them wisely", 60);

            _brakeInstructions =
                UI.GetWrappedText("Hold mouse right to rapidly decelerate. " +
                "You can use this to swiftly transition to walking.", 60);

            _tutorialEndMessage =
                UI.GetWrappedText("That's it! You've finished the tutorial. " +
                "NOW GO EVISCERATE THOSE PINHEADS!!! " +
                "Press 'Esc' to return to main menu", 60);

            _isPausable = true;
        }

        // Methods
        public void Update(GameTime gameTime)
        {
            // Add text scroll
            if (_writeLength < _curText.Length && _incrementCharTimeCounter < _waitToIncrementCharTime)
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
                gm.PauseGame(Game1.State != GameState.Pause);

            // Update current cutscene
            switch (Scene)
            {
                case Cutscene.Tutorial:
                    Game1.Player.Update(gameTime);

                    Game1.TutorialRoom.Update(gameTime);

                    RunTutorialCutscene();
                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            // Draw current cutscene
            switch (Scene)
            {
                case Cutscene.Tutorial:
                    Game1.TutorialRoom.Draw(sb);

                    Game1.Player.Draw(sb);

                    DrawTutorialCutscene(sb);
                    break;
            }
        }

        public void StartCutscene(Cutscene scene)
        {
            switch (scene)
            {
                case Cutscene.Tutorial:

                    _hasPrimedLaunch = false;
                    _hasLaunched = false;
                    _hasRedirected = false;
                    _hasUsedBrake = false;
                    _hasWalked = false;

                    _curText = _walkInstructions;

                    Scene = Cutscene.Tutorial;

                    PhaseNum = 1;
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
                    else if (PhaseNum == 4) _curText = _tutorialEndMessage;

                    _writeLength = 0;
                    _incrementCharTimeCounter = 0;
                    break;
            }

            // Move to next phase
            PhaseNum++;
        }

        public void EndCurrentScene()
        {
            // End Scene
            PhaseNum = 0;
            Scene = Cutscene.None;
        }

        #region Cutscene-Specific Helper Methods

        private void RunTutorialCutscene()
        {

            switch (PhaseNum)
            {
                case 1:
                    // Check if player walks

                    if (Game1.Player.State == PlayerState.Walking &&
                        (Game1.CurKB.IsKeyDown(Keys.W) || Game1.CurKB.IsKeyDown(Keys.A) ||
                        Game1.CurKB.IsKeyDown(Keys.S) || Game1.CurKB.IsKeyDown(Keys.D)))
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
                        if (Game1.IsMouseButtonPressed(1))
                        {
                            _hasPrimedLaunch = true;
                        }
                    }

                    if (_hasPrimedLaunch && Game1.IsMouseButtonReleased(1))
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
                        if (Game1.IsMouseButtonPressed(1))
                        {
                            _hasPrimedLaunch = true;
                        }

                        if (_hasPrimedLaunch && Game1.IsMouseButtonReleased(1))
                        {
                            _hasLaunched = true;
                            _hasRedirected = true;
                        }
                    }

                    if (_hasRedirected && _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }

                    break;

                case 4:
                    // Check if player brakes
                    if (Game1.Player.State == PlayerState.Rolling)
                    {
                        if (Game1.IsMouseButtonPressed(2))
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

            }
        }

        private void DrawTutorialCutscene(SpriteBatch sb)
        {
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

        #endregion
    }
}
