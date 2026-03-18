using System.Globalization;
using DeepFocus.Models;

namespace DeepFocus;

public class ConsoleRenderer
{
    private const string TimeFormat = "HH:mm:ss";

    public void RenderHelp()
    {
        Console.WriteLine("DeepFocus (df) - Focus session tracker");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  df start                Start a focus session");
        Console.WriteLine("  df pause-st             Start a pause");
        Console.WriteLine("  df pause-end            End a pause");
        Console.WriteLine("  df end                  End current session");
        Console.WriteLine("  df focus-check          Show all logs");
        Console.WriteLine("  df focus-check --today  Show today's log only");
        Console.WriteLine("  df --help               Show help");
    }

    public void RenderMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void RenderSessions(List<Session> sessions, DateTime now)
    {
        if (sessions.Count == 0)
        {
            Console.WriteLine("No sessions found.");
            return;
        }

        foreach (var session in sessions)
        {
            Console.WriteLine(FormatSessionLine(session, now));
        }
    }

    private string FormatSessionLine(Session session, DateTime now)
    {
        var pausesText = session.Pauses.Count == 0
            ? "none"
            : string.Join(", ", session.Pauses.Select(p => $"[{ShortTime(p.PauseStart)}→{ShortTime(p.PauseEnd ?? "...")}]"));

        var endText = session.End ?? "in progress";
        var netFocus = CalculateNetFocus(session, now);

        return $"{session.Date} | Start: {session.Start} | Pauses: {pausesText} | End: {endText} | Net focus: {netFocus.Hours}h {netFocus.Minutes:D2}m";
    }

    private static string ShortTime(string value)
    {
        if (DateTime.TryParseExact(value, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            return dt.ToString("HH:mm");
        }

        return value;
    }

    private TimeSpan CalculateNetFocus(Session session, DateTime now)
    {
        var baseDate = DateOnly.ParseExact(session.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var start = ParseDateTime(baseDate, session.Start);
        var end = session.End is null ? now : ParseDateTime(baseDate, session.End);

        if (end < start)
        {
            return TimeSpan.Zero;
        }

        var total = end - start;
        var pauseTotal = TimeSpan.Zero;

        foreach (var pause in session.Pauses)
        {
            var pauseStart = ParseDateTime(baseDate, pause.PauseStart);
            var pauseEnd = pause.PauseEnd is null ? now : ParseDateTime(baseDate, pause.PauseEnd);

            if (pauseEnd > pauseStart)
            {
                pauseTotal += pauseEnd - pauseStart;
            }
        }

        var net = total - pauseTotal;
        return net < TimeSpan.Zero ? TimeSpan.Zero : net;
    }

    private DateTime ParseDateTime(DateOnly date, string time)
    {
        var parsedTime = TimeOnly.ParseExact(time, TimeFormat, CultureInfo.InvariantCulture);
        return date.ToDateTime(parsedTime);
    }
}
