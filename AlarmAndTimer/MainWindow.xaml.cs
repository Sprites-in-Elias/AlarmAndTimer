using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;

namespace AlarmAndTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private NotifyIcon? trayIcon;
        public MainWindow()
        {
            InitializeComponent(); _viewModel = new MainViewModel();
            SetupTrayIcon();
            this.DataContext = _viewModel;

            // UI의 리스트뷰에 데이터 연결
            TimerListView.ItemsSource = _viewModel.Timers;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon();
            // 트레이 아이콘 이미지 설정 (프로젝트 폴더에 .ico 파일 하나 넣어둬)
            trayIcon.Icon = System.Drawing.SystemIcons.Application;
            trayIcon.Text = "타이머";

            // 트레이 아이콘을 더블 클릭하면 다시 창이 나타나게 함
            trayIcon.DoubleClick += (s, e) => {
                this.Show();
                this.WindowState = WindowState.Normal;
                trayIcon.Visible = false;
            };
        }
        private void AlwaysTop_Checked(object sender, RoutedEventArgs e) { this.Topmost = true; }
        private void AlwaysTop_Unchecked(object sender, RoutedEventArgs e) { this.Topmost = false; }
        private void AlwaysTop_HyperLink(object sender, RoutedEventArgs e) { AlwaysTop_CheckBox.IsChecked = !AlwaysTop_CheckBox.IsChecked; }
        private void TrayMenu_HyperLink(object sender, RoutedEventArgs e)
        {
            this.Hide();
            trayIcon!.Visible = true;
        }
        private void Minimize_HyperLink(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
        private void CompletelyClose_HyperLink(object sender, RoutedEventArgs e) { System.Windows.Application.Current.Shutdown(); }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Visibility 상태를 반전시킴
            if (SubButtonsPanel.Visibility == Visibility.Visible)
            {
                SubButtonsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                SubButtonsPanel.Visibility = Visibility.Visible;
            }
        }
        private void TimerHourUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TimerHourInput.Text, out int hour))
            {
                hour = (hour + 1); // 0-99 사이로 순환
                if (hour == 100) return;
                TimerHourInput.Text = hour.ToString();
            }
            else if (string.IsNullOrWhiteSpace(TimerHourInput.Text))
            {
                TimerHourInput.Text = "0";
            }
        }
        private void TimerHourDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TimerHourInput.Text, out int hour))
            {
                hour = (hour - 1); // 0-99 사이로 순환
                if (hour == -1) return;
                TimerHourInput.Text = hour.ToString();
            }
            else if (string.IsNullOrWhiteSpace(TimerHourInput.Text))
            {
                TimerHourInput.Text = "0";
            }
        }
        private void TimerMinuteUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TimerMinuteInput.Text, out int minute))
            {
                minute = (minute + 1) % 60; // 0-59 사이로 순환
                TimerMinuteInput.Text = minute.ToString();
            }
            else if (string.IsNullOrWhiteSpace(TimerMinuteInput.Text))
            {
                TimerMinuteInput.Text = "0";
            }
        }
        private void TimerMinuteDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TimerMinuteInput.Text, out int minute))
            {
                minute = (minute - 1 + 60) % 60; // 0-59 사이로 순환
                TimerMinuteInput.Text = minute.ToString();
            }
            else if (string.IsNullOrWhiteSpace(TimerMinuteInput.Text))
            {
                TimerMinuteInput.Text = "0";
            }
        }
        private void TimerSecondUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TimerSecondInput.Text, out int second))
            {
                second = (second + 1) % 60; // 0-59 사이로 순환
                TimerSecondInput.Text = second.ToString();
            }
            else if (string.IsNullOrWhiteSpace(TimerSecondInput.Text))
            {
                TimerSecondInput.Text = "0";
            }
        }
        private void TimerSecondDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TimerSecondInput.Text, out int second))
            {
                second = (second - 1 + 60) % 60; // 0-59 사이로 순환
                TimerSecondInput.Text = second.ToString();
            }
            else if (string.IsNullOrWhiteSpace(TimerSecondInput.Text))
            {
                TimerSecondInput.Text = "0";
            }
        }

        private void OpenAlarmPanel_Click(object sender, RoutedEventArgs e)
        {
            // 현재 시간 가져오기
            DateTime now = DateTime.Now;

            // 각 TextBox에 현재 시간 대입
            // (네가 만든 TextBox의 x:Name이 각각 HourInput, MinuteInput, SecondInput이라고 가정할게)
            int tempHour = now.Hour;
            if (tempHour > 12)
            {
                tempHour -= 12;
                AmPmButton.Content = "PM";
            }
            else if (tempHour == 12)
            {
                AmPmButton.Content = "PM";
            }
            else if (tempHour == 0)
            {
                tempHour = 12;
                AmPmButton.Content = "AM";
            }
            else { AmPmButton.Content = "AM"; }
            AlarmHourInput.Text = tempHour.ToString();
            AlarmMinuteInput.Text = now.Minute.ToString();
            AlarmSecondInput.Text = now.Second.ToString();

            AddButtonPackage.Visibility = Visibility.Collapsed;
            SubButtonsPanel.Visibility = Visibility.Collapsed;
            AlarmMakePanel.Visibility = Visibility.Visible;
        }
        private void AlarmHourUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmHourInput.Text, out int hour))
            {
                hour = (hour + 1); // 0-12 사이로 순환
                if (hour == 13) { hour = 1; }
                AlarmHourInput.Text = hour.ToString();
            }
        }
        private void AlarmHourDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmHourInput.Text, out int hour))
            {
                hour = (hour - 1); // 0-12 사이로 순환
                if (hour == 0) { hour = 12; }
                AlarmHourInput.Text = hour.ToString();
            }
        }
        private void AlarmMinuteUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmMinuteInput.Text, out int minute))
            {
                minute = (minute + 1) % 60; // 0-59 사이로 순환
                AlarmMinuteInput.Text = minute.ToString();
            }
        }
        private void AlarmMinuteDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmMinuteInput.Text, out int minute))
            {
                minute = (minute - 1 + 60) % 60; // 0-59 사이로 순환
                AlarmMinuteInput.Text = minute.ToString();
            }
        }
        private void AlarmSecondUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmSecondInput.Text, out int second))
            {
                second = (second + 1) % 60; // 0-59 사이로 순환
                AlarmSecondInput.Text = second.ToString();
            }
        }
        private void AlarmSecondDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmSecondInput.Text, out int second))
            {
                second = (second - 1 + 60) % 60; // 0-59 사이로 순환
                AlarmSecondInput.Text = second.ToString();
            }
        }
        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            int second = string.IsNullOrWhiteSpace(TimerSecondInput.Text) ? 0 :
             (int.TryParse(TimerSecondInput.Text, out int s) ? s : -1);

            int minute = string.IsNullOrWhiteSpace(TimerMinuteInput.Text) ? 0 :
                         (int.TryParse(TimerMinuteInput.Text, out int m) ? m : -1);

            int hour = string.IsNullOrWhiteSpace(TimerHourInput.Text) ? 0 :
                       (int.TryParse(TimerHourInput.Text, out int h) ? h : -1);
            if (second == -1 || minute == -1 || hour == -1 || second < 0 || minute < 0 || hour < 0)
            {
                System.Windows.MessageBox.Show("올바른 숫자를 입력해줘! (예: 1시간 30분 45초 -> 1, 30, 45)");
                return;
            }
            string memo = TimerMemo.Text;
            // 리스트에 새 타이머 추가
            var newTimer = new TimerItem(second + minute * 60 + hour * 60 * 60, memo);
            _viewModel.Timers.Add(newTimer);

            TimerMakePanel.Visibility = Visibility.Hidden;
            AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void TimerMakeCancel_Click(object sender, RoutedEventArgs e)
        {
            TimerMakePanel.Visibility = Visibility.Hidden;
            AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void OpenTimerPanel_Click(object sender, RoutedEventArgs e)
        {
            AddButtonPackage.Visibility = Visibility.Collapsed;
            SubButtonsPanel.Visibility = Visibility.Collapsed;
            TimerMakePanel.Visibility = Visibility.Visible;
        }

        private void ToggleAmPm_Click(Object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                if (button.Content.ToString() == "AM") { button.Content = "PM"; }
                else { button.Content = "AM"; }
            }
        }
        private void StartAlarm_Click(object sender, RoutedEventArgs e)
        {
            int second = string.IsNullOrWhiteSpace(AlarmSecondInput.Text) ? 0 :
             (int.TryParse(AlarmSecondInput.Text, out int s) ? s : -1);

            int minute = string.IsNullOrWhiteSpace(AlarmMinuteInput.Text) ? 0 :
                         (int.TryParse(AlarmMinuteInput.Text, out int m) ? m : -1);

            int hour = string.IsNullOrWhiteSpace(AlarmHourInput.Text) ? 0 :
                       (int.TryParse(AlarmHourInput.Text, out int h) ? h : -1);

            string memo = AlarmMemo.Text;
            if (second == -1 || minute == -1 || hour == -1 || second < 0 || minute < 0 || hour < 0)
            {
                System.Windows.MessageBox.Show("올바른 숫자를 입력해줘! (예: 1시간 30분 45초 -> 1, 30, 45)");
                return;
            }

            if (AmPmButton.Content?.ToString() == "PM" && hour < 12) { hour += 12; }
            if (AmPmButton.Content?.ToString() == "AM" && hour == 12) { hour = 0; }

            DateTime now = DateTime.Now;
            DateTime alarmTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
            Debug.WriteLine($"시각 {hour}, {now.Hour}");
            alarmTime = alarmTime.AddSeconds(1);

            // 4. 만약 설정한 시간이 이미 지났다면 '내일' 알람으로 설정
            if (alarmTime <= now)
            {
                alarmTime = alarmTime.AddDays(1);
            }

            // 5. 남은 시간 계산 (TimeSpan 사용)
            TimeSpan remaining = alarmTime - now;

            // 결과: 총 몇 초 남았는지 확인
            double totalSecondsLeft = remaining.TotalSeconds;

            Debug.WriteLine($"알람까지 남은 시간: {remaining.Hours}시간 {remaining.Minutes}분 {remaining.Seconds}초\n총 {totalSecondsLeft:F0}초 후 울림");
            _viewModel.Timers.Add(new TimerItem((int)totalSecondsLeft, memo));

            AlarmMakePanel.Visibility = Visibility.Hidden;
            AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void AlarmMakeCancel_Click(object sender, RoutedEventArgs e)
        {
            AlarmMakePanel.Visibility = Visibility.Hidden;
            AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void DeleteTimer_Click(object sender, RoutedEventArgs e)
        {
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
            Timers.Add(new TimerItem(600, "qqq"));
            Timers.Add(new TimerItem(100, ""));
        }
    }
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

        // 생성자: 태어나는 순간 자신만의 1초 시계를 가동함
        public TimerItem(int initialSeconds, string memo)
        {
            _isPaused = false;
            RemainingSeconds = initialSeconds;

            // ★ 각자 독립된 타이머 객체 생성
            _individualTimer = new DispatcherTimer();
            _individualTimer.Interval = TimeSpan.FromSeconds(1);
            _individualTimer.Tick += IndividualTimer_Tick;
            _timerMemo += memo;

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