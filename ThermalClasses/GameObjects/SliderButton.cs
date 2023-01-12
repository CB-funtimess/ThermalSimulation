using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects;

public class SliderButton : Button
{
    #region Fields
    private readonly int minX, maxX;
    private int changeInX;
    #endregion

    #region Properties
    public int ChangeInX { get { return changeInX; } }
    #endregion

    #region Methods
    public SliderButton(Texture2D texture, SpriteFont font, Vector2 position, Color unclickedColour, Color penColour, Point dimensions, int minX, int maxX) : base(texture, font, position, unclickedColour, penColour, dimensions)
    {
        changeInX = 0;
        this.minX = minX;
        this.maxX = maxX;
    }

    public override void Update(GameTime gameTime)
    {
        previousState = currentState;
        currentState = Mouse.GetState();
        Clicked = false;

        var oldMouseRectangle = new Rectangle(previousState.X, previousState.Y, 1, 1);
        var mouseRectangle = new Rectangle(currentState.X, currentState.Y, 1, 1);

        isHovering = false;

        // If the button is pressed, move button
        if (mouseRectangle.Intersects(ObjectRectangle) && oldMouseRectangle.Intersects(ObjectRectangle))
        {
            isHovering = true;

            if (currentState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Pressed)
            {
                changeInX += currentState.X - previousState.X;
                Vector2 newPosition = new Vector2(changeInX, 0);
                if (newPosition.X >= minX && newPosition.X <= maxX)
                {
                    SetPosition(newPosition);
                }
            }
            else if (currentState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                Click?.Invoke(this, EventArgs.Empty);
                Clicked = true;
            }
            else
            {
                changeInX = 0;
            }
        }
    }
    #endregion
}