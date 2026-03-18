using System.Text.Json;
using System.Text.Json.Serialization;
using DeepFocus.Models;

namespace DeepFocus;

public interface ILogRepository
{
    List<Session> LoadSessions();
    void SaveSessions(List<Session> sessions);
}

public class LogRepository : ILogRepository
{
    private readonly string _logPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new DateOnlyJsonConverter(),
            new TimeOnlyJsonConverter()
        }
    };

    public LogRepository(string logPath)
    {
        _logPath = logPath;
    }

    public List<Session> LoadSessions()
    {
        try
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
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Cannot read log file '{_logPath}': invalid JSON format. {ex.Message}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Cannot read log file '{_logPath}': {ex.Message}");
        }
    }

    public void SaveSessions(List<Session> sessions)
    {
        var directory = Path.GetDirectoryName(_logPath) ?? AppContext.BaseDirectory;
        var tempPath = Path.Combine(directory, $"{Path.GetFileName(_logPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            Directory.CreateDirectory(directory);
            var json = JsonSerializer.Serialize(sessions, _jsonOptions);
            File.WriteAllText(tempPath, json);

            if (File.Exists(_logPath))
            {
                File.Move(tempPath, _logPath, overwrite: true);
            }
            else
            {
                File.Move(tempPath, _logPath);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Cannot write log file '{_logPath}': {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}

public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateOnly.ParseExact(value!, Format);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}

public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm:ss";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TimeOnly.ParseExact(value!, Format);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}
