using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
    internal class FullScreen
    {
        private static GraphicsDeviceManager graphics;
        private static bool isFullScreen = false;

        public static void Initialize(GraphicsDeviceManager graphicsDeviceManager)
        {
            graphics = graphicsDeviceManager;
        }

        public static void ToggleFullScreen(Game game)
        {
            if (graphics != null)
            {
                graphics.IsFullScreen = !graphics.IsFullScreen;
                graphics.ApplyChanges();
                isFullScreen = true;
            }
        }

        public static void SetFullScreen(Game game, bool isFullScreen)
        {
            if (graphics != null)
            {
                graphics.IsFullScreen = isFullScreen;
                graphics.ApplyChanges();
            }
        }

        public static int GetScreenWidth()
        {
            GraphicsAdapter adapter = GraphicsAdapter.DefaultAdapter;
            return adapter.CurrentDisplayMode.Width;
        }

        public static int GetScreenHeight()
        {
            GraphicsAdapter adapter = GraphicsAdapter.DefaultAdapter;
            return adapter.CurrentDisplayMode.Height;
        }

        public static void Update(Game game)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.F11))
            {
                ToggleFullScreen(game);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F12) && isFullScreen)
            {
                SetFullScreen(game, false);
                isFullScreen = false;
            }
        }
    }
}
