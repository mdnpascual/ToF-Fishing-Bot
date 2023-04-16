using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsHook;
using System.Drawing;
using Config.Net;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ToF_Fishing_Bot
{
    public partial class MainWindow : Window
    {
        private Button defaultButton = new();

        private DispatcherTimer timer;
        private System.Drawing.Point previousPosition = new System.Drawing.Point(-1, 1);
        private System.Drawing.Point mousePosition = new System.Drawing.Point(-1, 1);
        private bool inEyeDropMode;
        private bool inCoordSelectMode;
        private bool isDarkMode;
        private System.Windows.Media.Color activeColor;
        private Button activeButton;
        private TextBlock activeLabel = new();
        private TextBlock activeCoordsLabel = new();
        private string backupButtonText = String.Empty;

        private IAppSettings settings;
        private IKeyboardMouseEvents m_GlobalHook;
        private FishingThread fishBot;
        private Thread fishBotThread;
        private Lens_Form lens_form;

        public MainWindow()
        {
            InitializeComponent();
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseMoveExt += GlobalHookMouseMove;
            m_GlobalHook.MouseClick += GlobalHookMouseLeftClick;

            settings = new ConfigurationBuilder<IAppSettings>()
                .UseJsonFile("settings.json")
                .Build();

            activeButton = defaultButton;

            ReadSettings();
            InitTheme(isDarkMode);

            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                timer.Tick += new EventHandler(timer_Tick);
            }
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

            fishBot = new FishingThread(
                settings, 
                LeftBox, 
                RightBox, 
                cursor, 
                bar, 
                StatusLabel, 
                middleBarImage, 
                cursorImage, 
                FishStaminaColorBtn, 
                PlayerStaminaColorBtn,
                null);
            fishBotThread = new Thread(fishBot.Start);
        }

        private void GlobalHookMouseLeftClick(object sender, WindowsHook.MouseEventArgs e)
        {
            if (inEyeDropMode || inCoordSelectMode)
            {
                WriteSettings();
                timer.Stop();
                ResetTempVars();
                lens_form.Dispose();
            }
        }

        private void ResetTempVars()
        {
            activeLabel.Text = backupButtonText;
            inEyeDropMode = false;
            inCoordSelectMode = false;
            activeButton = defaultButton;
            activeLabel = new();
            activeCoordsLabel = new();
            backupButtonText = String.Empty;
        }

        private void GlobalHookMouseMove(object? sender, MouseEventExtArgs e)
        {
            if (inEyeDropMode)
            {
                mousePosition = new System.Drawing.Point(e.Location.X, e.Location.Y);
            }
            if (inCoordSelectMode)
            {
                activeCoordsLabel.Text = "X: " + e.Location.X + "\nY: " + e.Location.Y;
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            fishBot.Stop();
            inEyeDropMode = false;
            inCoordSelectMode = false;
            activeButton = new();
            activeLabel = new();
            activeCoordsLabel = new();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (previousPosition != mousePosition && inEyeDropMode)
            {
                Bitmap bmp = new Bitmap(1, 1);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(mousePosition, new System.Drawing.Point(0, 0), new System.Drawing.Size(1, 1));
                }
                var color = bmp.GetPixel(0, 0);
                activeColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                activeButton.Background = new SolidColorBrush(activeColor);
                var invertedColor = System.Drawing.Color.FromArgb(color.ToArgb() ^ 0xffffff);
                activeLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(invertedColor.A, invertedColor.R, invertedColor.G, invertedColor.B));

            }
            previousPosition = mousePosition;
        }

        private void FishStaminaColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if(activeButton == defaultButton)
            {
                HandleButtonClick(FishStaminaColorBtn, FishStaminaColorLabel, "Press Left click to select\nColor and bottom most point", FishStaminaCoords);
            }
        }

        private void MiddleBarColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == defaultButton)
            {
                HandleButtonClick(MiddleBarColorBtn, MiddleBarColorLabel, "Press Left click\nto select Color");
            }
        }

        private void PlayerStaminaColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == defaultButton)
            {
                HandleButtonClick(PlayerStaminaColorBtn, PlayerStaminaColorLabel, "Press Left click to select\nColor and bottom most point", PlayerStaminaCoords);
            }
        }

        private void UpperLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == defaultButton)
            {
                HandleButtonClick(UpperLeftBtn, UpperLeftLabel, "Press Left click\nto specify coords", UpperLeftCoords);
            }
        }

        private void LowerRightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == defaultButton)
            {
                HandleButtonClick(LowerRightBtn, LowerRightLabel, "Press Left click\nto specify coords", LowerRightCoords);
            }
        }

        private void HandleButtonClick(
            Button btn,
            TextBlock label,
            string labelText,
            TextBlock? coordLabel = null)
        {
            activeButton = btn;
            activeLabel = label;
            if(coordLabel != null)
            {
                activeCoordsLabel = coordLabel;
                inCoordSelectMode = true;
            }
            backupButtonText = new TextRange(activeLabel.ContentStart, activeLabel.ContentEnd).Text;
            activeLabel.Text = labelText;
            timer.Start();
            inEyeDropMode = true;

            lens_form = new Lens_Form()
            {
                Size = new System.Drawing.Size(settings.ZoomSize_X, settings.ZoomSize_Y),
                AutoClose = true,
                HideCursor = false,
                ZoomFactor = settings.ZoomFactor,
                NearestNeighborInterpolation = false
            };
            lens_form.Show();
        }

        private void ReadSettings()
        {
            FishStaminaColorBtn.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (byte)settings.FishStaminaColor_A,
                (byte)settings.FishStaminaColor_R,
                (byte)settings.FishStaminaColor_G,
                (byte)settings.FishStaminaColor_B));
            var tempColor = System.Drawing.Color.FromArgb(System.Drawing.Color.FromArgb(
                settings.FishStaminaColor_A,
                settings.FishStaminaColor_R,
                settings.FishStaminaColor_G,
                settings.FishStaminaColor_B).ToArgb() ^ 0xffffff);
            FishStaminaColorLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(tempColor.A, tempColor.R, tempColor.G, tempColor.B));

            MiddleBarColorBtn.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (byte)settings.MiddleBarColor_A,
                (byte)settings.MiddleBarColor_R,
                (byte)settings.MiddleBarColor_G,
                (byte)settings.MiddleBarColor_B));
            tempColor = System.Drawing.Color.FromArgb(System.Drawing.Color.FromArgb(
                settings.MiddleBarColor_A,
                settings.MiddleBarColor_R,
                settings.MiddleBarColor_G,
                settings.MiddleBarColor_B).ToArgb() ^ 0xffffff);
            MiddleBarColorLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(tempColor.A, tempColor.R, tempColor.G, tempColor.B));

            PlayerStaminaColorBtn.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (byte)settings.PlayerStaminaColor_A,
                (byte)settings.PlayerStaminaColor_R,
                (byte)settings.PlayerStaminaColor_G,
                (byte)settings.PlayerStaminaColor_B));
            tempColor = System.Drawing.Color.FromArgb(System.Drawing.Color.FromArgb(
                settings.PlayerStaminaColor_A,
                settings.PlayerStaminaColor_R,
                settings.PlayerStaminaColor_G,
                settings.PlayerStaminaColor_B).ToArgb() ^ 0xffffff);
            PlayerStaminaColorLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(tempColor.A, tempColor.R, tempColor.G, tempColor.B));

            FishStaminaCoords.Text = "X: " + settings.FishStaminaPoint_X + "\nY: " + settings.FishStaminaPoint_Y;
            PlayerStaminaCoords.Text = "X: " + settings.PlayerStaminaPoint_X + "\nY: " + settings.PlayerStaminaPoint_Y;
            UpperLeftCoords.Text = "X: " + settings.UpperLeftBarPoint_X + "\nY: " + settings.UpperLeftBarPoint_Y;
            LowerRightCoords.Text = "X: " + settings.LowerRightBarPoint_X + "\nY: " + settings.LowerRightBarPoint_Y;

            isDarkMode = settings.IsDarkMode > 0;

            MoveLeftLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_MoveLeft);
            MoveRightLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_MoveRight);
        }

        private bool WriteSettings()
        {
            switch (activeButton.Name)
            {
                case "FishStaminaColorBtn":
                    settings.FishStaminaColor_A = activeColor.A;
                    settings.FishStaminaColor_R = activeColor.R;
                    settings.FishStaminaColor_G = activeColor.G;
                    settings.FishStaminaColor_B = activeColor.B;
                    settings.FishStaminaPoint_X = mousePosition.X;
                    settings.FishStaminaPoint_Y = mousePosition.Y;
                    break;
                case "MiddleBarColorBtn":
                    settings.MiddleBarColor_A = activeColor.A;
                    settings.MiddleBarColor_R = activeColor.R;
                    settings.MiddleBarColor_G = activeColor.G;
                    settings.MiddleBarColor_B = activeColor.B;
                    break;
                case "PlayerStaminaColorBtn":
                    settings.PlayerStaminaColor_A = activeColor.A;
                    settings.PlayerStaminaColor_R = activeColor.R;
                    settings.PlayerStaminaColor_G = activeColor.G;
                    settings.PlayerStaminaColor_B = activeColor.B;
                    settings.PlayerStaminaPoint_X = mousePosition.X;
                    settings.PlayerStaminaPoint_Y = mousePosition.Y;
                    break;
                case "UpperLeftBtn":
                    settings.UpperLeftBarPoint_X = mousePosition.X;
                    settings.UpperLeftBarPoint_Y = mousePosition.Y;
                    break;
                case "LowerRightBtn":
                    settings.LowerRightBarPoint_X = mousePosition.X;
                    settings.LowerRightBarPoint_Y = mousePosition.Y;
                    break;
                case "ThemeModeBtn":
                    settings.IsDarkMode = isDarkMode ? 1 : 0 ;
                    break;
            }
            return true;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if(SanityCheck())
            {
                var gameHandle = GetGameHandle();
                if (!fishBot.isRunning)
                {
                    fishBot.isRunning = true;
                    StartLabel.Text = "Stop\nFishing";
                    if (!fishBotThread.IsAlive)
                    {
                        fishBot.GameHandle = gameHandle;
                        fishBotThread.Start();
                    }
                }
                else
                {
                    fishBot.Stop();
                    StartLabel.Text = "Start\nFishing";
                    fishBot = new FishingThread(
                        settings,
                        LeftBox,
                        RightBox,
                        cursor,
                        bar,
                        StatusLabel,
                        middleBarImage,
                        cursorImage,
                        FishStaminaColorBtn,
                        PlayerStaminaColorBtn,
                        gameHandle);
                    fishBotThread = new Thread(fishBot.Start);
                }
            }
        }

        private IntPtr? GetGameHandle()
        {
            var message = String.Empty;
            var noErrors = true;

            Process[] processes = Process.GetProcessesByName(settings.GameProcessName);

            if (processes.Length == 0)
            {
                message = "Failed to find the Game. Either it's not running or the tool is not ran as admin";
                noErrors = false;
            }
            else if (processes.Length > 1)
            {
                message = "Found more than one instance of the Game. This is not normal";
                noErrors = false;
            }

            if (!noErrors)
            {
                MessageBox.Show(message, "Game Not Found. Running Tool as Simulation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            return processes.First().MainWindowHandle;
        }

        private bool SanityCheck()
        {
            var noErrors = true;
            var message = String.Empty;
            if(settings.LowerRightBarPoint_Y < settings.UpperLeftBarPoint_Y)
            {
                message += "Lower Right Bar Point Y value must be greater than Upper Left Bar Point.\n";
                noErrors = false;
            }
            if (settings.LowerRightBarPoint_X < settings.UpperLeftBarPoint_X)
            {
                message += "Lower Right Bar Point X value must be greater than Upper Left Bar Point.\n";
                noErrors = false;
            }
            if (settings.LowerRightBarPoint_Y > settings.FishStaminaPoint_Y)
            {
                message += "Fish Stamina point is not the lowest point\n";
                noErrors = false;
            }
            if (settings.LowerRightBarPoint_Y > settings.PlayerStaminaPoint_Y)
            {
                message += "Player Stamina point is not the lowest point\n";
                noErrors = false;
            }
            if (settings.LowerRightBarPoint_Y - settings.UpperLeftBarPoint_Y < settings.MinimumMiddleBarHeight)
            {
                message += $"The difference between the Upper left point and Lower left point must be greater than or equal to {settings.MinimumMiddleBarHeight} pixels\n";
                noErrors = false;
            }

            if (!noErrors)
            {
                MessageBox.Show(message, "Sanity Checks", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return noErrors;
        }

        private void ThemeModeBtn_Click(object sender, RoutedEventArgs e)
        {
            isDarkMode = !isDarkMode;
            activeButton = ThemeModeBtn;
            WriteSettings();
            activeButton = defaultButton;
            InitTheme(isDarkMode);
        }

        private void InitTheme(bool darkModeTheme)
        {
            ThemeModeImg.Source = darkModeTheme ? Theme.DayImage : Theme.NightImage;
            SettingImg.Source = darkModeTheme ? Theme.DaySettingImage : Theme.NightSettingImage;
            MainWindows.Background = darkModeTheme ? Theme.ColorAccent1 : Theme.WhiteColor;
            MiddleBarGBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            MiddleBarGBox.BorderBrush = darkModeTheme ? Theme.ColorAccent2 : Theme.GBoxDefaultBorderColor;
            StaminaGBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            StaminaGBox.BorderBrush = darkModeTheme ? Theme.ColorAccent2 : Theme.GBoxDefaultBorderColor;
            OutputGBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            OutputGBox.BorderBrush = darkModeTheme ? Theme.ColorAccent2 : Theme.GBoxDefaultBorderColor;
            cursor.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            bar.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            StatusLabel.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            LeftBox.Stroke = darkModeTheme ? Theme.ColorAccent3 : Theme.BlackColor;
            RightBox.Stroke = darkModeTheme ? Theme.ColorAccent3 : Theme.BlackColor;

            if (UpperLeftBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || UpperLeftBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                UpperLeftBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                UpperLeftBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                UpperLeftLabel.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (MiddleBarColorBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || MiddleBarColorBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                MiddleBarColorBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                MiddleBarColorBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                MiddleBarColorLabel.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (LowerRightBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || LowerRightBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                LowerRightBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                LowerRightBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                LowerRightLabel.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (FishStaminaColorBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || FishStaminaColorBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                FishStaminaColorBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                FishStaminaColorBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                FishStaminaColorLabel.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (PlayerStaminaColorBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || PlayerStaminaColorBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                PlayerStaminaColorBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                PlayerStaminaColorBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                PlayerStaminaColorLabel.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (StartBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || StartBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                StartBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                StartBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                StartLabel.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (ThemeModeBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || ThemeModeBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                ThemeModeBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                ThemeModeBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
            }
            if (SettingBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || SettingBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                SettingBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                SettingBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
            }
            if (LeftBox.Fill.ToString().Equals(Theme.ColorAccent3.ToString()) || LeftBox.Fill.ToString().Equals(Theme.GreenColor.ToString()))
            {
                LeftBox.Fill = darkModeTheme ? Theme.ColorAccent3 : Theme.GreenColor;
            }
            if (RightBox.Fill.ToString().Equals(Theme.ColorAccent3.ToString()) || RightBox.Fill.ToString().Equals(Theme.GreenColor.ToString()))
            {
                RightBox.Fill = darkModeTheme ? Theme.ColorAccent3 : Theme.GreenColor;
            }
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!inEyeDropMode && !inCoordSelectMode)
            {
                SettingsWindow settingWindow = new(settings, m_GlobalHook);
                settingWindow.ShowDialog();
                ReadSettings();
            }
        }
    }
}
