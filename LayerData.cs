using System.Collections.Generic;

namespace Monad.FLParser
{
    // Why does a layer have unneeded properties like polyphony, tracking?
    class LayerData : IChannelData
    {
        public HashSet<uint> ParentOf { get; set; } = new HashSet<uint>();

        // TODO: Layering
        public uint Flags { get; set; } = 0;

        // Top-right corner
        public uint Panning { get; set; } = 6400;
        public uint Volume { get; set; } = 10000;
        public uint PitchShiftInCents { get; set; } = 0;
        /* unknown
         * public uint U1;
         * public uint U2;
         * public uint U3;
         */

        // Levels adjustment
        public uint PanOffset { get; set; } = 0;
        public uint OffsetVolMultiplier { get; set; } = 12800;
        // public uint U1 { get; set; } = 0; // unknown
        public uint OffsetModX { get; set; } = 0;
        public uint OffsetModY { get; set; } = 0;
    }
}
