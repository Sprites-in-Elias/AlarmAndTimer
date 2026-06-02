using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
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
        public Settings(MainViewModel viewModel)
        {
            InitializeComponent();
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
            bool isChecked = checkBox.IsChecked ?? false;
            Properties.Settings.Default.SoundUse = checkBox!.IsChecked ?? false;
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
    }
}
