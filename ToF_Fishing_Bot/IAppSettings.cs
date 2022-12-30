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
        int FishCaptureButtonPoint_X { get; set; }
        [DefaultValue(0)]
        int FishCaptureButtonPoint_Y { get; set; }
        [DefaultValue(0)]
        int TapToClosePoint_X { get; set; }
        [DefaultValue(0)]
        int TapToClosePoint_Y { get; set; }
    }
}
