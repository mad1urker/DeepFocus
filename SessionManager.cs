using DeepFocus.Models;

namespace DeepFocus;

public class SessionManager
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm:ss";
    private readonly LogRepository _repository;

    public SessionManager(LogRepository repository)
    {
        _repository = repository;
    }

    public (bool Success, string Message) StartSession(DateTime now)
    {
        var sessions = _repository.LoadSessions();
        var today = now.ToString(DateFormat);

        if (sessions.Any(s => s.Date == today))
        {
            return (false, "Session for today already exists.");
        }

        sessions.Add(new Session
        {
            Date = today,
            Start = now.ToString(TimeFormat),
            Pauses = new List<Pause>(),
            End = null
        });

        _repository.SaveSessions(sessions);
        return (true, $"Session started at {now.ToString(TimeFormat)}.");
    }

    public (bool Success, string Message) StartPause(DateTime now)
    {
        var sessions = _repository.LoadSessions();
        var activeSession = GetActiveSessionForToday(sessions, now);

        if (activeSession is null)
        {
            return (false, "No active session.");
        }

        var openPause = activeSession.Pauses.LastOrDefault(p => p.PauseEnd is null);
        if (openPause is not null)
        {
            return (false, "Already on pause. Use 'df pause-end'.");
        }

        activeSession.Pauses.Add(new Pause
        {
            PauseStart = now.ToString(TimeFormat),
            PauseEnd = null
        });

        _repository.SaveSessions(sessions);
        return (true, $"Pause started at {now.ToString(TimeFormat)}.");
    }

    public (bool Success, string Message) EndPause(DateTime now)
    {
        var sessions = _repository.LoadSessions();
        var activeSession = GetActiveSessionForToday(sessions, now);

        if (activeSession is null)
        {
            return (false, "No active pause.");
        }

        var openPause = activeSession.Pauses.LastOrDefault(p => p.PauseEnd is null);
        if (openPause is null)
        {
            return (false, "No active pause.");
        }

        openPause.PauseEnd = now.ToString(TimeFormat);
        _repository.SaveSessions(sessions);
        return (true, $"Pause ended at {now.ToString(TimeFormat)}.");
    }

    public (bool Success, string Message, string? Warning) EndSession(DateTime now)
    {
        var sessions = _repository.LoadSessions();
        var activeSession = GetActiveSessionForToday(sessions, now);

        if (activeSession is null)
        {
            return (false, "No active session to end.", null);
        }

        string? warning = null;
        var openPause = activeSession.Pauses.LastOrDefault(p => p.PauseEnd is null);
        if (openPause is not null)
        {
            var closeTime = now.ToString(TimeFormat);
            openPause.PauseEnd = closeTime;
            warning = $"Warning: open pause was automatically closed at {closeTime}.";
        }

        activeSession.End = now.ToString(TimeFormat);
        _repository.SaveSessions(sessions);

        return (true, $"Session ended at {activeSession.End}.", warning);
    }

    public List<Session> GetSessions(bool todayOnly, DateTime now)
    {
        var sessions = _repository.LoadSessions();
        if (!todayOnly)
        {
            return sessions.OrderBy(s => s.Date).ToList();
        }

        var today = now.ToString(DateFormat);
        return sessions.Where(s => s.Date == today).OrderBy(s => s.Date).ToList();
    }

    private Session? GetActiveSessionForToday(List<Session> sessions, DateTime now)
    {
        var today = now.ToString(DateFormat);
        return sessions.FirstOrDefault(s => s.Date == today && s.End is null);
    }
}
