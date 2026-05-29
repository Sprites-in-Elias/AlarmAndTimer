using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace AlarmAndTimer
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManageCard : System.Windows.Controls.UserControl
    {
        public ManageCard()
        {
            InitializeComponent();
        }
        public ManageCard(InputItem item) // : this()를 붙여서 기본 생성자도 같이 호출해야 함!
        {
            InitializeComponent();
            if (item.Type == "alarm")
            {
                TypeSelector.SelectedItem = ComboAlarm;
                if (item.AmPm == "am")
                {
                    AmPmSelector.SelectedItem = ComboAm;
                }
                else if (item.AmPm == "pm")
                {
                    AmPmSelector.SelectedItem = ComboPm;
                }
            }
            else if (item.Type == "timer")
            {
                TypeSelector.SelectedItem = ComboTimer;
            }
            HourInput.Text = item.Hour;
            MinuteInput.Text = item.Minute;
            SecondInput.Text = item.Second;
            MemoInput.Text = item.Memo;
        }

        private void ManageCard_Loaded(object sender, RoutedEventArgs e)
        {
            TypeSelector.Focus();
        }
        private void TypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 콤보박스에서 선택된 아이템 가져오기
            if (TypeSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string? selectedValue = selectedItem.Content.ToString();

                // 메뉴 변경 시 실행될 함수 호출
                OnTypeChanged(selectedValue!);
            }
        }

        private void OnTypeChanged(string newType)
        {
            if (newType == "Timer")
            {
                AmPmSelector.Visibility = Visibility.Collapsed;
                HourInput.Text = "";
                MinuteInput.Text = "";
                SecondInput.Text = "";
                ExecuteTimerLogic();
            }
            else if (newType == "Alarm")
            {
                GanText.Visibility = Visibility.Collapsed;
                DateTime now = DateTime.Now;

                int tempHour = now.Hour;
                if (tempHour > 12)
                {
                    tempHour -= 12;
                    AmPmSelector.SelectedItem = ComboPm;
                }
                else if (tempHour == 12)
                {
                    AmPmSelector.SelectedItem = ComboPm;
                }
                else if (tempHour == 0)
                {
                    tempHour = 12;
                    AmPmSelector.SelectedItem = ComboAm;
                }
                else { AmPmSelector.SelectedItem = ComboAm; }
                HourInput.Text = tempHour.ToString();
                MinuteInput.Text = now.Minute.ToString();
                SecondInput.Text = now.Second.ToString();
                ExecuteAlarmLogic();
            }
        }

        private void ExecuteTimerLogic()
        {
            // 여기에 Timer용 로직 구현
            System.Diagnostics.Debug.WriteLine("타이머 모드 활성화");
            TimeInputBox.Visibility = Visibility.Visible;
            GanText.Visibility=Visibility.Visible;
        }

        private void ExecuteAlarmLogic()
        {
            // 여기에 Alarm용 로직 구현
            System.Diagnostics.Debug.WriteLine("알람 모드 활성화");
            AmPmSelector.Visibility = Visibility.Visible;
            TimeInputBox.Visibility = Visibility.Visible;
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            // 1. 현재 이 UserControl 안으로 포커스가 남아있는지 확인
            // (예: 콤보박스를 클릭해서 펼쳤을 때 삭제되면 안 되니까)
            if (this.IsKeyboardFocusWithin) return;

            // 2. 콤보박스에 아무것도 선택되지 않았는지 확인
            if (TypeSelector.SelectedItem == null)
            {
                // 3. 부모 컨테이너(StackPanel 등)에서 나를 제거
                if (this.Parent is System.Windows.Controls.Panel parentPanel)
                {
                    parentPanel.Children.Remove(this);
                }
            }
        }
        private void Hour_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox!.Text == null || textBox.Text.Length == 0) { return; }
            string? typeValue = TypeSelector.Text.ToLower();
            Debug.WriteLine($"type:{typeValue}");
            if (typeValue == "alarm")
            {
                if (int.TryParse(textBox!.Text, out int value))
                {
                    if (value < 1 || value > 12)
                    {
                        MessageBox.Show("1부터 12 사이의 숫자만 입력 가능합니다.");
                        textBox.Text = ""; // 또는 이전 값으로 복구
                        textBox.Focus();    // 다시 포커스를 주어 수정하게 함
                    }
                }
                else
                {
                    MessageBox.Show("숫자만 입력해 주세요.");
                    textBox.Text = "";
                    textBox.Focus();
                }
            }
            else if (typeValue == "timer")
            {
                if (int.TryParse(textBox!.Text, out int value))
                {
                    if (value < 0 || value > 99)
                    {
                        MessageBox.Show("0부터 99 사이의 숫자만 입력 가능합니다.");
                        textBox.Text = ""; // 또는 이전 값으로 복구
                        textBox.Focus();    // 다시 포커스를 주어 수정하게 함
                    }
                }
                else
                {
                    MessageBox.Show("숫자만 입력해 주세요.");
                    textBox.Text = "";
                    textBox.Focus();
                }
            }
        }
        private void MinuteSecond_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox!.Text == null ||  textBox.Text.Length == 0) { return; }
            // 1. 숫자로 변환 시도
            if (int.TryParse(textBox!.Text, out int value))
            {
                // 2. 0 ~ 59 범위 검사
                if (value < 0 || value > 59)
                {
                    MessageBox.Show("0부터 59 사이의 숫자만 입력 가능합니다.");
                    textBox.Text = ""; // 또는 이전 값으로 복구
                    textBox.Focus();    // 다시 포커스를 주어 수정하게 함
                }
            }
            else
            {
                // 숫자가 아닐 경우
                MessageBox.Show("숫자만 입력해 주세요.");
                textBox.Text = "";
                textBox.Focus();
            }
        }
        private void DeleteCard_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is System.Windows.Controls.Panel parentPanel)
            {
                parentPanel.Children.Remove(this);
            }
        }
        private void TextBoxAllSelect(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // 텍스트박스가 아직 포커스를 가지고 있지 않은 경우에만 선택 수행
                if (!textBox.IsKeyboardFocusWithin)
                {
                    textBox.Focus(); // 포커스 주기
                    textBox.SelectAll(); // 전체 선택
                    e.Handled = true; // 마우스 클릭 이벤트를 여기서 차단하여 선택이 풀리지 않게 함
                }
            }
        }
        private void NoPropagation(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("낫프로파게이션");
            e.Handled = true;
        }
    }
}
