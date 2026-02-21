using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using MaceTweaks.Models;
using MaceTweaks.Services;

namespace MaceTweaks.Views
{
    public partial class MainWindow : Window
    {
        private readonly AppConfig _cfg;
        private readonly HotkeyManager _hkm;
        private readonly DispatcherTimer _pulseTimer;
        private bool _pulseBright = true;

        // Execution counters
        private int _maceCount, _breachCount, _attrCount;

        // Delay spinbox references
        private readonly Dictionary<string, TextBox> _maceSpins = new();
        private readonly Dictionary<string, TextBox> _breachSpins = new();
        private readonly Dictionary<string, TextBox> _attrSpins = new();

        // Tab buttons + panels
        private Button[] _tabButtons = null!;
        private UIElement[] _tabPanels = null!;

        public MainWindow()
        {
            InitializeComponent();

            // Check for updates on startup
            AutoUpdater.Start("https://raw.githubusercontent.com/tjay1back/MaceTweaks/main/update.xml");

            _cfg = AppConfig.Load();
            _hkm = new HotkeyManager(_cfg, new Dictionary<string, Action>
            {
                { "mace", FireMace },
                { "breach", FireBreach },
                { "attr", FireAttr },
            });

            LoadConfigToUI();
            BuildDelaySpins();
            LoadWatermark();
            LoadCreditsIcon();

            _tabButtons = new[] { TabMace, TabBreach, TabAttr, TabCredits };
            _tabPanels = new UIElement[] { MacePanel, BreachPanel, AttrPanel, CreditsPanel };
            SwitchTab(0);

            _pulseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
            _pulseTimer.Tick += (_, _) =>
            {
                _pulseBright = !_pulseBright;
                PulseDot.Fill = _pulseBright
                    ? (SolidColorBrush)FindResource("AccentBrush")
                    : (SolidColorBrush)FindResource("AccentDimBrush");
            };
            _pulseTimer.Start();

            _hkm.Start();
            Closed += (_, _) => _hkm.Stop();
        }

        // ── Config → UI ──
        private void LoadConfigToUI()
        {
            MaceToggle.IsChecked = _cfg.MaceActive;
            MaceHkLabel.Text = _cfg.MaceHotkey.Label;
            MaceHotkeyDisplay.Text = _cfg.MaceHotkey.Label;
            MaceK1Label.Text = _cfg.MaceKey1.Label;
            MaceK2Label.Text = _cfg.MaceKey2.Label;

            BreachToggle.IsChecked = _cfg.BreachActive;
            BreachHkLabel.Text = _cfg.BreachHotkey.Label;
            BreachHotkeyDisplay.Text = _cfg.BreachHotkey.Label;
            BreachK1Label.Text = _cfg.BreachKey1.Label;
            BreachK2Label.Text = _cfg.BreachKey2.Label;

            AttrToggle.IsChecked = _cfg.AttrActive;
            AttrHkLabel.Text = _cfg.AttrHotkey.Label;
            AttrHotkeyDisplay.Text = _cfg.AttrHotkey.Label;
            AttrK1Label.Text = _cfg.AttrKey1.Label;
            AttrK2Label.Text = _cfg.AttrKey2.Label;
        }

        // ── Build Delay Spinboxes ──
        private void BuildDelaySpins()
        {
            var maceFields = new (string key, string label, int val)[]
            {
                ("d1", "After Key 1 press", _cfg.MaceD1),
                ("d2", "LClick hold", _cfg.MaceD2),
                ("d3", "After LClick", _cfg.MaceD3),
                ("d4", "After Key 2 press", _cfg.MaceD4),
                ("d5", "After Key 2 release", _cfg.MaceD5),
                ("d6", "LClick hold (2)", _cfg.MaceD6),
            };
            foreach (var (key, label, val) in maceFields)
                AddDelayRow(MaceDelaysPanel, _maceSpins, key, label, val);

            var breachFields = new (string key, string label, int val)[]
            {
                ("d1", "Key 1 → Key 2 delay", _cfg.BreachD1),
                ("d2", "Key 2 → LMB delay", _cfg.BreachD2),
            };
            foreach (var (key, label, val) in breachFields)
                AddDelayRow(BreachDelaysPanel, _breachSpins, key, label, val);

            var attrFields = new (string key, string label, int val)[]
            {
                ("d1", "Key 1 → Key 2 delay", _cfg.AttrD1),
                ("d2", "Key 2 → LMB delay", _cfg.AttrD2),
            };
            foreach (var (key, label, val) in attrFields)
                AddDelayRow(AttrDelaysPanel, _attrSpins, key, label, val);
        }

