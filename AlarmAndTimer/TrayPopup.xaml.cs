using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            // 입력창에 적힌 글자가 숫자인지 확인
            if (int.TryParse(TimerInput.Text, out int seconds) && seconds > 0)
            {
                // 리스트에 새 타이머 추가
                var newTimer = new TimerItem(seconds);
                _viewModel.Timers.Add(newTimer);
            }
            else
            {
                System.Windows.MessageBox.Show("올바른 초 단위 숫자를 입력해줘! (예: 60)");
            }
        }
        private void DeleteTimer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TimerItem timerItem)
            {
                timerItem.StopTimer();
                _viewModel.Timers.Remove(timerItem);
            }
        }
    }
    public class MainViewModel
    {
        public ObservableCollection<TimerItem> Timers { get; set; } = new ObservableCollection<TimerItem>();
    }
    public class TimerItem : INotifyPropertyChanged
    {
        private DispatcherTimer _individualTimer;
        private int _remainingSeconds;

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
                if (RemainingSeconds <= 0) return "⌛ 타임아웃!";
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
            }
        }

        // 생성자: 태어나는 순간 자신만의 1초 시계를 가동함
        public TimerItem(int initialSeconds)
        {
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
    }
}