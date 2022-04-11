using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace TasollerLED
{
    public class KeyCode
    {
        private static readonly int[] tasoller32keymap = { 0x31, 0x41, 0x51, 0x5A, 0x32, 0x53, 0x57, 0x58, 0x33, 0x44, 0x45, 0x43, 0x34, 0x46, 0x52, 0x56, 0x35, 0x47, 0x54, 0x42, 0x36, 0x48, 0x59, 0x4E, 0x37, 0x4A, 0x55, 0x4D, 0x38, 0x4B, 0x6F, 0xBC };
        public static int[] Tasoller32KeyMap => tasoller32keymap.Reverse().ToArray();
        public static int[] NotReverseTasoller32KeyMap => tasoller32keymap;
    }

    public class win32api
    {
        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    }

    public class KB_EVENT
    {
        public static void KeyDown(int key)
        {
            win32api.keybd_event((byte)key, 0, 0, (UIntPtr)0);
        }

        public static void KeyUp(KEY_DATA keydata)
        {
            win32api.keybd_event((byte)keydata.key, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
        }

        public KEY_DATA Get_instance(int key)
        {
            var keydata = new KEY_DATA
            {
                key = key
            };
            return keydata;
        }
    }

    public class KEY_DATA
    {
        public int key { get; set; }
        public bool isdown { get; set; } = false;
    }
}
