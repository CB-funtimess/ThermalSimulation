using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects;

public class SliderButton : Button
{
    #region Fields
    private readonly int minX, maxX;
    #endregion

    #region Properties
    #endregion

    #region Methods
    public SliderButton(Texture2D texture, SpriteFont font, Vector2 position, Color unclickedColour, Color penColour, Point dimensions, int minX, int maxX) : base(texture, font, position, unclickedColour, penColour, dimensions)
    {
        this.minX = minX;
        this.maxX = maxX;
    }

    public override void Update(GameTime gameTime)
    {
        previousState = currentState;
        currentState = Mouse.GetState();
        clicked = false;

        var oldMouseRectangle = new Rectangle(previousState.X, previousState.Y, 1, 1);
        var mouseRectangle = new Rectangle(currentState.X, currentState.Y, 1, 1);

        isHovering = false;

        // If the button is pressed, move button
        if (oldMouseRectangle.Intersects(ObjectRectangle))
        {
            isHovering = true;
            if (currentState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Pressed)
            {
                float newPositionX = currentState.X;
                if (newPositionX >= minX && newPositionX <= maxX)
                {
                    SetXPosition(newPositionX);
                }
                Click?.Invoke(this, EventArgs.Empty);
                clicked = true;
            }
            else if (currentState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                Click?.Invoke(this, EventArgs.Empty);
                clicked = true;
            }
        }
    }
    #endregion
}