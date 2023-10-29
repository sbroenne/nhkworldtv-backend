using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace sbroennelab.nhkworldtv.Tests
{
    public class ListLogger : ILogger
    {
        public IList<string> Logs;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => false;

        public ListLogger()
        {
            Logs = new List<string>();
        }

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            Logs.Add(string.Format("{0}.{1}: {2}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond, message));
        }
    }
}