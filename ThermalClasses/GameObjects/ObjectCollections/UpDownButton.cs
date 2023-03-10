using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class UpDownButton : ObjectCollection
{
    #region Fields
    private Label textLabel;
    #endregion

    #region Properties
    public Button UpButton, DownButton;
    #endregion

    #region Methods
    public UpDownButton(Button upButton, Button downButton, Label textLabel)
    {
        Enabled = true;
        UpButton = upButton;
        DownButton = downButton;
        this.textLabel = textLabel;
    }
    public UpDownButton(Texture2D upTexture, Texture2D downTexture, Texture2D labelTexture, Rectangle size, string labelText, SpriteFont font, Color penColour, Color baseColour, Color hoverColour)
    {
        Enabled = true;
        Point buttonSize = new Point(size.Height, size.Height); // Buttons are square textures
        DownButton = new Button(downTexture, font, new Vector2(size.X + (buttonSize.X / 2), size.Y + (buttonSize.Y / 2)), baseColour, Color.White, buttonSize)
        {
            HoverColour = hoverColour,
        };
        Point labelTopLeft = new Point(size.X + buttonSize.X, size.Y);
        Rectangle labelRect = new Rectangle(labelTopLeft, new Point(size.Width - (2 * buttonSize.X), buttonSize.Y));
        textLabel = new Label(labelTexture, baseColour, labelRect, font, penColour)
        {
            Text = labelText,
        };
        Vector2 upPosition = new Vector2(size.Right - (buttonSize.X / 2), size.Top + (buttonSize.Y / 2));
        UpButton = new Button(upTexture, font, upPosition, baseColour, Color.White, buttonSize)
        {
            HoverColour = hoverColour,
        };
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            DownButton.Draw(_spriteBatch);
            textLabel.Draw(_spriteBatch);
            UpButton.Draw(_spriteBatch);
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (Enabled)
        {
            UpButton.Update(gameTime);
            DownButton.Update(gameTime);
        }
    }

    public override void ChangePenColour(Color colour)
    {
        textLabel.PenColour = colour;
    }
    #endregion
}
