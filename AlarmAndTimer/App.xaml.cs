using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using AlarmAndTimer.Properties;

namespace AlarmAndTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Settings.settings에서 저장된 테마 이름 가져오기 (없으면 기본값 "Light")
            string savedTheme = AlarmAndTimer.Properties.Settings.Default.ColorMode ?? "Light";
            // 2. 테마 적용
            ApplyTheme(savedTheme);
        }

        public static void ApplyTheme(string themeName)
        {
            var app = System.Windows.Application.Current;
            app.Resources.MergedDictionaries.Clear();

            var themeUri = new Uri($"Resources/{themeName}_Theme.xaml", UriKind.Relative);
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = themeUri });

            // 3. 설정 저장
            AlarmAndTimer.Properties.Settings.Default.ColorMode = themeName;
            AlarmAndTimer.Properties.Settings.Default.Save();
        }
    }

}
