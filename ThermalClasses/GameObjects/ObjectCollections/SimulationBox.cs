using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class SimulationBox : GameComponent
{
    #region Fields
    private GameObject fixedBox;
    private GameObject movingBox;
    private readonly int minMovingX, maxMovingX; // The min and max x coordinates that the right side of movingBox can reach
    #endregion

    #region Properties
    public int Height { get; }
    public int Width { get; private set; }
    public int Depth { get; }
    public Rectangle BoxRect { get
    {
        Rectangle movingRect = movingBox.ObjectRectangle;
        Rectangle fixedRect = fixedBox.ObjectRectangle;
        return new Rectangle(movingRect.Right + 3, movingRect.Top + 10, fixedRect.Width - movingRect.Width - 10, movingRect.Height - 20);
    }}
    #endregion

    #region Methods
    public SimulationBox(Game game, GameObject fixedBox, GameObject movingBox, int maxWidth, int minWidth) : base(game) // Width is a length, not an x coordinate
    {
        Depth = 1;
        this.fixedBox = fixedBox;
        this.movingBox = movingBox;
        int rightBoundaryX = (int)(fixedBox.Position.X + fixedBox.XRadius);
        maxMovingX = rightBoundaryX - maxWidth;
        minMovingX = rightBoundaryX - minWidth;
        Height = (int)(fixedBox.YRadius * 2);
    }

    // Method to change the 'volume' of the box
    // Similar to the Update() method called by the game, except this is event-driven
    public void MoveBox(int changeByX)
    {
        int movingBoxLeftX = (int)(movingBox.Position.X + movingBox.XRadius);
        if (movingBoxLeftX <= maxMovingX && movingBoxLeftX >= minMovingX)
        {
            movingBox.Position = new Vector2(-changeByX, 0);
            // Change the position of the rectangle
        }
    }

    public void Draw(SpriteBatch _spriteBatch)
    {
        fixedBox.Draw(_spriteBatch);
        movingBox.Draw(_spriteBatch);
    }
    #endregion
}