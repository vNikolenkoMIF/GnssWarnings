using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Meemim.VGis.Unity.Plugins.Interfaces.Log;
using UnityEngine;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts
{ 
    public sealed class IssueMonitoringWorker
    {
        private const int MIN_FREQUENCY = 3;

        public bool IsRunning { get; private set; }

        public int TimeRange { get; }

        public int Frequency { get; private set; }

        public int ListenersCount => _monitoringEventsManger.ListenersCount;

        public event Action TimeRangeElapsed;

        private readonly MonitoringEventsManger _monitoringEventsManger;
        private readonly ILog _log;

        private readonly int _delayBetweenSteps;

        private int _currentStep;
 

        private CancellationTokenSource _cancellationSource;

        public IssueMonitoringWorker(int timeRange, int frequency)
        {
            _log = LogManager.GetLogger(nameof(IssueMonitoringWorker));
            _monitoringEventsManger = new MonitoringEventsManger();

            TimeRange = timeRange;
            SetFrequency(frequency);
            _delayBetweenSteps = timeRange / Frequency;
        }

        public void Start()
        {
            if (IsRunning) {
                return;
            }

            if (_monitoringEventsManger.ListenersCount == 0) {
                _log.LogError("Can't run monitoring when no one listener was added");
                return;
            }

            var token = CreateCancelation();
            RunMonitoringAsync(token).Forget(ex => {
                IsRunning = false;
                DisposeCancelation();
            });

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) {
                return;
            }

            CancelAndDispose();
            IsRunning = false;
        }

        public void Reset()
        {
            Stop();
            ResetAll();

            _monitoringEventsManger.ClearAllListeners();
        }

        public bool AddListener(IMonitoringLifecycleListener listener)
        {
            return _monitoringEventsManger.AddListener(listener);
        }

        public bool RemoveListener(IMonitoringLifecycleListener listener)
        {
            return _monitoringEventsManger.RemoveListener(listener);
        }

        public void ClearAllListeners()
        {
            _monitoringEventsManger.ClearAllListeners();
        }

        private async UniTask RunMonitoringAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested) {
                        break;
                    }

                    await RunMonitoringLoopAsync();
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                _log.LogException(e);
                throw;
            }
        }

        private async UniTask RunMonitoringLoopAsync()
        {
            _monitoringEventsManger.DoBeforeRangeStart();

            var delayInMs = TimeSpan.FromSeconds(_delayBetweenSteps);

            while (_currentStep <= Frequency)
            {
                await UniTask.Delay(delayInMs);

                _monitoringEventsManger.DoAfterStep();

                _currentStep++;
            }

            _monitoringEventsManger.DoAfterRangeEnd();

            ResetAll();
            
            TimeRangeElapsed?.Invoke();
        }

        private void SetFrequency(int possibleFrequency)
        {
            var averageFrequency = Mathf.Clamp(possibleFrequency, MIN_FREQUENCY, possibleFrequency);
            // Value must not be even for cases when good and bad cases will have the same count.
            Frequency = possibleFrequency % 2 == 0 ? averageFrequency + 1 : averageFrequency;
        }

        private void ResetAll()
        {
            _currentStep = 0;
            _monitoringEventsManger.Reset();
        }

        private CancellationToken CreateCancelation()
        {
            if (_cancellationSource != null) {
                return _cancellationSource.Token;
            }

            _cancellationSource = new CancellationTokenSource();
            return _cancellationSource.Token;
        }

        private void CancelAndDispose()
        {
            if (_cancellationSource == null) {
                return;
            }

            _cancellationSource.Cancel();
            DisposeCancelation();
        }

        private void DisposeCancelation()
        {
            _cancellationSource.Dispose();
            _cancellationSource = null;
        }
    }
}
