using System;

namespace Monad.FLParser
{
    public static class Enums
    {
        public enum Event
        {
            ByteChanEnabled = 0,
            Byte = ByteChanEnabled,
            ByteNoteOn = 1,
            ByteChanVol = 2,    // Obsolete
            ByteChanPan = 3,    // Obsolete
            ByteMidiChan = 4,
            ByteMidiNote = 5,
            ByteMidiPatch = 6,
            ByteMidiBank = 7,
            ByteLoopActive = 9,
            ByteShowInfo = 10,
            ByteShuffle = 11,
            ByteMainVol = 12,
            ByteStretch = 13,
            BytePitchable = 14,
            ByteZipped = 15,
            ByteDelayFlags = 16,
            BytePatLength = 17,
            ByteBlockLength = 18,
            ByteUseLoopPoints = 19,
            ByteLoopType = 20,
            ByteChanType = 21,
            ByteChanMixerTrack = 22,
            ByteSsLength = 25,
            ByteSsLoop = 26,
            ByteEffectProperties = 27,
            ByteAPDC = 29,
            BytePlayTruncatedNotes = 30,
            ByteEEAutoMode = 31,
            ByteTimeMarkerNumo = 33,
            ByteTimeMarkerDeno = 34,
            // 36
            // 37
            // 38

            Word = 64,
            WordNewChan = Word,
            WordNewPat = Word + 1,
            WordTempo = Word + 2,   // Obsolete
            WordCurrentPatNum = Word + 3,
            WordPatData = Word + 4,
            WordEq = Word + 5,
            WordFadeStereo = Word + 6,
            WordCutOff = Word + 7,
            WordDotVol = Word + 8,
            WordDotPan = Word + 9,
            WordPreAmp = Word + 10,
            WordDecay = Word + 11,
            WordAttack = Word + 12,
            WordDotNote = Word + 13,
            WordDotPitch = Word + 14,
            WordDotMix = Word + 15,
            WordMainPitch = Word + 16,
            WordRandChan = Word + 17,
            WordMixChan = Word + 18,
            WordResonance = Word + 19,
            WordLoopBar = Word + 20,    // Obsolete
            WordStereoDelay = Word + 21,
            WordPogo = Word + 22,
            WordDotReso = Word + 23,
            WordDotCutOff = Word + 24,
            WordShiftTime = Word + 25,
            WordLoopEndBar = Word + 26, // Obsolete
            WordDot = Word + 27,
            WordDotShift = Word + 28,
            WordFineTempo = Word +29, // Obsolete
            WordLayerParentOf = Word + 30,
            WordInsertIcon = Word + 31,
            WordDotRel = Word + 32,
            WordSwingMix = Word + 33,   
            WordCurrentSlotNum = Word + 34,
            // 99
            // 100

            Int = 128,
            DWordColor = Int,
            DWordPlayListItem = Int + 1,
            DWordEcho = Int + 2,
            DWordFxSine = Int + 3,
            DWordCutCutBy = Int + 4,
            DWordWindowHeight = Int + 5,
            DWordWindowWidth = Int + 6,
            DWordMiddleNote = Int + 7,
            DWordReserved = Int + 8,
            DWordMainResoCutOff = Int + 9,
            DWordDelayModXY = Int + 10,
            DWordGenReverb = Int + 11,
            DWordIntStretch = Int + 12,
            DWordSsNote = Int + 13,     // SimSynth patch middle note
            DWordFineTune = Int + 14,
            DWordLayerFlags = Int + 16,
            DWordChannelFilterNum = Int + 17,
            DWordChannelFilterCurrentNum = Int + 18,
            DWordMixerOutput = Int + 19,
            DWordTimeMarker = Int + 20,
            DWordInsertColor = Int + 21,
            DWordPatternColor = Int + 22,
            DWordSongLoopPos = Int + 24,
            DWordAUSampleRate = Int + 25,
            DWordMixerInput = Int + 26,
            DWordPluginIcon = Int + 27,
            DWordFineTempo = Int + 28,

            Undef = 192,
            Text = Undef,
            TextDefaultChanName = Text,    // Obsolete
            TextPatName = Text + 1,
            TextTitle = Text + 2,
            TextComment = Text + 3,
            TextSampleFileName = Text + 4,
            TextUrl = Text + 5,
            TextCommentRtf = Text + 6,
            TextVersion = Text + 7,
            TextRegName = Text + 8,     
            TextPluginDefName = Text + 9,
            TextDataPath = Text + 10,   
            TextPluginName = Text + 11,
            TextInsertName = Text + 12,
            TextTimeMarkerName = Text + 13,
            TextGenre = Text + 14,
            TextAuthor = Text + 15,
            TextMidiCtrls = Text + 16,
            TextChanFilterName = Text + 39,
            TextTrackName = Text + 47,

