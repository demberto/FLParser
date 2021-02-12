namespace Monad.FLParser
{
    public interface IPlaylistItem
    {
        int Position { get; set; }
        int Length { get; set; }
        uint StartOffset { get; set; }
        uint EndOffset { get; set; }
        ushort Group { get; set; }  // 0 for individual tracks or same value for tracks grouped together
        bool Muted { get; set; }
    }
}
