using System.Collections.Generic;

namespace Monad.FLParser
{
    public class GeneratorData : IChannelData
    {
        public byte[] PluginSettings { get; set; } = null;
        public Plugin Plugin { get; set; } = new Plugin();
        public string GeneratorName { get; set; } = "";
        public uint BaseNote { get; set; } = 57;
        public int Insert { get; set; } = -1;

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

        // Group
        public ushort GroupCuts { get; set; } = 0;
        public ushort GroupCutBy { get; set; } = 0;

        /* Not found in plain audio samples (drag and dropped, not into Sampler) */
        // Arpeggiator
        public Enums.ArpDirection ArpDir { get; set; } = Enums.ArpDirection.Off;
        public int ArpRange { get; set; } = 0;
        public int ArpChord { get; set; } = 0;
        public int ArpRepeat { get; set; } = 0;
        public double ArpTime { get; set; } = 100;
        public double ArpGate { get; set; } = 100;
        public bool ArpSlide { get; set; } = false;
        
        // Polyphony
        public uint MaxPolyphony { get; set; } = 0;
        public uint PolyphonySlideTime { get; set; } = 820;
        public Enums.PolyphonyFlags PolyphonyType { get; set; } = Enums.PolyphonyFlags.None;
        
        // Time
        public ushort TimeShift { get; set; } = 0;
        public ushort TimeSwing { get; set; } = 128;
        
        // Echo delay / fat mode
        public int DelayFeedback { get; set; } = 0;
        public int DelayPan { get; set; } = 6400;
        public int DelayPitchShift { get; set; } = 0;
        public int DelayEchos { get; set; } = 4;
        public int DelayTime { get; set; } = 144;
        public int DelayModY { get; set; } = 128;
        public int DelayModX { get; set; } = 128;

        // Tracking: Vol & Key
        public byte[] TrackingMidValue { get; set; } = new byte[] { /* Vol */ 100, /* Key */ 60 };
        public short[] TrackingPan { get; set; } = new short[] { 0, 0 };
        public short[] TrackingModX { get; set; } = new short[] { 0, 0 };
        public short[] TrackingModY { get; set; } = new short[] { 0, 0 };

        // Found in plain audio as well as sampler
        public string SampleFileName { get; set; } = "";
        public uint SampleAUSampleRate { get; set; } = 8000;

        /* Sampler only */
        // Playback
        public bool SampleUseLoopPoints { get; set; } = false;

        // Precomputed effects
        public ushort SampleEQ { get; set; } = 128;
        public ushort SamplePreAmp { get; set; } = 0;
        public bool SampleReversed { get; set; } = false;
        public bool SampleReverseStereo { get; set; } = false;
        public GeneratorReverb SampleReverb { get; set; } = new GeneratorReverb();
        public ushort SampleFilterCutoff { get; set; } = 1024;
        public ushort SampleFilterResonance { get; set; } = 0;
        public ushort SampleStereoDelay { get; set; } = 2048;
        public ushort SamplePogo { get; set; } = 256;

        // Envelope / instrument settings
        public ChannelEnvelopeLfo[] Envelopes { get; set; } = new ChannelEnvelopeLfo[5];

        // unknown
        /* public ushort SampleDecay { get; set; } = 0;
         * public ushort SampleAttack { get; set; } = 0;
         * public uint SampleSine { get; set; } = 8388608;
         * public ushort SampleFlags { get; set; } = 0;
         */

        public bool IsVst()
        {
            return GeneratorName == "Fruity Wrapper";
        }
    }
}
