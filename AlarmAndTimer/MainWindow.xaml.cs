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
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private NotifyIcon? trayIcon;
        public MainWindow()
        {
            InitializeComponent(); _viewModel = new MainViewModel();
            SetupTrayIcon();
            this.DataContext = _viewModel;

            TimerListView.ItemsSource = _viewModel.Timers;
        }
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            AlwaysTopContextMenu.IsChecked = this.Topmost;
            if (AlarmMakePanel.Visibility == Visibility.Visible || TimerMakePanel.Visibility == Visibility.Visible)
            {
                MakeTimerContextMenu.IsEnabled = false;
                MakeAlarmContextMenu.IsEnabled = false;
            }
            else
            {
                MakeTimerContextMenu.IsEnabled = true;
                MakeAlarmContextMenu.IsEnabled = true;
            }
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
            trayIcon.Icon = System.Drawing.SystemIcons.Application;
            trayIcon.Text = "타이머";

            trayIcon.DoubleClick += (s, e) => {
                this.Show();
                this.WindowState = WindowState.Normal;
                trayIcon.Visible = false;
            };
        }
        private void AlwaysTop_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("켜짐");
            AlwaysTop_CheckBox.IsChecked = true;
            
            this.Topmost = true;
        }
        private void AlwaysTop_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("꺼짐");
            AlwaysTop_CheckBox.IsChecked = false;
            this.Topmost = false;
        }
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
                hour = (hour + 1);
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
                hour = (hour - 1); 
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
                minute = (minute + 1) % 60;
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
                minute = (minute - 1 + 60) % 60;
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
                second = (second + 1) % 60;
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
                second = (second - 1 + 60) % 60;
                TimerSecondInput.Text = second.ToString();
            }
            else if (string.IsNullOrWhiteSpace(TimerSecondInput.Text))
            {
                TimerSecondInput.Text = "0";
            }
        }

        private void OpenAlarmPanel_Click(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;

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

            //AddButtonPackage.Visibility = Visibility.Collapsed;
            //SubButtonsPanel.Visibility = Visibility.Collapsed;
            MakePanelBackground.Visibility = Visibility.Visible;
            AlarmMakePanel.Visibility = Visibility.Visible;
        }
        private void AlarmHourUpButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("gggg");
            if (int.TryParse(AlarmHourInput.Text, out int hour))
            {
                hour = (hour + 1); 
                if (hour == 13) { hour = 1; }
                AlarmHourInput.Text = hour.ToString();
            }
        }
        private void AlarmHourDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmHourInput.Text, out int hour))
            {
                hour = (hour - 1); 
                if (hour == 0) { hour = 12; }
                AlarmHourInput.Text = hour.ToString();
            }
        }
        private void AlarmMinuteUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmMinuteInput.Text, out int minute))
            {
                minute = (minute + 1) % 60;
                AlarmMinuteInput.Text = minute.ToString();
            }
        }
        private void AlarmMinuteDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmMinuteInput.Text, out int minute))
            {
                minute = (minute - 1 + 60) % 60; 
                AlarmMinuteInput.Text = minute.ToString();
            }
        }
        private void AlarmSecondUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmSecondInput.Text, out int second))
            {
                second = (second + 1) % 60; 
                AlarmSecondInput.Text = second.ToString();
            }
        }
        private void AlarmSecondDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AlarmSecondInput.Text, out int second))
            {
                second = (second - 1 + 60) % 60;
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

            MakePanelBackground.Visibility = Visibility.Hidden;
            TimerMakePanel.Visibility = Visibility.Hidden;
            //AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void TimerMakeCancel_Click(object sender, RoutedEventArgs e)
        {
            MakePanelBackground.Visibility = Visibility.Hidden;
            TimerMakePanel.Visibility = Visibility.Hidden;
            //AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void OpenTimerPanel_Click(object sender, RoutedEventArgs e)
        {
            //AddButtonPackage.Visibility = Visibility.Collapsed;
            //SubButtonsPanel.Visibility = Visibility.Collapsed;
            MakePanelBackground.Visibility = Visibility.Visible;
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

            if (alarmTime <= now)
            {
                alarmTime = alarmTime.AddDays(1);
            }

            TimeSpan remaining = alarmTime - now;

            double totalSecondsLeft = remaining.TotalSeconds;

            Debug.WriteLine($"알람까지 남은 시간: {remaining.Hours}시간 {remaining.Minutes}분 {remaining.Seconds}초\n총 {totalSecondsLeft:F0}초 후 울림");
            _viewModel.Timers.Add(new TimerItem((int)totalSecondsLeft, memo));

            MakePanelBackground.Visibility = Visibility.Hidden;
            AlarmMakePanel.Visibility = Visibility.Hidden;
            //AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void AlarmMakeCancel_Click(object sender, RoutedEventArgs e)
        {
            MakePanelBackground.Visibility = Visibility.Hidden;
            AlarmMakePanel.Visibility = Visibility.Hidden;
            //AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void CloseMakePanel(object sender, MouseButtonEventArgs e)
        {
            MakePanelBackground.Visibility = Visibility.Hidden;
            TimerMakePanel.Visibility = Visibility.Hidden;
            AlarmMakePanel.Visibility = Visibility.Hidden;
            //AddButtonPackage.Visibility = Visibility.Visible;
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