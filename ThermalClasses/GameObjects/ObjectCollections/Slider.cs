using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class Slider : ObjectCollection
{
    #region Fields
    private Label slider;
    private int minX, maxX;
    #endregion

    #region Properties
    public SliderButton sliderButton;
    public bool Moveable;
    public int MinX => minX;
    public int MaxX => maxX;
    #endregion

    public Slider(Texture2D buttonTexture, Texture2D hoverTexture, Texture2D labelTexture, SpriteFont font, Rectangle position, float scale, Color baseColour, Color penColour)
    {
        Moveable = true;
        minX = (int)(position.Left + (position.Height * scale / 2));
        maxX = (int)(position.Right - (position.Height * scale / 2));

        slider = new Label(labelTexture, baseColour, position, font, penColour);
        Vector2 buttonPos = new Vector2(minX, position.Top + (position.Height / 2));
        Point buttonDimensions = new Point((int)(position.Height * scale));
        sliderButton = new SliderButton(buttonTexture, font, buttonPos, baseColour, penColour, buttonDimensions, minX, maxX)
        {
            hoverTexture = hoverTexture,
            HoverColour = baseColour,
        };
    }

    public Slider(Texture2D buttonTexture, Texture2D hoverTexture, Texture2D labelTexture, SpriteFont font, Rectangle buttonMovement, Rectangle sliderPos, float scale, Color baseColour, Color penColour)
    {
        Moveable = true;
        minX = (int)(buttonMovement.Left + (buttonMovement.Height * scale / 2));
        maxX = (int)(buttonMovement.Right - (buttonMovement.Height * scale / 2));

        slider = new Label(labelTexture, baseColour, sliderPos, font, penColour);
        Vector2 buttonPos = new Vector2(minX, buttonMovement.Top + (buttonMovement.Height / 2));
        Point buttonDimensions = new Point((int)(buttonMovement.Height * scale));
        sliderButton = new SliderButton(buttonTexture, font, buttonPos, baseColour, penColour, buttonDimensions, minX, maxX)
        {
            hoverTexture = hoverTexture,
            HoverColour = baseColour,
        };
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        slider.Draw(_spriteBatch);
        sliderButton.Draw(_spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        if (Moveable)
        {
            sliderButton.Update(gameTime);
        }
    }

    public override void ChangePenColour(Color colour)
    {
        slider.PenColour = colour;
    }
}