using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ThermalClasses.GameObjects.Particles;

public class NumInput : GameObject
{
    #region Fields
    private bool selected;
    private bool isHovering;
    private KeyboardState prevKeyState, keyState;
    private MouseState mouseState;
    private SpriteFont font;
    private string text;
    #endregion

    #region Properties
    public Color PenColour;
    public Color HoverColour;
    public string Text => text;
    public EventHandler Enter;
    public string DefaultText;
    #endregion

    #region Methods
    public NumInput(Texture2D texture, Rectangle position, SpriteFont font, Color colour, Color penColour, string defaultText) : base(texture, colour, position)
    {
        DefaultText = defaultText;
        PenColour = penColour;
        text = defaultText;
        this.font = font;
        selected = false;
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            if (isHovering)
            {
                _spriteBatch.Draw(texture, ObjectRectangle, HoverColour);
            }
            else
            {
                _spriteBatch.Draw(texture, ObjectRectangle, colour);
            }

            if (String.IsNullOrEmpty(text) && !selected)
            {
                text = DefaultText;
            }

            if (!String.IsNullOrEmpty(text))
            {
                Vector2 textSize = font.MeasureString(text);
                var y = ObjectRectangle.Y + (textSize.Y / 2);

                _spriteBatch.DrawString(font, text, new Vector2(ObjectRectangle.X+5, y), PenColour);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        prevKeyState = keyState;
        mouseState = Mouse.GetState();
        keyState = Keyboard.GetState();

        isHovering = false;

        Rectangle mouseRectangle = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

        if (mouseRectangle.Intersects(ObjectRectangle))
        {
            isHovering = true;
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                selected = true;
                if (text == DefaultText)
                {
                    text = "";
                }
            }
        }
        else if (!mouseRectangle.Intersects(ObjectRectangle) && mouseState.LeftButton == ButtonState.Pressed)
        {
            selected = false;
        }

        if (selected && keyState.GetPressedKeyCount() != prevKeyState.GetPressedKeyCount())
        {
            Keys[] pressedKeys = keyState.GetPressedKeys();
            if (pressedKeys.Contains(Keys.Back))
            {
                if (text.Length > 0)
                {
                    text = text.Remove(text.Length - 1);
                }
            }
            else if (pressedKeys.Contains(Keys.Enter))
            {
                Enter?.Invoke(this, EventArgs.Empty);
            }
            if (pressedKeys.Length > 0 && font.MeasureString(text).X < ObjectRectangle.Width - 20)
            {
                if (pressedKeys.Contains(Keys.LeftShift) && pressedKeys.Contains(Keys.D6))
                {
                    text += '^';
                }
                else
                {
                    char toAdd = KeyMatch(pressedKeys[^1]);
                    if (toAdd != 'A')
                    {
                        text += toAdd;
                    }
                }
            }
        }
    }

    private static char KeyMatch(Keys key)
    {
        return key switch
        {
            Keys.NumPad0 => '0',
            Keys.D0 => '0',
            Keys.NumPad1 => '1',
            Keys.D1 => '1',
            Keys.NumPad2 => '2',
            Keys.D2 => '2',
            Keys.NumPad3 => '3',
            Keys.D3 => '3',
            Keys.NumPad4 => '4',
            Keys.D4 => '4',
            Keys.NumPad5 => '5',
            Keys.D5 => '5',
            Keys.NumPad6 => '6',
            Keys.D6 => '6',
            Keys.NumPad7 => '7',
            Keys.D7 => '7',
            Keys.NumPad8 => '8',
            Keys.D8 => '8',
            Keys.NumPad9 => '9',
            Keys.D9 => '9',
            Keys.Decimal => '.',
            Keys.E => 'E',
            _ => 'A',
        };
    }
    #endregion
}