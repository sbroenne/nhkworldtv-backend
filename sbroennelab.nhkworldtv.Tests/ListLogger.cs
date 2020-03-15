using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
namespace sbroennelab.nhkworldtv.Tests
{
    public class ListLogger : ILogger
    {
        public IList<string> Logs;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => false;

        public ListLogger()
        {
            this.Logs = new List<string>();
        }

        public void Log<TState>(LogLevel logLevel, 
        						EventId eventId,
        						TState state,
        						Exception exception,
        						Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            this.Logs.Add(String.Format("{0}.{1}: {2}",DateTime.UtcNow.Second,DateTime.UtcNow.Millisecond, message));
        }
    }
}