using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.XImgProc;
using ToF_Fishing_Bot.Addon.DiscordInteractive;
using WindowsInput;

namespace ToF_Fishing_Bot
{
    class FishingThread
    {
        private IAppSettings settings;
        public bool isRunning = false;
        private DateTime _startTime = DateTime.UtcNow;
        private DateTime? _lastResetTime = null;
        private InputSimulator InputSimulator;

        private System.Windows.Shapes.Rectangle left;
        private System.Windows.Shapes.Rectangle right;
        private System.Windows.Controls.Label cursorLabel;
        private System.Windows.Controls.Label middleBarLabel;
        private System.Windows.Controls.Label statusLabel;
        private System.Windows.Controls.Image middleBarImage;
        private System.Windows.Controls.Image cursorImage;
        private System.Windows.Controls.Button fishStaminaButton;
        private System.Windows.Controls.Button playerStaminaButton;

        private bool APressed = false;
        private bool DPressed = false;

        private bool PlayerStamina_lagCompensationDone = false;
        private DispatcherTimer LagCompensationDelay = new();

        private Bitmap bmp = new(1, 1);
        private BitmapImage bi1;
        private BitmapImage bi2;
        private MemoryStream ms1;
        private MemoryStream ms2;

        private double colorThreshold;
        private double middleBarCenterThreshold;

        private ScreenStateLogger screenStateLogger;

        private Dispatcher dis = Dispatcher.CurrentDispatcher;
        private DispatcherTimer ReelDelay = new DispatcherTimer();
        private DispatcherTimer CaptureDelay = new DispatcherTimer();
        private DispatcherTimer ResetDelay = new DispatcherTimer();
        private FishingState state;

        public IntPtr? GameHandle = null;

        private IDiscordService discordService;

