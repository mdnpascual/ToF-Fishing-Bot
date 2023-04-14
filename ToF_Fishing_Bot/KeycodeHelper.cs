using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ToF_Fishing_Bot
{
    public class KeycodeHelper
    {
        public static string KeycodeToString(int keyCode)
        {
            StringBuilder charPressed = new(256);
            ToUnicode((uint)keyCode, 0, new byte[256], charPressed, charPressed.Capacity, 0);

            return charPressed.ToString().ToUpper();
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(
        uint virtualKeyCode,
        uint scanCode,
        byte[] keyboardState,
        StringBuilder receivingBuffer,
        int bufferSize,
        uint flags
    );
    }

    
}
