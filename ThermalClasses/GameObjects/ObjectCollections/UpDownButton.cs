using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class UpDownButton
{
    #region Properties
    public Button UpButton;
    public Button DownButton;
    public Label TextLabel;
    #endregion

    #region Methods
    public UpDownButton(Button upButton, Button downButton, Label textLabel)
    {
        UpButton = upButton;
        DownButton = downButton;
        TextLabel = textLabel;
    }
    public UpDownButton(Texture2D upTexture, Texture2D downTexture, Texture2D labelTexture, Rectangle size, string labelText, SpriteFont font, Color penColour, Color backgroundColour, Color hoverColour)
    {
        Point buttonSize = new Point(size.Height, size.Height); // Buttons are square textures
        DownButton = new Button(downTexture, font, new Vector2(size.X, size.Y), backgroundColour, Color.White, buttonSize)
        {
            HoverColour = hoverColour,
        };
        Point labelTopLeft = new Point(size.X + buttonSize.X, size.Y);
        Rectangle labelRect = new Rectangle(labelTopLeft, new Point(size.Width - (2 * buttonSize.X), buttonSize.Y));
        TextLabel = new Label(labelTexture, backgroundColour, labelRect, font, penColour)
        {
            Text = labelText,
        };
        Vector2 upPosition = new Vector2(size.Right - buttonSize.X, size.Top);
        UpButton = new Button(upTexture, font, upPosition, backgroundColour, Color.White, buttonSize)
        {
            HoverColour = hoverColour,
        };
    }

    public void Draw(SpriteBatch _spriteBatch)
    {
        DownButton.Draw(_spriteBatch);
        TextLabel.Draw(_spriteBatch);
        UpButton.Draw(_spriteBatch);
    }

    public void Update(GameTime gameTime)
    {
        UpButton.Update(gameTime);
        DownButton.Update(gameTime);
    }
    #endregion
}
