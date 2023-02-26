using System.Data;
using Microsoft.Data.Sqlite;
namespace ThermalClasses.QuestionClasses;

public static class DatabaseConnection
{
    #region Fields
    private static string connection = "Data Source=Questions.db";
    #endregion

    #region Properties
    #endregion

    #region Methods
    // Creates the table
    public static void InitialiseTable()
    {
        string connectionString = new SqliteConnectionStringBuilder(connection)
        {
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.OpenAsync();

            var command = dbConnection.CreateCommand();
            command.CommandText =
            @"CREATE TABLE questions
            (
                question TEXT PRIMARY KEY NOT NULL,
                questiontype TEXT NOT NULL,
                type TEXT NOT NULL,
                difficulty REAL NOT NULL,
                answer REAL NOT NULL,
                first TEXT,
                second TEXT,
                third TEXT
            )";
            command.ExecuteNonQueryAsync();
        }
    }

    public static void FillTableMathematical()
    {
        string[] mathsQuestions = {
            "A box under a pressure of ___Pa has a temperature of ___K and is filled with ___mol of gas. Calculate the volume.",
            "A box of volume ___m^3 under a pressure of ___Pa contains ___mol of gas. Calculate the temperature.",
            "A box of volume ___m^3 under a temperature of ___K contains ___mol of gas. Calculate the pressure.",
            "A box of volume ___m^3 under a temperature of ___K has a pressure of ___Pa. Calculate the number of moles of gas in the box.",
            "A box of volume ___m^3 under a temperature of ___K has a pressure of ___Pa. Calculate the number gas particles in the box.",
            "A box with a constant volume of ___m^3 has a pressure of ___Pa has an initial temperature of ___K. It is heated to ___K. What is the new pressure?",
            "A box with a variable volume of ___m^3 has a constant pressure of ___Pa and an initial temperature of ___K. The volume is changed to ___m^3. What is the new temperature?",
            "A box with an initial volume of ___m^3 and pressure of ___Pa is kept under a constant temperature of ___K. The volume is slowly changed to ___m^3. What is the new pressure?",
            "A box with a volume of ___m^3 has a pressure of ___Pa. It contains ___mol of gas, each with a mass of ___g. Calculate the root mean square velocity of the particles.",
            "A box is kept at a temperature of ___K, and each particle has a mass of ___g. Calculate the root mean square velocity of the particles.",
            "A box has a length of ___m , a width of ___m and a height of ___m. Calculate the volume of the box.",
            "A cylinder has a length of ___m and a circular radius of ___m. Calculate the volume of the cylinder.",
            "Convert ___mol of gas into the number of particles.",
            "Convert ___ particles into the number of moles."
        };
        string[] questionTypes = {
            "Volume",
            "Temperature",
            "Pressure",
            "Moles",
            "Particles",
            "Proportion1",
            "Proportion2",
            "Proportion3",
            "VRMS1",
            "VRMS2",
            "BasicV",
            "BasicV",
            "BasicM",
            "BasicP"
        };
        double[] difficulties = { 0.3, 0.3, 0.3, 0.3, 0.3, 0.7, 0.7, 0.7, 0.8, 0.8, 0.0, 0.0, 0.0, 0.0 };
        Question[] questions = new Question[mathsQuestions.Length];
        for (var i = 0; i < questions.Length; i++)
        {
            questions[i] = new Question(mathsQuestions[i], questionTypes[i], QuestionType.Mathematical, difficulties[i], " ");
        }
        InputMathQuestions(questions);
    }

