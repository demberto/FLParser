using System.Drawing;

namespace Monad.FLParser
{
    public class InsertSlot
    {
        public int Volume { get; set; } = 100;  // Use?
        public bool Enabled { get; set; } = false;
        public int DryWet { get; set; } = -1;
        public string DefaultName { get; set; } = string.Empty; // For VSTs this is 'Fruity Wrapper'
        public string Name { get; set; } = string.Empty;        // This is like default name for VSTs, this isn't Plugin.Name
        public int Icon { get; set; } = 0;
        public Color Color { get; set; } = ColorTranslator.FromHtml("#485156");
        public byte[] PluginSettings { get; set; } = null;
        public Plugin Plugin { get; set; } = new Plugin();
    }
}
