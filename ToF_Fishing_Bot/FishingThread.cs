using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.XImgProc;
using WindowsInput;

namespace ToF_Fishing_Bot
{
    class FishingThread
    {
        private IAppSettings settings;
        public bool isRunning = false;
        private InputSimulator InputSimulator;
        public System.Drawing.Point lastMousePosition;
        public int counter = 0;

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

        private System.Windows.Media.Color green = System.Windows.Media.Color.FromArgb(255, 0, 255, 0);

        /*private System.Drawing.Point upperLeftSource;
        private System.Drawing.Size blockRegionSize;
        private System.Drawing.Point upperLeftDestination;*/
        private Bitmap bmp;
        private BitmapImage bi1;
        private BitmapImage bi2;
        private MemoryStream ms1;
        private MemoryStream ms2;

        private double colorThreshold = 40.0;
        private double middleBarCenterThreshold = 10.0;

        private ScreenStateLogger screenStateLogger;

        private Dispatcher dis;
        private DispatcherTimer ReelDelay;
        private DispatcherTimer CaptureDelay;
        private DispatcherTimer ResetDelay;
        private FishingState state;

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
            System.Windows.Controls.Button _playerStaminaButton)
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

            /*upperLeftSource = new System.Drawing.Point(
                settings.UpperLeftBarPoint_X,
                settings.UpperLeftBarPoint_Y);
            blockRegionSize = new System.Drawing.Size(
                                    settings.LowerRightBarPoint_X - settings.UpperLeftBarPoint_X,
                                    settings.LowerRightBarPoint_Y - settings.UpperLeftBarPoint_Y);
            upperLeftDestination = new System.Drawing.Point(0, 0);*/

            bmp = new Bitmap(settings.LowerRightBarPoint_X - settings.UpperLeftBarPoint_X + 20, settings.LowerRightBarPoint_Y - settings.UpperLeftBarPoint_Y);
            screenStateLogger = new ScreenStateLogger();
        }

        public void Start()
        {
            screenStateLogger.ScreenRefreshed += (sender, data) =>
            {
                statusLabel.Dispatcher.Invoke(new Action(() => { statusLabel.Content = "Status: " + state.ToString(); }));
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

                Mat frame = ExtractCroppedFrame();
                var middleBarPos = GetMiddleBarAveragePos(frame);
                var fishingCursorPos = GetFishingCursorPos(frame);
                /*cursorLabel.Dispatcher.Invoke(new Action(() => { cursorLabel.Content = fishingCursorPos.ToString("0.##"); }));
                middleBarLabel.Dispatcher.Invoke(new Action(() => { middleBarLabel.Content = middleBarPos.ToString("0.##"); }));*/
                CenterCursor(middleBarPos, fishingCursorPos);

                // HANDLE STATE
                switch (state)
                {
                    case FishingState.NotFishing:
                        break;
                    case FishingState.Fishing:
                        // HANDLER IS ABOVE
                        break;
                    case FishingState.Reeling:
                        ClickFishCaptureButton();
                        ClickFishCaptureButton();
                        break;
                    case FishingState.Captured:
                        ClickTapToCloseButton();
                        ClickTapToCloseButton();
                        ClickTapToCloseButton();
                        ResetKeys();
                        break;
                    case FishingState.Reset:
                        ClickFishCaptureButton();
                        ClickFishCaptureButton();
                        break;
                }

                // CHECK STATE
                switch (state)
                {
                    case FishingState.NotFishing:
                        if (fishStaminaDetected && playerStaminaDetected) state = FishingState.Fishing;
                        break;
                    case FishingState.Fishing:
                        if (!fishStaminaDetected && playerStaminaDetected)
                        {
                            state = FishingState.ReelingStart;
                            ReelDelay = new DispatcherTimer(DispatcherPriority.Send, dis);
                            ReelDelay.Interval = new TimeSpan(0,0,0,2);
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
                        CaptureDelay.Interval = new TimeSpan(0, 0, 0, 2);
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
                        ResetDelay.Interval = new TimeSpan(0, 0, 0, 2);
                        ResetDelay.Tick += (o, e) =>
                        {
                            state = FishingState.Reset;
                            ResetDelay.Stop();
                        };
                        ResetDelay.Start();
                        break;
                    case FishingState.ResetStart:
                        // DO NOTHING
                        break;
                    case FishingState.Reset:
                        state = FishingState.NotFishing;
                        break;
                }
            };
            isRunning = true;
            screenStateLogger.Start();
        }

        private Mat ExtractCroppedFrame()
        {
            var cropped = bmp.CropSmall(
                                settings.UpperLeftBarPoint_X,
                                settings.UpperLeftBarPoint_Y,
                                settings.LowerRightBarPoint_X - settings.UpperLeftBarPoint_X,
                                settings.LowerRightBarPoint_Y - settings.UpperLeftBarPoint_Y);

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
                    InputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_D);
                    InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_A);
                    DPressed = true;
                    APressed = false;

                    right.Dispatcher.Invoke(() =>
                    {
                        right.Fill = new SolidColorBrush(green);
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
                    InputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_A);
                    InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_D);
                    DPressed = false;
                    APressed = true;

                    left.Dispatcher.Invoke(() =>
                    {
                        left.Fill = new SolidColorBrush(green);
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
                    InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_A);
                    APressed = false;
                    left.Dispatcher.Invoke(() =>
                    {
                        left.Fill = System.Windows.Media.Brushes.Transparent;
                    });
                }
                if (DPressed)
                {
                    InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_D);
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
            screenStateLogger.Stop();
            screenStateLogger = new ScreenStateLogger();
        }

        /*public Bitmap OldCapture()
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(
                    upperLeftSource,
                    upperLeftDestination,
                    blockRegionSize);
            }
            return bmp;
        }*/

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

            var lineDetect = FastLineDetector.Create();
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
            catch(Exception){}
            return 0;
        }

        public double GetFishingCursorPos(Mat frame)
        {
            // MASK WHITE COLOR
            var lowerBoundsColor = new OpenCvSharp.Scalar(225, 225, 225, 255);
            var upperBoundsColor = new OpenCvSharp.Scalar(255,255,255,255);

            // GET X POSITION OF FISHING CURSOR
            var masked = new Mat();
            Cv2.InRange(frame, lowerBoundsColor, upperBoundsColor, masked);
            /*Cv2.ImShow("masked", masked);*/
            var lineDetect = FastLineDetector.Create();

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
            catch (Exception){}
            return 0;
        }

        public void ClickFishCaptureButton()
        {
            SetCursorPos(settings.FishCaptureButtonPoint_X, settings.FishCaptureButtonPoint_Y);
            InputSimulator.Mouse.Sleep(25);
            InputSimulator.Mouse.LeftButtonClick();
        }

        public void ClickTapToCloseButton()
        {
            SetCursorPos(settings.TapToClosePoint_X, settings.TapToClosePoint_Y);
            InputSimulator.Mouse.Sleep(25);
            InputSimulator.Mouse.LeftButtonClick();
        }

        public void ResetKeys()
        {
            InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_A);
            APressed = false;
            left.Dispatcher.Invoke(() =>
            {
                left.Fill = System.Windows.Media.Brushes.Transparent;
            });

            InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_D);
            DPressed = false;
            right.Dispatcher.Invoke(() =>
            {
                right.Fill = System.Windows.Media.Brushes.Transparent;
            });
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        private static extern int SetCursorPos(int x, int y);

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
