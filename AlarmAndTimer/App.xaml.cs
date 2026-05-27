using AlarmAndTimer;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace AlarmAndTimer
{
    public partial class App : Application
    {
        private NotifyIcon _notifyIcon;
        private TrayPopup _popup;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 창이 닫혀도 프로그램이 완전히 종료되지 않도록 백그라운드 유지
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 1. 트레이 아이콘 생성
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = SystemIcons.Application; // 기본 시스템 아이콘 모양 사용
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "나의 트레이 앱";

            // 2. 트레이 우클릭 메뉴 설정
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("열기", null, (s, a) => ShowPopup());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("종료", null, (s, a) => ExitApp());
            _notifyIcon.ContextMenuStrip = contextMenu;

            // 3. 트레이 좌클릭 시 팝업 띄우기 설정
            _notifyIcon.MouseClick += (s, a) =>
            {
                if (a.Button == MouseButtons.Left)
                {
                    ShowPopup();
                }
            };

            // 팝업창 객체 미리 만들어두기
            _popup = new TrayPopup();
        }

        // 마우스 위치를 계산해서 팝업창을 보여주는 핵심 메서드
        private void ShowPopup()
        {
            if (_popup.IsVisible)
            {
                _popup.Activate();
                return;
            }

            // 현재 마우스 커서 위치 구하기
            var mousePos = System.Windows.Forms.Cursor.Position;
            // 현재 마우스가 있는 모니터의 작업 영역(작업 표시줄 제외 영역) 구하기
            var workingArea = System.Windows.Forms.Screen.FromPoint(mousePos).WorkingArea;

            // 마우스 위치 기준으로 팝업 창 좌표 설정 (작업 표시줄 바로 위에 안착하게)
            _popup.Left = mousePos.X - (_popup.Width / 2);
            _popup.Top = workingArea.Bottom - _popup.Height - 5;

            // 화면 오른쪽 끝을 벗어나지 않도록 보정
            if (_popup.Left + _popup.Width > workingArea.Right) _popup.Left = workingArea.Right - 5;
            if (_popup.Left < workingArea.Left) _popup.Left = workingArea.Left + 5;

            // 팝업 짜잔!
            _popup.Show();
            _popup.Activate();
        }

        // 프로그램 완전 종료
        private void ExitApp()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            Shutdown();
        }
    }
}