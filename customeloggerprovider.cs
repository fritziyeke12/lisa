using Microsoft.Extensions.Logging;

public class CustomEventLogLoggerProvider : ILoggerProvider
{
    private readonly string _source;
    private readonly string _logName;

    public CustomEventLogLoggerProvider(string source, string logName)
    {
        _source = source;
        _logName = logName;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomEventLogLogger(categoryName, _source, _logName);
    }

    public void Dispose() { }
}
