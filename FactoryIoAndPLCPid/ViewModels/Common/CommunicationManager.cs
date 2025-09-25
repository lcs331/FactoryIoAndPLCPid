using Device.IService;
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

        public CommunicationManager(IConfigurationService config)
        {
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
                c = _cts;
                t = _backgroundTask;
                _cts = null;
                _backgroundTask = null;
            }
            if (c != null)
            {
                c.Cancel();
                try { await t.ConfigureAwait(false); } catch { /* swallow when cancelling */ }
                c.Dispose();
            }
        }

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            // Wrap synchronous connect in Task.Run to avoid blocking caller
            bool ok = await Task.Run(() => _config.ModbusTcpConnect(ip, port));
            SetConnected(ok);
            return ok;
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(() => _config.ModbusTcpDisconnect());
            SetConnected(false);
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
                        bool hb = false;
                        try
                        {
                            hb = await Task.Run(() => _config.ReadHearbeat(), token);
                        }
                        catch (Exception ex) { Debug.WriteLine("Heartbeat read error: " + ex.Message); }

                        HeartbeatChanged?.Invoke(hb);
                        if (!hb)
                        {
                            heartbeatErrors++;
                            if (heartbeatErrors >= 5)
                            {
                                // force disconnect and allow reconnect loop to handle retry
                                await DisconnectAsync();
                                heartbeatErrors = 0;
                            }
                        }
                        else
                        {
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
                        catch (Exception ex) { Debug.WriteLine("Stats error: " + ex.Message); }

                        // optionally write heartbeat (fire-and-forget is ok, but catch)
                        try { await Task.Run(() => _config.WriteHearbeat(), token); } catch { }

                        await Task.Delay(600, token);
                    }
                    else
                    {
                        if (_autoReconnect)
                        {
                            bool ok = false;
                            try
                            {
                                ok = await Task.Run(() => _config.ModbusTcpConnect(_ip, _port), token);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Reconnect attempt failed: " + ex.Message);
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
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Debug.WriteLine("Background loop exception: " + ex.Message);
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
            catch { }
        }


    }
}
