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
        public static bool IsFullscreen { get; private set; }

        public static void Initialize(GraphicsDeviceManager graphicsDeviceManager)
        {
            graphics = graphicsDeviceManager;
        }

        public static void ToggleFullScreen()
        {
            if (graphics != null)
            {
                graphics.IsFullScreen = !graphics.IsFullScreen;
                graphics.ApplyChanges();
                IsFullscreen = !IsFullscreen;
            }
        }

        public static void SetFullScreen(bool isFullScreen)
        {
            if (graphics != null)
            {
                graphics.IsFullScreen = isFullScreen;
                graphics.ApplyChanges();
                FullScreen.IsFullscreen = isFullScreen;
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
            if (Game1.SingleKeyPress(Keys.F11))
            {
                ToggleFullScreen();
            }
        }
    }
}
