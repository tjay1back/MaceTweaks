using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MaceTweaks.Services
{
    public class KeyRecorder
    {
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);

        public enum RecordMode { Keyboard, Mouse }

        private volatile bool _running;
        private Thread? _thread;
        private readonly RecordMode _mode;
        private readonly Action<int, int, string> _onResult; // vk, mods, label

        private static readonly HashSet<int> IgnoreVks = new() { 0x10, 0x11, 0x12, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 };
        private static readonly int[] MouseVks = { 0x01, 0x02, 0x04, 0x05, 0x06 };

        public KeyRecorder(RecordMode mode, Action<int, int, string> onResult)
        {
            _mode = mode;
            _onResult = onResult;
        }

        public void Start()
        {
            _running = true;
            _thread = new Thread(Poll) { IsBackground = true };
            _thread.Start();
        }

        public void Stop() => _running = false;

        private void Poll()
        {
            Thread.Sleep(200); // wait for release
            var prev = new Dictionary<int, bool>();

            while (_running)
            {
                if (_mode == RecordMode.Mouse)
                {
                    foreach (int vk in MouseVks)
                    {
                        bool pressed = (GetAsyncKeyState(vk) & 0x8000) != 0;
                        if (pressed && !prev.GetValueOrDefault(vk))
                        {
                            _running = false;
                            int mods = GetMods();
                            _onResult(vk, mods, VkNames.GetLabel(vk, mods));
                            return;
                        }
                        prev[vk] = pressed;
                    }
                }
                else
                {
                    for (int vk = 0x08; vk <= 0xDF; vk++)
                    {
                        if (IgnoreVks.Contains(vk)) continue;
                        bool pressed = (GetAsyncKeyState(vk) & 0x8000) != 0;
                        if (pressed && !prev.GetValueOrDefault(vk))
                        {
                            _running = false;
                            int mods = GetMods();
                            _onResult(vk, mods, VkNames.GetLabel(vk, mods));
                            return;
                        }
                        prev[vk] = pressed;
                    }
                }
                Thread.Sleep(5);
            }
        }

        private static int GetMods()
        {
            int m = 0;
            if ((GetAsyncKeyState(0x11) & 0x8000) != 0) m |= 0x0002; // CTRL
            if ((GetAsyncKeyState(0x10) & 0x8000) != 0) m |= 0x0004; // SHIFT
            if ((GetAsyncKeyState(0x12) & 0x8000) != 0) m |= 0x0001; // ALT
            if ((GetAsyncKeyState(0x5B) & 0x8000) != 0 ||
                (GetAsyncKeyState(0x5C) & 0x8000) != 0) m |= 0x0008; // WIN
            return m;
        }
    }

    public static class VkNames
    {
        private static readonly Dictionary<int, string> Names = new()
        {
            {0x08,"Backspace"},{0x09,"Tab"},{0x0D,"Enter"},{0x10,"Shift"},{0x11,"Ctrl"},
            {0x12,"Alt"},{0x13,"Pause"},{0x14,"CapsLock"},{0x1B,"Esc"},{0x20,"Space"},
            {0x21,"PgUp"},{0x22,"PgDn"},{0x23,"End"},{0x24,"Home"},
            {0x25,"←"},{0x26,"↑"},{0x27,"→"},{0x28,"↓"},
            {0x2D,"Insert"},{0x2E,"Delete"},
            {0x30,"0"},{0x31,"1"},{0x32,"2"},{0x33,"3"},{0x34,"4"},
            {0x35,"5"},{0x36,"6"},{0x37,"7"},{0x38,"8"},{0x39,"9"},
            {0x41,"A"},{0x42,"B"},{0x43,"C"},{0x44,"D"},{0x45,"E"},{0x46,"F"},{0x47,"G"},
            {0x48,"H"},{0x49,"I"},{0x4A,"J"},{0x4B,"K"},{0x4C,"L"},{0x4D,"M"},{0x4E,"N"},
            {0x4F,"O"},{0x50,"P"},{0x51,"Q"},{0x52,"R"},{0x53,"S"},{0x54,"T"},{0x55,"U"},
            {0x56,"V"},{0x57,"W"},{0x58,"X"},{0x59,"Y"},{0x5A,"Z"},
            {0x70,"F1"},{0x71,"F2"},{0x72,"F3"},{0x73,"F4"},{0x74,"F5"},{0x75,"F6"},
            {0x76,"F7"},{0x77,"F8"},{0x78,"F9"},{0x79,"F10"},{0x7A,"F11"},{0x7B,"F12"},
            {0xBA,";"},{0xBB,"+"},{0xBC,","},{0xBD,"-"},{0xBE,"."},{0xBF,"/"},
            {0xC0,"`"},{0xDB,"["},{0xDC,"\\"},{0xDD,"]"},{0xDE,"'"},
            {0x05,"MB4"},{0x06,"MB5"},{0x01,"LMB"},{0x02,"RMB"},{0x04,"MMB"},
        };

        public static string GetName(int vk) => Names.GetValueOrDefault(vk, $"VK{vk:X2}");

        public static string GetLabel(int vk, int mods = 0)
        {
            var parts = new List<string>();
            if ((mods & 0x0002) != 0) parts.Add("Ctrl");
            if ((mods & 0x0004) != 0) parts.Add("Shift");
            if ((mods & 0x0001) != 0) parts.Add("Alt");
            if ((mods & 0x0008) != 0) parts.Add("Win");
            parts.Add(GetName(vk));
            return string.Join("+", parts);
        }
    }
}
