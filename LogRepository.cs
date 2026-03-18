using System.Text.Json;
using DeepFocus.Models;

namespace DeepFocus;

public class LogRepository
{
    private readonly string _logPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public LogRepository(string logPath)
    {
        _logPath = logPath;
    }

    public List<Session> LoadSessions()
    {
        if (!File.Exists(_logPath))
        {
            return new List<Session>();
        }

        var json = File.ReadAllText(_logPath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Session>();
        }

        return JsonSerializer.Deserialize<List<Session>>(json, _jsonOptions) ?? new List<Session>();
    }

    public void SaveSessions(List<Session> sessions)
    {
        var json = JsonSerializer.Serialize(sessions, _jsonOptions);
        File.WriteAllText(_logPath, json);
    }
}