        public FishingThread(
            IAppSettings _settings,
            System.Windows.Shapes.Rectangle _left,
            System.Windows.Shapes.Rectangle _right,
            System.Windows.Controls.Label _cursorLabel,
            System.Windows.Controls.Label _middleBarLabel,
            System.Windows.Controls.Label _statusLabel,
            System.Windows.Controls.Image _middleBarImage,
            System.Windows.Controls.Image _cursorImage,
            System.Windows.Controls.Button _fishStaminaButton,
            System.Windows.Controls.Button _playerStaminaButton,
            IntPtr? _gameHandle)
        {
            settings = _settings;
            left = _left;
            right = _right;
            cursorLabel = _cursorLabel;
            middleBarLabel = _middleBarLabel;
            statusLabel = _statusLabel;
            middleBarImage = _middleBarImage;
            cursorImage = _cursorImage;
            fishStaminaButton = _fishStaminaButton;
            playerStaminaButton = _playerStaminaButton;
            InputSimulator = new InputSimulator();
            bi1 = new BitmapImage();
            bi2 = new BitmapImage();
            ms1 = new MemoryStream();
            ms2 = new MemoryStream();
            dis = Dispatcher.CurrentDispatcher;
            state = FishingState.NotFishing;

            if (_gameHandle.HasValue)
            {
                GameHandle = _gameHandle.Value;
            }

            colorThreshold = settings.StaminaColorDetectionThreshold;
            middleBarCenterThreshold = settings.MiddlebarColorDetectionThreshold;

            screenStateLogger = new ScreenStateLogger();

            if (!string.IsNullOrEmpty(_settings.DiscordHookUrl))
            {

                try
                {
                    _lastResetTime = null;
                    discordService = new DiscordService(_settings.DiscordHookUrl, _settings.DiscordUserId);
                }
                catch (ArgumentException e)
                {
                    MessageBox.Show(e.Message, "Discord Hook URL invalid", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Start()
        {
            screenStateLogger.ScreenRefreshed += (sender, data) =>
            {
                statusLabel.Dispatcher.Invoke(new Action(() => { statusLabel.Content = "Status: " + state.ToString() + (GameHandle == null ? " (Game not running)" : String.Empty); }));
                bmp = new Bitmap(new MemoryStream(data));
                bool fishStaminaDetected = FishStaminaDetector(bmp);
                bool playerStaminaDetected = PlayerStaminaDetector(bmp);
                fishStaminaButton.Dispatcher.Invoke(new Action(() =>
                {
                    fishStaminaButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                        255,
                        fishStaminaDetected ? (byte)0 : (byte)255,
                        fishStaminaDetected ? (byte)255 : (byte)0,
                        0));
                }));
                playerStaminaButton.Dispatcher.Invoke(new Action(() =>
                {
                    playerStaminaButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                        255,
                        playerStaminaDetected ? (byte)0 : (byte)255,
                        playerStaminaDetected ? (byte)255 : (byte)0,
                        0));
                }));

                Mat frame = ExtractCroppedFrame(bmp);
                var middleBarPos = GetMiddleBarAveragePos(frame);
                var fishingCursorPos = GetFishingCursorPos(frame);
                /*cursorLabel.Dispatcher.Invoke(new Action(() => { cursorLabel.Content = fishingCursorPos.ToString("0.##"); }));
                middleBarLabel.Dispatcher.Invoke(new Action(() => { middleBarLabel.Content = middleBarPos.ToString("0.##"); }));*/
                CenterCursor(middleBarPos, fishingCursorPos);

                // HANDLE STATE
                switch (state)
                {
                    case FishingState.NotFishing:
                        if (discordService != null && _lastResetTime != null && DateTime.UtcNow - _lastResetTime > TimeSpan.FromMinutes(3))
                        {
                            var notificationMsgTask = discordService.BuildOutOfBaitNotification(_startTime);
                            notificationMsgTask.Wait();
                            var notificationMsg = notificationMsgTask.Result;
                            var sendMsgTask = discordService.SendMessage(notificationMsg);
                            sendMsgTask.Wait();
                            _lastResetTime = null;
                        }
                        break;
                    case FishingState.Fishing:
                        // HANDLER IS ABOVE
                        break;
                    case FishingState.Reeling:
                        ClickFishCaptureButton();
                        break;
                    case FishingState.Captured:
                        CloseFishCaptureDialog();
                        ResetKeys();
                        break;
                    case FishingState.Reset:
                        ClickFishCaptureButton();
                        break;
                }

                // CHECK STATE
                switch (state)
                {
                    case FishingState.NotFishing:
                        if (fishStaminaDetected && playerStaminaDetected)
                        {
                            state = FishingState.Fishing;
                            PlayerStamina_lagCompensationDone = false;
                            LagCompensationDelay = new DispatcherTimer(DispatcherPriority.Send, dis);
                            LagCompensationDelay.Interval = new TimeSpan(0, 0, 0, 0, settings.Delay_LagCompensation);
                            LagCompensationDelay.Tick += (o, e) =>
                            {
                                PlayerStamina_lagCompensationDone = true;
                                LagCompensationDelay.Stop();
                            };
                            LagCompensationDelay.Start();
                        }
                        break;
                    case FishingState.Fishing:
                        if (!fishStaminaDetected && playerStaminaDetected && PlayerStamina_lagCompensationDone)
                        {
                            state = FishingState.ReelingStart;
                            ReelDelay = new DispatcherTimer(DispatcherPriority.Send, dis);
                            ReelDelay.Interval = new TimeSpan(0, 0, 0, 0, settings.Delay_FishCapture);
                            ReelDelay.Tick += (o, e) =>
                            {
                                state = FishingState.Reeling;
                                ReelDelay.Stop();
                            };
                            ReelDelay.Start();
                        }
                        else if (!playerStaminaDetected) state = FishingState.Captured;
                        break;
                    case FishingState.ReelingStart:
                        // DO NOTHING
                        break;
                    case FishingState.Reeling:
                        state = FishingState.CaptureStart;
                        CaptureDelay = new DispatcherTimer(DispatcherPriority.Send, dis);
                        CaptureDelay.Interval = new TimeSpan(0, 0, 0, 0, settings.Delay_DismissFishCaptureDialogue);
                        CaptureDelay.Tick += (o, e) =>
                        {
                            state = FishingState.Captured;
                            CaptureDelay.Stop();
                        };
                        CaptureDelay.Start();
                        break;
                    case FishingState.CaptureStart:
                        // DO NOTHING
                        break;
                    case FishingState.Captured:
                        state = FishingState.ResetStart;
                        ResetDelay = new DispatcherTimer(DispatcherPriority.Send, dis);
                        ResetDelay.Interval = new TimeSpan(0, 0, 0, 0, settings.Delay_Restart);
                        ResetDelay.Tick += (o, e) =>
                        {
                            state = FishingState.Reset;
                            ResetDelay.Stop();
                        };
                        ResetDelay.Start();
                        break;
                    case FishingState.ResetStart:
                        _lastResetTime = DateTime.UtcNow;
                        break;
                    case FishingState.Reset:
                        state = FishingState.NotFishing;
                        break;
                }
            };
            isRunning = true;
            _startTime = DateTime.UtcNow;
            screenStateLogger.Start();
        }

