using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MaceTweaks.Services
{
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")] private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")] private static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam, lParam;
            public uint time;
            public System.Drawing.Point pt;
        }

        private const uint WM_HOTKEY = 0x0312;
        private const uint WM_QUIT = 0x0012;
        private const uint MOD_NOREPEAT = 0x4000;

        private readonly Models.AppConfig _cfg;
        private readonly Dictionary<string, Action> _callbacks;
        private readonly Dictionary<int, string> _hotkeyIds = new();
        private int _idCounter = 100;
        private volatile bool _running;
        private Thread? _msgThread;
        private Thread? _mouseThread;
        private uint _msgThreadId;

        public HotkeyManager(Models.AppConfig cfg, Dictionary<string, Action> callbacks)
        {
            _cfg = cfg;
            _callbacks = callbacks;
        }

        public void Start()
        {
            _running = true;
            _msgThread = new Thread(MessageLoop) { IsBackground = true };
            _msgThread.Start();
            _mouseThread = new Thread(MousePoll) { IsBackground = true };
            _mouseThread.Start();
        }

        public void Stop()
        {
            _running = false;
            try { PostThreadMessage(_msgThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero); } catch { }
        }

        public void Reload()
        {
            // Unregister all and re-register
            UnregisterAll();
            RegisterAll();
        }

        private void RegisterAll()
        {
            RegisterKb("breach", _cfg.BreachHotkey, _cfg.BreachActive);
            RegisterKb("attr", _cfg.AttrHotkey, _cfg.AttrActive);
        }

        private void RegisterKb(string name, Models.KeyboardHotkey hk, bool active)
        {
            if (!active || hk.Vk == 0) return;
            uint mods = (uint)hk.Mods | MOD_NOREPEAT;
            int hid = _idCounter++;
            if (RegisterHotKey(IntPtr.Zero, hid, mods, (uint)hk.Vk))
                _hotkeyIds[hid] = name;
        }

        private void UnregisterAll()
        {
            foreach (var hid in _hotkeyIds.Keys)
                UnregisterHotKey(IntPtr.Zero, hid);
            _hotkeyIds.Clear();
        }

        private void MessageLoop()
        {
            _msgThreadId = GetCurrentThreadId();
            RegisterAll();

            while (_running)
            {
                if (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
                {
                    if (msg.message == WM_HOTKEY)
                    {
                        int id = msg.wParam.ToInt32();
                        if (_hotkeyIds.TryGetValue(id, out var name) && _callbacks.TryGetValue(name, out var cb))
                            ThreadPool.QueueUserWorkItem(_ => cb());
                    }
                }
                else break;
            }
            UnregisterAll();
        }

        private void MousePoll()
        {
            bool prev = false;
            while (_running)
            {
                if (_cfg.MaceActive)
                {
                    int mvk = _cfg.MaceHotkey.MouseVk;
                    bool pressed = (GetAsyncKeyState(mvk) & 0x8000) != 0;
                    if (pressed && !prev)
                    {
                        if (_callbacks.TryGetValue("mace", out var cb))
                            ThreadPool.QueueUserWorkItem(_ => cb());
                    }
                    prev = pressed;
                }
                Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
