using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class Slider
{
    #region Fields
    private SliderButton sliderButton;
    private Label slider;
    private int minX, maxX;
    #endregion

    public Slider(Texture2D buttonTexture, Texture2D hoverTexture, Texture2D labelTexture, SpriteFont font, Rectangle position, Color baseColour, Color penColour)
    {
        minX = position.X + (position.Height / 2);
        maxX = position.Right - (position.Height / 2) - 20;

        slider = new Label(labelTexture, baseColour, position, font, penColour);
        Vector2 buttonPos = new Vector2(position.X, position.Y);
        Point buttonDimensions = new Point((int)(position.Height * 0.8), (int)(position.Height * 0.8));
        sliderButton = new SliderButton(buttonTexture, font, buttonPos, baseColour, penColour, buttonDimensions, minX, maxX)
        {
            hoverTexture = hoverTexture,
            HoverColour = baseColour,
        };
    }

    public void Draw(SpriteBatch _spriteBatch)
    {
        slider.Draw(_spriteBatch);
        sliderButton.Draw(_spriteBatch);
    }

    public void Update(GameTime gameTime)
    {
        sliderButton.Update(gameTime);
    }
}