﻿using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
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

            MoveLeftLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_MoveLeft);
            MoveRightLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_MoveRight);
            ReelInLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_FishCapture);
            DismissLabel.Text = KeycodeHelper.KeycodeToString(settings.KeyCode_DismissFishDialogue);
            RestartDelayTextBox.Text = settings.Delay_Restart.ToString();
            LagCompensationDelayTextBox.Text = settings.Delay_LagCompensation.ToString();
            DimissDelayTextBox.Text = settings.Delay_DismissFishCaptureDialogue.ToString();
            FishCaptureDelayTextBox.Text = settings.Delay_FishCapture.ToString();


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

            ArrowRow7Column1.Source = darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage;
            ArrowRow7Column2.Source = darkModeTheme ? Theme.DayArrowImage : Theme.NightArrowImage;

            DelayFishCaptureLabel.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
            InfoRow7Column2.Source = darkModeTheme ? Theme.DayInfoImage : Theme.NightInfoImage;
            FishCaptureDelayTextBox.Background = darkModeTheme ? Theme.ColorAccent2 : Theme.WhiteColor;
            FishCaptureDelayTextBox.Foreground = darkModeTheme ? Theme.ColorAccent4 : Theme.BlackColor;
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
    }
}
