using System.Runtime.InteropServices;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.QuestionClasses;
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

    #region Methods
    public QuestionHandler(Game game, Rectangle renderRectangle)
    {
        this.game = game;
        this.renderRectangle = renderRectangle;
        content = game.Content;

        Enabled = true;
    }

    #region Initialisation
    public override void Initialize()
    {
        questions = new QuestionInterface();
        objects = new List<GameObject>();
    }

    public override void LoadContent()
    {
        SpriteFont smallFont = content.Load<SpriteFont>("GeneralAssets/SmallArial");
        SpriteFont font = content.Load<SpriteFont>("GeneralAssets/Arial");

        Texture2D surroundTexture = content.Load<Texture2D>("GeneralAssets/DataBox");
        Texture2D labelTexture = content.Load<Texture2D>("GeneralAssets/LabelBox1");
        Texture2D scoreTexture = content.Load<Texture2D>("QuestionAssets/ScoreBox");
        Texture2D tickboxTexture = content.Load<Texture2D>("GeneralAssets/TickButton");
        Texture2D resetTexture = content.Load<Texture2D>("GeneralAssets/ResetButton");
        Texture2D largeTextInputTexture = content.Load<Texture2D>("GeneralAssets/LargeTextInputBox");
        Texture2D buttonUnchecked = content.Load<Texture2D>("GeneralAssets/Button_Unchecked");
        Texture2D buttonChecked = content.Load<Texture2D>("GeneralAssets/Button_Checked");

        // Surrounding box
        Rectangle surroundRect = new Rectangle(new Point((int)(renderRectangle.Width * 0.73), 0), new Point((int)(renderRectangle.Width * 0.27), (int)(renderRectangle.Height * 0.6)));
        surroundBox = new GameObject(surroundTexture, UnclickedColour, surroundRect);

        // Score label - needs insetting
        Rectangle scoreRect = new Rectangle(new Point(surroundRect.Right - (int)(surroundRect.Width * 0.1)-10, 0), new Point((int)(surroundRect.Width * 0.1) + 10, 21));
        scoreLabel = new Label(scoreTexture, UnclickedColour, scoreRect, font, PenColour)
        {
            Text = $"{questions.CorrectQuestions}/{questions.QuestionsAnswered}"
        };

        // Submit button - used to mark a question
        Point buttonSize = new Point(40, 40);
        Rectangle submitRect = new Rectangle(new Point(surroundRect.Right - 20 - buttonSize.X, surroundRect.Bottom - 20 - buttonSize.Y), buttonSize);
        submitButton = new Button(tickboxTexture, font, submitRect, UnclickedColour, PenColour)
        {
            HoverColour = HoverColour
        };
        submitButton.Click += SubmitButton_Click;

        // Reset button - used to generate a new question
        Rectangle resetRect = new Rectangle(new Point(submitRect.X - 20 - buttonSize.X, submitRect.Y), buttonSize);
        resetQuestionButton = new Button(resetTexture, font, resetRect, UnclickedColour, PenColour)
        {
            HoverColour = HoverColour
        };
        resetQuestionButton.Click += ResetButton_Click;

        // Question label
        Point questionSize = new Point((int)(surroundRect.Width * 0.85), 80);
        Rectangle questionRect = new Rectangle(new Point(surroundRect.Center.X - (questionSize.X / 2), surroundRect.Top + 40), questionSize);
        questionLabel = new Label(labelTexture, UnclickedColour, questionRect, font, PenColour);

        // Input for mathematical questions
        Point mathsSize = new Point(questionSize.X, (int)(surroundRect.Height * 0.5));
        Rectangle mathsRect = new Rectangle(new Point(surroundRect.Center.X - (mathsSize.X / 2), questionRect.Bottom + 20), mathsSize);
        mathematical = new NumInput(largeTextInputTexture, mathsRect, font, UnclickedColour, PenColour, "Enter text here...", Color.Gold)
        {
            HoverColour = HoverColour
        };

        // Input for MCQ
        Rectangle mcqRect = mathsRect;
        correctMCQIndex = 0;
        multipleChoice = new RadioButtons(buttonUnchecked, buttonChecked, labelTexture, surroundTexture, mcqRect, new Vector2(mcqRect.Left + 20, mcqRect.Top + 20), new string[]{"", "", "", ""}, font, UnclickedColour, HoverColour, PenColour, 0);

        objects.Add(surroundBox);
        objects.Add(scoreLabel);
        objects.Add(submitButton);
        objects.Add(resetQuestionButton);
        objects.Add(questionLabel);
        objects.Add(mathematical);

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
            multipleChoice.Draw(_spriteBatch);
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
            multipleChoice.Update(gameTime);
        }
    }
    #endregion Drawing and Updating

    public override void ChangePenColour()
    {
        multipleChoice.ChangePenColour(PenColour);
        scoreLabel.PenColour = PenColour;
        mathematical.PenColour = PenColour;
        multipleChoice.ChangePenColour(PenColour);
        questionLabel.PenColour = PenColour;
    }

    private void NewQuestion()
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
    private void SubmitButton_Click(object sender, EventArgs e)
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

    private void ResetButton_Click(object sender, EventArgs e)
    {
        correctMCQIndex = 0;
        multipleChoice.ChangeIndex(correctMCQIndex);
        NewQuestion();
        surroundBox.ChangeColourMask(Color.White);
    }
    #endregion Events
    #endregion Methods
}