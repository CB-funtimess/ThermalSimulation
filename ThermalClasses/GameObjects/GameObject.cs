using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ThermalClasses.GameObjects;

public class GameObject
{
    #region Fields
    protected Texture2D texture;
    protected Vector2 position; // The central position of the object
    protected Color colour;
    protected Point dimensions;
    #endregion

    #region Properties
    public bool Enabled;
    public Vector2 Position => position;
    public Vector2 TopLeftPoint => new Vector2(position.X - XRadius, position.Y - YRadius);
    public Rectangle ObjectRectangle => new Rectangle((int)TopLeftPoint.X, (int)TopLeftPoint.Y, dimensions.X, dimensions.Y);
    public int XRadius => dimensions.X / 2;
    public int YRadius => dimensions.Y / 2;
    #endregion

    #region Methods
    public GameObject(Texture2D texture, Vector2 centralPosition, Color colour, Point dimensions)
    {
        this.dimensions = dimensions;
        this.texture = texture;
        position = centralPosition;
        this.colour = colour;
        Enabled = true;
    }

    public GameObject(Texture2D texture, Color colour, Rectangle size)
    {
        dimensions = size.Size;
        this.texture = texture;
        this.colour = colour;
        Enabled = true;
        position = size.Center.ToVector2();
    }

    // Method to draw objects using a rectangle
    public virtual void Draw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            _spriteBatch.Draw(texture, ObjectRectangle, colour);
        }
    }

    public virtual void Update(GameTime gameTime) { }

    public void ChangePositionBy(Vector2 changeBy)
    {
        position = Vector2.Add(position, changeBy);
    }

    public void SetPosition(Vector2 newPosition)
    {
        position = newPosition;
    }

    public void SetXPosition(float xValue)
    {
        position.X = xValue;
    }
    #endregion
}
