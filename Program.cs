using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace CalendarApp
{
    public class Schedule
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool HasNotification { get; set; }
    }

    public class MainForm : Form
    {
        private readonly MonthCalendar calendar = new();
        private readonly Button btnAdd = new();
        private readonly Button btnEdit = new();
        private readonly Button btnDelete = new();
        private readonly ListBox scheduleList = new();
        private readonly List<Schedule> schedules = new();

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "일정 관리 앱";
            this.Size = new Size(800, 600);

            calendar.Location = new Point(20, 20);
            calendar.DateSelected += Calendar_DateSelected;

            scheduleList.Location = new Point(300, 20);
            scheduleList.Size = new Size(460, 400);

            btnAdd.Text = "일정 추가";
            btnAdd.Location = new Point(300, 440);
            btnAdd.Size = new Size(100, 30);
            btnAdd.Click += BtnAdd_Click;

            btnEdit.Text = "일정 수정";
            btnEdit.Location = new Point(420, 440);
            btnEdit.Size = new Size(100, 30);
            btnEdit.Click += BtnEdit_Click;

            btnDelete.Text = "일정 삭제";
            btnDelete.Location = new Point(540, 440);
            btnDelete.Size = new Size(100, 30);
            btnDelete.Click += BtnDelete_Click;

            this.Controls.AddRange(new Control[] { calendar, scheduleList, btnAdd, btnEdit, btnDelete });
        }

        private void Calendar_DateSelected(object? sender, DateRangeEventArgs e)
        {
            UpdateScheduleList(e.Start);
        }

        private void UpdateScheduleList(DateTime date)
        {
            scheduleList.Items.Clear();
            var daySchedules = schedules.FindAll(s => s.Date.Date == date.Date);
            foreach (var schedule in daySchedules)
            {
                scheduleList.Items.Add($"{schedule.Title} - {schedule.Description}");
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (var form = new ScheduleForm(calendar.SelectionStart))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    schedules.Add(form.Schedule);
                    UpdateScheduleList(calendar.SelectionStart);

                    if (form.Schedule.HasNotification)
                    {
                        SetNotification(form.Schedule);
                    }
                }
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (scheduleList.SelectedIndex == -1) return;

            var selectedSchedule = schedules.Find(s => 
                s.Date.Date == calendar.SelectionStart.Date && 
                scheduleList.SelectedItem?.ToString()?.StartsWith(s.Title) == true);

            if (selectedSchedule != null)
            {
                using (var form = new ScheduleForm(selectedSchedule))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        schedules.Remove(selectedSchedule);
                        schedules.Add(form.Schedule);
                        UpdateScheduleList(calendar.SelectionStart);

                        if (form.Schedule.HasNotification)
                        {
                            SetNotification(form.Schedule);
                        }
                    }
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (scheduleList.SelectedIndex == -1) return;

            var selectedSchedule = schedules.Find(s => 
                s.Date.Date == calendar.SelectionStart.Date && 
                scheduleList.SelectedItem?.ToString()?.StartsWith(s.Title) == true);

            if (selectedSchedule != null)
            {
                schedules.Remove(selectedSchedule);
                UpdateScheduleList(calendar.SelectionStart);
            }
        }

        private void SetNotification(Schedule schedule)
        {
            var notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.BalloonTipTitle = "일정 알림";
            notifyIcon.BalloonTipText = $"{schedule.Title}\n{schedule.Description}";
            notifyIcon.Visible = true;

            System.Windows.Forms.Timer timer = new();
            timer.Interval = Math.Max(1, (int)(schedule.Date - DateTime.Now).TotalMilliseconds);
            timer.Tick += (sender, e) =>
            {
                notifyIcon.ShowBalloonTip(5000);
                timer.Stop();
                timer.Dispose();
                notifyIcon.Dispose();
            };
            timer.Start();
        }
    }

    public class ScheduleForm : Form
    {
        private readonly TextBox txtTitle = new();
        private readonly TextBox txtDescription = new();
        private readonly DateTimePicker dtpDateTime = new();
        private readonly CheckBox chkNotification = new();
        private readonly Button btnSave = new();
        private readonly Button btnCancel = new();

        public Schedule Schedule { get; private set; } = new();

        public ScheduleForm(DateTime defaultDate)
        {
            InitializeComponent();
            dtpDateTime.Value = defaultDate;
        }

        public ScheduleForm(Schedule schedule)
        {
            InitializeComponent();
            txtTitle.Text = schedule.Title;
            txtDescription.Text = schedule.Description;
            dtpDateTime.Value = schedule.Date;
            chkNotification.Checked = schedule.HasNotification;
        }

        private void InitializeComponent()
        {
            this.Text = "일정 추가/수정";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblTitle = new Label() { Text = "제목:", Location = new Point(20, 20) };
            txtTitle.Location = new Point(100, 20);
            txtTitle.Size = new Size(260, 20);

            var lblDescription = new Label() { Text = "내용:", Location = new Point(20, 50) };
            txtDescription.Location = new Point(100, 50);
            txtDescription.Size = new Size(260, 60);
            txtDescription.Multiline = true;

            var lblDateTime = new Label() { Text = "날짜/시간:", Location = new Point(20, 120) };
            dtpDateTime.Location = new Point(100, 120);
            dtpDateTime.Size = new Size(260, 20);

            chkNotification.Text = "알림 설정";
            chkNotification.Location = new Point(100, 150);

            btnSave.Text = "저장";
            btnSave.Location = new Point(100, 200);
            btnSave.Size = new Size(100, 30);
            btnSave.Click += BtnSave_Click;

            btnCancel.Text = "취소";
            btnCancel.Location = new Point(220, 200);
            btnCancel.Size = new Size(100, 30);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // 컨트롤 추가
            this.Controls.AddRange(new Control[] { 
                lblTitle, txtTitle,
                lblDescription, txtDescription,
                lblDateTime, dtpDateTime,
                chkNotification,
                btnSave, btnCancel
            });
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("제목을 입력해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Schedule = new Schedule
            {
                Title = txtTitle.Text,
                Description = txtDescription.Text,
                Date = dtpDateTime.Value,
                HasNotification = chkNotification.Checked
            };

            this.DialogResult = DialogResult.OK;
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
