using System.Collections.Generic;

namespace Monad.FLParser
{
    public class Insert
    {
        public const int MaxSlotCount = 10;

        public int Id { get; set; } = 0;
        public string Name { get; set; } = "Insert";
        public uint Color { get; set; } = 0x000000;
        public ushort Icon { get; set; } = 0;
        public Enums.InsertFlags Flags { get; set; } = 0;
        public int Volume { get; set; } = 12800;
        public int Pan { get; set; } = 0;
        public int StereoSep { get; set; } = 0;
        public int LowLevel { get; set; } = 0;      // 0.0 dB
        public int BandLevel { get; set; } = 0;
        public int HighLevel { get; set; } = 0;
        public int LowFreq { get; set; } = 5777;    // 90 Hz
        public int BandFreq { get; set; } = 33145;  // 1500 Hz
        public int HighFreq { get; set; } = 55825;  // 8000 Hz
        public int LowWidth { get; set; } = 17500;  // 0.27
        public int BandWidth { get; set; } = 17500;
        public int HighWidth { get; set; } = 17500;
        public int InputChannel { get; set; } = -1;
        public int OutputChannel { get; set; } = -1;
        public HashSet<byte> Routes { get; set; } = new HashSet<byte>();
        public int[] RouteVolumes { get; set; } = new int[Project.MaxInsertCount];
        public InsertSlot[] Slots { get; set; } = new InsertSlot[MaxSlotCount];

        public Insert()
        {
            for (var i = 0; i < MaxSlotCount; i++)
            {
                Slots[i] = new InsertSlot();
            }

            for (var i = 0; i < Project.MaxInsertCount; i++)
            {
                RouteVolumes[i] = 12800;
            }
        }
    }
}