        private void AddDelayRow(StackPanel parent, Dictionary<string, TextBox> dict, string key, string label, int val)
        {
            if (parent.Children.Count > 0)
            {
                var div = new Border
                {
                    Height = 1,
                    Background = (SolidColorBrush)FindResource("BorderBrush"),
                    Margin = new Thickness(0, 3, 0, 3)
                };
                parent.Children.Add(div);
            }

            var dock = new DockPanel();
            var lbl = new TextBlock
            {
                Text = label,
                Foreground = (SolidColorBrush)FindResource("Text2Brush"),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            dock.Children.Add(lbl);

            var sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var minus = new Button { Content = "−", Style = (Style)FindResource("SpinBtn") };
            var tb = new TextBox { Text = val.ToString(), Style = (Style)FindResource("SpinTextBox") };
            var plus = new Button { Content = "+", Style = (Style)FindResource("SpinBtn") };
            var msLbl = new TextBlock
            {
                Text = " ms",
                Foreground = (SolidColorBrush)FindResource("Text3Brush"),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11
            };

            minus.Click += (_, _) =>
            {
                if (int.TryParse(tb.Text, out int v) && v > 1) tb.Text = (v - 1).ToString();
            };
            plus.Click += (_, _) =>
            {
                if (int.TryParse(tb.Text, out int v) && v < 9999) tb.Text = (v + 1).ToString();
            };

            sp.Children.Add(minus);
            sp.Children.Add(tb);
            sp.Children.Add(plus);
            sp.Children.Add(msLbl);
            DockPanel.SetDock(sp, Dock.Right);
            dock.Children.Add(sp);
            parent.Children.Add(dock);
            dict[key] = tb;
        }

        // ── Watermark ──
        private void LoadWatermark()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/MaceTweaks.ico");
                var bmp = new BitmapImage(uri);
                Watermark.Source = bmp;
                Watermark.Width = 200;
                Watermark.Height = 200;
            }
            catch { }
        }

