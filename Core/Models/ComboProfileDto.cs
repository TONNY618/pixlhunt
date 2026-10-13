using System.Collections.Generic;
using System.Text.Json;

namespace PixelMacroEngine.Core.Models
{
    public class ComboProfileDto
    {
        public string ComboName { get; set; } = "Untitled Combo";
        public bool IsOnOff { get; set; } = true;
        public bool IsCooldownGlobal { get; set; } = true;
        public string Priority { get; set; } = "50";
        public bool PauseIfWASD { get; set; } = true;

        public List<ComboElementDto> Triggers { get; set; } = new();
        public List<ComboElementDto> Actions { get; set; } = new();
    }

    public class ComboElementDto
    {
        public string Type { get; set; } = string.Empty;
        public int Indent { get; set; } = 0;
        public JsonElement Data { get; set; }
    }
}
