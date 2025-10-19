using Spectre.Console;
using System.Diagnostics;
using System.Globalization;
namespace CodingTracker;

internal class CodingSessionView
{
    public static void Title()
    {
        AnsiConsole.MarkupLine("[bold cyan][[Coding Tracker]][/]");
    }

    public static void PageTitle(string title)
    {
        AnsiConsole.MarkupLine($"""
            [bold]{title}[/]
            ----------------
            """);
            
    }

    public void Run()
    {
        SessionController sessionController = new();
        Validation dateTimeValidator = new();
        Title();
        while (true)
        {            
            DateTime startDateTime, endDateTime;
            MenuOption selectedOption = MainMenu();
            switch (selectedOption)
            {
                case MenuOption.AddSession:
                    PageTitle("Add Session");                    
                    while (true)
                    {
                        try
                        {
                            startDateTime = GetValidatedDateTime(dateTimeValidator, "Enter start date (dd-MM-yyyy): ", "Enter start time (HH:mm): ");
                            endDateTime = GetValidatedDateTime(dateTimeValidator, "Enter end date (dd-MM-yyyy): ", "Enter end time (HH:mm): ");
                            if (!dateTimeValidator.ValidateDateTimeRange(startDateTime, endDateTime))
                            {
                                throw new ArgumentException("Invalid date range.");
                            }
                            // Break out of the loop if inputs are valid
                            break;
                        }
                        catch (Exception e)
                        {
                            AnsiConsole.MarkupLine($"[red]{e.Message} Please re-enter the values.[/]");
                        }
                    }

                    // Add the session after successful validation
                    sessionController.AddSession(startDateTime, endDateTime);
                    AnsiConsole.MarkupLine("[bold green]Session added successfully[/]");

                    break;

                case MenuOption.ViewAllSessions:
                    PageTitle("View All Sessions");
                    List<CodingSession> allSessions = sessionController.ViewAllSessions();
                        DisplaySession(allSessions);
                    break;

                case MenuOption.ViewByRange:
                    PageTitle("View By Range");
                    bool isValidRange = false;
                    do
                    {
                        startDateTime = GetValidatedDateTime(dateTimeValidator, "Enter start date (dd-MM-yyyy): ");
                        endDateTime = GetValidatedDateTime(dateTimeValidator, "Enter end date (dd-MM-yyyy): ");
                        isValidRange = dateTimeValidator.ValidateDateTimeRange(startDateTime, endDateTime);

                        if (!isValidRange)
                        {
                            AnsiConsole.MarkupLine("[bold red]Invalid date range. Please try again.[/]");
                        }
                    } while (!isValidRange);

                    List<CodingSession> sessionRange = sessionController.ViewByRange(startDateTime, endDateTime);

                        DisplaySession(sessionRange);
                        DisplayStats(sessionRange);
                    break;

                case MenuOption.ViewById:
                    PageTitle("View By Id");

                    int id = AnsiConsole.Ask<int>("Enter the session id: ");
                    if (sessionController.TryGetSession(id, out CodingSession session))
                    {
                      DisplaySession(session);
                    }
                        break;

                case MenuOption.UpdateSession:
                    PageTitle("Update Session");
                    int updateId = AnsiConsole.Ask<int>("Enter the session id to update: ");
                    if (!sessionController.TryGetSession(updateId, out CodingSession existingSession))
                        break;
                    DisplaySession(existingSession);
                    startDateTime = GetValidatedDateTime(dateTimeValidator, "Enter start date (dd-MM-yyyy): ", "Enter start time (HH:mm): ");
                    endDateTime = GetValidatedDateTime(dateTimeValidator, "Enter end date (dd-MM-yyyy): ", "Enter end time (HH:mm): ");
                    if (!dateTimeValidator.ValidateDateTimeRange(startDateTime, endDateTime))
                    {
                        throw new ArgumentException("Invalid date range.");
                    }

                    bool updated = sessionController.UpdateSession(updateId, startDateTime, endDateTime);

                    if (!updated)
                    {
                        AnsiConsole.MarkupLine("[bold red]Update failed.[/]");
                    }

                    break;

                case MenuOption.Stopwatch:
                    CodingSession stopWatchSession = new CodingSession();
                    AnsiConsole.MarkupLine("[bold green]Stopwatch started. Press Enter to stop...[/]");
                    stopWatchSession.Start = DateTime.Now;
                    var stopwatch = Stopwatch.StartNew();
                    while (true)
                    {
                        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
                            break;

                        TimeSpan t = stopwatch.Elapsed;
                        AnsiConsole.Write($"\rElapsed: {t:hh\\:mm\\:ss}");

                        Thread.Sleep(1000);
                    }

                    stopWatchSession.End = DateTime.Now;
                    stopwatch.Stop();
                    AnsiConsole.WriteLine();
                    if (sessionController.AddSession(stopWatchSession.Start, stopWatchSession.End))
                    {
                        AnsiConsole.MarkupLine("[bold green]Stopwatch session recorded successfully[/]");
                        DisplaySession(sessionController.ViewLatestSession());
                    }
                    break;

                case MenuOption.DeleteSession:
                    PageTitle("Delete Session");              

                    int deleteId = AnsiConsole.Ask<int>("Enter the session id: ");
                    sessionController.DeleteSession(deleteId);

                    break;
                case MenuOption.InsertTestData:
                    PageTitle("Inserting test data");
                    if(sessionController.InsertTestData())
                    {
                        AnsiConsole.MarkupLine("[bold green]Test data inserted successfully[/]");
                    }
                    break;
                case MenuOption.Exit:
                    Environment.Exit(0);
                    return;

                
            }            
            AnsiConsole.MarkupLine("""

                [bold]Press any key to continue...[/]
                """);
                
            Console.ReadLine();
            AnsiConsole.Clear();
        }
    }

