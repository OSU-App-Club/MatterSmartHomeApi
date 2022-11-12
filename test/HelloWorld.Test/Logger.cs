using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace HelloWorld.Tests;

// https://stackoverflow.com/a/69113346/16762230
public class Logger : ILogger
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly LoggerExternalScopeProvider _scopeProvider;
    private readonly string _categoryName;

    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper) => new Logger(testOutputHelper, new LoggerExternalScopeProvider(), "");

    public Logger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, string categoryName)
    {
        _testOutputHelper = testOutputHelper;
        _scopeProvider = scopeProvider;
        _categoryName = categoryName;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var sb = new StringBuilder();
        sb.Append(GetLogLevelString(logLevel))
            .Append(" [").Append(_categoryName).Append("] ")
            .Append(formatter(state, exception));

        if (exception != null)
        {
            sb.Append('\n').Append(exception);
        }

        // Append scopes
        _scopeProvider.ForEachScope((scope, state) =>
        {
            state.Append("\n => ");
            state.Append(scope);
        }, sb);

        _testOutputHelper.WriteLine(sb.ToString());
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trace",
            LogLevel.Debug => "debug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "error",
            LogLevel.Critical => "critical",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return _scopeProvider.Push(state);
    }
}