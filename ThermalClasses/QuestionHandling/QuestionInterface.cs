using ThermalClasses.PhysicsLaws;

namespace ThermalClasses.QuestionHandling;

public class QuestionInterface
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
    public QuestionInterface()
    {
        rnd = new Random();
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
                keyWords[i] = keyWords[i].Remove(0, 3);
                double currentNum = Math.Round(rnd.Next(1, 250) * (1 + rnd.NextDouble()), 2);
                numbers.Add(currentNum);
                keyWords[i] = currentNum.ToString() + keyWords[i];
            }
        }

        // ALL EQUATIONS USED NEED MODIFYING TO USE n NOT N
        if (CurrentQuestion.questionType.Contains("Volume"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcVolume(numbers[0], numbers[2], numbers[1]).ToString());
        }
        else if (CurrentQuestion.questionType.Contains("Temperature"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcTemperature(numbers[1], numbers[0], numbers[2]).ToString());
        }
        else if (CurrentQuestion.questionType.Contains("Pressure"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcPressure(numbers[0], numbers[2], numbers[1]).ToString());
        }
        else if (CurrentQuestion.questionType.Contains("Moles"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcMoles((int)numbers[0], numbers[1], numbers[2]).ToString());
        }
        else if (CurrentQuestion.questionType.Contains("Particles"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.CalcParticles((int)numbers[0], numbers[1], numbers[2]).ToString());
        }
        else if (CurrentQuestion.questionType.Contains("Proportion"))
        {
            if (CurrentQuestion.questionType.Contains('1'))
            {
                CurrentQuestion.SetAnswer(PhysicsEquations.ProportionPressure(numbers[1], (int)numbers[0], numbers[2], (int)numbers[0], numbers[3]).ToString());
            }
            else if (CurrentQuestion.questionType.Contains('2'))
            {
                CurrentQuestion.SetAnswer(PhysicsEquations.ProportionTemperature(numbers[1], (int)numbers[0], numbers[2], numbers[1], (int)numbers[3]).ToString());
            }
            else if (CurrentQuestion.questionType.Contains('3'))
            {
                CurrentQuestion.SetAnswer(PhysicsEquations.ProportionPressure(numbers[1], (int)numbers[0], numbers[2], (int)numbers[3], numbers[2]).ToString());
            }
        }
        else if (CurrentQuestion.questionType.Contains("VRMS"))
        {
            if (CurrentQuestion.questionType.Contains('1'))
            {
                CurrentQuestion.SetAnswer(PhysicsEquations.CalcVRMS(numbers[1], numbers[0], numbers[2], numbers[3] * 0.001).ToString());
            }
            else if (CurrentQuestion.questionType.Contains('2'))
            {
                CurrentQuestion.SetAnswer(PhysicsEquations.CalcVRMS(numbers[0], numbers[1] * 0.001).ToString());
            }
        }
        else if (CurrentQuestion.questionType.Contains("BasicV"))
        {
            if (numbers.Count == 2)
            {
                CurrentQuestion.SetAnswer(PhysicsEquations.CalcCylinderVolume(numbers[0], numbers[1]).ToString());
            }
            else
            {
                CurrentQuestion.SetAnswer(PhysicsEquations.CalcBoxVolume(numbers[0], numbers[1], numbers[2]).ToString());
            }
        }
        else if (CurrentQuestion.questionType.Contains("BasicM"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.MolesToNumber(numbers[0]).ToString());
        }
        else if (CurrentQuestion.questionType.Contains("BasicP"))
        {
            CurrentQuestion.SetAnswer(PhysicsEquations.NumberToMoles((int)numbers[0]).ToString());
        }

        keyWords[0] = "";
        CurrentQuestion.question = ListToString(keyWords);
    }

    private static string ListToString(string[] input)
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
        bool isCorrect = CurrentQuestion.CheckAnswer(answer, answer * 0.05); // 5% uncertainty
        AnswerQuestion(isCorrect);
        return isCorrect;
    }

    public bool AnswerMultipleChoiceQuestion(int index, int correctIndex)
    {
        questionsAnswered++;
        bool isCorrect = correctIndex == index;
        AnswerQuestion(isCorrect);
        return isCorrect;
    }

    private void AnswerQuestion(bool isCorrect)
    {
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
