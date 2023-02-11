using ThermalClasses.PhysicsLaws;

namespace ThermalClasses.QuestionHandling;

public class QuestionHandler
{
    #region Fields
    private int correctQuestions, questionsAnswered, correctStreak, incorrectStreak;
    private double difficulty;
    private Random rnd;
    #endregion

    #region Properties
    public Question CurrentQuestion;
    public int QuestionsAnswered => questionsAnswered;
    public int CorrectQuestions => correctQuestions;
    public double Streak { get { return correctStreak - incorrectStreak; } }
    #endregion

    #region Methods
    public QuestionHandler()
    {
        correctQuestions = questionsAnswered = 0;
        correctStreak = incorrectStreak = 0;
        difficulty = 0.4d;
    }

    public void GenerateQuestion()
    {
        double choice = rnd.NextDouble();
        if (choice < 0.5d)
        {
            GenerateMCQ();
        }
        else
        {
            GenerateMathematicalQuestion();
        }
    }

    private void GenerateMCQ()
    {
        CurrentQuestion = GenerateQuestion(DatabaseConnection.GetMCQs(difficulty));
    }

    private void GenerateMathematicalQuestion()
    {
        CurrentQuestion = GenerateQuestion(DatabaseConnection.GetMathematicalQs(difficulty));
        // Assign random values to question

        List<double> numbers = new();
        string[] keyWords = CurrentQuestion.question.Split(" ");
        for (int i = 0; i < keyWords.Length; i++)
        {
            if (keyWords[i].Contains("___"))
            {
                keyWords[i] = keyWords[i].Where(x => x != '_').ToString();
                double currentNum = rnd.Next(1, 200) * (1 + rnd.NextDouble());
                numbers.Add(currentNum);
                keyWords[i] = currentNum.ToString() + keyWords[i];
            }
        }

        if (keyWords[0].Contains("Volume"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcVolume(numbers[0], (int)numbers[2], numbers[1]).ToString());
        }
        else if (keyWords[0].Contains("Temperature"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcTemperature(numbers[1], numbers[0], (int)numbers[2]).ToString());
        }
        else if (keyWords[0].Contains("Pressure"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcPressure(numbers[0], (int)numbers[2], numbers[1], 4).ToString());
        }
        else if (keyWords[0].Contains("Moles"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcMoles((int)numbers[0], numbers[1], numbers[2]).ToString());
        }
        else if (keyWords[0].Contains("Particles"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcParticles((int)numbers[0], numbers[1], numbers[2]).ToString());
        }

        keyWords[0] = "";
        CurrentQuestion.question = ListToString(keyWords);
    }

    private string ListToString(string[] input)
    {
        string output = "";
        for (var i = 0; i < input.Length; i++)
        {
            output += input[i];
            if (i + 1 != input.Length)
            {
                output += " ";
            }
        }
        return output;
    }

    private Question GenerateQuestion(List<Question> input)
    {
        double lowerDifficulty = difficulty * (1 / 4d);
        double upperDifficulty = difficulty - (difficulty * (1 / 4d));

        if (Streak < -3) // Pick lower quarter of questions
        {
            input = input.Where(x => x.difficulty <= lowerDifficulty).ToList();
        }
        else if (Streak > 3) // Pick upper quarter of questions
        {
            input = input.Where(x => x.difficulty >= upperDifficulty).ToList();
        }
        else // Choose middle 50% of questions
        {
            input = input.Where(x => x.difficulty > lowerDifficulty && x.difficulty < upperDifficulty).ToList();
        }

        // Choose a random question from the resulting list
        Random rnd = new Random();
        return input[rnd.Next(input.Count)];
    }

    public bool AnswerMathematicalQuestion(double answer)
    {
        questionsAnswered++;
        bool isCorrect = CurrentQuestion.CheckAnswer(answer, answer * 0.02); // 2% uncertainty
        if (isCorrect)
        {
            incorrectStreak = 0;
            correctStreak++;
            correctQuestions++;
        }
        else
        {
            incorrectStreak++;
            correctStreak = 0;
        }
        ChangeDifficulty();
        return isCorrect;
    }

    private void ChangeDifficulty()
    {
        double a = Streak;
        if (a >= 0)
        {
            difficulty += (a + 1) / 20 * Math.Pow(difficulty - 1, 2);
        }
        else
        {
            difficulty += (a + 1) / 20 * Math.Pow(difficulty, 2);
        }
    }
    #endregion
}
