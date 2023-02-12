using System.Data;
using Microsoft.Data.Sqlite;
namespace ThermalClasses.QuestionHandling;

public static class DatabaseConnection
{
    #region Fields
    private static string connection = "Data Source=Questions.db";
    //private static string filename = "Questions.db";
    #endregion

    #region Properties
    #endregion

    #region Methods
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
            AD type = 'MCQ'";
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
