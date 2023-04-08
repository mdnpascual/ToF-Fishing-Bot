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

        private ImageSource dayImage = new BitmapImage(new Uri("pack://application:,,,/img/day.png"));
        private ImageSource nightImage = new BitmapImage(new Uri("pack://application:,,,/img/night.png"));
        private ResourceDictionary styling = new() { Source = new Uri("/Tof_Fishing_Bot;component/Resources/StylingDictionary.xaml", UriKind.RelativeOrAbsolute) };
        private Style darkStyle = new();
        private Style lightStyle = new();

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

            if(styling != null)
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                darkStyle = styling["btnRoundDark"] as Style;
                lightStyle = styling["btnRoundLight"] as Style;
#pragma warning restore CS8601 // Possible null reference assignment.
            }
            else
            {
                MessageBox.Show("Styling Not Found. This shouldn't happen", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

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
                activeLabel.Text = backupButtonText;
                timer.Stop();
                inEyeDropMode = false;
                inCoordSelectMode = false;
                activeButton = defaultButton;
                activeLabel = new();
                activeCoordsLabel = new();
                backupButtonText = String.Empty;
                lens_form.Dispose();
            }
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
            InitTheme(isDarkMode);
        }

        private void InitTheme(bool darkModeTheme)
        {
            ThemeModeImg.Source = darkModeTheme ? dayImage : nightImage;
            MainWindows.Background = darkModeTheme ? ColorAccent1 : WhiteColor;
            MiddleBarGBox.Foreground = darkModeTheme ? ColorAccent4 : BlackColor;
            MiddleBarGBox.BorderBrush = darkModeTheme ? ColorAccent2 : GBoxDefaultBorderColor;
            StaminaGBox.Foreground = darkModeTheme ? ColorAccent4 : BlackColor;
            StaminaGBox.BorderBrush = darkModeTheme ? ColorAccent2 : GBoxDefaultBorderColor;
            OutputGBox.Foreground = darkModeTheme ? ColorAccent4 : BlackColor;
            OutputGBox.BorderBrush = darkModeTheme ? ColorAccent2 : GBoxDefaultBorderColor;
            cursor.Foreground = darkModeTheme ? ColorAccent4 : BlackColor;
            bar.Foreground = darkModeTheme ? ColorAccent4 : BlackColor;
            StatusLabel.Foreground = darkModeTheme ? ColorAccent4 : BlackColor;
            LeftBox.Stroke = darkModeTheme ? ColorAccent3 : BlackColor;
            RightBox.Stroke = darkModeTheme ? ColorAccent3 : BlackColor;

            if (UpperLeftBtn.Background.ToString().Equals(ButtonDefaultBGColor.ToString()) || UpperLeftBtn.Background.ToString().Equals(ColorAccent2.ToString()))
            {
                UpperLeftBtn.Background = darkModeTheme ? ColorAccent2 : ButtonDefaultBGColor;
                UpperLeftBtn.Style = darkModeTheme ? darkStyle : lightStyle;
                UpperLeftLabel.Foreground = darkModeTheme ? ColorAccent5 : BlackColor;
            }
            if (MiddleBarColorBtn.Background.ToString().Equals(ButtonDefaultBGColor.ToString()) || MiddleBarColorBtn.Background.ToString().Equals(ColorAccent2.ToString()))
            {
                MiddleBarColorBtn.Background = darkModeTheme ? ColorAccent2 : ButtonDefaultBGColor;
                MiddleBarColorBtn.Style = darkModeTheme ? darkStyle : lightStyle;
                MiddleBarColorLabel.Foreground = darkModeTheme ? ColorAccent5 : BlackColor;
            }
            if (LowerRightBtn.Background.ToString().Equals(ButtonDefaultBGColor.ToString()) || LowerRightBtn.Background.ToString().Equals(ColorAccent2.ToString()))
            {
                LowerRightBtn.Background = darkModeTheme ? ColorAccent2 : ButtonDefaultBGColor;
                LowerRightBtn.Style = darkModeTheme ? darkStyle : lightStyle;
                LowerRightLabel.Foreground = darkModeTheme ? ColorAccent5 : BlackColor;
            }
            if (FishStaminaColorBtn.Background.ToString().Equals(ButtonDefaultBGColor.ToString()) || FishStaminaColorBtn.Background.ToString().Equals(ColorAccent2.ToString()))
            {
                FishStaminaColorBtn.Background = darkModeTheme ? ColorAccent2 : ButtonDefaultBGColor;
                FishStaminaColorBtn.Style = darkModeTheme ? darkStyle : lightStyle;
                FishStaminaColorLabel.Foreground = darkModeTheme ? ColorAccent5 : BlackColor;
            }
            if (PlayerStaminaColorBtn.Background.ToString().Equals(ButtonDefaultBGColor.ToString()) || PlayerStaminaColorBtn.Background.ToString().Equals(ColorAccent2.ToString()))
            {
                PlayerStaminaColorBtn.Background = darkModeTheme ? ColorAccent2 : ButtonDefaultBGColor;
                PlayerStaminaColorBtn.Style = darkModeTheme ? darkStyle : lightStyle;
                PlayerStaminaColorLabel.Foreground = darkModeTheme ? ColorAccent5 : BlackColor;
            }
            if (StartBtn.Background.ToString().Equals(ButtonDefaultBGColor.ToString()) || StartBtn.Background.ToString().Equals(ColorAccent2.ToString()))
            {
                StartBtn.Background = darkModeTheme ? ColorAccent2 : ButtonDefaultBGColor;
                StartBtn.Style = darkModeTheme ? darkStyle : lightStyle;
                StartLabel.Foreground = darkModeTheme ? ColorAccent5 : BlackColor;
            }
            if (ThemeModeBtn.Background.ToString().Equals(ButtonDefaultBGColor.ToString()) || ThemeModeBtn.Background.ToString().Equals(ColorAccent2.ToString()))
            {
                ThemeModeBtn.Background = darkModeTheme ? ColorAccent2 : ButtonDefaultBGColor;
                ThemeModeBtn.Style = darkModeTheme ? darkStyle : lightStyle;
            }
            if (LeftBox.Fill.ToString().Equals(ColorAccent3.ToString()) || LeftBox.Fill.ToString().Equals(GreenColor.ToString()))
            {
                LeftBox.Fill = darkModeTheme ? ColorAccent3 : GreenColor;
            }
            if (RightBox.Fill.ToString().Equals(ColorAccent3.ToString()) || RightBox.Fill.ToString().Equals(GreenColor.ToString()))
            {
                RightBox.Fill = darkModeTheme ? ColorAccent3 : GreenColor;
            }
        }

        private readonly SolidColorBrush WhiteColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
        private readonly SolidColorBrush BlackColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
        private readonly SolidColorBrush GreenColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 255, 0));
        private readonly SolidColorBrush GBoxDefaultBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 213, 223, 229));
        private readonly SolidColorBrush ButtonDefaultBGColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 221, 221, 221));
        private readonly SolidColorBrush ColorAccent1 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 9, 25, 40));
        private readonly SolidColorBrush ColorAccent2 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 26, 72, 116));
        private readonly SolidColorBrush ColorAccent3 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 53, 167, 241));
        private readonly SolidColorBrush ColorAccent4 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 141, 203, 246));
        private readonly SolidColorBrush ColorAccent5 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 226, 242, 252));
    }
}
