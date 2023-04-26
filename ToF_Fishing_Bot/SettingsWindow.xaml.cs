using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ToF_Fishing_Bot.Addon.DiscordInteractive;
using WindowsHook;

namespace ToF_Fishing_Bot
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private IAppSettings settings;
        private IKeyboardMouseEvents m_GlobalHook;

        private Button defaultButton = new();
        private TextBlock defaultLabel = new();
        private Button activeButton;
        private TextBlock activeLabel;

        private string savedHotkeyText = "";

        private int _updateDiscordUser = 0;
        private int _updateDiscordUrl = 0;

        public SettingsWindow(
            IAppSettings _settings,
            IKeyboardMouseEvents _m_GlobalHook)
        {
            InitializeComponent();

            settings = _settings;
            m_GlobalHook = _m_GlobalHook;
            m_GlobalHook.KeyUp += GlobalHookKeyUp;

            activeButton = defaultButton;
            activeLabel = defaultLabel;
            InitTheme(_settings.IsDarkMode == 1);
        }

        private void GlobalHookKeyUp(object sender, WindowsHook.KeyEventArgs e)
        {
            if (activeButton != defaultButton)
            {
                activeLabel.Text = KeycodeHelper.KeycodeToString(e.KeyValue);
                WriteSettings(e.KeyValue);

                ResetHotkeyButtons();
                ResetHotkeyLabels(activeLabel, false);
            }
        }

        private bool WriteSettings(int keyCode)
        {
            switch (activeButton.Name)
            {
                case "MoveLeftBtn":
                    settings.KeyCode_MoveLeft = keyCode;
                    break;
                case "MoveRightBtn":
                    settings.KeyCode_MoveRight = keyCode;
                    break;
                case "ReelInBtn":
                    settings.KeyCode_FishCapture = keyCode;
                    break;
                case "DismissBtn":
                    settings.KeyCode_DismissFishDialogue= keyCode;
                    break;
            }
            return true;
        }

        private void InitTheme(bool darkModeTheme)
        {
            SettingsWindow1.Background = darkModeTheme ? Theme.ColorAccent1 : Theme.WhiteColor;
            ButtonsGBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayGBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DiscordGBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            MoveLeftLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_MoveLeft);
            MoveRightLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_MoveRight);
            ReelInLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_FishCapture);
            DismissLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_DismissFishDialogue);
            RestartDelayTextBox.Text = settings.Delay_Restart.ToString();
            LagCompensationDelayTextBox.Text = settings.Delay_LagCompensation.ToString();
            DimissDelayTextBox.Text = settings.Delay_DismissFishCaptureDialogue.ToString();
            FishCaptureDelayTextBox.Text = settings.Delay_FishCapture.ToString();
            DiscordUserIdTextBox.Text = settings.DiscordUserId;
            DiscordWebHookTextBox.Text = settings.DiscordHookUrl;


            if (MoveLeftBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || MoveLeftBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                MoveLeftBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                MoveLeftBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                MoveLeftBtn.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
                MoveLeftDescription.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (MoveRightBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || MoveRightBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                MoveRightBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                MoveRightBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                MoveRightBtn.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
                MoveRightDescription.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (ReelInBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || ReelInBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                ReelInBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                ReelInBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                ReelInBtn.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
                ReelInDescription.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (DismissBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || DismissBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                DismissBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                DismissBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                DismissBtn.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
                DismissDescription.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }
            if (ResetBtn.Background.ToString().Equals(Theme.ButtonDefaultBGColor.ToString()) || ResetBtn.Background.ToString().Equals(Theme.ColorAccent2.ToString()))
            {
                ResetBtn.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.ButtonDefaultBGColor;
                ResetBtn.Style = darkModeTheme ? Theme.DarkStyle : Theme.LightStyle;
                ResetBtn.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
                MoveLeftDescription.Foreground = darkModeTheme ? Theme.ColorAccent5 : Theme.BlackColor;
            }

            // Row 1
            LabelRow1.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            // Row 2
            ArrowRow2.Source = RotateImage(darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage, 90);
            // Row 3
            DelayRestartLabel.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            InfoRow3Column1.Source = darkModeTheme ? Theme.DayInfoImage : Theme.NightInfoImage;
            RestartDelayTextBox.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.WhiteColor;
            RestartDelayTextBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            ArrowRow3Column1.Source = darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage;
            ArrowRow3Column2.Source = darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage;

            DelayLagCompensationLabel.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            InfoRow3Column2.Source = darkModeTheme ? Theme.DayInfoImage : Theme.NightInfoImage;
            LagCompensationDelayTextBox.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.WhiteColor;
            LagCompensationDelayTextBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            // Row 4
            ArrowRow4Column1.Source = RotateImage(darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage, 270);
            ArrowRow4Column2.Source = RotateImage(darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage, 90);
            // Row 6
            ArrowRow6Column1.Source = RotateImage(darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage, 270);
            ArrowRow6Column2.Source = RotateImage(darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage, 90);
            // Row 7
            DelayDimissLabel.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            InfoRow7Column1.Source = darkModeTheme ? Theme.DayInfoImage : Theme.NightInfoImage;
            DimissDelayTextBox.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.WhiteColor;
            DimissDelayTextBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            ArrowRow7Column1.Source = RotateImage(darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage, 180);
            ArrowRow7Column2.Source = RotateImage(darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage, 180);

            DelayFishCaptureLabel.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            InfoRow7Column2.Source = darkModeTheme ? Theme.DayInfoImage : Theme.NightInfoImage;
            FishCaptureDelayTextBox.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.WhiteColor;
            FishCaptureDelayTextBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            // Discord Interation
            DiscordUserIdLabel.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DiscordInfoRow1.Source = darkModeTheme ? Theme.DayInfoImage : Theme.NightInfoImage;
            DiscordUserIdTextBox.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.WhiteColor;
            DiscordUserIdTextBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            DiscordWebHookLabel.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DiscordInfoRow2.Source = darkModeTheme ? Theme.DayInfoImage : Theme.NightInfoImage;
            DiscordWebHookTextBox.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.WhiteColor;
            DiscordWebHookTextBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            // Tooltips
            DelayRestartTooltip.Background = darkModeTheme ? Theme.ColorAccent1 : Theme.WhiteColor;
            DelayRestartTooltipHeader.Background = darkModeTheme ? Theme.ColorAccent2 : System.Windows.Media.Brushes.Tan;
            DelayRestartTooltipHeaderValue.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayRestartTooltipDescription.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayRestartTooltipLine.Stroke = darkModeTheme ? Theme.ColorAccent5 : System.Windows.Media.Brushes.Gray;
            DelayRestartTooltipDefault.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            DelayLagCompensationTooltip.Background = darkModeTheme ? Theme.ColorAccent1 : Theme.WhiteColor;
            DelayLagCompensationTooltipHeader.Background = darkModeTheme ? Theme.ColorAccent2 : System.Windows.Media.Brushes.Tan;
            DelayLagCompensationTooltipHeaderValue.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayLagCompensationTooltipDescription.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayLagCompensationTooltipLine.Stroke = darkModeTheme ? Theme.ColorAccent5 : System.Windows.Media.Brushes.Gray;
            DelayLagCompensationTooltipDefault.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            DelayDimissTooltip.Background = darkModeTheme ? Theme.ColorAccent1 : Theme.WhiteColor;
            DelayDimissTooltipHeader.Background = darkModeTheme ? Theme.ColorAccent2 : System.Windows.Media.Brushes.Tan;
            DelayDimissTooltipHeaderValue.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayDimissTooltipDescription.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayDimissTooltipLine.Stroke = darkModeTheme ? Theme.ColorAccent5 : System.Windows.Media.Brushes.Gray;
            DelayDimissTooltipDefault.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            DelayFishCaptureTooltip.Background = darkModeTheme ? Theme.ColorAccent1 : Theme.WhiteColor;
            DelayFishCaptureTooltipHeader.Background = darkModeTheme ? Theme.ColorAccent2 : System.Windows.Media.Brushes.Tan;
            DelayFishCaptureTooltipHeaderValue.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayFishCaptureTooltipDescription.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DelayFishCaptureTooltipLine.Stroke = darkModeTheme ? Theme.ColorAccent5 : System.Windows.Media.Brushes.Gray;
            DelayFishCaptureTooltipDefault.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            DiscordUserIdTooltip.Background = darkModeTheme ? Theme.ColorAccent1 : Theme.WhiteColor;
            DiscordUserIdTooltipHeader.Background = darkModeTheme ? Theme.ColorAccent2 : System.Windows.Media.Brushes.Tan;
            DiscordUserIdTooltipHeaderValue.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DiscordUserIdTooltipDescription.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DiscordUserIdTooltipLine.Stroke = darkModeTheme ? Theme.ColorAccent5 : System.Windows.Media.Brushes.Gray;
            DiscordUserIdTooltipDefault.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;

            DiscordWebHookTooltip.Background = darkModeTheme ? Theme.ColorAccent1 : Theme.WhiteColor;
            DiscordWebHookTooltipHeader.Background = darkModeTheme ? Theme.ColorAccent2 : System.Windows.Media.Brushes.Tan;
            DiscordWebHookTooltipHeaderValue.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DiscordWebHookTooltipDescription.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            DiscordWebHookTooltipLine.Stroke = darkModeTheme ? Theme.ColorAccent5 : System.Windows.Media.Brushes.Gray;
            DiscordWebHookTooltipDefault.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
        }
        private void HandleHotkeyButtonClick(
            Button btn,
            TextBlock label)
        {
            activeButton = btn;
            activeLabel = label;
            DisableHotkeyButtons(activeButton);

            savedHotkeyText = label.Text;
            label.FontSize = 12;
            label.Text = "Press a key to set as hotkey";
        }

        private void MoveLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == defaultButton)
            {
                HandleHotkeyButtonClick(MoveLeftBtn, MoveLeftLabel);
            } else
            {
                ResetHotkeyButtons();
                ResetHotkeyLabels(MoveLeftLabel);
            }
        }

        private void MoveRightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == defaultButton)
            {
                HandleHotkeyButtonClick(MoveRightBtn, MoveRightLabel);
            }
            else
            {
                ResetHotkeyButtons();
                ResetHotkeyLabels(MoveRightLabel);
            }
        }

        private void ReelInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == defaultButton)
            {
                HandleHotkeyButtonClick(ReelInBtn, ReelInLabel);
            }
            else
            {
                ResetHotkeyButtons();
                ResetHotkeyLabels(ReelInLabel);
            }
        }

        private void DismissBtn_Click(object sender, RoutedEventArgs e)
        {
            if (activeButton == defaultButton)
            {
                HandleHotkeyButtonClick(DismissBtn, DismissLabel);
            }
            else
            {
                ResetHotkeyButtons();
                ResetHotkeyLabels(DismissLabel);
            }
        }

        private void DisableHotkeyButtons(Button clickedButton)
        {
            MoveLeftBtn.IsEnabled = MoveLeftBtn.Equals(clickedButton);
            MoveRightBtn.IsEnabled = MoveRightBtn.Equals(clickedButton);
            ReelInBtn.IsEnabled = ReelInBtn.Equals(clickedButton);
            DismissBtn.IsEnabled = DismissBtn.Equals(clickedButton);
        }

        private void ResetHotkeyButtons()
        {
            MoveLeftBtn.IsEnabled = true;
            MoveRightBtn.IsEnabled = true;
            ReelInBtn.IsEnabled = true;
            DismissBtn.IsEnabled = true;
            activeButton = defaultButton;
        }

        private void ResetHotkeyLabels(
            TextBlock label,
            bool resetText = true)
        {
            label.FontSize = 24;
            if(resetText)
            {
                label.Text = savedHotkeyText;
            }
            savedHotkeyText = "";
            activeLabel = defaultLabel;
        }

        private void SettingsWindow1_Closed(object sender, EventArgs e)
        {
            m_GlobalHook.KeyUp -= GlobalHookKeyUp;
            if(string.IsNullOrEmpty(settings.DiscordHookUrl) is false &&
                string.IsNullOrEmpty(settings.DiscordUserId) is false &&
                (_updateDiscordUrl > 2 && _updateDiscordUser > 2))
            {
                try
                {
                    var discordService = new DiscordService(settings.DiscordHookUrl, settings.DiscordUserId);
                    var notificationMsgTask = discordService.BuildGenericNotification("If you receive this message, then the Discord Integration settings is succesfuly setup");
                    notificationMsgTask.Wait();
                    var notificationMsg = notificationMsgTask.Result;
                    var sendMsgTask = discordService.SendMessage(notificationMsg);
                    sendMsgTask.Wait();
                }
                catch (ArgumentException error)
                {
                    MessageBox.Show(error.Message, "Discord Hook URL invalid", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private TransformedBitmap RotateImage(ImageSource imageSource, int rotation)
        {
            TransformedBitmap rotatedBitmap = new TransformedBitmap();
            rotatedBitmap.BeginInit();
            rotatedBitmap.Source = (BitmapImage)imageSource;
            rotatedBitmap.Transform = new RotateTransform(rotation);
            rotatedBitmap.EndInit();

            return rotatedBitmap;
        }

        private void PositiveNumbersOnlyValidation(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int i) && i >= 0);
        }

        private void DelayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveSettings(((TextBox)sender).Name, ((TextBox)sender).Text);
        }

        private void SaveSettings(string textBoxName, string value)
        {
            switch (textBoxName)
            {
                case "RestartDelayTextBox":
                    settings.Delay_Restart = int.Parse(value); break;
                case "LagCompensationDelayTextBox":
                    settings.Delay_LagCompensation = int.Parse(value); break;
                case "DimissDelayTextBox":
                    settings.Delay_DismissFishCaptureDialogue = int.Parse(value); break;
                case "FishCaptureDelayTextBox":
                    settings.Delay_FishCapture = int.Parse(value); break;
                case "DiscordUserIdTextBox":
                    if (_updateDiscordUser > 1)
                    {
                        settings.DiscordUserId = value;
                    }
                    _updateDiscordUser++;
                    break;
                case "DiscordWebHookTextBox":
                    if (_updateDiscordUrl > 1)
                    {
                        settings.DiscordHookUrl = value;
                    }
                    _updateDiscordUrl++;
                    break;
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            settings.KeyCode_MoveLeft = 65;
            settings.KeyCode_MoveRight = 68;
            settings.KeyCode_FishCapture = 49;
            settings.KeyCode_DismissFishDialogue = 27;

            settings.Delay_LagCompensation = 5000;
            settings.Delay_FishCapture = 3000;
            settings.Delay_DismissFishCaptureDialogue = 4000;
            settings.Delay_Restart = 2000;

            settings.DiscordHookUrl = "";
            settings.DiscordUserId = "";

            InitTheme(settings.IsDarkMode == 1);
        }
    }
}
