using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryIoAndPLCPid.ViewModels.Common
{
    public class EventAggregatorLoggerProvider : ILoggerProvider
    {

        private readonly IEventAggregator _eventAggregator;

        public EventAggregatorLoggerProvider(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public ILogger CreateLogger(string categoryName)
        {
           return new EventAggregatorLogger(_eventAggregator, categoryName);
        }

        public void Dispose()
        {
         
        }
    }
}
