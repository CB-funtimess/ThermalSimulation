using Microsoft.Data.Sqlite;
namespace ThermalClasses.QuestionHandling;

public static class DatabaseConnection
{
    #region Fields
    private static string connection = "Data Source=Questions.db";
    private static string filename = "Questions.db";
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

    public static void InputMathQuestion(Question question)
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
            @"INSERT INTO questions(question, type, difficulty, answer)
            VALUES(
                $question,
                $type,
                $difficulty,
                $answer
            )";
            command.Parameters.AddWithValue("$question", question.question);
            command.Parameters.AddWithValue("$type", question.type.ToString());
            command.Parameters.AddWithValue("$difficulty", question.difficulty.ToString());
            command.Parameters.AddWithValue("$answer", question.Answer);

            command.ExecuteNonQueryAsync();
        }
    }

    public static void InputMCQuestion(Question question)
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
            @"INSERT INTO questions(question, type, difficulty, answer, first, second, third)
            VALUES(
                $question,
                $type,
                $difficulty,
                $answer,
                $first,
                $second,
                $third
            )";
            command.Parameters.AddWithValue("$question", question.question);
            command.Parameters.AddWithValue("$type", question.type.ToString());
            command.Parameters.AddWithValue("$difficulty", question.difficulty.ToString());
            command.Parameters.AddWithValue("$answer", question.Answer);
            command.Parameters.AddWithValue("$first", question.first);
            command.Parameters.AddWithValue("$second", question.second);
            command.Parameters.AddWithValue("$third", question.third);

            command.ExecuteNonQueryAsync();
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
                while(reader.Read())
                {
                    Question tempQuestion = new Question(reader.GetString(0), ToQuestion(reader.GetString(1)), reader.GetDouble(2), reader.GetString(3));
                    questions.Add(tempQuestion);
                }
            }
        }

        return questions;
    }

    private static QuestionType ToQuestion(string input)
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
                while(reader.Read())
                {
                    Question tempQuestion = new Question(reader.GetString(0), ToQuestion(reader.GetString(1)), reader.GetDouble(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6));
                    questions.Add(tempQuestion);
                }
            }
        }

        return questions;
    }
    #endregion
}