            DataChannelDelay = 209,
            Data = DataChannelDelay,
            DataTs404Params = Data + 1,
            DataDelayLine = Data + 2,   // Obsolete
            DataNewPlugin = Data + 3,   // 13 DWORDs, VST & obsolete DirectX
            DataPluginParams = Data + 4,
            DataChanParams = Data + 6,      // VST don't have this
            DataPlaylistSelection = Data + 8,
            DataEnvLfoParams = Data + 9,    // 17 DWORDs, reverse ADSR/LFO fields
            DataBasicChanParams = Data + 10,
            DataOldFilterParams = Data + 11,
            DataChanPolyphony = Data + 12,
            DataOldAutomationData = Data + 14,
            DataPatternNotes = Data + 15,
            DataInsertParams = Data + 16,
            DataAutomationChannels = Data + 18,
            DataChanTracking = Data + 19,   // Vol & Key Tracking use same event, first Vol event occurs
            DataChanLevelOffsets = Data + 20,
            DataPlayListItems = Data + 24,
            DataAutomationData = Data + 25,
            DataInsertRoutes = Data + 26,
            DataInsertFlags = Data + 27,
            DataSaveTimestamp = Data + 28,
            DataPlaylistTrackInfo = Data + 29
        }

        public enum PluginType
        {
            Vst = 8
        }

        public enum ChannelReverbType
        {
            A = 0,
            B = 65536
        }

        /*public enum FilterType
        {
            LowPass = 0,
            HiPass = 1,
            BandPassCsg = 2,
            BandPassCzpg = 3,
            Notch = 4,
            AllPass = 5,
            Moog = 6,
            DoubleLowPass = 7,
            LowPassRc12 = 8,
            BandPassRc12 = 9,
            HighPassRc12 = 10,
            LowPassRc24 = 11,
            BandPassRc24 = 12,
            HighPassRc24 = 13,
            FormantFilter = 14
        }*/

        public enum ArpDirection
        {
            Off = 0,
            Up = 1,
            Down = 2,
            UpDownBounce = 3,
            UpDownSticky = 4,
            Random = 5
        }

        /*public enum EnvelopeTarget
        {
            Volume = 0,
            Cut = 1,
            Resonance = 2,
            NumTargets = 3
        }*/

        public enum ChannelLfoShape: byte
        {
            Sine = 0,
            Triangle = 1,
            Pulse = 2
        }

        [Flags]
        public enum ChannelEnvLfoFlags : byte
        {
            None = 0,
            EnvTempo = 1 << 0,
            LfoTempo = 1 << 1,
            LfoGlobal = 1 << 5
        }

        public enum PluginChunkId
        {
            Midi = 1,
            Flags = 2,
            Io = 30,
            InputInfo = 31,
            OutputInfo = 32,
            PluginInfo = 50,
            VstPlugin = 51,
            Guid = 52,
            State = 53,
            Name = 54,
            Filename = 55,  // Personal info: actual location of author's plugin folder, not required actually
            VendorName = 56
        }

        public enum InsertParam
        {
            SlotEnabled = 0x00,
            SlotVolume = 0x01,
            SlotDryWet = 0x02,
            SendLevelToTrack = 0x40,
            Volume = 0xC0,
            Pan = 0xC1,
            StereoSep = 0xC2,
            LowLevel = 0xD0,
            BandLevel = 0xD1,
            HighLevel = 0xD2,
            LowFreq = 0xD8,
            BandFreq = 0xD9,
            HighFreq = 0xDA,
            LowWidth = 0xE0,
            BandWidth = 0xE1,
            HighWidth = 0xE2
        }

        [Flags]
        public enum InsertFlags : ushort
        {
            ReversePolarity = 1,
            SwapChannels = 1 << 1,
            Unknown3 = 1 << 2,
            Unmute = 1 << 3,
            DisableThreaded = 1 << 4,
            Unknown6 = 1 << 5,
            DockedMiddle = 1 << 6,
            DockedRight = 1 << 7,
            Unknown9 = 1 << 8,
            Unknown10 = 1 << 9,
            Separator = 1 << 10,
            Lock = 1 << 11,
            Solo = 1 << 12,
            Unknown14 = 1 << 13,
            Unknown15 = 1 << 14,
            Unknown16 = 1 << 15
        }

        [Flags]
        public enum PolyphonyFlags : byte
        {
            None = 0x00,
            Mono = 0x01,
            Porta = 0x02,
            U1 = 0x04,
            U2 = 0x08,
            U3 = 0x10,
            U4 = 0x20,
            U5 = 0x40,
            U6 = 0x80
        }

        public enum ChannelType : byte
        {
            Sampler = 0,
            Plugin = 2,
            Layer = 3,
            Audio = 4,
            Automation = 5
        }

        [Flags]
        public enum SamplerFXFlags : ushort
        {
            Clip = 1 << 2
        }

        // [Flags]
        // public enum LayerFlags : byte { }
    }
}
