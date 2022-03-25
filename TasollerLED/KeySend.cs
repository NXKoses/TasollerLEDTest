using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TasollerLED
{
    public class KeyCode
    {
        private static readonly int[] tasoller32keymap = { 0x31, 0x41, 0x51 , 0x5A, 0x32, 0x53, 0x57, 0x58 , 0x33, 0x44, 0x45, 0x43, 0x34, 0x46, 0x52, 0x56, 0x35, 0x47, 0x54, 0x42, 0x36, 0x48, 0x59, 0x4E, 0x37, 0x4A, 0x55, 0x4D, 0x38, 0x4B, 0x6F, 0xBC };
        public static int[] Tasoller32KeyMap => tasoller32keymap.Reverse().ToArray();
        public static int[] NotReverseTasoller32KeyMap => tasoller32keymap;
    }

    public class KeySend
    {
        #region Win32API Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public UIntPtr dwExtraInfo;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public UIntPtr dwExtraInfo;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT no;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        };
        #endregion

        #region Win32API Methods
        [DllImport("user32.dll")]
        private static extern void SendInput(int nInputs, ref INPUT pInputs, int cbsize);
        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        private static extern int MapVirtualKey(int wCode, int wMapType);
        #endregion

        #region Win32API Constants
        private const int INPUT_KEYBOARD = 1;
        private const int KEYEVENTF_KEYDOWN = 0x0;
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;
        #endregion

        #region Constants
        public static readonly UIntPtr MAGIC_NUMBER = (UIntPtr)0x10209;
        #endregion

        public INPUT KeyDown(int key, bool isExtend = false)
        {
            INPUT input = new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT()
                {
                    wVk = (short)key,
                    wScan = (short)MapVirtualKey((short)key, 0),
                    dwFlags = ((isExtend) ? (KEYEVENTF_EXTENDEDKEY) : 0x0) | KEYEVENTF_KEYDOWN,
                    time = 0,
                    dwExtraInfo = MAGIC_NUMBER
                },
            };

            SendInput(1, ref input, Marshal.SizeOf(input));
            return input;
        }

        public INPUT GetInstanseINPUT(int key, bool isExtend = false)
        {
            INPUT input = new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT()
                {
                    wVk = (short)key,
                    wScan = (short)MapVirtualKey((short)key, 0),
                    dwFlags = ((isExtend) ? (KEYEVENTF_EXTENDEDKEY) : 0x0) | KEYEVENTF_KEYDOWN,
                    time = 0,
                    dwExtraInfo = MAGIC_NUMBER
                },
            };

            return input;
        }

        public void KeyUp(INPUT input, bool isExtend = false)
        {
            input.ki.dwFlags = ((isExtend) ? (KEYEVENTF_EXTENDEDKEY) : 0x0) | KEYEVENTF_KEYUP;
            SendInput(1, ref input, Marshal.SizeOf(input));
        }
    }
}
