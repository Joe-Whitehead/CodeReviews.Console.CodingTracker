using Dapper;
using Spectre.Console;
using System.Data.SQLite;

namespace CodingTracker;

internal class Database
{
    private readonly string _databasePath;
    private readonly string _connectionString;
    private readonly SQLiteConnection DbConnection;

    public Database()
    {
        _databasePath = System.Configuration.ConfigurationManager.AppSettings.Get("DatabasePath") ?? "";
        _connectionString = System.Configuration.ConfigurationManager.AppSettings.Get("ConnectionString") ?? "";          
        DbConnection = new SQLiteConnection(_connectionString);
        DbConnection.Open();

        if (!File.Exists(_databasePath) || new FileInfo(_databasePath).Length == 0 || GetAll().Count < 1)
            CreateIfNotExist();
    }

    public void CreateIfNotExist()
    {
        string createTable = @"
            CREATE TABLE IF NOT EXISTS CodeSessions (
                SessionId INTEGER PRIMARY KEY,
                Start TEXT,
                End TEXT
            );
        ";
        DbConnection.Execute(createTable);   
    }

    internal void InsertTestData()
    {
        DeleteAll();
        const int sessionCount = 50;
        const string sql = "INSERT INTO CodeSessions (Start, End) VALUES (@Start, @End);";

        DateTime currentStart = new (2025, 2, 1); // Initial start date
        Random random = new();

        for (int i = 0; i < sessionCount; i++)
        {
            // Ensure the current start time is within the desired range (6 a.m. to midnight)
            if (currentStart.Hour < 6)
            {
                currentStart = currentStart.Date.AddHours(6); // Adjust to 6 a.m.
            }

            // Generate random session duration (1 to 4 hours)
            TimeSpan sessionDuration = TimeSpan.FromMinutes(random.Next(60, 240));
            DateTime end = currentStart.Add(sessionDuration);

            // Ensure the session ends before midnight
            if (end.Hour >= 24)
            {
                end = currentStart.Date.AddDays(1).AddHours(0); // Clamp to midnight
            }

            try
            {
                // Execute SQL with an anonymous object
                DbConnection.Execute(sql, new { Start = currentStart, End = end });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting session {i + 1}: {ex.Message}");
            }
            // Generate random gap before the next session (10 minutes to 2 hours)
            TimeSpan randomGap = TimeSpan.FromMinutes(random.Next(10, 120));
            currentStart = end.Add(randomGap);

            // Skip to 6 a.m. the next day if the next start time is past midnight
            if (currentStart.Hour < 6)
            {
                currentStart = currentStart.Date.AddDays(1).AddHours(6);
            }
        }
    }

    private void DeleteAll()
    {
        string sql = "DELETE FROM CodeSessions;";
        DbConnection.Execute(sql);
    }

    public void Insert(CodingSession session)
    {
        string sql = "INSERT INTO CodeSessions (Start, End) VALUES (@Start, @End);";
        DbConnection.Execute(sql, session);
    }

    public void Delete(CodingSession session)
    {
        string sql = "DELETE FROM CodeSessions WHERE SessionId = @SessionId;";
        DbConnection.Execute(sql, session);
    }

    public void Update(CodingSession session)
    {
       string sql = "UPDATE CodeSessions SET Start = @Start, End = @End WHERE SessionId = @SessionId;";
       DbConnection.Execute(sql, session);
    }

    public List<CodingSession> GetAll()
    {
        string sql = "SELECT * FROM CodeSessions;";
        var sessions = DbConnection.Query<CodingSession>(sql).ToList();
        if (sessions.Count == 0)
            throw new Exception("No sessions found");

        return CalculateDuration(sessions);
    }
    public List<CodingSession> GetByRange(DateTime start, DateTime end)
    {
        string sql = "SELECT * FROM CodeSessions WHERE Start >= @start AND End <= @end;";
        var sessions = DbConnection.Query<CodingSession>(sql, new { start, end }).ToList();
        if (sessions.Count == 0)
            throw new Exception("No sessions found in the specified range");

        return CalculateDuration(sessions);
    }

    public CodingSession GetById(int id)
    {
        string sql = "SELECT * FROM CodeSessions WHERE SessionId = @id;";

        var session = DbConnection.QuerySingleOrDefault<CodingSession>(sql, new { id }) ?? throw new Exception($"No session found with ID {id}");
        
        // Calculate duration for the session
        session.Duration = session.End - session.Start;

        return session;
                
    }

    public CodingSession GetLatest()
    {
        string sql = "SELECT * FROM CodeSessions ORDER BY Start DESC LIMIT 1;";
        var session = DbConnection.QuerySingleOrDefault<CodingSession>(sql) ?? throw new Exception("No sessions found");
        // Calculate duration for the session
        session.Duration = session.End - session.Start;
        return session;
    }

    private List<CodingSession> CalculateDuration(List<CodingSession> sessions)
    {
        foreach (var session in sessions)
        {
            session.Duration = session.End - session.Start;
        }
        return sessions;
    }
}