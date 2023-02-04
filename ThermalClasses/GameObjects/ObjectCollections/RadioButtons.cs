using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ThermalClasses.GameObjects.ObjectCollections;

public class RadioButtons : ObjectCollection
{
    #region Fields
    private List<Label> labels;
    private Label surround;
    private int checkedIndex;
    private List<CheckButton> buttons;
    private bool changedIndex;
    #endregion

    #region Properties
    public bool ChangedIndex => changedIndex;
    public int CheckedIndex => checkedIndex;
    #endregion

    #region Methods
    public RadioButtons(Texture2D uncheckedTexture, Texture2D checkedTexture, Texture2D labelTexture, Texture2D surroundTexture, Rectangle rect, Vector2 buttonStartPos, string[] text, SpriteFont font, Color baseColour, Color hoverColour, Color penColour, int startIndex)
    {
        changedIndex = false;
        surround = new Label(surroundTexture, baseColour, rect, font, penColour);
        int numButtons = text.Length;
        Point buttonSize = new Point(15, 15);
        buttons = new List<CheckButton>();
        labels = new List<Label>();

        for (var i = 0; i < numButtons; i++)
        {
            Rectangle labelRect = new Rectangle(new Point((int)(buttonStartPos.X + buttonSize.X), (int)buttonStartPos.Y - buttonSize.Y), new Point(rect.Width * (3/4), buttonSize.Y));
            labels.Add(new Label(labelTexture, baseColour, labelRect, font, penColour)
            {
                Text = text[i],
            });
            buttons.Add(new CheckButton(uncheckedTexture, checkedTexture, font, buttonStartPos, baseColour, penColour, buttonSize)
            {
                HoverColour = hoverColour
            });
            buttonStartPos.Y += buttonSize.Y + 10;
        }
        checkedIndex = startIndex;
        buttons[startIndex].Check();
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
        surround.Draw(_spriteBatch);
        for (var i = 0; i < buttons.Count; i++)
        {
            labels[i].DrawStringUncentered(_spriteBatch);
            buttons[i].Draw(_spriteBatch);
        }
    }

    public override void Update(GameTime gameTime)
    {
        changedIndex = false;
        for (var i = 0; i < buttons.Count; i++)
        {
            buttons[i].Update(gameTime);
            if (buttons[i].Clicked)
            {
                changedIndex = true;
                buttons[checkedIndex].Uncheck();
                checkedIndex = i;
                break;
            }
        }
        buttons[checkedIndex].Check();
    }

    public override void ChangePenColour(Color colour)
    {
        for (var i = 0; i < buttons.Count; i++)
        {
            buttons[i].PenColour = colour;
            labels[i].PenColour = colour;
        }
    }
    #endregion
}