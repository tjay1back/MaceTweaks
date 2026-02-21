using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MaceTweaks.Services
{
    public static class InputSimulator
    {
        // ── Native Structs ──
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx, dy;
            public uint mouseData, dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk, wScan;
            public uint dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT_UNION
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUT_UNION union;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const uint INPUT_MOUSE = 0;
        private const uint INPUT_KEYBOARD = 1;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private static void Send(INPUT input)
        {
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }

        public static void MouseDown()
        {
            var i = new INPUT { type = INPUT_MOUSE };
            i.union.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            Send(i);
        }

        public static void MouseUp()
        {
            var i = new INPUT { type = INPUT_MOUSE };
            i.union.mi.dwFlags = MOUSEEVENTF_LEFTUP;
            Send(i);
        }

        public static void KeyDown(int vk)
        {
            var i = new INPUT { type = INPUT_KEYBOARD };
            i.union.ki.wVk = (ushort)vk;
            Send(i);
        }

        public static void KeyUp(int vk)
        {
            var i = new INPUT { type = INPUT_KEYBOARD };
            i.union.ki.wVk = (ushort)vk;
            i.union.ki.dwFlags = KEYEVENTF_KEYUP;
            Send(i);
        }

        // ── Macros ──
        public static void MacroMace(Models.AppConfig cfg)
        {
            int k1 = cfg.MaceKey1.Vk, k2 = cfg.MaceKey2.Vk;
            KeyDown(k1);   Thread.Sleep(cfg.MaceD1);
            MouseDown();   Thread.Sleep(cfg.MaceD2);
            MouseUp();     Thread.Sleep(cfg.MaceD3);
            KeyDown(k2);   Thread.Sleep(cfg.MaceD4);
            KeyUp(k2);     Thread.Sleep(cfg.MaceD5);
            MouseDown();   Thread.Sleep(cfg.MaceD6);
            MouseUp();
        }

        public static void MacroBreach(Models.AppConfig cfg)
        {
            int k1 = cfg.BreachKey1.Vk, k2 = cfg.BreachKey2.Vk;
            KeyDown(k1); Thread.Sleep(cfg.BreachD1);
            KeyDown(k2); Thread.Sleep(cfg.BreachD2);
            MouseDown(); MouseUp();
        }

        public static void MacroAttr(Models.AppConfig cfg)
        {
            int k1 = cfg.AttrKey1.Vk, k2 = cfg.AttrKey2.Vk;
            KeyDown(k1); Thread.Sleep(cfg.AttrD1);
            KeyDown(k2); Thread.Sleep(cfg.AttrD2);
            MouseDown(); MouseUp();
        }
    }
}
