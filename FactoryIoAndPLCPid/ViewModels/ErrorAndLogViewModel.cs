using FactoryIoAndPLCPid.ViewModels.Common;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryIoAndPLCPid.ViewModels
{
    public class ErrorAndLogViewModel:BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        public ObservableCollection<LogMessage> LogMessages { get; set; } = new();
        public ErrorAndLogViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<LogEvent>().Subscribe(OnLogReceived, ThreadOption.UIThread);
        }

        private void OnLogReceived(LogMessage message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                LogMessages.Add(message);   
                if (LogMessages.Count > 100)
                {
                    LogMessages.RemoveAt(0);
                }
            });
        }
    }
}
