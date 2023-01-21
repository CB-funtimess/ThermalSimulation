using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects;

public class CheckButton : Button
{
    #region Fields
    private Texture2D checkedTexture;
    private Texture2D drawTexture;
    private bool isChecked;
    #endregion

    #region Properties
    public bool Checked => isChecked;
    #endregion

    #region Methods
    public CheckButton(Texture2D uncheckedTexture, Texture2D checkedTexture, SpriteFont font, Vector2 position, Color unclickedColour, Color penColour, Point dimensions) : base(uncheckedTexture, font, position, unclickedColour, penColour, dimensions)
    {
        this.checkedTexture = checkedTexture;
        drawTexture = uncheckedTexture;
        isChecked = false;
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

            _spriteBatch.Draw(drawTexture, ObjectRectangle, tempColour);

            if (!String.IsNullOrEmpty(Text))
            {
                var x = ObjectRectangle.X + (ObjectRectangle.Width / 2) - (font.MeasureString(Text).X / 2);
                var y = ObjectRectangle.Y + (ObjectRectangle.Height / 2) - (font.MeasureString(Text).Y / 2);

                _spriteBatch.DrawString(font, Text, new Vector2(x, y), PenColour);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (clicked)
        {
            if (drawTexture == checkedTexture)
            {
                drawTexture = texture;
                isChecked = false;
            }
            else
            {
                drawTexture = checkedTexture;
                isChecked = true;
            }
        }
    }

    public void Uncheck()
    {
        isChecked = false;
        drawTexture = texture;
    }

    public void Check()
    {
        isChecked = true;
        drawTexture = checkedTexture;
    }
    #endregion
}