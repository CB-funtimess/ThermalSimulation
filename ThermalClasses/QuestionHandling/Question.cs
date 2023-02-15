namespace ThermalClasses.QuestionHandling;

public class Question
{
    #region Fields
    private string answer;
    #endregion

    #region Properties
    public string question;
    public string questionType;
    public QuestionType type;
    public double difficulty;
    public string first;
    public string second;
    public string third;
    public string Answer => answer;
    #endregion

    #region Methods
    /// <summary>
    /// Initialises a mathematical question.
    /// </summary>
    /// <param name="q">Question</param>
    /// <param name="type">Mathematical question type</param>
    /// <param name="difficulty">Rating of difficulty between 0 and 1</param>
    /// <param name="answer">answer to the question</param>
    public Question(string q, string questionType, QuestionType type, double difficulty, string answer)
    {
        question = q;
        this.questionType = questionType;
        this.type = type;
        this.difficulty = difficulty;
        this.answer = answer;
    }

    /// <summary>
    /// Initialises an multiple-choice question.
    /// </summary>
    /// <param name="q">Question</param>
    /// <param name="type">Multiple choice question type</param>
    /// <param name="difficulty">Rating of difficulty between 0 and 1</param>
    /// <param name="answer">answer to the question</param>
    /// <param name="firstAns">An incorrect answer</param>
    /// <param name="secondAns">An incorrect answer</param>
    /// <param name="thirdAns">An incorrect answer</param>
    public Question(string q, string questionType, QuestionType type, double difficulty, string answer, string firstAns, string secondAns, string thirdAns) : this(q, questionType, type, difficulty, answer)
    {
        first = firstAns;
        second = secondAns;
        third = thirdAns;
    }

    /// <summary>
    /// Checks a mathematical answer.
    /// </summary>
    /// <param name="input">User's answer</param>
    /// <returns></returns>
    public bool CheckAnswer(double input)
    {
        return double.Parse(answer) == input;
    }

    /// <summary>
    /// Checks a mathematical answer, with room for error.
    /// </summary>
    /// <param name="input">User's answer</param>
    /// <param name="uncertainty">Fixed uncertainty</param>
    /// <returns></returns>
    public bool CheckAnswer(double input, double uncertainty)
    {
        return input + uncertainty > double.Parse(answer) && input - uncertainty < double.Parse(answer);
    }

    public void SetAnswer(string answer)
    {
        this.answer = answer;
    }
    #endregion
}