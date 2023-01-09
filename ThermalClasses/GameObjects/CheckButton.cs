using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects;

public class CheckButton : Button
{
    #region Fields
    private Texture2D checkedTexture;
    private Texture2D drawTexture;
    #endregion

    #region Methods
    public CheckButton(Texture2D uncheckedTexture, Texture2D checkedTexture, SpriteFont font, Vector2 position, Color unclickedColour, Color penColour, Point dimensions) : base(uncheckedTexture, font, position, unclickedColour, penColour, dimensions)
    {
        this.checkedTexture = checkedTexture;
        drawTexture = uncheckedTexture;
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            Color tempColour = colour;
            // If the mouse is hovering over the button
            if (isHovering)
            {
                tempColour = HoverColour;
            }

            _spriteBatch.Draw(drawTexture, Rectangle, tempColour);

            if (!String.IsNullOrEmpty(Text))
            {
                var x = Rectangle.X + (Rectangle.Width / 2) - (font.MeasureString(Text).X / 2);
                var y = Rectangle.Y + (Rectangle.Height / 2) - (font.MeasureString(Text).Y / 2);

                _spriteBatch.DrawString(font, Text, new Vector2(x, y), PenColour);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Clicked)
        {
            if (drawTexture == checkedTexture)
            {
                drawTexture = texture;
            }
            else
            {
                drawTexture = checkedTexture;
            }
        }
    }
    #endregion
}