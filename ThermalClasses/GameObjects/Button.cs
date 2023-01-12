using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects;

public class Button : GameObject
{
    #region Fields
    protected MouseState currentState;
    protected MouseState previousState;
    protected readonly SpriteFont font;
    protected bool isHovering;
    #endregion

    #region Properties
    public EventHandler Click;
    public Texture2D hoverTexture;
    public bool Clicked { get; protected set; }
    public string Text { get; set; }
    public Color PenColour { get; set; }
    public Color HoverColour { get; set; }
    #endregion

    #region Methods
    public Button(Texture2D texture, SpriteFont font, Vector2 position, Color unclickedColour, Color penColour, Point dimensions) : base(texture, position, unclickedColour, dimensions)
    {
        this.font = font;

        PenColour = penColour;
        Clicked = false;
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            Texture2D tempTexture = texture;
            Color tempColour = colour;
            // If the mouse is hovering over the button
            if (isHovering)
            {
                tempColour = HoverColour;
                if (hoverTexture != null)
                {
                    tempTexture = hoverTexture;
                }
            }

            _spriteBatch.Draw(tempTexture, ObjectRectangle, tempColour);

            if (!String.IsNullOrEmpty(Text))
            {
                var x = ObjectRectangle.X + (ObjectRectangle.Width / 2) - (font.MeasureString(Text).X / 2);
                var y = ObjectRectangle.Y + (ObjectRectangle.Height / 2) - (font.MeasureString(Text).Y / 2);

                _spriteBatch.DrawString(font, Text, new Vector2(x, y), PenColour);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        previousState = currentState;
        currentState = Mouse.GetState();
        Clicked = false;

        var mouseRectangle = new Rectangle(currentState.X, currentState.Y, 1, 1);

        isHovering = false;

        // If the button is pressed, trigger a new event
        if (mouseRectangle.Intersects(ObjectRectangle))
        {
            isHovering = true;

            if (currentState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                Click?.Invoke(this, EventArgs.Empty);
                Clicked = true;
            }
        }
    }

    #endregion
}