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

namespace ToF_Fishing_Bot
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private System.Drawing.Point previousPosition = new System.Drawing.Point(-1, 1);
        private System.Drawing.Point mousePosition = new System.Drawing.Point(-1, 1);
        private System.Drawing.Point mousePositionWithLeftClick = new System.Drawing.Point(-1, 1);
        private bool inEyeDropMode;
        private bool inCoordSelectMode;
        private System.Windows.Media.Color activeColor;
        private Button activeButton = null;
        private TextBlock activeLabel = null;
        private TextBlock activeCoordsLabel = null;
        private string backupButtonText = "";

        private IAppSettings settings;
        private IKeyboardMouseEvents m_GlobalHook;
        private FishingThread fishBot;
        private Thread fishBotThread;

        public MainWindow()
        {
            InitializeComponent();
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseMoveExt += GlobalHookMouseMove;
            m_GlobalHook.MouseClick += GlobalHookMouseLeftClick;

            settings = new ConfigurationBuilder<IAppSettings>()
                .UseJsonFile("settings.json")
                .Build();

            ReadSettings();

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
                PlayerStaminaColorBtn);
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
                activeButton = null;
                activeLabel = null;
                activeCoordsLabel = null;
                backupButtonText = null;
            }
            mousePositionWithLeftClick = new System.Drawing.Point(e.Location.X, e.Location.Y);
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
            activeButton = null;
            activeLabel = null;
            activeCoordsLabel = null;
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
            if(activeButton == null)
            {
                HandleButtonClick(FishStaminaColorBtn, FishStaminaColorLabel, "Press Left click to select\nColor and bottom most point", FishStaminaCoords);
            }
        }

        private void MiddleBarColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == null)
            {
                HandleButtonClick(MiddleBarColorBtn, MiddleBarColorLabel, "Press Left click\nto select Color");
            }
        }

        private void PlayerStaminaColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == null)
            {
                HandleButtonClick(PlayerStaminaColorBtn, PlayerStaminaColorLabel, "Press Left click to select\nColor and bottom most point", PlayerStaminaCoords);
            }
        }

        private void UpperLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == null)
            {
                HandleButtonClick(UpperLeftBtn, UpperLeftLabel, "Press Left click\nto specify coords", UpperLeftCoords);
            }
        }

        private void LowerRightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == null)
            {
                HandleButtonClick(LowerRightBtn, LowerRightLabel, "Press Left click\nto specify coords", LowerRightCoords);
            }
        }

        private void FishCaptureBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == null)
            {
                HandleButtonClick(FishCaptureBtn, FishCaptureLabel, "Press Left click\nto specify coords", FishCaptureCoords);
            }
        }

        private void TapToCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == null)
            {
                HandleButtonClick(TapToCloseBtn, TapToCloseLabel, "Press Left click\nto specify coords", TapToCloseCoords);
            }
        }

        private void HandleButtonClick(
            Button btn,
            TextBlock label,
            string labelText,
            TextBlock coordLabel = null)
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
            FishCaptureCoords.Text = "X: " + settings.FishCaptureButtonPoint_X + "\nY: " + settings.FishCaptureButtonPoint_Y;
            TapToCloseCoords.Text = "X: " + settings.TapToClosePoint_X + "\nY: " + settings.TapToClosePoint_Y;

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
                case "FishCaptureBtn":
                    settings.FishCaptureButtonPoint_X = mousePosition.X;
                    settings.FishCaptureButtonPoint_Y = mousePosition.Y;
                    break;
                case "TapToCloseBtn":
                    settings.TapToClosePoint_X = mousePosition.X;
                    settings.TapToClosePoint_Y = mousePosition.Y;
                    break;
            }
            return true;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if(SanityCheck())
            {
                if (!fishBot.isRunning)
                {
                    fishBot.isRunning = true;
                    StartLabel.Text = "Stop\nFishing";
                    if (!fishBotThread.IsAlive)
                    {
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
                        PlayerStaminaColorBtn);
                    fishBotThread = new Thread(fishBot.Start);
                }
            }
        }

        private bool SanityCheck()
        {
            var noErrors = true;
            var message = "";
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
    }
}
