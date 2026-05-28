using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading; // 윈폼의 트레이 기능을 가져옴

namespace AlarmAndTimer
{
    public partial class TrayPopup : Window
    {
        private MainViewModel _viewModel;
        public TrayPopup()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            // UI의 리스트뷰에 데이터 연결
            TimerListView.ItemsSource = _viewModel.Timers;
        }

        // 팝업 밖의 영역(바탕화면 등)을 클릭하면 자동으로 숨김
        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void Window_Loaded(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                // 현재 시간 가져오기
                DateTime now = DateTime.Now;

                // 각 TextBox에 현재 시간 대입
                // (네가 만든 TextBox의 x:Name이 각각 HourInput, MinuteInput, SecondInput이라고 가정할게)
                AlarmHourInput.Text = now.Hour.ToString();
                AlarmMinuteInput.Text = now.Minute.ToString();
                AlarmSecondInput.Text = now.Second.ToString();
            }
        }
        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            int second = string.IsNullOrWhiteSpace(TimerSecondInput.Text) ? 0 :
             (int.TryParse(TimerSecondInput.Text, out int s) ? s : 0);

            int minute = string.IsNullOrWhiteSpace(TimerMinuteInput.Text) ? 0 :
                         (int.TryParse(TimerMinuteInput.Text, out int m) ? m : 0);

            int hour = string.IsNullOrWhiteSpace(TimerHourInput.Text) ? 0 :
                       (int.TryParse(TimerHourInput.Text, out int h) ? h : 0);
            Debug.WriteLine(minute);
            if (second == -1 || minute == -1 || hour == -1 || second < 0 || minute < 0 || hour < 0)
            {
                System.Windows.MessageBox.Show("올바른 숫자를 입력해줘! (예: 1시간 30분 45초 -> 1, 30, 45)");
                return;
            }
            // 리스트에 새 타이머 추가
            var newTimer = new TimerItem(second + minute * 60 + hour * 60 * 60);
            _viewModel.Timers.Add(newTimer);
        }
        private void StartAlarm_Click(object sender, RoutedEventArgs e)
        {

        }
        private void DeleteTimer_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("=========================================");
            Debug.WriteLine($"1. sender의 진짜 타입: {sender.GetType().FullName}");
            Debug.WriteLine($"2. sender의 문자열 표현: {sender.ToString()}");
            Debug.WriteLine(sender is System.Windows.Controls.Button);
            Debug.WriteLine("=========================================");
            if (sender is System.Windows.Controls.Button button && button.DataContext is TimerItem timerItem)
            {
                timerItem.StopTimer();
                _viewModel.Timers.Remove(timerItem);
            }
        }
        private void PauseTimer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is TimerItem timerItem)
            {
                timerItem.TogglePause();

                // 버튼 텍스트가 바로 안 바뀐다면 강제로 UI를 업데이트해줘야 할 수도 있어.
                // 하지만 보통은 INotifyPropertyChanged를 구현했으면 자동으로 바뀔 거야.
            }
        }
    }
    public class MainViewModel
    {
        public ObservableCollection<TimerItem> Timers { get; set; } = new ObservableCollection<TimerItem>();
    }
    public class DesignViewModel : MainViewModel
    {
        public DesignViewModel()
        {
            Timers.Add(new TimerItem(600));
            Timers.Add(new TimerItem(100));
        }
    }
    public class TimerItem : INotifyPropertyChanged
    {
        private DispatcherTimer _individualTimer;
        private int _remainingSeconds;
        private bool _isPaused;

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

        // 생성자: 태어나는 순간 자신만의 1초 시계를 가동함
        public TimerItem(int initialSeconds)
        {
            _isPaused = false;
            RemainingSeconds = initialSeconds;

                      // ★ 각자 독립된 타이머 객체 생성
            _individualTimer = new DispatcherTimer();
            _individualTimer.Interval = TimeSpan.FromSeconds(1);
            _individualTimer.Tick += IndividualTimer_Tick;

                    // 등록 버튼을 누른 바로 '그 소수점 밀리초 시점'부터 1초를 세기 시작함!
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
                    _individualTimer.Stop(); // 0초 되면 자기 타이머는 종료
                }
            }
        }



        // 삭제버튼 누를 때 호출해서 백그라운드 타이머를 확실히 죽여줌
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