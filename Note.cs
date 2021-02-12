namespace Monad.FLParser
{
    public class Note
    {
        public uint Position { get; set; }
        public ushort Flags { get; set; } = 16384;
        public uint Length { get; set; } = 0;   // Infinite
        public ushort Key { get; set; }
        public byte FinePitch { get; set; } = 120;
        public byte Release { get; set; } = 64;
        public byte MidiChannel { get; set; } = 0;
        public byte Pan { get; set; } = 64;
        public byte Velocity { get; set; } = 100;
        public byte ModX { get; set; } = 128;
        public byte ModY { get; set; } = 128;
    }
}
