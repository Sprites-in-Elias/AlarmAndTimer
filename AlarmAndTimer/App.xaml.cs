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

            string savedTheme = AlarmAndTimer.Properties.Settings.Default.ColorMode ?? "Light";
            ApplyTheme(savedTheme);

            Utils.GetLanguageFromSystem();
            //Utils.GetLanguageFromIni();
            string langCode = AlarmAndTimer.Properties.Settings.Default.LanguageSetting;
            Utils.ApplyLanguage(langCode);
        }

        public static void ApplyTheme(string themeName)
        {
            var app = System.Windows.Application.Current;
            // 1. 기존 테마 딕셔너리 찾아내기 (이름으로 구분)
            var oldTheme = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.ToString().EndsWith("_Theme.xaml"));

            // 2. 새로운 테마 경로
            var themeUri = new Uri($"Resources/{themeName}_Theme.xaml", UriKind.Relative);
            var newTheme = new ResourceDictionary() { Source = themeUri };

            // 3. 기존 테마가 있다면 교체, 없다면 추가
            if (oldTheme != null)
            {
                int index = app.Resources.MergedDictionaries.IndexOf(oldTheme);
                app.Resources.MergedDictionaries.RemoveAt(index);
                app.Resources.MergedDictionaries.Insert(index, newTheme);
            }
            else
            {
                app.Resources.MergedDictionaries.Add(newTheme);
            }

            // 3. 설정 저장
            AlarmAndTimer.Properties.Settings.Default.ColorMode = themeName;
            AlarmAndTimer.Properties.Settings.Default.Save();
        }
    }

}
