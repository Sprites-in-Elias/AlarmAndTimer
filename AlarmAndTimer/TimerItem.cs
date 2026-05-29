using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AlarmAndTimer
{
    public class TimerItem : INotifyPropertyChanged
    {
        private DispatcherTimer _individualTimer;
        private int _remainingSeconds;
        private bool _isPaused;
        private String _timerMemo;

        public int RemainingSeconds
        {
            get => _remainingSeconds;
            set
            {
                if (_remainingSeconds != value)
                {
                    _remainingSeconds = value;
                    OnPropertyChanged(nameof(RemainingSeconds));
                    OnPropertyChanged(nameof(TimeLeftString));
                }
            }
        }

        public string TimerMemo
        {
            get => _timerMemo;
            set
            {
                _timerMemo = value;
                OnPropertyChanged(nameof(TimerMemo));
            }
        }

        public string TimeLeftString
        {
            get
            {
                TimeSpan t = TimeSpan.FromSeconds(RemainingSeconds);
                if (RemainingSeconds <= 0) return "종료";
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
            }
        }
        public string EndTimeString
        {
            get
            {
                return DateTime.Now.AddSeconds(RemainingSeconds).ToString("HH:mm:ss");
            }
        }

        public string PauseButtonText => _isPaused ? "재개" : "정지";

        public TimerItem(int initialSeconds, string memo)
        {
            _isPaused = false;
            RemainingSeconds = initialSeconds;

            _individualTimer = new DispatcherTimer();
            _individualTimer.Interval = TimeSpan.FromSeconds(1);
            _individualTimer.Tick += IndividualTimer_Tick;
            _timerMemo += memo;

            _individualTimer.Start();
        }

        private void IndividualTimer_Tick(object? sender, EventArgs e)
        {
            if (RemainingSeconds > 0)
            {
                RemainingSeconds--;

                if (RemainingSeconds == 0)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    _individualTimer.Stop();
                }
            }
        }



        public void StopTimer()
        {
            _individualTimer?.Stop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        internal void TogglePause()
        {
            if (_isPaused)
            {
                _individualTimer.Start();
                OnPropertyChanged(nameof(EndTimeString));
            }
            else
            {
                _individualTimer.Stop();
            }
            _isPaused = !_isPaused;
            OnPropertyChanged(nameof(PauseButtonText));
        }
    }
}
