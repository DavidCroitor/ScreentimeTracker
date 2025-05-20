using System.Windows.Forms.DataVisualization.Charting;

namespace ScreentimeTracker.UI
{
    partial class StatsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ImageList _appIconList;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._appIconList = new System.Windows.Forms.ImageList(this.components);
            
            this.Text = "Screen Time Statistics";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

            _mainSplitContainer = new SplitContainer();
            _mainSplitContainer.Dock = DockStyle.Fill;
            _mainSplitContainer.Orientation = Orientation.Horizontal; 
            _mainSplitContainer.Panel1MinSize = 300; 
            _mainSplitContainer.Panel2MinSize = 150;
            _mainSplitContainer.SplitterWidth = 5;
            this.Controls.Add(_mainSplitContainer);
            
            _mainSplitContainer.SplitterDistance = 300;
            
            // Top panel - Chart area
            _mainPanel = new Panel();
            _mainPanel.Dock = DockStyle.Fill;
            _mainSplitContainer.Panel1.Controls.Add(_mainPanel);
            
            // Week navigation panel
            Panel weekNavPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(10)
            };
            
            _prevWeekButton = new Button
            {
                Text = "< Previous Week",
                Dock = DockStyle.Left,
                Width = 120
            };
            
            _nextWeekButton = new Button
            {
                Text = "Next Week >",
                Dock = DockStyle.Right,
                Width = 120
            };
            
            _weekRangeLabel = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
            };
            
            weekNavPanel.Controls.Add(_prevWeekButton);
            weekNavPanel.Controls.Add(_nextWeekButton);
            weekNavPanel.Controls.Add(_weekRangeLabel);
            _mainPanel.Controls.Add(weekNavPanel);
            
            // Weekly chart panel
            Panel chartPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            _weeklyChart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.ControlLightLight
            };
            
            // Configure chart
            ChartArea chartArea = new ChartArea("WeeklyUsage");
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.Title = "Day of Week";
            chartArea.AxisY.Title = "Hours";
            chartArea.AxisY.LabelStyle.Format = "0.0";
            _weeklyChart.ChartAreas.Add(chartArea);
            
            Series series = new Series("ScreenTime")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.SteelBlue
            };
            _weeklyChart.Series.Add(series);
            
            chartPanel.Controls.Add(_weeklyChart);
            _mainPanel.Controls.Add(chartPanel);

            this._appIconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._appIconList.ImageSize = new System.Drawing.Size(16, 16);

            // Bottom panel - App list area
            Panel appUsagePanel = new Panel();
            appUsagePanel.Dock = DockStyle.Fill;
            _mainSplitContainer.Panel2.Controls.Add(appUsagePanel);
            
            // Selected day details
            _selectedDayLabel = new Label
            {
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(10, 5, 10, 5),
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
            };
            
            appUsagePanel.Controls.Add(_selectedDayLabel);

            // App usage list
            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            appUsagePanel.Controls.Add(tableLayout);


            _appListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                UseCompatibleStateImageBehavior = false,
                BorderStyle = BorderStyle.FixedSingle,
                // GridLines = true,
                OwnerDraw = true
            };
            
            ColumnHeader appColumn = new ColumnHeader();
            appColumn.Text = "Application";
            appColumn.Width = 250;
            appColumn.TextAlign = HorizontalAlignment.Center;

            ColumnHeader timeColumn = new ColumnHeader();
            timeColumn.Text = "Time Used";
            timeColumn.Width = 100;
            timeColumn.TextAlign = HorizontalAlignment.Center;

            ColumnHeader percentageColumn = new ColumnHeader();
            percentageColumn.Text = "Percentage";
            percentageColumn.Width = 100;
            percentageColumn.TextAlign = HorizontalAlignment.Center;

            _appListView.Columns.AddRange(new ColumnHeader[] { 
                appColumn, timeColumn, percentageColumn 
            });
            this._appListView.SmallImageList = this._appIconList;
            
            tableLayout.Controls.Add(_appListView, 1, 0);
        }

        #endregion

        private SplitContainer _mainSplitContainer;
        private Panel _mainPanel;
        private Chart _weeklyChart;
        private Button _prevWeekButton;
        private Button _nextWeekButton;
        private Label _weekRangeLabel;
        private Label _selectedDayLabel;
        private ListView _appListView;
    }
}