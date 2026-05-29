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
using Point = System.Windows.Point;
using Microsoft.Win32;
using System.IO;

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
        private void ContextMenuTempButton_Click(object sender, RoutedEventArgs e)
        {
            string? path = Utils.GetScriptPath();
            if (path == null) return;
            Utils.ProcessFileContent(path);
        }


        private void MangeScript_Click(object sender, EventArgs e)
        {
            ScriptManager editor = new ScriptManager();
            editor.ShowDialog();
        }

        private void LoadScript_Click(object sender, EventArgs e)
        {
            string? path = Utils.GetScriptPath();
            if (path == null)
            {
                //System.Windows.MessageBox.Show($"뭔가 오류가 있어요.. 다시 시도해 보세요");
                return;
            }
            List<InputItem>? results = Utils.ProcessFileContent(path);
            if (results == null) { return; }
            if (this.DataContext is MainViewModel vm)
            {
                int count = vm.Timers.Count;
                if (count > 0)
                {
                    // 삭제 로직 실행
                    MessageBoxResult result = System.Windows.MessageBox.Show(
                    "목록에 항목이 있습니다. 기존 목록을 삭제하고 스크립트를 불러옵니다",
                    "확인",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                    // 3. '예'를 눌렀을 때만 삭제 로직 실행
                    if (result == MessageBoxResult.Yes)
                    {
                        vm.Timers.Clear();
                    }
                    else
                    {
                        Debug.WriteLine("취소합니다");
                        return;
                    }
                }
            }
            foreach (InputItem item in results)
            {
                if (item.Type == "timer")
                {
                    StartTimer(item.Second, item.Minute, item.Hour, item.Memo);
                }
                else if (item.Type == "alarm")
                {
                    StartAlarm(item.Second, item.Minute, item.Hour, item.Memo, item.AmPm);
                }
            }
        }

        private Point _mouseDownPoint;
        private bool _isDragging = false;

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 1. 마우스 누른 위치 저장
            _mouseDownPoint = e.GetPosition(this);
            _isDragging = false;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point currentPoint = e.GetPosition(this);

                // 5픽셀 이상 움직이면 드래그로 간주
                if (Math.Abs(currentPoint.X - _mouseDownPoint.X) > 5 ||
                    Math.Abs(currentPoint.Y - _mouseDownPoint.Y) > 5)
                {
                    _isDragging = true;
                    this.DragMove(); // 드래그 시작 (여기서 윈도우 이동)
                }
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 드래그가 아니었을 때만 '클릭'으로 판정
            if (!_isDragging)
            {
                var element = e.Source as FrameworkElement;
                if (element != null && element.Name == "MakePanelBackgroundBorder")
                {
                    CloseMakePanel();
                }
            }

            _isDragging = false; // 상태 초기화
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
        private void StartTimer(string s, string m, string h, string memo)
        {
            Debug.WriteLine($"####{s},, {m},, {h}");
            int second = string.IsNullOrWhiteSpace(s) ? 0 :
             (int.TryParse(s, out int ss) ? ss : -1);

            int minute = string.IsNullOrWhiteSpace(m) ? 0 :
                         (int.TryParse(m, out int mm) ? mm : -1);

            int hour = string.IsNullOrWhiteSpace(h) ? 0 :
                       (int.TryParse(h, out int hh) ? hh : -1);
            if (second == -1 || minute == -1 || hour == -1 || second < 0 || minute < 0 || hour < 0)
            {
                Debug.WriteLine($"@@{second},, {minute},, {hour}");
                System.Windows.MessageBox.Show("올바른 숫자를 입력해줘! (예: 1시간 30분 45초 -> 1, 30, 45)");
                return;
            }
            // 리스트에 새 타이머 추가
            var newTimer = new TimerItem(second + minute * 60 + hour * 60 * 60, memo);
            _viewModel.Timers.Add(newTimer);

            MakePanelBackground.Visibility = Visibility.Hidden;
            TimerMakePanel.Visibility = Visibility.Hidden;
            //AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            string s = TimerSecondInput.Text;
            string m = TimerMinuteInput.Text;
            string h = TimerHourInput.Text;
            string memo = TimerMemo.Text;
            StartTimer(s, m, h, memo);
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
        private void StartAlarm(string s, string m, string h, string memo, string? amPm)
        {
            // 강제 선택이 되있기 때문에 만족할 일이 없는 조건문
            if (amPm == null || amPm.Length == 0)
            {
                System.Windows.MessageBox.Show("AM/PM을 선택해 주세요.");
                return;
            }
            int second = string.IsNullOrWhiteSpace(s) ? 0 :
             (int.TryParse(s, out int ss) ? ss : -1);

            int minute = string.IsNullOrWhiteSpace(m) ? 0 :
                         (int.TryParse(m, out int mm) ? mm : -1);

            int hour = string.IsNullOrWhiteSpace(h) ? 0 :
                       (int.TryParse(h, out int hh) ? hh : -1);
            if (second == -1 || minute == -1 || hour == -1 || second < 0 || minute < 0 || hour < 0)
            {
                Debug.WriteLine($"!!{second},, {minute},, {hour}");
                System.Windows.MessageBox.Show("올바른 숫자를 입력해줘! (예: 1시간 30분 45초 -> 1, 30, 45)");
                return;
            }

            if (amPm == "pm" && hour < 12) { hour += 12; }
            if (amPm == "am" && hour == 12) { hour = 0; }

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
        private void StartAlarm_Click(object sender, RoutedEventArgs e)
        {
            string s = AlarmSecondInput.Text;
            string m = AlarmMinuteInput.Text;
            string h = AlarmHourInput.Text;
            string memo = AlarmMemo.Text;
            string? amPm = AmPmButton.Content?.ToString().ToLower();
            StartAlarm(s, m, h, memo, amPm);            
        }
        private void AlarmMakeCancel_Click(object sender, RoutedEventArgs e)
        {
            MakePanelBackground.Visibility = Visibility.Hidden;
            AlarmMakePanel.Visibility = Visibility.Hidden;
            //AddButtonPackage.Visibility = Visibility.Visible;
        }
        private void CloseMakePanel()
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
}