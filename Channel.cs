using System.Collections.Generic;
using System.Drawing;

namespace Monad.FLParser
{
    public class Channel
    {
        public int Id { get; set; } = 0;
        public Enums.ChannelType Type { get; set; } = Enums.ChannelType.Plugin;
        public string Name { get; set; } = "";  // 'Fruity Wrapper' for VST
        public int Icon { get; set; } = 0;
        public Color Color { get; set; } = ColorTranslator.FromHtml("#485156");
        public HashSet<int> ChannelFilterNum { get; set; } = new HashSet<int>();
        public IChannelData Data { get; set; } = null;
        public bool Enabled { get; set; } = true;
    }
}