        private void LoadCreditsIcon()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/MaceTweaks.ico");
                var bmp = new BitmapImage(uri);
                CreditsIcon.Source = bmp;
            }
            catch { }
        }

        // ── Tab Switching ──
        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            int idx = Array.IndexOf(_tabButtons, btn);
            if (idx >= 0) SwitchTab(idx);
        }

        private void SwitchTab(int idx)
        {
            if (_tabButtons == null || _tabPanels == null) return;

            var bgBrush = (SolidColorBrush)FindResource("BgBrush");
            var surfBrush = (SolidColorBrush)FindResource("SurfaceBrush");
            var accent2 = (SolidColorBrush)FindResource("Accent2Brush");
            var text3 = (SolidColorBrush)FindResource("Text3Brush");

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                _tabButtons[i].Background = i == idx ? bgBrush : surfBrush;
                _tabButtons[i].Foreground = i == idx ? accent2 : text3;
                _tabPanels[i].Visibility = i == idx ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // ── Toggle Handlers ──
        private void MaceToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_cfg == null) return;
            bool on = MaceToggle.IsChecked == true;
            _cfg.MaceActive = on;
            MaceStatusText.Text = on ? "Active" : "Inactive";
            MaceStatusText.Foreground = on ? (SolidColorBrush)FindResource("TextBrush") : (SolidColorBrush)FindResource("Text3Brush");
            MaceDot.Fill = on ? (SolidColorBrush)FindResource("GreenBrush") : (SolidColorBrush)FindResource("RedBrush");
        }

        private void BreachToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_cfg == null) return;
            bool on = BreachToggle.IsChecked == true;
            _cfg.BreachActive = on;
            BreachStatusText.Text = on ? "Active" : "Inactive";
            BreachStatusText.Foreground = on ? (SolidColorBrush)FindResource("TextBrush") : (SolidColorBrush)FindResource("Text3Brush");
            BreachDot.Fill = on ? (SolidColorBrush)FindResource("GreenBrush") : (SolidColorBrush)FindResource("RedBrush");
        }

        private void AttrToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_cfg == null) return;
            bool on = AttrToggle.IsChecked == true;
            _cfg.AttrActive = on;
            AttrStatusText.Text = on ? "Active" : "Inactive";
            AttrStatusText.Foreground = on ? (SolidColorBrush)FindResource("TextBrush") : (SolidColorBrush)FindResource("Text3Brush");
            AttrDot.Fill = on ? (SolidColorBrush)FindResource("GreenBrush") : (SolidColorBrush)FindResource("RedBrush");
        }

        // ── Key Recording ──
        private void StartRecord(TextBlock label, Button btn, KeyRecorder.RecordMode mode, Action<int, int, string> onDone)
        {
            label.Text = "...";
            label.Foreground = (SolidColorBrush)FindResource("AmberBrush");
            btn.Content = "Listening…";
            btn.Foreground = (SolidColorBrush)FindResource("AmberBrush");

            var rec = new KeyRecorder(mode, (vk, mods, lbl) =>
            {
                Dispatcher.Invoke(() =>
                {
                    onDone(vk, mods, lbl);
                    btn.Content = mode == KeyRecorder.RecordMode.Mouse ? "● Record" : "Change";
                    btn.Foreground = (SolidColorBrush)FindResource("TextBrush");
                });
            });
            rec.Start();
        }

        // Mace hotkey (mouse)
        private void MaceHk_Record(object sender, RoutedEventArgs e) =>
            StartRecord(MaceHkLabel, MaceHkBtn, KeyRecorder.RecordMode.Mouse, (vk, _, lbl) =>
            {
                _cfg.MaceHotkey = new MouseHotkey { MouseVk = vk, Label = lbl };
                MaceHkLabel.Text = lbl;
                MaceHkLabel.Foreground = (SolidColorBrush)FindResource("Accent2Brush");
                MaceHotkeyDisplay.Text = lbl;
            });

        // Mace keys
        private void MaceK1_Record(object sender, RoutedEventArgs e) =>
            StartRecord(MaceK1Label, MaceK1Btn, KeyRecorder.RecordMode.Keyboard, (vk, _, lbl) =>
            {
                _cfg.MaceKey1 = new KeyBinding { Vk = vk, Label = VkNames.GetName(vk) };
                MaceK1Label.Text = VkNames.GetName(vk);
                MaceK1Label.Foreground = (SolidColorBrush)FindResource("TextBrush");
            });

        private void MaceK2_Record(object sender, RoutedEventArgs e) =>
            StartRecord(MaceK2Label, MaceK2Btn, KeyRecorder.RecordMode.Keyboard, (vk, _, lbl) =>
            {
                _cfg.MaceKey2 = new KeyBinding { Vk = vk, Label = VkNames.GetName(vk) };
                MaceK2Label.Text = VkNames.GetName(vk);
                MaceK2Label.Foreground = (SolidColorBrush)FindResource("TextBrush");
            });

        // Breach hotkey (keyboard)
        private void BreachHk_Record(object sender, RoutedEventArgs e) =>
            StartRecord(BreachHkLabel, BreachHkBtn, KeyRecorder.RecordMode.Keyboard, (vk, mods, lbl) =>
            {
                _cfg.BreachHotkey = new KeyboardHotkey { Mods = mods, Vk = vk, Label = lbl };
                BreachHkLabel.Text = lbl;
                BreachHkLabel.Foreground = (SolidColorBrush)FindResource("Accent2Brush");
                BreachHotkeyDisplay.Text = lbl;
            });

        private void BreachK1_Record(object sender, RoutedEventArgs e) =>
            StartRecord(BreachK1Label, (Button)sender, KeyRecorder.RecordMode.Keyboard, (vk, _, lbl) =>
            {
                _cfg.BreachKey1 = new KeyBinding { Vk = vk, Label = VkNames.GetName(vk) };
                BreachK1Label.Text = VkNames.GetName(vk);
                BreachK1Label.Foreground = (SolidColorBrush)FindResource("TextBrush");
            });

        private void BreachK2_Record(object sender, RoutedEventArgs e) =>
            StartRecord(BreachK2Label, (Button)sender, KeyRecorder.RecordMode.Keyboard, (vk, _, lbl) =>
            {
                _cfg.BreachKey2 = new KeyBinding { Vk = vk, Label = VkNames.GetName(vk) };
                BreachK2Label.Text = VkNames.GetName(vk);
                BreachK2Label.Foreground = (SolidColorBrush)FindResource("TextBrush");
            });

        // Attr hotkey
        private void AttrHk_Record(object sender, RoutedEventArgs e) =>
            StartRecord(AttrHkLabel, AttrHkBtn, KeyRecorder.RecordMode.Keyboard, (vk, mods, lbl) =>
            {
                _cfg.AttrHotkey = new KeyboardHotkey { Mods = mods, Vk = vk, Label = lbl };
                AttrHkLabel.Text = lbl;
                AttrHkLabel.Foreground = (SolidColorBrush)FindResource("Accent2Brush");
                AttrHotkeyDisplay.Text = lbl;
            });

        private void AttrK1_Record(object sender, RoutedEventArgs e) =>
            StartRecord(AttrK1Label, (Button)sender, KeyRecorder.RecordMode.Keyboard, (vk, _, lbl) =>
            {
                _cfg.AttrKey1 = new KeyBinding { Vk = vk, Label = VkNames.GetName(vk) };
                AttrK1Label.Text = VkNames.GetName(vk);
                AttrK1Label.Foreground = (SolidColorBrush)FindResource("TextBrush");
            });

        private void AttrK2_Record(object sender, RoutedEventArgs e) =>
            StartRecord(AttrK2Label, (Button)sender, KeyRecorder.RecordMode.Keyboard, (vk, _, lbl) =>
            {
                _cfg.AttrKey2 = new KeyBinding { Vk = vk, Label = VkNames.GetName(vk) };
                AttrK2Label.Text = VkNames.GetName(vk);
                AttrK2Label.Foreground = (SolidColorBrush)FindResource("TextBrush");
            });

        // ── Save ──
        private int GetSpin(Dictionary<string, TextBox> d, string k)
        {
            if (d.TryGetValue(k, out var tb) && int.TryParse(tb.Text, out int v))
                return Math.Clamp(v, 1, 9999);
            return 10;
        }

        private async void ShowSaved(Button btn)
        {
            btn.Background = (SolidColorBrush)FindResource("GreenBrush");
            btn.Content = "✓  Saved";
            await Task.Delay(2000);
            btn.Background = (SolidColorBrush)FindResource("AccentBrush");
            btn.Content = "Save Configuration";
        }

        private void MaceSave_Click(object sender, RoutedEventArgs e)
        {
            _cfg.MaceD1 = GetSpin(_maceSpins, "d1");
            _cfg.MaceD2 = GetSpin(_maceSpins, "d2");
            _cfg.MaceD3 = GetSpin(_maceSpins, "d3");
            _cfg.MaceD4 = GetSpin(_maceSpins, "d4");
            _cfg.MaceD5 = GetSpin(_maceSpins, "d5");
            _cfg.MaceD6 = GetSpin(_maceSpins, "d6");
            _cfg.Save();
            _hkm.Reload();
            ShowSaved(MaceSaveBtn);
        }

        private void BreachSave_Click(object sender, RoutedEventArgs e)
        {
            _cfg.BreachD1 = GetSpin(_breachSpins, "d1");
            _cfg.BreachD2 = GetSpin(_breachSpins, "d2");
            _cfg.Save();
            _hkm.Reload();
            ShowSaved(BreachSaveBtn);
        }

        private void AttrSave_Click(object sender, RoutedEventArgs e)
        {
            _cfg.AttrD1 = GetSpin(_attrSpins, "d1");
            _cfg.AttrD2 = GetSpin(_attrSpins, "d2");
            _cfg.Save();
            _hkm.Reload();
            ShowSaved(AttrSaveBtn);
        }

        // ── Macro Firing ──
        private void FireMace()
        {
            if (!_cfg.MaceActive) return;
            Dispatcher.Invoke(() => MaceCount.Text = (++_maceCount).ToString());
            InputSimulator.MacroMace(_cfg);
        }

        private void FireBreach()
        {
            if (!_cfg.BreachActive) return;
            Dispatcher.Invoke(() => BreachCount.Text = (++_breachCount).ToString());
            InputSimulator.MacroBreach(_cfg);
        }

        private void FireAttr()
        {
            if (!_cfg.AttrActive) return;
            Dispatcher.Invoke(() => AttrCount.Text = (++_attrCount).ToString());
            InputSimulator.MacroAttr(_cfg);
        }

        // ── Credits Link ──
        private void Link_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://guns.lol/jay_back") { UseShellExecute = true });
        }

        private void Link_Enter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LinkLabel.Foreground = (SolidColorBrush)FindResource("AccentBrush");
        }

        private void Link_Leave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LinkLabel.Foreground = (SolidColorBrush)FindResource("Accent2Brush");
        }

        // ── GitHub Link ──
        private void Github_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/tjay1back") { UseShellExecute = true });
        }

        private void Github_Enter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            GithubLabel.Foreground = (SolidColorBrush)FindResource("AccentBrush");
        }

        private void Github_Leave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            GithubLabel.Foreground = (SolidColorBrush)FindResource("Accent2Brush");
        }

        // ── Discord Link ──
        private void Discord_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://discord.gg/XPS2bJVyn9") { UseShellExecute = true });
        }

        private void Discord_Enter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DiscordLabel.Foreground = (SolidColorBrush)FindResource("AccentBrush");
        }

        private void Discord_Leave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DiscordLabel.Foreground = (SolidColorBrush)FindResource("Accent2Brush");
        }
    }
}
