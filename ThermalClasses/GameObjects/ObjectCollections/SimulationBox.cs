using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class SimulationBox
{
    #region Fields
    private GameObject fixedBox;
    private GameObject movingBox;
    private readonly int minX, maxX; // The min and max x coordinates that the right side of movingBox can reach
    #endregion

    #region Properties
    public Rectangle BoxRect
    {
        get
        {
            Rectangle movingRect = movingBox.ObjectRectangle;
            Rectangle fixedRect = fixedBox.ObjectRectangle;
            return new Rectangle(movingRect.Right + 3, movingRect.Top + 10, fixedRect.Width - movingRect.Right - 10, movingRect.Height - 20);
        }
    }
    #endregion

    #region Methods
    public SimulationBox(Game game, GameObject fixedBox, GameObject movingBox, int maxX, int minX) // Width is a length, not an x coordinate
    {
        this.fixedBox = fixedBox;
        this.movingBox = movingBox;
        this.maxX = maxX;
        this.minX = minX;
    }

    // Method to change the 'volume' of the box
    // Similar to the Update() method called by the game, except this is event-driven
    public void SetVolume(float setX)
    {
        if (setX >= minX && setX <= maxX)
        {
            movingBox.SetPosition(new Vector2(setX, movingBox.Position.Y));
        }
    }

    public void Draw(SpriteBatch _spriteBatch)
    {
        fixedBox.Draw(_spriteBatch);
        movingBox.Draw(_spriteBatch);
    }
    #endregion
}