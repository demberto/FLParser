// demberto
namespace Monad.FLParser
{
    public class ChannelFilter
    {
        public int Num { get; set; } = -1;
        public string Name { get; set; } = string.Empty;

        public ChannelFilter()
        {
            ++Num;
        }
    }
}
