using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ThermalClasses.GameObjects;
public class Label : GameObject // This class is a label with a box texture
{
    #region Methods
    private SpriteFont font;

    #endregion

    #region Properties
    public string Text { get; set; }
    public Color PenColour { get; set; }
    #endregion

    #region Methods
    public Label(Texture2D texture, Color colour, Rectangle size, SpriteFont font, Color penColour) : base(texture, colour, size)
    {
        this.font = font;
        PenColour = penColour;
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        base.Draw(_spriteBatch);
        // Drawing Text centred to the box
        if (!String.IsNullOrEmpty(Text))
        {
            // Position text correctly so that it does not overrun x bounds
            for (int i = 0; i < Text.Length; i++)
            {
                if (font.MeasureString(Text[..i]).X >= ObjectRectangle.Width - 2)
                {
                    // Backtrack to make the closest space a new line
                    for (int j = i-1; j > 0; j--)
                    {
                        if (Text[j].Equals(' '))
                        {
                            Text = string.Concat(Text.AsSpan(0,j), "\n", Text.AsSpan(j+1));
                            break;
                        }
                    }
                }
            }

            float x = ObjectRectangle.X + (ObjectRectangle.Width / 2) - (font.MeasureString(Text).X / 2);
            float y = ObjectRectangle.Y + (ObjectRectangle.Height / 2) - (font.MeasureString(Text).Y / 2);

            _spriteBatch.DrawString(font, Text, new Vector2(x, y), PenColour);
        }
    }

    public void DrawStringUncentered(SpriteBatch _spriteBatch)
    {
        base.Draw(_spriteBatch);
        if (!String.IsNullOrEmpty(Text))
        {
             var x = ObjectRectangle.X + (ObjectRectangle.Width / 2);
             var y = ObjectRectangle.Y + (ObjectRectangle.Height / 2);

            _spriteBatch.DrawString(font, Text, new Vector2(x,y), PenColour);
        }
    }
    #endregion
}