using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Brush = System.Windows.Media.Brush;

namespace AlarmAndTimer
{
    public class TimerItem : INotifyPropertyChanged
    {
        private DispatcherTimer _individualTimer;
        private int _remainingSeconds;
        private bool _isPaused;
        private string _timerMemo;

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
                    Debug.WriteLine($"{RemainingSeconds}, {CurrentColor}");
                    OnPropertyChanged(nameof(CurrentColor));
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
                if (RemainingSeconds >= 0)
                {
                    TimeSpan t = TimeSpan.FromSeconds(RemainingSeconds);
                    return string.Format("-{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
                }
                else
                {
                    // 음수일 때 (종료 후 경과 시간)
                    TimeSpan t = TimeSpan.FromSeconds(Math.Abs(RemainingSeconds));
                    return string.Format("(종료)+{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
                }
            }
        }
        public string EndTimeString
        {
            get
            {
                return DateTime.Now.AddSeconds(RemainingSeconds).ToString("HH:mm:ss");
            }
        }

        //public string PauseButtonText => _isPaused ? "재개" : "정지";

        public TimerItem(int initialSeconds, string memo)
        {
            _isPaused = false;
            RemainingSeconds = initialSeconds;

            _individualTimer = new DispatcherTimer();
            _individualTimer.Interval = TimeSpan.FromSeconds(1);
            _individualTimer.Tick += IndividualTimer_Tick;
            _timerMemo = memo;

            _individualTimer.Start();
        }
        public event Action AlarmTriggered;

        private void IndividualTimer_Tick(object? sender, EventArgs e)
        {
            if (RemainingSeconds == 0)
            {
                // 여기서 이벤트를 쏘면 구독 중인 MainWindow가 알람을 울림
                AlarmTriggered?.Invoke();
            }
            RemainingSeconds = RemainingSeconds - 1;
        }

        public void StopTimer()
        {
            _individualTimer?.Stop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Brush CurrentColor
        {
            get
            {
                // 0보다 작으면 경고색(Red), 아니면 기본 테마 텍스트 색상
                return RemainingSeconds < 0
                    ? (Brush)System.Windows.Application.Current.Resources["TextAlertColor"]
                    : (Brush)System.Windows.Application.Current.Resources["TextColor"];
            }
        }

        //internal void TogglePause()
        //{
        //    if (_isPaused)
        //    {
        //        _individualTimer.Start();
        //        OnPropertyChanged(nameof(EndTimeString));
        //    }
        //    else
        //    {
        //        _individualTimer.Stop();
        //    }
        //    _isPaused = !_isPaused;
        //    OnPropertyChanged(nameof(PauseButtonText));
        //}
    }
}
