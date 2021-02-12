namespace Monad.FLParser
{
    public class ChannelEnvelopeLfo
    {
        // Envelope
        public Enums.ChannelEnvLfoFlags Flags { get; set; } = Enums.ChannelEnvLfoFlags.None;
        public bool EnvEnabled { get; set; } = false;
        public uint EnvDelay { get; set; } = 100;
        public uint EnvAttack { get; set; } = 20_000;
        public uint EnvHold { get; set; } = 20_000;
        public uint EnvDecay { get; set; } = 30_000;
        public int EnvSustain { get; set; } = 50;
        public uint EnvRelease { get; set; } = 20_000;
        public int EnvAmount { get; set; } = 0;
        public int EnvAttackTension { get; set; } = 0;
        public int EnvDecayTension { get; set; } = 0;
        public int EnvReleaseTension { get; set; } = 0;

        // LFO
        public uint LFODelay { get; set; } = 100;
        public uint LFOAttack { get; set; } = 20_000;
        public int LFOAmount { get; set; } = 0;
        public uint LFOSpeed { get; set; } = 32950;
        public Enums.ChannelLfoShape LFOShape { get; set; } = Enums.ChannelLfoShape.Sine;

    }
}
