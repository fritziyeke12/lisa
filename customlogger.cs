using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class CustomEventLogLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logName;
    private readonly string _source;

    public CustomEventLogLogger(string categoryName, string source, string logName)
    {
        _categoryName = categoryName;
        _source = source;
        _logName = logName;

        if (!EventLog.SourceExists(_source))
        {
            EventLog.CreateEventSource(new EventSourceCreationData(_source, _logName));
        }
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = formatter(state, exception);

        if (exception != null)
        {
            message += Environment.NewLine + exception;
        }

        EventLogEntryType entryType = logLevel switch
        {
            LogLevel.Critical => EventLogEntryType.Error,
            LogLevel.Error => EventLogEntryType.Error,
            LogLevel.Warning => EventLogEntryType.Warning,
            _ => EventLogEntryType.Information,
        };

        EventLog.WriteEntry(_source, message, entryType);
    }
}
