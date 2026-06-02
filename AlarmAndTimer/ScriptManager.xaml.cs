using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Win32;
using System.IO;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.Diagnostics;

namespace AlarmAndTimer
{
    /// <summary>
    /// ScriptManager.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScriptManager : Window
    {
        public ScriptManager()
        {
            InitializeComponent();
        }
        private void MakeManageCard(object sender, EventArgs e)
        {
            ManageCard manageCard = new ManageCard();
            TimerList.Children.Add(manageCard);
        }
        private void SaveScript(object sender, EventArgs e)
        {
            int index = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var row in TimerList.Children.OfType<ManageCard>())
            {
                index++;
                // 콤마(,)로 구분하면 나중에 다시 불러올 때 파싱하기 쉬워 (CSV 형식)
                //sb.AppendLine($"{row.AlarmType},{row.TimeValue}");
                string type = row.TypeSelector.Text.ToLower();
                string blankChar = " ";
                string colChar = ":";
                string newLineChar = "\n";
                sb.Append(row.TypeSelector.Text + blankChar);
                string hourInput = row.HourInput.Text;
                string minuteInput = row.MinuteInput.Text;
                string secondInput = row.SecondInput.Text;
                string memoInput = row.MemoInput.Text;
                if (type == "timer")
                {
                    if (hourInput == null || hourInput.Length == 0) hourInput = "0";
                    if (minuteInput == null || minuteInput.Length == 0) minuteInput = "0";
                    if (secondInput == null || secondInput.Length == 0) secondInput = "0";
                    if (hourInput == "0" && minuteInput == "0" && secondInput == "0")
                    {
                        System.Windows.MessageBox.Show($"Line ({index}) : 빈칸을 채워주세요");
                        return;
                    }
                }
                else if (type == "alarm")
                {
                    string ampm = row.AmPmSelector.Text;
                    if (ampm == null || ampm.Length == 0
                        || hourInput == null || hourInput.Length == 0
                        || minuteInput == null || minuteInput.Length == 0
                        || secondInput == null || secondInput.Length == 0)
                    {
                        System.Windows.MessageBox.Show($"Line ({index}) : 빈칸을 채워주세요");
                        return;
                    }
                    sb.Append(ampm).Append(blankChar);
                }
                sb.Append(hourInput).Append(colChar).Append(minuteInput).Append(colChar).Append(secondInput)
                    .Append(blankChar).Append(memoInput).Append(newLineChar);
            }
            if (sb.Length == 0)
            {
                System.Windows.MessageBox.Show($"저장할 데이터가 없습니다");
                return;
            }
            sb.Remove(sb.Length - 1, 1);
            SaveDataWithDialog(sb);
        }
        private void SaveDataWithDialog(StringBuilder sb)
        {
            // 1. SaveFileDialog 생성
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // 2. 창 설정
            saveFileDialog.Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*"; // 저장할 파일 형식 필터
            saveFileDialog.DefaultExt = "txt"; // 기본 확장자
            saveFileDialog.Title = "스크립트 저장";

            // 3. 사용자가 저장 버튼을 눌렀는지 확인
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"저장 오류: {ex.Message}");
                }
            }
        }
        private void LoadScript(object sender, RoutedEventArgs e)
        {
            string? path = Utils.GetScriptPath("텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*");
            if (path == null)
            {
                //System.Windows.MessageBox.Show($"뭔가 오류가 있어요.. 다시 시도해 보세요");
                return;
            }
            List<InputItem>? results = Utils.ProcessFileContent(path);
            if (results == null) { return; }
            if (TimerList.Children.Count > 0)
            {
                // 2. 확인 대화상자 띄우기
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    "목록에 항목이 있습니다. 기존 목록을 삭제하고 스크립트를 불러옵니다",
                    "확인",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                // 3. '예'를 눌렀을 때만 삭제 로직 실행
                if (result == MessageBoxResult.Yes)
                {
                    TimerList.Children.Clear(); // 자식 전체 삭제
                }
                else
                {
                    Debug.WriteLine("취소합니다");
                    return;
                }
            }
            foreach (InputItem item in results)
            {
                Debug.WriteLine(item);
                ManageCard card = new ManageCard(item);
                TimerList.Children.Add(card);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /*
        public void MoveTimerItem(UIElement item, int direction)
        {
            int index = TimerList.Children.IndexOf(item);
            int newIndex = index + direction;
            Debug.WriteLine($"{index} ,, {newIndex}");

            if (newIndex >= 0 && newIndex < TimerList.Children.Count)
            {
                TimerList.Children.RemoveAt(index);
                TimerList.Children.Insert(newIndex, item);
            }
        }
        */
    }
}