    public static void FillTableMCQ()
    {
        string[] mcqs = {
            "A gas occupies a volume V. Its particles have a root mean square speed (crms) of u. The gas is compressed at constant temperature to a volume 0.5V. What is the root mean square speed of the gas particles after compression?",
            "Which is not an assumption about gas particles in the kinetic theory model for a gas?",
            "Two flasks X and Y are filled with an ideal gas and are connected by a tube of negligible volume compared to that of the flasks. The volume of X is twice the volume of Y. X is held at a temperature of 150 K and Y is held at a temperature of 300 K. What is the ratio (mass of gas in X) / (mass of gas in Y)?",
            "The average mass of an air molecule is 4.8 x 10^-26 kg. What is the mean square speed of an air molecule at 750 K?",
            "What assumptions are made about the size of molecules in the kinetic theory model?",
            "What is the formula to convert a temperature from degrees, C, to kelvin, K?",
            "Which value is the correct representation of absolute 0?",
            "What is the correct formula for the work done on a gas to change its volume under constant pressure?",
            "What is the change in momentum for a particle of mass m colliding with a wall with a velocity of u?",
            "What does the kinetic theory model assume about the internal energy of particles?",
            "Why is the root mean square value of speed used in kinetic theory equations, as opposed to the mean?"
        };
        string[,] answers = new string[,]{
            {"u", "u/2", "2u", "4u"},
            {"They travel between the container walls in negligibly short times.", "They collide elastically with the container walls", "They have negligible size compared to the distance between the container walls.", "They collide with the container walls in negligibly short times."},
            {"4", "8", "0.125", "0.25"},
            {"6.5x10^5 m2 s-2", "3.3x10^5 m2 s-2", "4.3x10^5 m2 s-2", "8.7x10^5 m2 s-2"},
            {"The size is negligible.", "The size is massive.", "The size doesn't matter.", "Molecules are as large as the container"},
            {"K = C + 273", "K = C - 273", "K = C", "K = (C - 32) * 5/9"},
            {"-273 degrees C", "-273K", "0 degrees C", "273K"},
            {"Work done = pressure * change in volume", "Work done = temperature", "Work done = pressure * change in temperature", "Work done = volume * temperature"},
            {"2mu", "mu", "-mu", "-2mu"},
            {"There is no potential energy.", "There is no kinetic energy.", "Kinetic energy = potential energy.", "There is no internal energy."},
            {"Components of velocity could be negative so mean would not be representative.", "Root mean square is more accurate.", "Root mean square gives a negative answer", "Root mean square is more useful for random motion."}
        };
        double[] difficulties = {0.5, 0.5, 0.8, 0.3, 0.0, 0.0, 0.0, 0.4, 0.5, 0.5, 0.9};
        Question[] questions = new Question[mcqs.Length];
        for (var i = 0; i < questions.Length; i++)
        {
            questions[i] = new Question(mcqs[i], "Kinetic Theory", QuestionType.MCQ, difficulties[i], answers[i,0], answers[i,1], answers[i,2], answers[i,3]);
        }
        InputMCQuestions(questions);
    }