        private Mat ExtractCroppedFrame(Bitmap image)
        {
            var cropped = image.CropSmall(
                                settings.UpperLeftBarPoint_X,
                                settings.UpperLeftBarPoint_Y,
                                settings.LowerRightBarPoint_X - settings.UpperLeftBarPoint_X,
                                settings.LowerRightBarPoint_Y - settings.UpperLeftBarPoint_Y);

            if (cropped.Height > 5)
            {
                var bordered = new Bitmap(cropped.Width + 20, cropped.Height);
                using (Graphics g = Graphics.FromImage(bordered))
                {
                    g.DrawImage(cropped, new System.Drawing.Point(9, 0));
                }

                var frame = BitmapConverter.ToMat(bordered);
                /*Cv2.ImShow("test", frame);
                  Cv2.WaitKey();*/
                /*var frame = BitmapConverter.ToMat(OldCapture());*/
                return frame;
            }
            else
            {
                var bordered = new Bitmap(cropped.Width * 4 + 20, cropped.Height * 4);
                using (Graphics g = Graphics.FromImage(bordered))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(cropped, 9, 0, cropped.Width * 4 + 20, cropped.Height * 4);
                }

                var frame = BitmapConverter.ToMat(bordered);
                /*Cv2.ImShow("test", frame);
                  Cv2.WaitKey();*/
                /*var frame = BitmapConverter.ToMat(OldCapture());*/
                return frame;
            }
        }

        private bool FishStaminaDetector(Bitmap image)
        {
            System.Drawing.Color pixelColor = image.GetPixel(settings.FishStaminaPoint_X, settings.FishStaminaPoint_Y);
            var colorDistance = Math.Sqrt(
                Math.Pow(pixelColor.R - settings.FishStaminaColor_R, 2) +
                Math.Pow(pixelColor.G - settings.FishStaminaColor_G, 2) +
                Math.Pow(pixelColor.B - settings.FishStaminaColor_B, 2));
            cursorLabel.Dispatcher.Invoke(new Action(() => { cursorLabel.Content = colorDistance.ToString("0.##"); }));
            return colorDistance < colorThreshold;
        }

        private bool PlayerStaminaDetector(Bitmap image)
        {
            System.Drawing.Color pixelColor = image.GetPixel(settings.PlayerStaminaPoint_X, settings.PlayerStaminaPoint_Y);
            var colorDistance = Math.Sqrt(
                Math.Pow(pixelColor.R - settings.PlayerStaminaColor_R, 2) +
                Math.Pow(pixelColor.G - settings.PlayerStaminaColor_G, 2) +
                Math.Pow(pixelColor.B - settings.PlayerStaminaColor_B, 2));
            middleBarLabel.Dispatcher.Invoke(new Action(() => { middleBarLabel.Content = colorDistance.ToString("0.##"); }));
            return colorDistance < colorThreshold;
        }

