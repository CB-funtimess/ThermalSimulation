using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class Slider
{
    #region Fields
    private Button sliderButton;
    private Label slider;
    #endregion

    public Slider(Texture2D buttonTexture, Texture2D hoverTexture, Texture2D labelTexture, SpriteFont font, Rectangle position, Color baseColour, Color penColour)
    {
        slider = new Label(labelTexture, baseColour, position, font, penColour);
        Vector2 buttonPos = new Vector2();
        Point buttonDimensions = new Point();
        sliderButton = new Button(buttonTexture, font, buttonPos, baseColour, penColour, buttonDimensions)
        {
            hoverTexture = hoverTexture,
        };
    }

    public void Draw(SpriteBatch _spriteBatch)
    {

    }

    public void Update(GameTime gameTime)
    {
        
    }
}