    public void DisplaySession(CodingSession session)
    {
        AnsiConsole.MarkupLine($"[bold]Session Id:[/] [green]{session.SessionId}[/]");
        AnsiConsole.MarkupLine($"[bold]Start Time:[/] {session.Start}");
        AnsiConsole.MarkupLine($"[bold]End Time:[/] {session.End}");
        AnsiConsole.MarkupLine($"[bold]Duration:[/] [cyan]{session.Duration:hh\\:mm}[/]");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("----------------");
        AnsiConsole.MarkupLine("");
    }

    public void DisplayStats(List<CodingSession> sessions)
    {
        SessionController sessionController = new();
        float totalHours = sessionController.TotalHours(sessions);
        float averageHours = sessionController.AverageHours(sessions);
        AnsiConsole.MarkupLine($"[bold]Total Hours:[/] [green]{totalHours:F2}[/]");
        AnsiConsole.MarkupLine($"[bold]Average Hours per Session:[/] [green]{averageHours:F2}[/]");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("----------------");
        AnsiConsole.MarkupLine("");
    }

    public void DisplaySession(List<CodingSession> sessions)    {        

        foreach (var session in sessions)
        {
            DisplaySession(session);
        }
    }

    public MenuOption MainMenu()
    {
        AnsiConsole.MarkupLine("""
            [Bold]Main Menu[/]
            ----------------
            [green]1[/] Add Session
            [green]2[/] View All Sessions
            [green]3[/] View By Range
            [green]4[/] View By Id
            [green]5[/] Update Session
            [green]6[/] StopWatch
            [green]7[/] Delete Session
            [green]8[/] Insert Test Data
            [red]9[/] Exit
            """);

       while (true)
       if (int.TryParse(AnsiConsole.Ask<string>("[green]Enter your choice[/]"), out int choice) && Enum.IsDefined(typeof(MenuOption), choice))
        {
                Console.Clear();
                return (MenuOption)choice;
        }
       else 
        {
                Console.Clear();
                AnsiConsole.MarkupLine("[red]Invalid choice[/]");
        }
    }
    private DateTime GetValidatedDateTime(Validation validator, string datePrompt, string timePrompt = "Null")
    {
        while (true)
        {
            AnsiConsole.Markup(datePrompt);
            string date = Console.ReadLine()!;
            if (!validator.ValidateDate(date))
            {
                AnsiConsole.MarkupLine("[red]Invalid date format. Please try again.[/]");
                continue;
            }

            //If Time is not provided, return the date
            if (timePrompt == "Null")
            {
                return DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            AnsiConsole.Markup(timePrompt);
            string time = Console.ReadLine()!;
            if (!validator.ValidateTime(time))
            {
                AnsiConsole.MarkupLine("[red]Invalid time format. Please try again.[/]");
                continue;
            }

            return DateTime.ParseExact($"{date} {time}", "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
enum MenuOption{AddSession = 1, ViewAllSessions, ViewByRange, ViewById, UpdateSession, Stopwatch, DeleteSession, InsertTestData, Exit}
