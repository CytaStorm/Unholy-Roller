using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public delegate void ButtonClicked();

    public class Button
    {
        public event ButtonClicked OnClicked;

        // Fields
        private Texture2D _staticImage;
        private Texture2D _hoverImage;
        private Texture2D _curImage;
        private Rectangle _bounds;
        private string _text = "";
        private SpriteFont _font;

        // Properties
        public Color TextColor { get; set; } = Color.White;
        public Color TintColor { get; set; } = Color.White;

        // Constructor

        public Button(Rectangle bounds, Texture2D staticImage, Texture2D hoverImage)
        {
            this._staticImage = staticImage;
            this._hoverImage = hoverImage;

            this._bounds = bounds;

            _curImage = _staticImage;
        }

        // Methods

        public void Update(GameTime gameTime, MouseState mState)
        {
            _curImage = _staticImage;

            // Check if underMouse
            if (_bounds.Contains(mState.Position))
            {   
                if (UI.MouseClick("left"))
                {
                    // Do something
                    if (OnClicked != null)
                        OnClicked();
                }

                if (mState.LeftButton == ButtonState.Pressed)
                {
                    // Depressed image
                }
                else if (mState.LeftButton == ButtonState.Released)
                {
                    _curImage = _hoverImage;
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(
                _curImage,
                _bounds,
                TintColor);

            if (!string.IsNullOrEmpty(_text) && _font != null)
            {
                Vector2 textDimensions = _font.MeasureString(_text);

                // Center text in button
                Vector2 textPos = new Vector2(
                    _bounds.X + _bounds.Width / 2 - textDimensions.X / 2,
                    _bounds.Y + _bounds.Height / 2 - textDimensions.Y / 2);

                sb.DrawString(
                    _font,
                    _text,
                    textPos,
                    TextColor);
            }
        }

        public void SetText(string text, SpriteFont font)
        {
            this._text = text;
            this._font = font;
        }

    }
}
