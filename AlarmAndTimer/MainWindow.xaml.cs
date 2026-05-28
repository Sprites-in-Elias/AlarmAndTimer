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

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // 파일 필터 설정 (사용자가 원하는 파일만 보이게)
            openFileDialog.Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*";
            string lastPath = Properties.Settings.Default.LastOpenedDirectory;
            if (!string.IsNullOrEmpty(lastPath) && Directory.Exists(lastPath))
            {
                openFileDialog.InitialDirectory = lastPath;
            }
            // 대화상자 띄우기
            if (openFileDialog.ShowDialog() == true)
            {
                // 사용자가 선택한 파일 경로
                string selectedPath = openFileDialog.FileName;

                string? folderPath = System.IO.Path.GetDirectoryName(selectedPath);
                if (folderPath != null)
                {
                    Properties.Settings.Default.LastOpenedDirectory = folderPath;
                    Properties.Settings.Default.Save();
                }

                // 이제 이 경로를 사용해!
                ProcessFileContent(selectedPath);
            }
        }

        private bool CheckTimeValid (string[] timeParts, int hourLimit, string line, int lineNumber)
        {
            if (int.TryParse(timeParts[0], out int hour) &&
                int.TryParse(timeParts[1], out int minute) &&
                int.TryParse(timeParts[2], out int second))
            {
                // 2. 시간(1~12), 분(0~59), 초(0~59) 범위 검사
                bool isHourValid = (hour >= 1 && hour <= hourLimit);
                bool isMinuteValid = (minute >= 0 && minute <= 59);
                bool isSecondValid = (second >= 0 && second <= 59);

                if (!isHourValid)
                {
                    System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n시간의 범위는 1~{hourLimit} 입니다", "형식이 맞지 않습니다");
                    return false;
                }
                if (!isMinuteValid)
                {
                    System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n분의 범위는 0~59 입니다", "형식이 맞지 않습니다");
                    return false;
                }
                if (!isSecondValid)
                {
                    System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n초의 범위는 0~59 입니다", "형식이 맞지 않습니다");
                    return false;
                }
                return true;
            }
            else
            {
                // 숫자 변환 실패 처리
                System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n세 번째 단어가 유효한 숫자가 아닙니다 \n ※올바른 형식의 예\n\n================\n   Timer 12:30:00\n   Alarm PM 02:22:12\n================", "형식이 맞지 않습니다");
                return false;
            }
        }

        private void ProcessFileContent(string filePath)
        {
            Debug.WriteLine($"{filePath}에서 찾기");
            int lineNumber = 0;
            // 파일의 모든 줄을 한 줄씩 읽기
            foreach (string line in File.ReadLines(filePath))
            {
                lineNumber++;
                Debug.WriteLine($"{lineNumber} : {line}");
                if (string.IsNullOrWhiteSpace(line)) continue; // 빈 줄은 건너뜀

                // 공백 기준으로 단어 나누기
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                string type = parts[0].ToLower(); // 첫 단어: Timer 또는 Alarm
                if (type != "timer" && type != "alarm")
                {
                    System.Windows.MessageBox.Show($"Line({lineNumber}) : {line}\n\n첫단어는 Alarm 혹은 Timer 입니다\n\n※ 올바른 형식의 예\n\n================\n   Timer 12:30:00\n   Alarm PM 02:22:12\n================", "형식이 맞지 않습니다");
                    break;
                }
                if (type == "timer" && parts.Length != 2)
                {
                    System.Windows.MessageBox.Show($"Line({lineNumber}) : {line}\n\n타이머는 하나의 시간 인수만 필요합니다\n\n※올바른 형식의 예\n\n================\n   Timer 12:30:00\n================", "형식이 맞지 않습니다");
                    break;
                }
                if (type == "alarm" && parts.Length != 3)
                {
                    System.Windows.MessageBox.Show($"Line({lineNumber}) : {line}\n\n알람은 AM/PM 및 시간 두 개의 인수만 필요합니다\n\n※올바른 형식의 예\n\n================\n   Alarm PM 12:30:00\n================", "형식이 맞지 않습니다");
                    break;
                }
                if (type == "timer")
                {
                    string timeData = parts[1];
                    string[] timeParts = timeData.Split(':');
                    if (timeParts.Length != 3)
                    {
                        System.Windows.MessageBox.Show($"Line{lineNumber} : {line}\n\n타이머의 인수는 :로 구분된 3개의 숫자입니다 \n 예) Timer 12:30:00", "형식이 맞지 않습니다");
                        break;
                    }
                    if (!CheckTimeValid(timeParts, 99, line, lineNumber)) { break; }
                    Debug.WriteLine($"쓰기성공 {timeParts[0]}, {timeParts[1]}, {timeParts[2]}");
                }
                else if (type == "alarm")
                {
                    string amPm = parts[1].ToLower(); // AM 또는 PM
                    string timeData = parts[2];
                    string[] timeParts = timeData.Split(':');
                    if (amPm != "am" && amPm != "pm")
                    {
                        System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n알람의 첫 번째 인수는 AM 혹은 PM 입니다 \n 예) Alarm PM 12:30:00", "형식이 맞지 않습니다");
                        break;
                    }
                    if (timeParts.Length != 3)
                    {
                        System.Windows.MessageBox.Show($"Line{lineNumber} : {line}\n\n알람의 두 번째 인수는 :로 구분된 3개의 숫자입니다 \n 예) Timer 12:30:00", "형식이 맞지 않습니다");
                        break;
                    }
                    if (!CheckTimeValid(timeParts, 12, line, lineNumber)) { break; }
                    Debug.WriteLine($"쓰기성공 {timeParts[0]}, {timeParts[1]}, {timeParts[2]}");
                }
            }
        }
        private void MangeScript_Click(object sender, EventArgs e)
        {
            ScriptManager editor = new ScriptManager();
            editor.ShowDialog();
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