using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace AlarmAndTimer
{
    public partial class MainWindow : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_TRAY = 2;
        private const uint FLASHW_TIMERNOFG = 12;
        private const uint FLASHW_STOP = 0;
        private const uint FLASHW_CAPTION = 0x00000001;
        private const uint FLASHW_TIMER = 0x00000004;

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9; // 최소화된 경우 복구 코드

        public void ActivateWindow()
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;

            // 최소화되어 있다면 원래 크기로 복구
            ShowWindow(windowHandle, SW_RESTORE);

            // 창을 최상위로 설정
            SetForegroundWindow(windowHandle);
        }

        // --- 호출용 퍼블릭 메서드 ---
        public static void StartFlashing()
        {
            var window = System.Windows.Application.Current.MainWindow;
            if (window == null) return;
            Debug.WriteLine(window);

            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = (uint)Marshal.SizeOf(fInfo);
            fInfo.hwnd = windowHandle;
            fInfo.dwFlags = FLASHW_CAPTION | FLASHW_TRAY | FLASHW_TIMER;
            fInfo.uCount = 0;
            fInfo.dwTimeout = 0;

            FlashWindowEx(ref fInfo);
        }
        public static void StopFlashing()
        {
            var window = System.Windows.Application.Current.MainWindow;
            if (window == null) return;

            IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO));
            fInfo.hwnd = windowHandle;
            fInfo.dwFlags = FLASHW_STOP; // STOP 플래그 사용
            fInfo.uCount = 0;
            fInfo.dwTimeout = 0;

            FlashWindowEx(ref fInfo);
        }

        private MainViewModel _viewModel;
        private NotifyIcon? trayIcon;
        public MainWindow()
        {
            InitializeComponent(); _viewModel = new MainViewModel();
            SetupTrayIcon();
            this.DataContext = _viewModel;

            TimerListView.ItemsSource = _viewModel.Timers;
            double left = Properties.Settings.Default.WindowLeft;
            double top = Properties.Settings.Default.WindowTop;
            double primaryWidth = SystemParameters.PrimaryScreenWidth;
            double primaryHeight = SystemParameters.PrimaryScreenHeight;
            bool isVisibleOnPrimary = (left >= 0 && left < primaryWidth) &&
                              (top >= 0 && top < primaryHeight);

            if (isVisibleOnPrimary && (left != 0 || top != 0))
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = left;
                this.Top = top;
            }
            else
            {
                // 주 모니터 밖이거나 저장된 좌표가 없으면 중앙에 배치
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
        private void ShowStackPanelWithAnimation()
        {
            DoubleAnimation moveAnim = new DoubleAnimation(50, 0, TimeSpan.FromSeconds(0.5));
            moveAnim.EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut };
            DoubleAnimation fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            MoveTransform.BeginAnimation(TranslateTransform.YProperty, moveAnim);
            MakePanelBackground.BeginAnimation(OpacityProperty, fadeAnim);
        }
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            AlwaysTopContextMenu.IsChecked = this.Topmost;
            string language = Properties.Settings.Default.LanguageSetting;
            foreach (var item in LanguageSet.Items)
            {
                if (item is MenuItem menuItem)
                {
                    // menu아이템의 Name과 language 문자열을 비교하여 체크 상태 설정
                    menuItem.IsChecked = (menuItem.Name == language);
                }
            }
            if (MakePanel.Visibility == Visibility.Visible) { CloseMakePanel(); }
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings st = new Settings(_viewModel, this.Topmost);
            st.ShowDialog();
        }
        private void MangeScript_Click(object sender, EventArgs e)
        {
            ScriptManager editor = new ScriptManager(this.Topmost);
            editor.ShowDialog();
        }
        private void ChangeLanguage(object sender, EventArgs e)
        {
            MenuItem? item = sender as MenuItem;
            Utils.ApplyLanguage(item!.Name);
            AlarmAndTimer.Properties.Settings.Default.LanguageSetting = item.Name;
            AlarmAndTimer.Properties.Settings.Default.Save();
        }
        private void LoadScript_Click(object sender, EventArgs e)
        {
            string? path = Utils.GetScriptPath("텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*");
            if (path == null)
            {
                return;
            }
            List<InputItem>? results = Utils.ProcessFileContent(path);
            if (results == null) { return; }
            if (this.DataContext is MainViewModel vm)
            {
                int count = vm.Timers.Count;
                if (count > 0)
                {
                    MessageBoxResult result = Utils.ShowLocalizedMessageBox(
                        "Msg_Content_ClearAndLoad",
                        "Msg_Title_Confirm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

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
            AlarmBreakingNotice.Visibility = Visibility.Collapsed;
            Utils.AlarmPlayerClose();
            StopFlashing();
            _mouseDownPoint = e.GetPosition(this);
            _isDragging = false;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point currentPoint = e.GetPosition(this);

                // 3픽셀 이상 움직이면 드래그로 간주
                if (Math.Abs(currentPoint.X - _mouseDownPoint.X) > 3 ||
                    Math.Abs(currentPoint.Y - _mouseDownPoint.Y) > 3)
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
                if (element != null && element.Name == "MakePanelBackgroundBorder")  // 여긴 메이크 판넬을 드래그 할때 잘 적용된건지 확인해야겠는데
                {
                    CloseMakePanel();
                }
            }

            _isDragging = false; // 상태 초기화
        }
        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon();
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/AppIcon/icon3.ico"));
            trayIcon.Icon = new Icon(sri.Stream);
            //trayIcon.Icon = System.Drawing.SystemIcons.Application;
            trayIcon.Text = "타이머";

            trayIcon.DoubleClick += (s, e) =>
            {
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
        private void HideInTrayMenu()
        {
            this.Hide();
            trayIcon!.Visible = true;
        }
        private void TrayMenu_HyperLink(object sender, RoutedEventArgs e) { HideInTrayMenu(); }
        private void HideFromTaskBar(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = false;
        }
        private void Minimize_HyperLink(object sender, RoutedEventArgs e) {
            if (!this.ShowInTaskbar)
            {
                this.ShowInTaskbar = true;
                HideInTrayMenu();
            }
            else
            {
                this.WindowState = WindowState.Minimized;
            }   
        }
        private void CompletelyClose_HyperLink(object sender, RoutedEventArgs e) { System.Windows.Application.Current.Shutdown(); }
        private void HourUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartButton.Tag.ToString() == "Timer")
            {
                if (int.TryParse(HourInput.Text, out int hour))
                {
                    hour = (hour + 1);
                    if (hour == 100) return;
                    HourInput.Text = hour.ToString();
                }
                else if (string.IsNullOrWhiteSpace(HourInput.Text))
                {
                    HourInput.Text = "0";
                }
            }
            else if (StartButton.Tag.ToString() == "Alarm")
            {
                if (int.TryParse(HourInput.Text, out int hour))
                {
                    hour = (hour + 1);
                    if (hour == 13) { hour = 1; }
                    HourInput.Text = hour.ToString();
                }
            }
        }
        private void HourDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartButton.Tag.ToString() == "Timer")
            {
                if (int.TryParse(HourInput.Text, out int hour))
                {
                    hour = (hour - 1);
                    if (hour == -1) return;
                    HourInput.Text = hour.ToString();
                }
                else if (string.IsNullOrWhiteSpace(HourInput.Text))
                {
                    HourInput.Text = "0";
                }
            }
            else if (StartButton.Tag.ToString() == "Alarm")
            {
                if (int.TryParse(HourInput.Text, out int hour))
                {
                    hour = (hour - 1);
                    if (hour == 0) { hour = 12; }
                    HourInput.Text = hour.ToString();
                }
            }
        }
        private void MinuteUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MinuteInput.Text, out int minute))
            {
                minute = (minute + 1) % 60;
                MinuteInput.Text = minute.ToString();
            }
            else if (string.IsNullOrWhiteSpace(MinuteInput.Text))
            {
                MinuteInput.Text = "0";
            }
        }
        private void MinuteDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MinuteInput.Text, out int minute))
            {
                minute = (minute - 1 + 60) % 60;
                MinuteInput.Text = minute.ToString();
            }
            else if (string.IsNullOrWhiteSpace(MinuteInput.Text))
            {
                MinuteInput.Text = "0";
            }
        }
        private void SecondUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(SecondInput.Text, out int second))
            {
                second = (second + 1) % 60;
                SecondInput.Text = second.ToString();
            }
            else if (string.IsNullOrWhiteSpace(SecondInput.Text))
            {
                SecondInput.Text = "0";
            }
        }
        private void SecondDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(SecondInput.Text, out int second))
            {
                second = (second - 1 + 60) % 60;
                SecondInput.Text = second.ToString();
            }
            else if (string.IsNullOrWhiteSpace(SecondInput.Text))
            {
                SecondInput.Text = "0";
            }
        }
        private void StartButton_Click(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine(StartButton.Tag);
            if (StartButton.Tag.ToString() == "Alarm")
            {
                StartAlarm(SecondInput.Text, MinuteInput.Text, HourInput.Text, Memo.Text, AmPmButton.Content.ToString()?.ToLower());
            }
            else if (StartButton.Tag.ToString() == "Timer")
            {
                StartTimer(SecondInput.Text, MinuteInput.Text, HourInput.Text, Memo.Text);
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
            HourInput.Text = tempHour.ToString();
            MinuteInput.Text = now.Minute.ToString();
            SecondInput.Text = now.Second.ToString();
            Memo.Text = "";

            //AddButtonPackage.Visibility = Visibility.Collapsed;
            //SubButtonsPanel.Visibility = Visibility.Collapsed;
            MakePanelBackground.Visibility = Visibility.Visible;
            AmPmButton.Visibility = Visibility.Visible;
            GanTextBlock.Visibility = Visibility.Visible;
            AlarmTail.Visibility = Visibility.Visible;
            TimerTail.Visibility = Visibility.Collapsed;
            MakePanel.Visibility = Visibility.Visible;
            StartButton.Tag = "Alarm";
            ShowStackPanelWithAnimation();
        }
        private void StartTimer(string s, string m, string h, string memo)
        {
            int second = string.IsNullOrWhiteSpace(s) ? 0 :
             (int.TryParse(s, out int ss) ? ss : -1);

            int minute = string.IsNullOrWhiteSpace(m) ? 0 :
                         (int.TryParse(m, out int mm) ? mm : -1);

            int hour = string.IsNullOrWhiteSpace(h) ? 0 :
                       (int.TryParse(h, out int hh) ? hh : -1);
            if (second == -1 || minute == -1 || hour == -1 || second < 0 || minute < 0 || hour < 0)
            {
                Utils.ShowLocalizedMessageBox("Msg_Content_EnterValidNumber", "Msg_Title_Error");
                return;
            }
            // 리스트에 새 타이머 추가
            var newTimer = new TimerItem(second + minute * 60 + hour * 60 * 60, memo);
            newTimer.AlarmTriggered += () => {
                // MainWindow의 PlayAlarm 호출
                if (Properties.Settings.Default.SoundUse) Utils.PlayAlarm();
                AlarmBreakingNotice.Visibility = Visibility.Visible;
                StartFlashing();
                if (Properties.Settings.Default.UseActivatingWindow) ActivateWindow();
            };
            _viewModel.Timers.Add(newTimer);

            MakePanelBackground.Visibility = Visibility.Hidden;
            MakePanel.Visibility = Visibility.Hidden;
        }
        private void OpenTimerPanel_Click(object sender, RoutedEventArgs e)
        {
            HourInput.Text = "";
            MinuteInput.Text = "";
            SecondInput.Text = "";
            Memo.Text = "";
            MakePanelBackground.Visibility = Visibility.Visible;
            AmPmButton.Visibility = Visibility.Collapsed;
            GanTextBlock.Visibility = Visibility.Collapsed;
            AlarmTail.Visibility = Visibility.Collapsed;
            TimerTail.Visibility = Visibility.Visible;
            MakePanel.Visibility = Visibility.Visible;
            StartButton.Tag = "Timer";
            ShowStackPanelWithAnimation();
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
                Utils.ShowLocalizedMessageBox("Msg_Content_SelectAmPm", "Msg_Title_Error");
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
                Utils.ShowLocalizedMessageBox("Msg_Content_EnterValidNumber", "Msg_Title_Error");
                return;
            }
            if (hour < 1 || hour > 12 || minute < 0 || minute > 59 || second < 0 || second > 59)
            {
                Utils.ShowLocalizedMessageBox("Msg_Content_TimeRangeError", "Msg_Title_Error");
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
            TimerItem item = new TimerItem((int)totalSecondsLeft, memo);
            item.AlarmTriggered += () => {
                // MainWindow의 PlayAlarm 호출
                if (Properties.Settings.Default.SoundUse) Utils.PlayAlarm();
                AlarmBreakingNotice.Visibility = Visibility.Visible;
                StartFlashing();
                if (Properties.Settings.Default.UseActivatingWindow) ActivateWindow();
            };
            _viewModel.Timers.Add(item);

            MakePanelBackground.Visibility = Visibility.Hidden;
            MakePanel.Visibility = Visibility.Hidden;
        }
        private void MakeCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseMakePanel();
        }
        private void CloseMakePanel()
        {
            DoubleAnimation hideAnim = new DoubleAnimation(0, MakePanel.ActualHeight, TimeSpan.FromSeconds(0.3));
            hideAnim.EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseIn };

            DoubleAnimation fadeAnim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));

            hideAnim.Completed += (s, e) =>
            {
                MakePanel.Visibility = Visibility.Hidden;
                MakePanelBackground.Visibility = Visibility.Hidden;
            };

            MoveTransform.BeginAnimation(TranslateTransform.YProperty, hideAnim);
            MakePanelBackground.BeginAnimation(OpacityProperty, fadeAnim);
        }
        private void DeleteTimer_Click(object sender, RoutedEventArgs e)
        {
            AlarmBreakingNotice.Visibility = Visibility.Collapsed;
            Utils.AlarmPlayerClose();
            if (sender is System.Windows.Controls.Button button && button.DataContext is TimerItem timerItem)
            {
                timerItem.StopTimer();
                _viewModel.Timers.Remove(timerItem);
            }
        }
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            TimerItem? item = button!.Tag as TimerItem; // 이게 바로 해당 열의 데이터!
            var vm = this.DataContext as MainViewModel;
            var index = vm!.Timers.IndexOf(item!);

            if (index > 0)
            {
                vm.Timers.Move(index, index - 1);
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            TimerItem? item = button!.Tag as TimerItem;
            var vm = this.DataContext as MainViewModel;
            int index = vm!.Timers.IndexOf(item!);

            if (index < vm.Timers.Count - 1)
            {
                vm.Timers.Move(index, index + 1);
            }
        }
        public void ClosedMainWindow(object sender, EventArgs e)
        {
            Properties.Settings.Default.WindowLeft = this.Left;
            Properties.Settings.Default.WindowTop = this.Top;
            Properties.Settings.Default.Save();
        }
    }
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _currentTime;
        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }
        private double _myFontSize;
        public double MyFontSize
        {
            get => _myFontSize;
            set
            { _myFontSize = value; OnPropertyChanged(); Debug.WriteLine($"폰트사이즈:{_myFontSize}"); }
        }
        public MainViewModel()
        {
            // 시계 타이머 설정
            DispatcherTimer clockTimer = new DispatcherTimer();
            clockTimer.Interval = TimeSpan.FromSeconds(1);
            clockTimer.Tick += (s, e) => { CurrentTime = DateTime.Now.ToString("HH:mm:ss"); };
            clockTimer.Start();

            MyFontSize = 10 + int.Parse(Properties.Settings.Default.FontSizeSetting) * 2;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public bool IsShowTime
        {
            get => Properties.Settings.Default.DisplayCurrentTime;
            set
            {
                Properties.Settings.Default.DisplayCurrentTime = value;
                Properties.Settings.Default.Save(); // 설정 저장
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TimerItem> Timers { get; set; } = new ObservableCollection<TimerItem>();
    }
    public class DesignViewModel : MainViewModel
    {
        //    public DesignViewModel()
        //    {
        //        Timers.Add(new TimerItem(600, "qqq"));
        //        Timers.Add(new TimerItem(100, ""));
        //    }
    }

}