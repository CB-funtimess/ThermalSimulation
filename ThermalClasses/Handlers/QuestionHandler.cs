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
    private Label scoreLabel, questionLabel;
    private Button submitButton, resetQuestionButton;
    private RadioButtons multipleChoice;
    private NumInput mathematical;
    private GameObject surroundBox;
    private List<GameObject> objects;
    private int correctMCQIndex;
    #endregion

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

        Texture2D surroundTexture = content.Load<Texture2D>("GeneralAssets/DataBox");
        Texture2D labelTexture = content.Load<Texture2D>("GeneralAssets/LabelBox1");
        Texture2D scoreTexture = content.Load<Texture2D>("QuestionAssets/ScoreBox");
        Texture2D tickboxTexture = content.Load<Texture2D>("GeneralAssets/TickButton");
        Texture2D resetTexture = content.Load<Texture2D>("GeneralAssets/ResetButton");

        // Surrounding box - may need to be moved to game class
        Rectangle surroundRect = new Rectangle(new Point((int)(renderRectangle.Width * 0.73), 0), new Point((int)(renderRectangle.Width * 0.27), (int)(renderRectangle.Height * 0.6)));
        surroundBox = new GameObject(surroundTexture, unclickedColour, surroundRect);

        // Score label - needs insetting
        Rectangle scoreRect = new Rectangle(new Point(surroundRect.Right - (int)(surroundRect.Width * 0.1)-1, 0), new Point((int)(surroundRect.Width * 0.1) + 1, 21));
        scoreLabel = new Label(scoreTexture, unclickedColour, scoreRect, font, PenColour)
        {
            Text = $"{questions.CorrectQuestions}/{questions.QuestionsAnswered}"
        };

        // Submit button - used to mark a question
        Point buttonSize = new Point(40, 40);
        Rectangle submitRect = new Rectangle(new Point(surroundRect.Right - 20 - buttonSize.X, surroundRect.Bottom - 20 - buttonSize.Y), buttonSize);
        submitButton = new Button(tickboxTexture, font, submitRect, unclickedColour, PenColour);
        submitButton.Click += SubmitButton_Click;

        // Reset button - used to generate a new question
        Rectangle resetRect = new Rectangle(new Point(submitRect.X - 20 - buttonSize.X, submitRect.Y), buttonSize);
        resetQuestionButton = new Button(resetTexture, font, resetRect, unclickedColour, PenColour);
        resetQuestionButton.Click += ResetButton_Click;

        // Input for mathematical questions
        Rectangle mathsRect = new Rectangle();
        mathematical = new NumInput()
        {
            DefaultText = ""
        };

        // Input for MCQ
        Rectangle mcqRect = new Rectangle();
        correctMCQIndex = 0;
        multipleChoice = new RadioButtons();

        objects.Add(surroundBox);
        objects.Add(scoreLabel);

        NewQuestion();

    }
    #endregion Initialisation

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
    #endregion Drawing and Updating

    public override void ChangePenColour()
    {

    }

    public void NewQuestion()
    {
        questions.GenerateQuestion();
        questionLabel.Text = questions.CurrentQuestion.question;
        if (questions.CurrentQuestion.type == QuestionType.Mathematical)
        {
            multipleChoice.Enabled = false;
            mathematical.Enabled = true;
            mathematical.ClearText();
        }
        else
        {
            multipleChoice.Enabled = true;
            mathematical.Enabled = false;
            // More logic here
            string[] listQuestions = {
                questions.CurrentQuestion.Answer,
                questions.CurrentQuestion.first,
                questions.CurrentQuestion.second,
                questions.CurrentQuestion.third
            };
            Random rnd = new Random();
            // Fisher-Yates shuffle
            for (var i = 0; i < listQuestions.Length; i++)
            {
                int j = rnd.Next(i, listQuestions.Length);
                string temp = listQuestions[i];
                listQuestions[i] = listQuestions[j];
                listQuestions[j] = temp;
            }
            // Finding the index of the correct answer
            for (var i = 0; i < listQuestions.Length; i++)
            {
                if (listQuestions[i] == questions.CurrentQuestion.Answer)
                {
                    correctMCQIndex = i;
                }
            }
            multipleChoice.ChangeText(listQuestions);
        }
    }

    #region Events
    public void SubmitButton_Click(object sender, EventArgs e)
    {
        bool correct = false;
        if (questions.CurrentQuestion.type == QuestionType.Mathematical)
        {
            try
            {
                correct = questions.AnswerMathematicalQuestion(double.Parse(mathematical.Text));
            }
            catch (FormatException)
            {
                correct = false;
            }
        }
        else if (questions.CurrentQuestion.type == QuestionType.MCQ)
        {
            // Do something here
            correct = questions.AnswerMultipleChoiceQuestion(multipleChoice.CheckedIndex, correctMCQIndex);
        }
        scoreLabel.Text = $"{questions.CorrectQuestions}/{questions.QuestionsAnswered}";

        if (correct)
        {
            surroundBox.ChangeColourMask(Color.Green);
        }
        else
        {
            surroundBox.ChangeColourMask(Color.Red);
        }
    }

    public void ResetButton_Click(object sender, EventArgs e)
    {
        NewQuestion();
    }
    #endregion Events
    #endregion Methods
}