        private void CenterCursor(double middleBarPos, double fishingCursorPos)
        {
            if (middleBarPos - middleBarCenterThreshold > fishingCursorPos)
            {
                if (!DPressed)
                {
                    if (GameHandle != null)
                    {
                        InputSimulator.Keyboard.KeyUpBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_MoveLeft);
                        InputSimulator.Keyboard.KeyDownBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_MoveRight);
                    }
                    DPressed = true;
                    APressed = false;

                    right.Dispatcher.Invoke(() =>
                    {
                        right.Fill = settings.IsDarkMode > 0 ? Theme.ColorAccent2 : Theme.GreenColor;
                    });
                    left.Dispatcher.Invoke(() =>
                    {
                        left.Fill = System.Windows.Media.Brushes.Transparent;
                    });
                }
            }
            else if (middleBarPos + middleBarCenterThreshold < fishingCursorPos)
            {
                if (!APressed)
                {
                    if (GameHandle != null)
                    {
                        InputSimulator.Keyboard.KeyUpBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_MoveRight);
                        InputSimulator.Keyboard.KeyDownBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_MoveLeft);
                    }
                    DPressed = false;
                    APressed = true;

                    left.Dispatcher.Invoke(() =>
                    {
                        left.Fill = settings.IsDarkMode > 0 ? Theme.ColorAccent2 : Theme.GreenColor;
                    });
                    right.Dispatcher.Invoke(() =>
                    {
                        right.Fill = System.Windows.Media.Brushes.Transparent;
                    });
                }
            }
            else
            {
                if (APressed)
                {
                    if (GameHandle != null)
                    {
                        InputSimulator.Keyboard.KeyUpBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_MoveLeft);
                    }
                    APressed = false;
                    left.Dispatcher.Invoke(() =>
                    {
                        left.Fill = System.Windows.Media.Brushes.Transparent;
                    });
                }
                if (DPressed)
                {
                    if (GameHandle != null)
                    {
                        InputSimulator.Keyboard.KeyUpBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_MoveRight);
                    }
                    DPressed = false;
                    right.Dispatcher.Invoke(() =>
                    {
                        right.Fill = System.Windows.Media.Brushes.Transparent;
                    });
                }
            }
        }

        public void Stop()
        {
            _lastResetTime = null;
            screenStateLogger.Stop();
            screenStateLogger = new ScreenStateLogger();
        }

        public double GetMiddleBarAveragePos(Mat frame)
        {
            // MASK MIDDLE RECTANGLE COLOR
            var lowerBoundsColor = new OpenCvSharp.Scalar(
                Math.Clamp(settings.MiddleBarColor_B * 0.90, 0, 255),
                Math.Clamp(settings.MiddleBarColor_G * 0.90, 0, 255),
                Math.Clamp(settings.MiddleBarColor_R * 0.90, 0, 255),
                255);
            var upperBoundsColor = new OpenCvSharp.Scalar(
                Math.Clamp(settings.MiddleBarColor_B * 1.10, 0, 255),
                Math.Clamp(settings.MiddleBarColor_G * 1.10, 0, 255),
                Math.Clamp(settings.MiddleBarColor_R * 1.10, 0, 255),
                255);

            // GET X POSITION OF MIDDLE BAR
            var masked = new Mat();
            Cv2.InRange(frame, lowerBoundsColor, upperBoundsColor, masked);
            /*Cv2.ImShow("masked", masked);*/

            var lineDetect = FastLineDetector.Create(lengthThreshold: settings.MinimumMiddleBarHeight - 1);
            try
            {
                var lines = lineDetect.Detect(masked);
                var max_X = lines.Max(line => line.Item0);
                var min_X = lines.Min(line => line.Item0);
                /*lineDetect.DrawSegments(frame, lines);
                Cv2.ImShow("color", frame);
                Cv2.WaitKey();*/

                lineDetect.DrawSegments(masked, lines);
                ms1 = new MemoryStream();
                masked.ToBitmap().Save(ms1, System.Drawing.Imaging.ImageFormat.Png);
                ms1.Position = 0;

                dis.Invoke(() =>
                {
                    bi1 = new BitmapImage();
                    bi1.BeginInit();
                    bi1.StreamSource = ms1;
                    bi1.EndInit();
                    bi1.Freeze();
                    middleBarImage.Dispatcher.Invoke(() =>
                    {
                        middleBarImage.Source = bi1;
                    });
                });

                return (min_X + max_X) / 2.0;
            }
            catch (Exception) { }
            return 0;
        }

        public double GetFishingCursorPos(Mat frame)
        {
            // MASK WHITE COLOR
            var lowerBoundsColor = new OpenCvSharp.Scalar(225, 225, 225, 255);
            var upperBoundsColor = new OpenCvSharp.Scalar(255, 255, 255, 255);

            // GET X POSITION OF FISHING CURSOR
            var masked = new Mat();
            Cv2.InRange(frame, lowerBoundsColor, upperBoundsColor, masked);
            /*Cv2.ImShow("masked", masked);*/
            var lineDetect = FastLineDetector.Create(lengthThreshold: settings.MinimumMiddleBarHeight - 1);

            try
            {
                var lines = lineDetect.Detect(masked);
                if (lines.Length > 1)
                {
                    var maxMiddleBar_X = lines.Max(line => line.Item0);
                    var minMiddleBar_X = lines.Min(line => line.Item0);

                    /*lineDetect.DrawSegments(frame, lines);
                    Cv2.ImShow("color", frame);
                    Cv2.WaitKey();*/
                    lineDetect.DrawSegments(masked, lines);
                    ms2 = new MemoryStream();
                    masked.ToBitmap().Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
                    ms2.Position = 0;

                    dis.Invoke(() =>
                    {
                        bi2 = new BitmapImage();
                        bi2.BeginInit();
                        bi2.StreamSource = ms2;
                        bi2.EndInit();
                        bi2.Freeze();
                        cursorImage.Dispatcher.Invoke(() =>
                        {
                            cursorImage.Source = bi2;
                        });
                    });

                    return (minMiddleBar_X + maxMiddleBar_X) / 2.0;
                }

                return lines[0].Item0;
            }
            catch (Exception) { }
            return 0;
        }

        public void ClickFishCaptureButton()
        {
            if (GameHandle != null)
            {
                InputSimulator.Keyboard.KeyDownBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_FishCapture);
                InputSimulator.Mouse.Sleep(25);
                InputSimulator.Keyboard.KeyUpBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_FishCapture);
                InputSimulator.Mouse.Sleep(25);
            }
        }

        public void CloseFishCaptureDialog()
        {
            if (GameHandle != null)
            {
                InputSimulator.Keyboard.KeyDownBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_DismissFishDialogue);
                InputSimulator.Mouse.Sleep(25);
                InputSimulator.Keyboard.KeyUpBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_DismissFishDialogue);
                InputSimulator.Mouse.Sleep(25);
            }
        }

        public void ResetKeys()
        {
            if (GameHandle != null)
            {
                InputSimulator.Keyboard.KeyUpBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_MoveLeft);
            }
            APressed = false;
            left.Dispatcher.Invoke(() =>
            {
                left.Fill = System.Windows.Media.Brushes.Transparent;
            });

            if (GameHandle != null)
            {
                InputSimulator.Keyboard.KeyUpBackground(GameHandle.Value, (WindowsInput.Native.VirtualKeyCode)settings.KeyCode_MoveRight);
            }
            DPressed = false;
            right.Dispatcher.Invoke(() =>
            {
                right.Fill = System.Windows.Media.Brushes.Transparent;
            });
        }

        enum FishingState
        {
            NotFishing,
            Fishing,
            ReelingStart,
            Reeling,
            CaptureStart,
            Captured,
            ResetStart,
            Reset
        }
    }
}
