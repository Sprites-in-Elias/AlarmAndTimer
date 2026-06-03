using AdsJumboWinForm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace AlarmAndTimer
{
    /// <summary>
    /// Settings.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class Settings : Window
    {
        private MainViewModel _viewModel;   
        public Settings(MainViewModel viewModel, bool AlwaysTop)
        {
            InitializeComponent();
            if (AlwaysTop) { this.Topmost = true; }
            _viewModel = viewModel;
            VersionText.Text = $"v{Assembly.GetExecutingAssembly().GetName().Version}";
            CurrentTime_CheckBox.IsChecked = Properties.Settings.Default.DisplayCurrentTime;
            string? colorMode = Properties.Settings.Default.ColorMode;
            if (colorMode == null) colorMode = "Light";
            if (colorMode == "Light")
            {
                ColorModeSelector.SelectedItem = LightMode;
            }
            else if (colorMode == "Dark")
            {
                ColorModeSelector.SelectedItem = DarkMode;
            }
            UseActiveWindow_CheckBox.IsChecked = Properties.Settings.Default.UseActivatingWindow;
            VolumeValueTextBox.Text = Properties.Settings.Default.SoundVolume;
            SoundUse_CheckBox.IsChecked = Properties.Settings.Default.SoundUse;
            string defaultSavedSound = Properties.Settings.Default.DefaultSoundPath;
            foreach (ComboBoxItem item in DefaultSoundSelector.Items)
            {
                if (item.Content.ToString() == defaultSavedSound)
                {
                    DefaultSoundSelector.SelectedItem = item;
                    break;
                }
            }
            UseCustomSound.IsChecked = Properties.Settings.Default.UseCustomSound;
            CustomSoundPathTextBlock.Text = Properties.Settings.Default.CustomSoundPath;
            AlarmRepeat_CheckBox.IsChecked = Properties.Settings.Default.RepeatSound;
            FontSizeValueTextBox.Text = Properties.Settings.Default.FontSizeSetting;
            string language = Properties.Settings.Default.LanguageSetting;
            foreach (ComboBoxItem item in Language_Selector.Items)
            {
                if (item.Name.ToString() == language)
                {
                    Language_Selector.SelectedItem = item;
                    break;
                }
            }
        }
        private void CurrentTime_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            bool isChecked = checkBox.IsChecked ?? false;
            Properties.Settings.Default.DisplayCurrentTime = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.Save();
            _viewModel.IsShowTime = isChecked;
            Debug.WriteLine($"{ Properties.Settings.Default.DisplayCurrentTime }");
        }
        private void ColorMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 콤보박스에서 선택된 아이템을 가져옴
            if (ColorModeSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                // Tag에 저장해둔 "Light" 혹은 "Dark" 값을 가져옴
                string? themeName = selectedItem.Tag.ToString();

                // 1. 테마 적용 (App 클래스의 static 메서드 호출)
                App.ApplyTheme(themeName!);

                // 2. 설정 저장 (다음에 켰을 때도 유지되도록)
                AlarmAndTimer.Properties.Settings.Default.ColorMode = themeName;
                AlarmAndTimer.Properties.Settings.Default.Save();
            }
        }
        private void SoundUse_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            bool isChecked = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.SoundUse = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }

        private void UseActivatingWindow_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            bool isChecked = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.UseActivatingWindow = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("선택됨");
            // 이벤트가 발생한 대상(TextBox)을 가져옴
            TextBox? tb = sender as TextBox;
            if (tb != null)
            {
                Debug.WriteLine("널아님");
                // 텍스트 전체 선택
                tb.Dispatcher.BeginInvoke(new Action(() => {
                    tb.SelectAll();
                }));
            }
            else
            {
                Debug.WriteLine("널임");
            }
        }
        private void AdjustValue_Click(object sender, RoutedEventArgs e)
        {
            // 1. 버튼의 Tag 값을 가져옴 (string으로 설정되어 있으니 int로 변환)
            Button btn = (Button)sender;
            int adjustment = int.Parse(btn.Tag.ToString());

            // 2. 현재 텍스트 박스의 값을 가져옴
            if (int.TryParse(VolumeValueTextBox.Text, out int currentValue))
            {
                // 3. 값 계산
                int newValue = currentValue + adjustment;

                // 4. (선택사항) 음수 방지 처리
                if (newValue < 0) newValue = 0;
                else if (newValue > 100) newValue = 100;

                // 5. 텍스트 박스 업데이트
                VolumeValueTextBox.Text = newValue.ToString();
                Properties.Settings.Default.SoundVolume = newValue.ToString();
                Properties.Settings.Default.Save();
            }
            else
            {
                // 숫자가 아닌 값이 들어있을 경우 초기화
                VolumeValueTextBox.Text = "50";
                Properties.Settings.Default.SoundVolume = "50";
                Properties.Settings.Default.Save();
            }
        }
        private void DefaultSound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb && cmb.SelectedItem is ComboBoxItem selectedItem)
            {
                // 2. 선택된 아이템의 Content를 저장 (예: "be-be-be-beep")
                string? soundName = selectedItem.Content.ToString();

                Properties.Settings.Default.DefaultSoundPath = soundName;
                Properties.Settings.Default.Save();

                Debug.WriteLine($"저장 완료: {soundName}");
            }
        }
        private void DefaultSoundListen(object sender, RoutedEventArgs e)
        {
            Utils.AlarmPlayerClose();
            Utils.PlayDefaultAlarm();
        }
        private void StopSound(object sender, RoutedEventArgs e)
        {
            Utils.AlarmPlayerClose();
        }
        private void UseCostomSound_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            bool isChecked = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.UseCustomSound = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }
        private void LoadCustomSound(object sender, RoutedEventArgs e)
        {
            string? path = Utils.GetScriptPath("음악 파일 (*.mp3;*.wav;*.wma)|*.mp3;*.wav;*.wma|모든 파일 (*.*)|*.*");
            if (path == null) { return; }
            CustomSoundPathTextBlock.Text = path;
            Properties.Settings.Default.CustomSoundPath = path;
            Properties.Settings.Default.Save();
        }
        private void CustomSoundListen(object sender, RoutedEventArgs e)
        {
            Utils.AlarmPlayerClose();
            Utils.PlayCustomAlarm();
        }
        private void AlarmRepeat_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            bool isChecked = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.RepeatSound = checkBox!.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }
        private void FontSizeAdjustValue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int change))
            {
                // 1. 현재 값 가져오기
                int currentLevel = int.Parse(Properties.Settings.Default.FontSizeSetting);

                // 2. 값 계산 및 제한 (1~5)
                int newLevel = Math.Clamp(currentLevel + change, 1, 5);

                // 3. 값 저장
                Properties.Settings.Default.FontSizeSetting = newLevel.ToString();
                Properties.Settings.Default.Save();

                // 4. UI 업데이트
                FontSizeValueTextBox.Text = newLevel.ToString();

                _viewModel.MyFontSize = 10 + newLevel * 2;
            }
        }
        private void Language_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (Language_Selector.SelectedItem is ComboBoxItem selectedItem)
            {
                string? language = selectedItem.Name.ToString();

                Utils.ApplyLanguage(language!);

                AlarmAndTimer.Properties.Settings.Default.LanguageSetting = selectedItem.Name;
                AlarmAndTimer.Properties.Settings.Default.Save();
            }
        }
        /*
        public async Task CheckForUpdates()
        {
            string repoOwner = "Sprites-in-Elias";
            string repoName = "AlarmAndTimer";
            string apiUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                // GitHub API는 User-Agent 헤더가 필수야
                client.DefaultRequestHeaders.Add("User-Agent", "MyApp");

                try
                {
                    string json = await client.GetStringAsync(apiUrl);
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        string? result = doc.RootElement.GetProperty("tag_name").GetString();
                        string cleanLatest = result!.TrimStart('v');
                        Version latestVersion = new Version(cleanLatest);
                        Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version!;

                        if (latestVersion > currentVersion)
                        {
                            // 업데이트 알림 로직
                            //MessageBox.Show($"새 버전이 있어요");
                            //Utils.ShowLocalizedMessageBox("Msg_UpdateAvailable", latestVersion);
                            MessageBoxResult download = MessageBox.Show(
                                "새버전이 있어요. 설치파일을 다운로드 할까요?", // 메시지 내용
                                "업데이트 확인",                   // 제목
                                MessageBoxButton.YesNo,            // 예/아니오 버튼
                                MessageBoxImage.Question           // 질문 아이콘
                            );
                            if (download == MessageBoxResult.Yes)
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = $"https://github.com/Sprites-in-Elias/AlarmAndTimer/releases/download/{result}/mysetup.exe",
                                    UseShellExecute = true // 중요: .NET Core/5+ 이상에서는 이 옵션을 true로 설정해야 함
                                });
                            }
                        }
                        else
                        {
                            // 최신 버전임
                            //MessageBox.Show("현재 최신 버전입니다.");
                            Utils.ShowLocalizedMessageBox("Msg_AlreadyLatest");
                        }
                    }
                }
                catch {
                    //MessageBox.Show("업데이트 확인 실패");
                    Utils.ShowLocalizedMessageBox("Msg_UpdateFailed");
                }
            }
        }
        private async void Hyperlink_Update_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            await CheckForUpdates();
            e.Handled = true;
        }
        */
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // 기본 브라우저를 열어서 URL로 이동
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        private void CopyEmail(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("okanekudasai6@proton.me");
            Utils.ShowLocalizedMessageBox("CopyConfirm");
        }
    }
}
