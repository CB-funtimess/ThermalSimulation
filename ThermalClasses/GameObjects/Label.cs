using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ThermalClasses.GameObjects;
public class Label : GameObject // This class is a label with a box texture
{
    #region Methods
    public SpriteFont font;

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
            var x = ObjectRectangle.X + (ObjectRectangle.Width / 2) - (font.MeasureString(Text).X / 2);
            var y = ObjectRectangle.Y + (ObjectRectangle.Height / 2) - (font.MeasureString(Text).Y / 2);

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