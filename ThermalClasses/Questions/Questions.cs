using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ThermalClasses.PhysicsLaws;

namespace ThermalClasses.Questions;

public class Questions
{
    #region Fields
    private double difficulty; // Value between 0 and 1 to determine how hard the question is
    private int correctQuestions;
    private int questionsAnswered;
    #endregion

    #region Properties
    public Question CurrentQuestion;
    public int QuestionsAnswered => questionsAnswered;
    public int CorrectQuestions => correctQuestions;
    #endregion

    #region Methods
    public Questions()
    {
        difficulty = 0.1;
        correctQuestions = questionsAnswered = 0;
        CurrentQuestion = new Question();
    }

    public Question GenerateMCQ()
    {
        List<Question> possibleQuestions = new List<Question>(); // A list of possible questions are added based on the current difficulty
        Random rnd = new Random();
        // Choose an equation and assign integer values to all inputs
        if (difficulty <= 0.3) // Ask easier questions
        {
            const double easyWeightingTotal = 0.6;
            const int numEasyQuestions = 4;
            // Volume and moles questions
            // Two volume questions
            // Box volume
            int length = rnd.Next(1, 1000);
            int width = rnd.Next(1, 500);
            int height = rnd.Next(1, 100);
            Question boxVolume = new Question($"A box has a height of {height}cm, a width of {width}cm, and a length of {width}cm. What is the volume of the box in metres?", easyWeightingTotal / numEasyQuestions, PhysicsEquations.CalcVolume(width / 100, height / 100, length / 100));
            double radius = (rnd.NextDouble() + 0.1) * rnd.Next(1, 75);
            Question cylinderVolume = new Question("A cylinder has a circular radius of {radius}m and a length of {length}m. What is the volume in metres?", easyWeightingTotal / numEasyQuestions, PhysicsEquations.CalcVolume(length, radius));

            possibleQuestions.Add(boxVolume);
            possibleQuestions.Add(cylinderVolume);

            // Two mole conversion questions
            double numMoles = (rnd.NextDouble() + 0.1) * rnd.Next(1, 50);
            int numParticles = rnd.Next(500, int.MaxValue);
            Question molesCalc = new Question("Calculate the number of moles in a sample of {numParticles} particles.", easyWeightingTotal / numEasyQuestions, PhysicsEquations.NumberToMoles(numParticles));
            Question particlesCalc = new Question("Calculate the number of particles in {numMoles}mol of gas.", easyWeightingTotal / numEasyQuestions, PhysicsEquations.MolesToNumber(numMoles));

            possibleQuestions.Add(molesCalc);
            possibleQuestions.Add(particlesCalc);
        }
        else if (difficulty >= 0.7)
        {
            const double hardWeightingTotal = 0.6;
            const int numHardQuestions = 8;
            // VRMS and kinetic theory questions
            // Proportionality questions
            // Three vrms questions

            // Three proportionality questions

            // Two more advanced question types

        }
        const double normalWeightingTotal = 0.4;

        // Choose a question from the list and calculate the answer
        return new Question();
    }

    public bool AnswerQuestion(double answer)
    {
        questionsAnswered++;
        // Answer rounded to 3 decimal places
        // Allow room for error
        if (Math.Round(answer, 3) == Math.Round(CurrentQuestion.answer, 3) || Math.Round(answer, 3) + 0.001 == Math.Round(CurrentQuestion.answer, 3) || Math.Round(answer, 3) - 0.001 == Math.Round(CurrentQuestion.answer, 3))
        {
            correctQuestions++;
            difficulty += 0.1;
            return true;
        }
        difficulty -= 0.1;
        return false;
    }
    #endregion
}

public struct Question
{
    public string question;
    public double weighting;
    public double answer;
    public Question()
    {
        question = "";
        weighting = answer = 0;
    }

    public Question(string question, double weighting, double answer)
    {
        this.question = question;
        this.weighting = weighting;
        this.answer = answer;
    }
}
