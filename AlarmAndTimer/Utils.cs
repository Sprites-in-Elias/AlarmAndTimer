using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace AlarmAndTimer
{
    public static class Utils
    {
        private static MediaPlayer alarmPlayer = new MediaPlayer();
        public static void GetLanguageFromSystem()
        {
            string setLanguage = Properties.Settings.Default.LanguageSetting;
            if (setLanguage != null && (setLanguage == "Korean" || setLanguage == "English"))
            {
                return;
            }
            string lang = CultureInfo.CurrentUICulture.Name;
            Debug.WriteLine(lang);
            if (lang == "ko-KR")
            {
                Properties.Settings.Default.LanguageSetting = "Korean";
            }
            else
            {
                Properties.Settings.Default.LanguageSetting = "English";
            }
        }
        public static void GetLanguageFromIni()
        {
            string setLanguage = Properties.Settings.Default.LanguageSetting;
            if (setLanguage != null && (setLanguage == "Korean" || setLanguage == "English"))
            {
                return;
            }
            string? result = null;
            string path = "config.ini";
            if (File.Exists(path))
            {
                // 파일의 모든 줄을 읽어오기
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    // "Language=" 로 시작하는 줄 찾기
                    if (line.StartsWith("Language="))
                    {
                        // "=" 뒷부분(ko 또는 en)을 반환
                        result = line.Split('=')[1].Trim();
                    }
                }
            }
            if (result == null || (result != "Korean" && result != "English"))
            {
                result = "Korean";
            }
            Properties.Settings.Default.LanguageSetting = result;
        }
        public static string? GetScriptPath(string fileType)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // 파일 필터 설정 (사용자가 원하는 파일만 보이게)
            openFileDialog.Filter = fileType;
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
                return selectedPath;
            }
            return null;
        }
        public static void ShowLocalizedMessageBox(string contentKey, params object[] args)
        {
            // 제목 없이 메시지 박스를 띄울 때 사용할 기본 제목 (원한다면 string.Empty로 설정 가능)
            var rawContent = System.Windows.Application.Current.TryFindResource(contentKey) as string ?? contentKey;

            string content = (args != null && args.Length > 0) ? string.Format(rawContent, args) : rawContent;

            System.Windows.MessageBox.Show(content);
        }
        public static void ShowLocalizedMessageBox(string contentKey, string titleKey, params object[] args)
        {
            var title = System.Windows.Application.Current.TryFindResource(titleKey) as string ?? titleKey;
            var rawContent = System.Windows.Application.Current.TryFindResource(contentKey) as string ?? contentKey;

            // args가 있다면 문자열 포맷팅 적용
            string content = (args != null && args.Length > 0) ? string.Format(rawContent, args) : rawContent;

            System.Windows.MessageBox.Show(content, title);
        }
        public static MessageBoxResult ShowLocalizedMessageBox(string contentKey, string titleKey, MessageBoxButton button, MessageBoxImage icon, params object[] args)
        {
            var title = System.Windows.Application.Current.TryFindResource(titleKey) as string ?? titleKey;
            var rawContent = System.Windows.Application.Current.TryFindResource(contentKey) as string ?? contentKey;

            string content = (args != null && args.Length > 0) ? string.Format(rawContent, args) : rawContent;

            return System.Windows.MessageBox.Show(content, title, button, icon);
        }
        private static bool CheckTimeValid(int[] numTimeParts, int hourStart, int hourLimit, string line, int lineNumber)
        {
            bool isHourValid = (numTimeParts[0] >= hourStart && numTimeParts[0] <= hourLimit);
            bool isMinuteValid = (numTimeParts[1] >= 0 && numTimeParts[1] <= 59);
            bool isSecondValid = (numTimeParts[2] >= 0 && numTimeParts[2] <= 59);

            if (numTimeParts[0] < hourStart || numTimeParts[0] > hourLimit)
            {
                ShowLocalizedMessageBox("Msg_Content_HourRange", "Msg_Title_Error", lineNumber, line, hourStart, hourLimit);
                return false;
            }
            if (numTimeParts[1] < 0 || numTimeParts[1] > 59)
            {
                ShowLocalizedMessageBox("Msg_Content_MinuteRange", "Msg_Title_Error", lineNumber, line);
                return false;
            }
            if (numTimeParts[2] < 0 || numTimeParts[2] > 59)
            {
                ShowLocalizedMessageBox("Msg_Content_SecondRange", "Msg_Title_Error", lineNumber, line);
                return false;
            }
            return true;
        }
        private static int[]? SplitTimeData(string[] timeParts)
        {
            int[] numTimeParts = new int[timeParts.Length];
            for (int i = 0; i < timeParts.Length; i++)
            {
                if (int.TryParse(timeParts[i], out int num)) numTimeParts[i] = num;
                else return null;
            }
            return numTimeParts;
        }
        public static List<InputItem>? ProcessFileContent(string filePath)
        {
            Debug.WriteLine($"{filePath}에서 찾기");
            bool flag = false;
            int lineNumber = 0;
            List<InputItem> results = new List<InputItem>();
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
                    ShowLocalizedMessageBox("Msg_Content_InvalidFirstWord", "Msg_Title_Error", lineNumber, line);
                    flag = true; break;
                }
                if (type == "timer" && parts.Length < 2)
                {
                    ShowLocalizedMessageBox("Msg_Content_TimerArgError", "Msg_Title_Error", lineNumber, line);
                    flag = true; break;
                }
                else if (type == "timer" && parts.Length > 2)
                {
                    string timerName = string.Join(" ", parts.Skip(2));
                    parts[2] = timerName;
                }
                if (type == "alarm" && parts.Length < 3)
                {
                    ShowLocalizedMessageBox("Msg_Content_AlarmArgError", "Msg_Title_Error", lineNumber, line);
                    flag = true; break;
                }
                else if (type == "alarm" && parts.Length > 3)
                {
                    string timerName = string.Join(" ", parts.Skip(3));
                    parts[3] = timerName;
                }
                if (type == "timer")
                {
                    string timeData = parts[1];
                    string[] timeParts = timeData.Split(':');
                    if (timeParts.Length != 3)
                    {
                        ShowLocalizedMessageBox("Msg_Content_TimerFormatError", "Msg_Title_Error", lineNumber, line);
                        flag = true; break;
                    }
                    int[]? numTimeParts = SplitTimeData(timeParts);
                    if (numTimeParts == null)
                    {
                        ShowLocalizedMessageBox("Msg_Content_ParsingError", "Msg_Title_Error", lineNumber, line);
                        flag = true; break;
                    }
                    if (!CheckTimeValid(numTimeParts, 0, 23, line, lineNumber)) { flag = true; break; }
                    string? memo = null;
                    if (parts.Length > 2) memo = parts[2];
                    InputItem item = new InputItem(type, null, timeParts[0], timeParts[1], timeParts[2], memo);
                    results.Add(item);
                    Debug.WriteLine($"쓰기성공 {timeParts[0]}, {timeParts[1]}, {timeParts[2]}");
                }
                else if (type == "alarm")
                {
                    string amPm = parts[1].ToLower(); // AM 또는 PM
                    string timeData = parts[2];
                    string[] timeParts = timeData.Split(':');
                    if (amPm != "am" && amPm != "pm")
                    {
                        // 수정됨: AM/PM 오류 메시지
                        ShowLocalizedMessageBox("Msg_Content_InvalidAmPm", "Msg_Title_Error", lineNumber, line);
                        flag = true; break;
                    }
                    if (timeParts.Length != 3)
                    {
                        // 수정됨: 알람 포맷 오류 메시지
                        ShowLocalizedMessageBox("Msg_Content_AlarmFormatError", "Msg_Title_Error", lineNumber, line);
                        flag = true; break;
                    }
                    int[]? numTimeParts = SplitTimeData(timeParts);
                    if (numTimeParts == null)
                    {
                        // 수정됨: 파싱 오류 메시지
                        ShowLocalizedMessageBox("Msg_Content_ParsingError", "Msg_Title_Error", lineNumber, line);
                        flag = true; break;
                    }
                    if (!CheckTimeValid(numTimeParts, 1, 12, line, lineNumber)) { flag = true; break; }
                    string? memo = null;
                    if (parts.Length > 3) memo = parts[3];
                    InputItem item = new InputItem(type, amPm, timeParts[0], timeParts[1], timeParts[2], memo);
                    results.Add(item);
                    Debug.WriteLine($"쓰기성공 {timeParts[0]}, {timeParts[1]}, {timeParts[2]}");
                }
            }
            if (flag)
            {
                return null;
            }
            return results;
        }

        public static void PlayAlarm()
        {
            AlarmPlayerClose();
            if (Properties.Settings.Default.RepeatSound) alarmPlayer.MediaEnded += AlarmPlayer_MediaEnded;
            Debug.WriteLine("왔음");
            if (Properties.Settings.Default.UseCustomSound) PlayCustomAlarm();
            else PlayDefaultAlarm();
        }
        public static void PlayDefaultAlarm()
        {
            string defaultSoundName = Properties.Settings.Default.DefaultSoundPath;
            Uri uri = new Uri($"Resources/SoundPack/{defaultSoundName}.mp3", UriKind.Relative);

            var streamInfo = System.Windows.Application.GetResourceStream(uri);
            if (streamInfo == null)
            {
                Debug.WriteLine("파일을 찾을 수 없음!");
                return;
            }

            // 임시 파일로 복사 (MediaPlayer는 파일 경로를 선호함)
            string tempPath = Path.Combine(Path.GetTempPath(), "alarm_temp.mp3");
            using (var fileStream = File.Create(tempPath))
            {
                streamInfo.Stream.CopyTo(fileStream);
            }

            alarmPlayer.Open(new Uri(tempPath));
            alarmPlayer.Volume = int.Parse(Properties.Settings.Default.SoundVolume) / 100.0;
            alarmPlayer.Play();
        }
        public static void PlayCustomAlarm()
        {
            string filePath = Properties.Settings.Default.CustomSoundPath;
            if (filePath == null)
            {
                ShowLocalizedMessageBox("Msg_Content_NoCustomSound", "Msg_Title_Info"); 
            }
            else if (!System.IO.File.Exists(filePath))
            {
                ShowLocalizedMessageBox("CustomSoundNotFound");
                //Debug.WriteLine("파일을 찾을 수 없어요");
                return;
            }
            alarmPlayer.Open(new Uri(filePath!, UriKind.Absolute));

            // 4. 재생
            alarmPlayer.Volume = int.Parse(Properties.Settings.Default.SoundVolume) / 100.0;
            alarmPlayer.Play();
        }
        private static void AlarmPlayer_MediaEnded(object sender, EventArgs e)
        {
            // 재생이 끝나면 다시 처음으로 돌리고 재생
            alarmPlayer.Position = TimeSpan.Zero;
            alarmPlayer.Play();
        }
        public static void AlarmPlayerClose()
        {
            alarmPlayer.MediaEnded -= AlarmPlayer_MediaEnded;
            alarmPlayer.Stop();
            alarmPlayer.Close();
        }
        public static void ApplyLanguage(string langCode)
        {
            var appResources = System.Windows.Application.Current.Resources.MergedDictionaries;

            // 기존 언어 딕셔너리 찾기 (보통 이름에 'StringResources'를 포함하게 만듦)
            var oldDict = appResources.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.Contains("StringResources"));

            if (oldDict != null)
            {
                appResources.Remove(oldDict);
            }

            // 새로운 언어 딕셔너리 추가
            var newDict = new ResourceDictionary();
            try
            {
                newDict.Source = new Uri($"Resources/StringResources/{langCode}.xaml", UriKind.Relative);
                appResources.Add(newDict);
            }
            catch
            {
                // 파일 로드 실패 시 한국어로 강제 복구
                newDict.Source = new Uri("Resources/StringResources/Korean.xaml", UriKind.Relative);
                appResources.Add(newDict);
            }
        }
    }
}
