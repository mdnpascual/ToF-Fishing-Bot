using System.ComponentModel;

namespace ToF_Fishing_Bot
{
    public interface IAppSettings
    {
        [DefaultValue(222)]
        int FishStaminaColor_A { get; set; }
        [DefaultValue(222)]
        int FishStaminaColor_R { get; set; }
        [DefaultValue(222)]
        int FishStaminaColor_G { get; set; }
        [DefaultValue(222)]
        int FishStaminaColor_B { get; set; }
        [DefaultValue(0)]
        int FishStaminaPoint_X { get; set; }
        [DefaultValue(0)]
        int FishStaminaPoint_Y { get; set; }
        [DefaultValue(222)]
        int MiddleBarColor_A { get; set; }
        [DefaultValue(222)]
        int MiddleBarColor_R { get; set; }
        [DefaultValue(222)]
        int MiddleBarColor_G { get; set; }
        [DefaultValue(222)]
        int MiddleBarColor_B { get; set; }
        [DefaultValue(222)]
        int PlayerStaminaColor_A { get; set; }
        [DefaultValue(222)]
        int PlayerStaminaColor_R { get; set; }
        [DefaultValue(222)]
        int PlayerStaminaColor_G { get; set; }
        [DefaultValue(222)]
        int PlayerStaminaColor_B { get; set; }
        [DefaultValue(0)]
        int PlayerStaminaPoint_X { get; set; }
        [DefaultValue(0)]
        int PlayerStaminaPoint_Y { get; set; }
        [DefaultValue(0)]
        int UpperLeftBarPoint_X { get; set; }
        [DefaultValue(0)]
        int UpperLeftBarPoint_Y { get; set; }
        [DefaultValue(0)]
        int LowerRightBarPoint_X { get; set; }
        [DefaultValue(0)]
        int LowerRightBarPoint_Y { get; set; }
        [DefaultValue(0)]
        int IsDarkMode { get; set; }
        [DefaultValue(300)]
        int ZoomSize_X { get; set; }
        [DefaultValue(300)]
        int ZoomSize_Y { get; set; }
        [DefaultValue(4)]
        int ZoomFactor { get; set; }
        [DefaultValue("QRSL")]
        string GameProcessName { get; set; }
        [DefaultValue(40.0)]
        double StaminaColorDetectionThreshold { get; set; }
        [DefaultValue(10.0)]
        double MiddlebarColorDetectionThreshold { get; set; }
        [DefaultValue(5000)]
        int Delay_LagCompensation { get; set; }
        [DefaultValue(2000)]
        int Delay_FishCapture { get; set; }
        [DefaultValue(4000)]
        int Delay_DismissFishCaptureDialogue { get; set; }
        [DefaultValue(2000)]
        int Delay_Restart { get; set; }
        [DefaultValue(5)]
        int MinimumMiddleBarHeight { get; set; }
        [DefaultValue(49)]
        int KeyCode_FishCapture { get; set; }
        [DefaultValue(27)]
        int KeyCode_DismissFishDialogue{ get; set; }
        [DefaultValue(65)]
        int KeyCode_MoveLeft { get; set; }
        [DefaultValue(68)]
        int KeyCode_MoveRight { get; set; }
    }
}
