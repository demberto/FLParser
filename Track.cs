using System.Collections.Generic;
using System.Drawing;

namespace Monad.FLParser
{
    public class Track
    {
        public string Name { get; set; }
        
        // 22b edition
        public ushort Id { get; set; } = 0;
        public Color Color { get; set; } = ColorTranslator.FromHtml("#565148");
        public int Icon { get; set; } = 0;
        public byte Enabled { get; set; } = 1;
        public float Height { get; set; } = 1;
        public float LockedHeight { get; set; } = 1;
        public byte LockedToContent { get; set; } = 0;
        
        // 47b edition - Performance settings
        public int Motion { get; set; } = 0;
        public int Press { get; set; } = 0;
        public int TriggerSync { get; set; } = 5;
        public int Queued { get; set; } = 0;
        public int Tolerant { get; set; } = 1;
        public int PositionSync { get; set; } = 0;

        public byte GroupedWithAbove { get; set; } = 0;

        // 49b edition
        /* unknown
         * public byte U1 { get; set; } = 0;
         * public byte U2 { get; set; } = 0;
         */

        // 62b edition
        /* unknown
         * public int U3 { get; set; } = -1;
         * public int U4 { get; set; } = -1;
         * public byte U5 { get; set; } = 1;
         */

        // 66b edition also
        public List<IPlaylistItem> Items { get; set; } = new List<IPlaylistItem>();
    }
}
