using System.Runtime.InteropServices;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.QuestionHandling;
using ThermalClasses.GameObjects;
using ThermalClasses.GameObjects.ObjectCollections;
namespace ThermalClasses.Handlers;

public class QuestionHandler : Handler
{
    #region Fields
    private QuestionInterface questions;
    private Label scoreLabel;
    private Label questionLabel;
    private Button submitButton;
    private Button resetQuestionButton;
    private RadioButtons multipleChoice;
    private NumInput mathematical;
    private GameObject surroundBox;
    #endregion

    private List<GameObject> objects;
    #region Properties
    #endregion

    #region Methods
    public QuestionHandler(Game game, Rectangle renderRectangle)
    {
        this.game = game;
        this.renderRectangle = renderRectangle;
        content = game.Content;

        Enabled = true;

        DatabaseConnection.InitialiseTable();
        DatabaseConnection.FillTableMathematical();
    }

    #region Initialisation
    public override void Initialize()
    {
        questions = new QuestionInterface();
        objects = new List<GameObject>();
    }

    public override void LoadContent()
    {
        Color unclickedColour = Color.White;

        SpriteFont smallFont = content.Load<SpriteFont>("QuestionAssets/SmallArial");
        SpriteFont font = content.Load<SpriteFont>("GeneralAssets/Arial");

        Texture2D outputBox = content.Load<Texture2D>("GeneralAssets/DataBox");
        Texture2D labelBox = content.Load<Texture2D>("GeneralAssets/LabelBox1");

        // Surrounding box - may need to be moved to game class
        Rectangle surroundRect = new Rectangle(new Point((int)(renderRectangle.Width * 0.73), 0), new Point((int)(renderRectangle.Width * 0.27), (int)(renderRectangle.Height * 0.6)));
        surroundBox = new GameObject(outputBox, unclickedColour, surroundRect);

        // Score label - needs insetting
        Rectangle scoreRect = new Rectangle(new Point(surroundRect.Right - (int)((surroundRect.Width * 0.1) + 1), 0), new Point((int)(surroundRect.Width * 0.1) + 1, 21));
        scoreLabel = new Label(labelBox, unclickedColour, scoreRect, font, PenColour)
        {
            Text = $"{questions.CorrectQuestions}/{questions.QuestionsAnswered}"
        };

        objects.Add(surroundBox);
        objects.Add(scoreLabel);
    }
    #endregion

    #region Drawing and Updating
    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        if (Enabled)
        {
            for (var i = 0; i < objects.Count; i++)
            {
                objects[i].Draw(_spriteBatch);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (Enabled)
        {
            for (var i = 0; i < objects.Count; i++)
            {
                objects[i].Update(gameTime);
            }
        }
    }
    #endregion

    public override void ChangePenColour()
    {

    }
    #endregion
}