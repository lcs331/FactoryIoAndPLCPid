using Device.IService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryIoAndPLCPid.ViewModels.Common
{
    public class CommunicationManager : IDisposable
    {
        private readonly ILogger<CommunicationManager> _logger;
        private readonly IConfigurationService _config;
        private readonly object _lock = new();
        private CancellationTokenSource _cts;
        private Task _backgroundTask;
        private string _ip;
        private int _port;
        private bool _autoReconnect;

        // Events to notify UI/VM
        public event Action<bool> ConnectionChanged;
        public event Action<bool> HeartbeatChanged;
        public event Action<int, int, double> StatsUpdated; // read, write, successRate
        public bool IsConnected { get; private set; }
        bool hb = false;
        bool ok = false;
        public CommunicationManager(ILogger<CommunicationManager> logger, IConfigurationService config)
        {
            _logger = logger;

            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        public void Start(string ip, int port, bool autoReconnect = true)
        {
            lock (_lock)
            {
                if (_cts != null) return;
                _ip = ip;
                _port = port;
                _autoReconnect = autoReconnect;
                _cts = new CancellationTokenSource();
                _backgroundTask = Task.Run(() => BackgroundLoopAsync(_cts.Token));
            }
        }

        public async Task StopAsync()
        {
            CancellationTokenSource c;
            Task t;
            lock (_lock)
            {
                HeartbeatChanged.Invoke(false);
                c = _cts;
                t = _backgroundTask;
                _cts = null;
                _backgroundTask = null;
            }
            if (c != null)
            {
                c.Cancel();
                try { await t.ConfigureAwait(false); } catch (Exception ex) { _logger.LogError(ex, "Source of cancellation exception"); }
                c.Dispose();
            }
        }

        public async Task<bool> ConnectAsync(string ip, int port, int timeoutMs = 3000)
        {
            try
            {
                var task = Task.Run(() => _config.ModbusTcpConnect(ip, port));
                if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
                {
                    // 同步方法完成
                    bool ok = task.Result;
                    SetConnected(ok);
                    return ok;
                }
                else
                {
                    // 超时
                    _logger.LogWarning("Connect timeout");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connect error");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await Task.Run(() => _config.ModbusTcpDisconnect());
                SetConnected(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Disconnect error");
            }
        }

        private void SetConnected(bool connected)
        {
            IsConnected = connected;
            ConnectionChanged?.Invoke(connected);
        }

        private async Task BackgroundLoopAsync(CancellationToken token)
        {
            int heartbeatErrors = 0;
            TimeSpan reconnectDelay = TimeSpan.FromSeconds(1);

            while (!token.IsCancellationRequested)
            {
                try
                {

                    if (IsConnected)
                    {
                        // heartbeat

                        try
                        {
                            hb = await Task.Run(() => _config.ReadHearbeat(), token);
                        }
                        catch (ObjectDisposedException o)
                        { 
                           _logger.LogError(o.Message, "Source of heartbeat exception");
                        }
                        catch (Exception ex)
                        { _logger.LogError(ex, "Heartbeat read error"); }


                        if (!hb)
                        {
                            heartbeatErrors++;
                            if (heartbeatErrors >= 5)
                            {
                                // force disconnect and allow reconnect loop to handle retry
                                HeartbeatChanged?.Invoke(false);
                                await DisconnectAsync();
                                heartbeatErrors = 0;
                            }
                        }
                        else
                        {
                            HeartbeatChanged?.Invoke(hb);
                            heartbeatErrors = 0;
                        }

                        // stats update (wrapped)
                        try
                        {
                            int r = await Task.Run(() => _config.GetReadSpeed(), token);
                            int w = await Task.Run(() => _config.GetWriteSpeed(), token);
                            double s = await Task.Run(() => _config.GetSuccessRate(), token);
                            StatsUpdated?.Invoke(r, w, s);
                        }
                        catch (Exception ex)
                        { _logger.LogError(ex, "Stats update error"); }

                        // optionally write heartbeat (fire-and-forget is ok, but catch)

                        
                            try { await Task.Run(() => _config.WriteHearbeat(), token); }
                            catch (ObjectDisposedException) { }
                            catch (Exception ex)
                            { _logger.LogError(ex.Message, "Source of write heartbeat exception"); }

                        await Task.Delay(600, token);
                    }
                    else
                    {
                        if (_autoReconnect)
                        {

                            try
                            {
                                ok = await Task.Run(() => _config.ModbusTcpConnect(_ip, _port), token);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Reconnect error");
                            }

                            SetConnected(ok);

                            if (!ok)
                            {
                                // exponential/backoff with cap
                                await Task.Delay(reconnectDelay, token);
                                reconnectDelay = TimeSpan.FromSeconds(Math.Min(30, reconnectDelay.TotalSeconds * 1.5));
                            }
                            else
                            {
                                reconnectDelay = TimeSpan.FromSeconds(1); // reset on success
                            }
                        }
                        else
                        {
                            await Task.Delay(1000, token);
                        }
                    }
                }
                catch (OperationCanceledException)
                { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in background loop");
                    await Task.Delay(1000, token); // avoid tight loop on unexpected errors
                }
            }
        }

        public void Dispose()
        {
            // best to call StopAsync before Dispose; do best-effort here
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch (Exception ex)
            { _logger.LogError(ex.Message, "Error disposing CommunicationManager"); }
        }


    }
}
