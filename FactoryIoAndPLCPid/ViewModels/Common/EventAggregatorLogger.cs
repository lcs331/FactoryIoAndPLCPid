using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
namespace FactoryIoAndPLCPid.ViewModels.Common
{
    public class EventAggregatorLogger : ILogger
    {

        private readonly IEventAggregator _eventAggregator;
        private readonly string _categoryName;
        public EventAggregatorLogger(IEventAggregator eventAggregator, string categoryName)
        {
            _eventAggregator = eventAggregator;
            _categoryName = categoryName;
        }


        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        { 
            return null;
        }
       

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            _eventAggregator.GetEvent<LogEvent>().Publish(
                new LogMessage { Time=DateTime.Now, Level = logLevel.ToString(), Message = message });

        }
    }
}
