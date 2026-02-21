using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaceTweaks.Models
{
    public class KeyBinding
    {
        [JsonPropertyName("vk")]
        public int Vk { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = "";
    }

    public class MouseHotkey
    {
        [JsonPropertyName("mouse_vk")]
        public int MouseVk { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = "";
    }

    public class KeyboardHotkey
    {
        [JsonPropertyName("mods")]
        public int Mods { get; set; }

        [JsonPropertyName("vk")]
        public int Vk { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = "";
    }

    public class AppConfig
    {
        // Mace
        [JsonPropertyName("mace_active")]
        public bool MaceActive { get; set; } = true;

        [JsonPropertyName("mace_hotkey")]
        public MouseHotkey MaceHotkey { get; set; } = new() { MouseVk = 0x05, Label = "MB4" };

        [JsonPropertyName("mace_key1")]
        public KeyBinding MaceKey1 { get; set; } = new() { Vk = 0x33, Label = "3" };

        [JsonPropertyName("mace_key2")]
        public KeyBinding MaceKey2 { get; set; } = new() { Vk = 0x32, Label = "2" };

        [JsonPropertyName("mace_d1")] public int MaceD1 { get; set; } = 10;
        [JsonPropertyName("mace_d2")] public int MaceD2 { get; set; } = 10;
        [JsonPropertyName("mace_d3")] public int MaceD3 { get; set; } = 10;
        [JsonPropertyName("mace_d4")] public int MaceD4 { get; set; } = 10;
        [JsonPropertyName("mace_d5")] public int MaceD5 { get; set; } = 10;
        [JsonPropertyName("mace_d6")] public int MaceD6 { get; set; } = 15;

        // Breach
        [JsonPropertyName("breach_active")]
        public bool BreachActive { get; set; } = true;

        [JsonPropertyName("breach_hotkey")]
        public KeyboardHotkey BreachHotkey { get; set; } = new() { Mods = 0x0002, Vk = 0x12, Label = "Ctrl+Alt" };

        [JsonPropertyName("breach_key1")]
        public KeyBinding BreachKey1 { get; set; } = new() { Vk = 0x31, Label = "1" };

        [JsonPropertyName("breach_key2")]
        public KeyBinding BreachKey2 { get; set; } = new() { Vk = 0x51, Label = "Q" };

        [JsonPropertyName("breach_d1")] public int BreachD1 { get; set; } = 10;
        [JsonPropertyName("breach_d2")] public int BreachD2 { get; set; } = 1;

        // Attribute
        [JsonPropertyName("attr_active")]
        public bool AttrActive { get; set; } = true;

        [JsonPropertyName("attr_hotkey")]
        public KeyboardHotkey AttrHotkey { get; set; } = new() { Mods = 0x0002, Vk = 0xDC, Label = "Ctrl+^" };

        [JsonPropertyName("attr_key1")]
        public KeyBinding AttrKey1 { get; set; } = new() { Vk = 0x31, Label = "1" };

        [JsonPropertyName("attr_key2")]
        public KeyBinding AttrKey2 { get; set; } = new() { Vk = 0x32, Label = "2" };

        [JsonPropertyName("attr_d1")] public int AttrD1 { get; set; } = 10;
        [JsonPropertyName("attr_d2")] public int AttrD2 { get; set; } = 1;

        // ── Persistence ──
        private static readonly string AppDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaceTweaks");
        private static readonly string ConfigPath = Path.Combine(AppDir, "config.json");

        public static AppConfig Load()
        {
            try
            {
                Directory.CreateDirectory(AppDir);
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch { }
            return new AppConfig();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(AppDir);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }
}
