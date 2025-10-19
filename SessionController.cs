using Spectre.Console;
using System.Data.SqlClient;
namespace CodingTracker;

internal class SessionController
{
    private readonly Database db = new();

    public bool AddSession(DateTime start, DateTime end)
    {
        var session = new CodingSession();
        session.Start = start;
        session.End = end;
        db.Insert(session);            
        return true;
    }

    public List<CodingSession> ViewAllSessions()
    {
        try
        {
            return db.GetAll();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]{ex.Message}[/]");
            return new List<CodingSession>();
        }
    }
   
    public List<CodingSession> ViewByRange(DateTime StartDate, DateTime EndDate)
    {
        try
        {
            return db.GetByRange(StartDate, EndDate);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]{ex.Message}[/]");
            return new List<CodingSession>();
        }
    }

    public CodingSession? ViewById(int id)
    {
        try
        {
            return db.GetById(id);
        }
        catch (SqlException sqlEx)
        {
            AnsiConsole.MarkupLine($"[bold red]Database error: {sqlEx.Message}[/]");
            return null;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]{ex.Message}[/]");   
            return null;
        }
    }

    public CodingSession? ViewLatestSession()
    {
        try
        {
            return db.GetLatest();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]{ex.Message}[/]");
            return null;
        }
    }

    public bool TryGetSession(int id, out CodingSession session)
    {
        session = ViewById(id)!;
        if (session is null || session.SessionId == 0)
        {
            session = null!;
            return false;
        }
        return true;
    }

    public float TotalHours(List<CodingSession> sessions)
    {
        float totalHours = 0;
        foreach (var session in sessions)
        {
            totalHours += (float)(session.End - session.Start).TotalHours;
        }
        return totalHours;
    }
    public float AverageHours(List<CodingSession> sessions)
    {
        if (sessions.Count == 0) return 0;
        return TotalHours(sessions) / sessions.Count;
    }

    public CodingSession DeleteSession(int id)
    {
        var session = ViewById(id);
        if (session == null)
        {
            AnsiConsole.MarkupLine("[bold red]Session not found.[/]");
            return new CodingSession();
        }
        
        db.Delete(session);
        AnsiConsole.MarkupLine("[bold green]Session deleted successfully[/]");
        return session;
    }

    public bool UpdateSession(int id, DateTime newStart, DateTime newEnd)
    {
        var session = ViewById(id);
        if (session == null)
        {
            AnsiConsole.MarkupLine("[bold red]Session not found.[/]");
            return false;
        }

        session.Start = newStart;
        session.End = newEnd;

        db.Update(session);
        AnsiConsole.MarkupLine("[bold green]Session updated successfully[/]");
        return true;
    }


    public bool InsertTestData()
    {
        db.InsertTestData();
        return true;
    }
}