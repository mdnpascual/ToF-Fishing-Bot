using System.Windows.Forms;

namespace ToF_Fishing_Bot
{
    public class KeycodeHelper
    {
        private static readonly KeysConverter kc = new();

        public static string KeycodeToString(int keyCode)
        {
            var buttonName = kc.ConvertToString(keyCode) ?? "???";
            return buttonName.ToUpper();
        }
    }
}
