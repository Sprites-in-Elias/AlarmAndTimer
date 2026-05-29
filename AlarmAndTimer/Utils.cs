using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmAndTimer
{
    public static class Utils
    {
        public static string? GetScriptPath()
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
                return selectedPath;
            }
            return null;
        }
        private static bool CheckTimeValid(int[] numTimeParts, int hourStart, int hourLimit, string line, int lineNumber)
        {
            bool isHourValid = (numTimeParts[0] >= hourStart && numTimeParts[0] <= hourLimit);
            bool isMinuteValid = (numTimeParts[1] >= 0 && numTimeParts[1] <= 59);
            bool isSecondValid = (numTimeParts[2] >= 0 && numTimeParts[2] <= 59);

            if (!isHourValid)
            {
                System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n시간의 범위는 {hourStart}~{hourLimit} 입니다", "형식이 맞지 않습니다");
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
                    System.Windows.MessageBox.Show($"Line({lineNumber}) : {line}\n\n첫단어는 Alarm 혹은 Timer 입니다\n\n※ 올바른 형식의 예\n\n================\n   Timer 12:30:00\n   Alarm PM 02:22:12\n================", "형식이 맞지 않습니다");
                    flag = true; break;
                }
                if (type == "timer" && parts.Length != 2 && parts.Length != 3)
                {
                    System.Windows.MessageBox.Show($"Line({lineNumber}) : {line}\n\n타이머는 시간 인수와 메모만 필요합니다\n\n※올바른 형식의 예\n\n================\n   Timer 12:30:00\n================", "형식이 맞지 않습니다");
                    flag = true; break;
                }
                if (type == "alarm" && parts.Length != 3 && parts.Length != 4)
                {
                    System.Windows.MessageBox.Show($"Line({lineNumber}) : {line}\n\n알람은 AM/PM, 시간 및 메모 세 개의 인수만 필요합니다\n\n※올바른 형식의 예\n\n================\n   Alarm PM 12:30:00\n================", "형식이 맞지 않습니다");
                    flag = true; break;
                }
                if (type == "timer")
                {
                    string timeData = parts[1];
                    string[] timeParts = timeData.Split(':');
                    if (timeParts.Length != 3)
                    {
                        System.Windows.MessageBox.Show($"Line{lineNumber} : {line}\n\n타이머의 인수는 :로 구분된 3개의 숫자입니다 \n 예) Timer 12:30:00", "형식이 맞지 않습니다");
                        flag = true; break;
                    }
                    int[]? numTimeParts = SplitTimeData(timeParts);
                    if (numTimeParts == null)
                    {
                        System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n인자가 유효한 숫자가 아닙니다 \n ※올바른 형식의 예\n\n================\n   Timer 12:30:00\n   Alarm PM 02:22:12\n================", "형식이 맞지 않습니다");
                        flag = true; break;
                    }
                    if (!CheckTimeValid(numTimeParts, 0, 99, line, lineNumber)) { flag = true; break; }
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
                        System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n알람의 첫 번째 인수는 AM 혹은 PM 입니다 \n 예) Alarm PM 12:30:00", "형식이 맞지 않습니다");
                        flag = true; break;
                    }
                    if (timeParts.Length != 3)
                    {
                        System.Windows.MessageBox.Show($"Line{lineNumber} : {line}\n\n알람의 두 번째 인수는 :로 구분된 3개의 숫자입니다 \n 예) Timer 12:30:00", "형식이 맞지 않습니다");
                        flag = true; break;
                    }
                    int[]? numTimeParts = SplitTimeData(timeParts);
                    if (numTimeParts == null)
                    {
                        System.Windows.MessageBox.Show($"(Line {lineNumber}) : {line}\n\n인자가 유효한 숫자가 아닙니다 \n ※올바른 형식의 예\n\n===================\n   Timer 12:30:00 memomemo\n   Alarm PM 02:22:12 memomemo\n===================", "형식이 맞지 않습니다");
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
    }
}
