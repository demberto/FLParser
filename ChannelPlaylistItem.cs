namespace Monad.FLParser
{
    public class ChannelPlaylistItem : IPlaylistItem
    {
        public int Position { get; set; }
        public int Length { get; set; }
        public uint StartOffset { get; set; }
        public uint EndOffset { get; set; }
        public bool Muted { get; set; }
        public ushort Group { get; set; }
        public Channel Channel { get; set; }
    }
}
