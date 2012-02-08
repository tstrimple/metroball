using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.UI
{
    class Button
    {
        private Color _color;
        private string _text;
        private SpriteFont _font;
        
        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }

            set
            {
                _position = value;
                RecalculateRect();
            }
        }

        private Rectangle _rectangle;
        public Rectangle Rectangle
        {
            get { return _rectangle; }
        }

        public Button(SpriteFont font, string text, Color color)
        {
            _font = font;
            _text = text;
            _color = color;

            RecalculateRect();
        }

        private void RecalculateRect()
        {
            var ms = _font.MeasureString(_text);
            _rectangle.Width = (int)ms.X;
            _rectangle.Height = (int)ms.Y;

            _rectangle.X = (int)_position.X;
            _rectangle.Y = (int)_position.Y;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.DrawString(_font, _text, _position, _color);
        }
    }
}
