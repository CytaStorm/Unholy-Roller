using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prototype.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public enum Cutscene
    {
        Tutorial,
        None
    }

    internal class CutsceneManager
    {
        private Game1 _gManager;

        public int PhaseNum { get; private set; }
        public Cutscene Scene { get; private set; } = Cutscene.None;

        // Tutorial
        public bool HasPrimedLaunch { get; private set; }
        public bool HasLaunched { get; private set; }
        public bool HasRedirected { get; private set; }
        public bool HasUsedBrake { get; private set; }
        public bool HasWalked { get; private set; }

        public string _launchInstructions { get; private set; }
        public string _redirectInstructions { get; private set; }
        public string _brakeInstructions { get; private set; }
        public string _walkInstructions { get; private set; }

        public string TutorialEndMessage { get; private set; }

        private double _phaseTransferDuration = 3;
        private double _phaseTransferTimer;

        public CutsceneManager(Game1 gm)
        {
            _gManager = gm;

            // Write tutorial
            _walkInstructions =
                "If your speed is zero, use W A S D to walk around";

            _launchInstructions =
                "You will only be able to roll through enemies if you launch.\n" +
                "Click mouse left in any direction to launch\n" +
                "Time will slow as long as you hold mouse left";

            _redirectInstructions =
                "If you launch while rolling, you will redirect\n" +
                "You get a limited number of these per launch so use them wisely\n";

            _brakeInstructions =
                "Hold mouse right to rapidly decelerate\n" +
                "You can use this to swiftly transition to walking";

            TutorialEndMessage =
                "That's it! You've finished the tutorial.\n" +
                "NOW GO EVISCERATE THOSE PINHEADS!!!\n\n" +
                "Press 'Esc' to return to main menu";
        }

        public void Update(GameTime gameTime)
        {
            if (_phaseTransferTimer > 0)
            {
                _phaseTransferTimer -= gameTime.ElapsedGameTime.TotalSeconds;

                if (_phaseTransferTimer <= 0) OnPhaseTransfer();
            }

            switch (Scene)
            {
                case Cutscene.Tutorial:
                    RunTutorialCutscene();
                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            switch (Scene)
            {
                case Cutscene.Tutorial:
                    DrawTutorialCutscene(sb);
                    break;
            }
        }

        public void RunTutorialCutscene()
        {
            MouseState mouseState = Mouse.GetState();

            switch (PhaseNum)
            {
                case 1:
                    // Check if player walks
                    KeyboardState kb = Keyboard.GetState();
                    
                    if (Game1.Player1.State == PlayerState.Walking &&
                        (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.A) ||
                        kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.D)))
                    {
                        HasWalked = true;
                    }

                    if (HasWalked && _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }
                    break;

                case 2:
                    // Check if player launches
                    if (Game1.Player1.State == PlayerState.Walking)
                    {
                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
                            HasPrimedLaunch = true;
                        }

                        if (HasPrimedLaunch && mouseState.LeftButton == ButtonState.Released)
                        {
                            HasLaunched = true;
                        }
                    }


                    if (HasLaunched && _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }

                    break;

                case 3:
                    // Check if player redirects
                    if (Game1.Player1.State == PlayerState.Rolling)
                    {
                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
                            HasPrimedLaunch = true;
                        }

                        if (HasPrimedLaunch && mouseState.LeftButton == ButtonState.Released)
                        {
                            HasLaunched = true;
                            HasRedirected = true;
                        }
                    }

                    if (HasRedirected && _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }

                    break;

                case 4:
                    // Check if player brakes
                    if (Game1.Player1.State == PlayerState.Rolling)
                    {
                        if (mouseState.RightButton == ButtonState.Pressed)
                        {
                            HasUsedBrake = true;
                        }
                    }

                    if (HasUsedBrake && _phaseTransferTimer <= 0)
                    {
                        // Wait some time then move to the next phase
                        _phaseTransferTimer = 3; // seconds
                    }

                    break;

            }
        }

        public void DrawTutorialCutscene(SpriteBatch sb)
        {
            if (Game1.GAMESTATE != Gamestate.Pause)
            {
                switch (PhaseNum)
                {
                    case 1:
                        sb.DrawString(
                            Game1.ARIAL32,
                            _walkInstructions,
                            new Vector2(0f, 350f),
                            Color.White);
                        break;

                    case 2:
                        sb.DrawString(
                            Game1.ARIAL32,
                            _launchInstructions,
                            new Vector2(0f, 350f),
                            Color.White);
                        break;

                    case 3:
                        sb.DrawString(
                            Game1.ARIAL32,
                            _redirectInstructions,
                            new Vector2(0f, 350f),
                            Color.White);
                        break;

                    case 4:
                        sb.DrawString(
                            Game1.ARIAL32,
                            _brakeInstructions,
                            new Vector2(0f, 350f),
                            Color.White);
                        break;

                    case 5:
                    
                        sb.DrawString(
                            Game1.ARIAL32,
                            TutorialEndMessage,
                            new Vector2(0f, 350f),
                            Color.White);

                        break;

                }
            }
        }

        public void StartCutscene(Cutscene scene)
        {
            switch (scene)
            {
                case Cutscene.Tutorial:

                    HasPrimedLaunch = false;
                    HasLaunched = false;
                    HasRedirected = false;
                    HasUsedBrake = false;
                    HasWalked = false;

                    Scene = Cutscene.Tutorial;

                    PhaseNum = 1;
                    break;
            }
        }

        private void OnPhaseTransfer()
        {
            if (PhaseNum == 2)
            {
                HasPrimedLaunch = false;
                HasLaunched = false;
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
    }
}