    public static void InputMathQuestion(Question question)
    {
        string connectionString = new SqliteConnectionStringBuilder(connection)
        {
            Mode = SqliteOpenMode.ReadWrite
        }.ToString();

        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.OpenAsync();

            var command = dbConnection.CreateCommand();
            command.CommandText =
            @"INSERT INTO questions(question, questiontype, type, difficulty, answer)
            VALUES(
                $question,
                $questiontype,
                $type,
                $difficulty,
                $answer
            )";
            // AddWithValue prevents SQL injection
            command.Parameters.AddWithValue("$question", question.question);
            command.Parameters.AddWithValue("$questiontype", question.questionType);
            command.Parameters.AddWithValue("$type", question.type.ToString());
            command.Parameters.AddWithValue("$difficulty", question.difficulty.ToString());
            command.Parameters.AddWithValue("$answer", question.Answer);

            command.ExecuteNonQueryAsync();
        }
    }

    public static void InputMathQuestions(Question[] questions)
    {
        string connectionString = new SqliteConnectionStringBuilder(connection)
        {
            Mode = SqliteOpenMode.ReadWrite
        }.ToString();

        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.OpenAsync();

            for (var i = 0; i < questions.Length; i++)
            {
                var command = dbConnection.CreateCommand();
                command.CommandText =
                @"INSERT INTO questions(question, questiontype, type, difficulty, answer)
                VALUES(
                    $question,
                    $questiontype,
                    $type,
                    $difficulty,
                    $answer
                )";
                command.Parameters.AddWithValue("$question", questions[i].question);
                command.Parameters.AddWithValue("$questiontype", questions[i].questionType);
                command.Parameters.AddWithValue("$type", questions[i].type.ToString());
                command.Parameters.AddWithValue("$difficulty", questions[i].difficulty.ToString());
                command.Parameters.AddWithValue("$answer", questions[i].Answer);

                command.ExecuteNonQueryAsync();
            }
        }
    }

    public static void InputMCQuestion(Question question)
    {
        string connectionString = new SqliteConnectionStringBuilder(connection)
        {
            Mode = SqliteOpenMode.ReadWrite
        }.ToString();

        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.OpenAsync();

            var command = dbConnection.CreateCommand();
            command.CommandText =
            @"INSERT INTO questions(question, questiontype, type, difficulty, answer, first, second, third)
            VALUES(
                $question,
                $questiontype,
                $type,
                $difficulty,
                $answer,
                $first,
                $second,
                $third
            )";
            command.Parameters.AddWithValue("$question", question.question);
            command.Parameters.AddWithValue("$questiontype", question.questionType);
            command.Parameters.AddWithValue("$type", question.type.ToString());
            command.Parameters.AddWithValue("$difficulty", question.difficulty.ToString());
            command.Parameters.AddWithValue("$answer", question.Answer);
            command.Parameters.AddWithValue("$first", question.first);
            command.Parameters.AddWithValue("$second", question.second);
            command.Parameters.AddWithValue("$third", question.third);

            command.ExecuteNonQueryAsync();
        }
    }

    public static void InputMCQuestions(Question[] questions)
    {
        string connectionString = new SqliteConnectionStringBuilder(connection)
        {
            Mode = SqliteOpenMode.ReadWrite
        }.ToString();

        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.OpenAsync();

            for (var i = 0; i < questions.Length; i++)
            {
                var command = dbConnection.CreateCommand();
                command.CommandText =
                @"INSERT INTO questions(question, questiontype, type, difficulty, answer, first, second, third)
                VALUES(
                    $question,
                    $questiontype,
                    $type,
                    $difficulty,
                    $answer,
                    $first,
                    $second,
                    $third
                )";
                command.Parameters.AddWithValue("$question", questions[i].question);
                command.Parameters.AddWithValue("$questiontype", questions[i].questionType);
                command.Parameters.AddWithValue("$type", questions[i].type.ToString());
                command.Parameters.AddWithValue("$difficulty", questions[i].difficulty.ToString());
                command.Parameters.AddWithValue("$answer", questions[i].Answer);
                command.Parameters.AddWithValue("$first", questions[i].first);
                command.Parameters.AddWithValue("$second", questions[i].second);
                command.Parameters.AddWithValue("$third", questions[i].third);

                command.ExecuteNonQueryAsync();
            }
        }
    }

    public static List<Question> GetMathematicalQs(double maxDifficulty)
    {
        List<Question> questions = new List<Question>();

        string connectionString = new SqliteConnectionStringBuilder(connection)
        {
            Mode = SqliteOpenMode.ReadOnly
        }.ToString();

        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.OpenAsync();

            var command = dbConnection.CreateCommand();
            command.CommandText =
            @"SELECT * FROM questions
            WHERE difficulty <= $maxdifficulty
            AND type = 'Mathematical'";
            command.Parameters.AddWithValue("$maxdifficulty", maxDifficulty);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Question tempQuestion = new Question(reader.GetString(0), reader.GetString(1), ToType(reader.GetString(2)), reader.GetDouble(3), reader.GetString(4));
                    questions.Add(tempQuestion);
                }
            }
        }

        return questions;
    }

    private static QuestionType ToType(string input)
    {
        if (input == "Mathematical")
        {
            return QuestionType.Mathematical;
        }
        return QuestionType.MCQ;
    }

    public static List<Question> GetMCQs(double maxDifficulty)
    {
        List<Question> questions = new List<Question>();

        string connectionString = new SqliteConnectionStringBuilder(connection)
        {
            Mode = SqliteOpenMode.ReadOnly
        }.ToString();

        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.OpenAsync();

            var command = dbConnection.CreateCommand();
            command.CommandText =
            @"SELECT * FROM questions
            WHERE difficulty <= $maxdifficulty
            AND type = 'MCQ'";
            command.Parameters.AddWithValue("$maxdifficulty", maxDifficulty);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Question tempQuestion = new Question(reader.GetString(0), reader.GetString(1), ToType(reader.GetString(2)), reader.GetDouble(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7));
                    questions.Add(tempQuestion);
                }
            }
        }

        return questions;
    }
    #endregion
}
