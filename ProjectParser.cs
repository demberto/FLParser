using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Monad.FLParser
{
    internal class ProjectParser
    {
        private readonly Project _project = new Project();
        private Pattern _curPattern;
        private Channel _curChannel;
        private Insert _curInsert;
        private InsertSlot _curSlot;
        private Track _curTrack;
        private TimeMarker _curTimeMarker;
        private readonly bool _verbose;
        private int _versionMajor;
        private int _curChanFilterNum;
        private bool _mixerMode = false;
        private bool _usesUnicode = false;  // FL 11.5+ uses UTF-16 strings, before that ASCII
        private bool _chanTrackingVolDone = false;
        private byte _curChanEnvLfo = 0;    // 5 per channel

        public ProjectParser(bool verbose)
        {
            _verbose = verbose;
        }

        public Project Parse(BinaryReader r)
        {
            ParseHeader(r);
            ParseFldt(r);

            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                ParseEvent(r);
            }

            return _project;
        }

        private void ParseHeader(BinaryReader r)
        {
            if (Encoding.ASCII.GetString(r.ReadBytes(4)) != "FLhd")
                throw new FlParseException("Invalid magic number", r.BaseStream.Position);

            // header + type
            var headerLength = r.ReadInt32();
            if (headerLength != 6)
                throw new FlParseException($"Expected header length 6, not {headerLength}", r.BaseStream.Position);

            var type = r.ReadInt16();
            if (type != 0) throw new FlParseException($"Type {type} is not supported", r.BaseStream.Position);

            // channels
            var channelCount = r.ReadInt16();
            if (channelCount < 1 || channelCount > 1000)
                throw new FlParseException($"Invalid number of channels: {channelCount}", r.BaseStream.Position);
            for (var i = 0; i < channelCount; i++)
            {
                _project.Channels.Add(new Channel { Id = i, Data = new GeneratorData() });
            }

            // ppq
            _project.ppqOffset = (uint)r.BaseStream.Position;
            _project.Ppq = r.ReadInt16();
            if (_project.Ppq < 0) throw new Exception($"Invalid PPQ: {_project.Ppq}");
        }

        private void ParseFldt(BinaryReader r)
        {
            string id;
            var len = 0;

            do
            {
                r.ReadBytes(len);

                id = Encoding.ASCII.GetString(r.ReadBytes(4));
                len = r.ReadInt32();

                // sanity check
                if (len < 0 || len > 0x10000000)
                    throw new FlParseException($"Invalid chunk length: {len}", r.BaseStream.Position);

            } while (id != "FLdt");
        }

        private void ParseEvent(BinaryReader r)
        {
            var startPos = r.BaseStream.Position;
            var eventId = (Enums.Event)r.ReadByte();
            _project.NumEvents += 1;    
            Output($"{eventId} ({(int)eventId:X2}) at {startPos:X} ");

            if (eventId < Enums.Event.Word) ParseByteEvent(eventId, r);
            else if (eventId < Enums.Event.Int) ParseWordEvent(eventId, r);
            else if (eventId < Enums.Event.Text) ParseDwordEvent(eventId, r);
            else if (eventId < Enums.Event.Data) ParseTextEvent(eventId, r);
            else ParseDataEvent(eventId, r);
        }

        private void ParseByteEvent(Enums.Event eventId, BinaryReader r)
        {
            var data = r.ReadByte();
            if (_verbose) OutputLine($"byte: {data:X2}");

            var genData = _curChannel?.Data as GeneratorData;

            switch (eventId)
            {
                
                case Enums.Event.ByteChanEnabled:
                    _curChannel.Enabled = Convert.ToBoolean(data);
                    break;
                case Enums.Event.ByteChanVol:
                    genData.Volume = data;
                    break;
                case Enums.Event.ByteChanPan:
                    genData.Panning = data;
                    break;
                case Enums.Event.ByteShowInfo:
                    _project.infoOnStartOffset = (uint)r.BaseStream.Position;
                    _project.ShowInfoOnStartup = Convert.ToBoolean(data);
                    break;
                case Enums.Event.ByteMainVol:
                    _project.MainVolume = data;
                    break;
                case Enums.Event.ByteUseLoopPoints:
                    if (genData != null) genData.SampleUseLoopPoints = true;
                    break;
                case Enums.Event.ByteChanType:
                    _curChannel.Type = (Enums.ChannelType)data;
                    break;
                case Enums.Event.ByteChanMixerTrack:
                    if (genData != null) genData.Insert = data;
                    break;
                case Enums.Event.ByteAPDC:
                    _project.APDC = Convert.ToBoolean(data);
                    break;
                case Enums.Event.BytePlayTruncatedNotes:
                    _project.PlayTruncatedNotes = Convert.ToBoolean(data);
                    break;
                case Enums.Event.ByteEEAutoMode:
                    _project.EEAutoMode = Convert.ToBoolean(data);
                    break;
                case Enums.Event.ByteTimeMarkerNumo:
                    _curTimeMarker.Numerator = data;
                    break;
                case Enums.Event.ByteTimeMarkerDeno:
                    _curTimeMarker.Denominator = data;
                    break;
            }
        }

        private void ParseWordEvent(Enums.Event eventId, BinaryReader r)
        {
            var data = r.ReadUInt16();
            OutputLine($"word: {data:X4}");

            var genData = _curChannel?.Data as GeneratorData;
            var layerData = _curChannel?.Data as LayerData;

            switch (eventId)
            {
                case Enums.Event.WordNewChan:
                    _curChannel = _project.Channels[data];
                    _chanTrackingVolDone = false;
                    _curChanEnvLfo = 0;
                    break;
                case Enums.Event.WordNewPat:
                    while (_project.Patterns.Count < data) _project.Patterns.Add(new Pattern { Id = _project.Patterns.Count });
                    _curPattern = _project.Patterns[data - 1];
                    break;
                case Enums.Event.WordTempo:
                    _project.Tempo = data;
                    break;
                case Enums.Event.WordEq:
                    if (genData != null) genData.SampleEQ = data;
                    break;
                case Enums.Event.WordFadeStereo:
                    if (genData == null) break;
                    if ((data & 0x02) != 0) genData.SampleReversed = true;
                    else if ((data & 0x100) != 0) genData.SampleReverseStereo = true;
                    break;
                case Enums.Event.WordPreAmp:
                    if (genData != null) genData.SamplePreAmp = data;
                    break;
                case Enums.Event.WordMainPitch:
                    _project.MainPitch = data;
                    break;
                case Enums.Event.WordLoopBar:
                    _project.SongLoopPos |= 0x0000FFFF;
                    break;
                case Enums.Event.WordStereoDelay:
                    if (genData != null) genData.SampleStereoDelay = data;
                    break;
                case Enums.Event.WordPogo:
                    if (genData != null) genData.SamplePogo = data;
                    break;
                case Enums.Event.WordShiftTime:
                    if (genData != null) genData.TimeShift = data;
                    break;
                case Enums.Event.WordLoopEndBar:
                    _project.SongLoopPos |= 0xFFFF0000;
                    break;
                case Enums.Event.WordFineTempo:
                    _project.Tempo += data / 1000.0F;
                    break;
                case Enums.Event.WordLayerParentOf:
                    if (layerData != null) layerData.ParentOf.Add(data);
                    break;
                case Enums.Event.WordInsertIcon:
                    _curInsert.Icon = data;
                    break;
                case Enums.Event.WordSwingMix:
                    if (genData != null) genData.TimeSwing = data;
                    break;
                // This event occurs Insert.MaxSlotCount times, regardless of whether the slot is used
                case Enums.Event.WordCurrentSlotNum:
                    if (_curSlot != null) // Current slot after plugin event, now re-arranged.
                    {
                        _curInsert.Slots[data] = _curSlot;
                    }
                    break;
            }
        }

        private void ParseDwordEvent(Enums.Event eventId, BinaryReader r)
        {
            var data = r.ReadUInt32();
            OutputLine($"int: {data:X8}");

            var genData = _curChannel?.Data as GeneratorData;
            var layerData = _curChannel?.Data as LayerData;

            // TODO: Is `_curChannel != null` required everywhere?
            switch (eventId)
            {
                case Enums.Event.DWordColor:
                    if (_mixerMode) { _curSlot.Color = Color.FromArgb((int)data); }
                    else { _curChannel.Color = Color.FromArgb((int)data); }
                    break;
                case Enums.Event.DWordCutCutBy:
                    if (_curChannel != null && genData != null)
                    {
                        var bytes = BitConverter.GetBytes(data);
                        genData.GroupCuts = BitConverter.ToUInt16(bytes, 0);
                        genData.GroupCutBy = BitConverter.ToUInt16(bytes, 2);
                    }
                    break;
                case Enums.Event.DWordMiddleNote:
                    if (_curChannel != null && genData != null) genData.BaseNote = data + 9;
                    break;               
                case Enums.Event.DWordDelayModXY:
                    if (_curChannel != null && genData != null)
                    {
                        var bytes = BitConverter.GetBytes(data);
                        genData.DelayModY = BitConverter.ToInt16(bytes, 0);
                        genData.DelayModX = BitConverter.ToInt16(bytes, 2);
                    }
                    break;               
                case Enums.Event.DWordGenReverb:
                    if (_curChannel != null && genData != null)
                    {
                        genData.SampleReverb = new GeneratorReverb
                        {
                            Type = data <= 65536 ? Enums.ChannelReverbType.A : Enums.ChannelReverbType.B,
                            Amount = (byte)(data <= 65536 ? 0 : 255)
                        };
                    }
                    break;
                case Enums.Event.DWordLayerFlags:
                    if (_curChannel != null && layerData != null) layerData.Flags = data;   // TODO: Discover these flags
                    break;
                case Enums.Event.DWordChannelFilterNum:
                    if (_curChannel != null) _curChannel.ChannelFilterNum.Add(Convert.ToInt32(data));
                    break;
                case Enums.Event.DWordChannelFilterCurrentNum:
                    _curChanFilterNum = (int)data;  // This can be negative also
                    break;
                case Enums.Event.DWordMixerOutput:
                    _curInsert.OutputChannel = (int)data;
                    break;
                // TimeMarker starts here
                case Enums.Event.DWordTimeMarker:
                    _curTimeMarker = new TimeMarker { Value = data };
                    break;
                case Enums.Event.DWordInsertColor:
                    _curInsert.Color = data;
                    break;
                case Enums.Event.DWordPatternColor:
                    _curPattern.Color = data;
                    break;
                case Enums.Event.DWordSongLoopPos:
                    _project.SongLoopPos = data;
                    break;
                case Enums.Event.DWordAUSampleRate:
                    genData.SampleAUSampleRate = data;
                    break;
                case Enums.Event.DWordMixerInput:
                    _curInsert.InputChannel = (int)data;
                    break;
                case Enums.Event.DWordPluginIcon:
                    if (_mixerMode) { _curSlot.Icon = (int)data; }
                    else { _curChannel.Icon = (int)data; }
                    break;
                case Enums.Event.DWordFineTempo:
                    _project.tempoOffset = (uint)r.BaseStream.Position;
                    _project.Tempo = data / 1000.0F;
                    break;
            }
        }

        private static int GetBufferLen(BinaryReader r)
        {
            var data = r.ReadByte();
            var dataLen = data & 0x7F;
            var shift = 0;
            while ((data & 0x80) != 0)
            {
                data = r.ReadByte();
                dataLen |= (data & 0x7F) << (shift += 7);
            }
            return dataLen;
        }

        private void ParseTextEvent(Enums.Event eventId, BinaryReader r)
        {
            var dataLen = GetBufferLen(r);
            var dataBytes = r.ReadBytes(dataLen);
            var str = _usesUnicode ? Encoding.Unicode.GetString(dataBytes) : Encoding.ASCII.GetString(dataBytes);
            if (str.EndsWith("\0")) str = str.Substring(0, str.Length - 1);

            OutputLine($"text: {str}");

            var genData = _curChannel?.Data as GeneratorData;

            switch (eventId)
            {
                case Enums.Event.TextDefaultChanName:
                    _curChannel.Name = str;
                    break;
                case Enums.Event.TextPatName:
                    _curPattern.Name = str;
                    break;
                case Enums.Event.TextTitle:
                    _project.titleOffset = (uint)r.BaseStream.Position;
                    _project.titleLength = (uint)dataLen;
                    _project.ProjectTitle = str;
                    break;
                case Enums.Event.TextComment:
                    _project.commentOffset = (uint)r.BaseStream.Position;
                    _project.commentLength = (uint)dataLen;
                    _project.Comment = str;
                    break;
                case Enums.Event.TextSampleFileName:
                    if (genData == null) break;
                    genData.SampleFileName = str;
                    genData.GeneratorName = "Sampler";  // Not really, what about plain audio?
                    break;
                case Enums.Event.TextUrl:
                    _project.urlOffset = (uint)r.BaseStream.Position;
                    _project.urlLength = (uint)dataLen;
                    _project.URL = str;
                    break;
                case Enums.Event.TextVersion:
                    _project.versionOffset = (uint)r.BaseStream.Position;
                    _project.versionLength = (uint)dataLen;
                    _project.VersionString = Encoding.ASCII.GetString(dataBytes);
                    float version;
                    if (float.TryParse(_project.VersionString.Substring(0, 4), out version))
                        if (version >= 11.5f) _usesUnicode = true;
                    if (_project.VersionString.EndsWith("\0")) _project.VersionString = _project.VersionString.Substring(0, _project.VersionString.Length - 1);
                    var numbers = _project.VersionString.Split('.');
                    _versionMajor = int.Parse(numbers[0]);
                    _project.Version = (int.Parse(numbers[0]) << 8) +
                                       (int.Parse(numbers[1]) << 4) +
                                       (int.Parse(numbers[2]) << 0);
                    Project.MaxTrackCount = (ushort)(version >= 20.0f ? 500 : 199);
                    Project.MaxInsertCount = (byte)(1 + 1 + (version >= 12f ? 125 : 104));
                    _project.Inserts = new Insert[Project.MaxInsertCount];
                    for (var i = 0; i < Project.MaxInsertCount; i++)
                    {
                        _project.Inserts[i] = new Insert { Id = i, Name = $"Insert {i}" };
                    }
                    _project.Inserts[0].Name = "Master";
                    _curInsert = _project.Inserts[0];
                    _project.Tracks = new Track[Project.MaxTrackCount];
                    for (var i = 0; i < Project.MaxTrackCount; i++)
                    {
                        _project.Tracks[i] = new Track { Name = $"Track {i}" };
                    }
                    break;
                case Enums.Event.TextPluginDefName:
                    if (_mixerMode)
                    {
                        _curSlot = new InsertSlot { DefaultName = str };  // This event marks a new slot
                    }
                    else
                    {
                        if (genData == null) break;
                        genData.GeneratorName = str;
                    }
                    break;               
                case Enums.Event.TextDataPath:
                    _project.dataPathOffset = (uint)r.BaseStream.Position;
                    _project.dataPathLength = (uint)dataLen;
                    _project.DataPath = str;
                    break;
                case Enums.Event.TextPluginName:
                    if (_mixerMode) { _curSlot.Name = str; }
                    else { _curChannel.Name = str; }
                    break;
                // Insert ends here
                case Enums.Event.TextInsertName:
                    _curInsert.Name = str;
                    break;
                // TimeMarker ends here
                case Enums.Event.TextTimeMarkerName:
                    _curTimeMarker.Name = str;
                    _project.TimeMarkers.Add(_curTimeMarker);
                    _curTimeMarker = null;
                    break;
                case Enums.Event.TextGenre:
                    _project.genreOffset = (uint)r.BaseStream.Position;
                    _project.genreLength = (uint)dataLen;
                    _project.Genre = str;
                    break;
                case Enums.Event.TextAuthor:
                    _project.authorOffset = (uint)r.BaseStream.Position;
                    _project.authorLength = (uint)dataLen;
                    _project.Author = str;
                    break;
                case Enums.Event.TextChanFilterName:
                    var channelFilter = new ChannelFilter { Name = str };
                    _project.ChannelFilters.Add(channelFilter);
                    break;
                case Enums.Event.TextTrackName:
                    if (_curTrack != null)
                    {
                        _curTrack.Name = str;
                        _project.Tracks[_curTrack.Id].Name = _curTrack.Name;
                    }
                    break;
            }
        }

        private void ParseDataEvent(Enums.Event eventId, BinaryReader r)
        {
            var dataLen = GetBufferLen(r);
            var dataStart = r.BaseStream.Position;
            var dataEnd = dataStart + dataLen;

            var genData = _curChannel?.Data as GeneratorData;
            var autData = _curChannel?.Data as AutomationData;
            var layerData = _curChannel?.Data as LayerData;
            var slotData = _curSlot;

            switch (eventId)
            {
                
                case Enums.Event.DataChannelDelay:
                    if (genData != null)
                    {
                        genData.DelayFeedback = r.ReadInt32();
                        genData.DelayPan = r.ReadInt32();
                        genData.DelayPitchShift = r.ReadInt32();
                        genData.DelayEchos = r.ReadInt32();
                        genData.DelayTime = r.ReadInt32();
                    }
                    break;
                case Enums.Event.DataPluginParams:
                    if (slotData != null)
                    {
                        OutputLine($"Found plugin settings for insert (id {_curInsert.Id})");
                        _curSlot.PluginSettings = r.ReadBytes(dataLen);
                        _curSlot.Plugin = ParsePluginChunk(slotData.PluginSettings);
                    }
                    else
                    {
                        if (genData == null) break;
                        if (genData.PluginSettings != null)
                            throw new Exception("Attempted to overwrite plugin");

                        OutputLine($"Found plugin settings for {genData.Plugin.Name} (id {_curChannel.Id})");
                        genData.PluginSettings = r.ReadBytes(dataLen);
                        genData.Plugin = ParsePluginChunk(genData.PluginSettings);
                    }
                    break;
                case Enums.Event.DataChanParams:
                    if (genData != null)
                    {
                        _ = r.ReadBytes(40);
                        genData.ArpDir = (Enums.ArpDirection)r.ReadInt32();
                        genData.ArpRange = r.ReadInt32();
                        genData.ArpChord = r.ReadInt32();
                        genData.ArpTime = r.ReadInt32() + 1;
                        genData.ArpGate = r.ReadInt32();
                        genData.ArpSlide = r.ReadBoolean();
                        _ = r.ReadBytes(31);
                        genData.ArpRepeat = r.ReadInt32();
                        while (r.BaseStream.Position < dataEnd) { r.ReadByte(); }
                    }
                    break;
                case Enums.Event.DataEnvLfoParams:
                    if (genData == null) break;
                    var envLfoFlags = r.ReadByte();
                    // random number gets added for Volume envelope/lfo flags only
                    // its the same for entire project, but there's no way to find it
                    if (_curChanEnvLfo == 1) envLfoFlags = 0;
                    _ = r.ReadBytes(3);    // always 0?
                    var envEnabled = Convert.ToBoolean(r.ReadUInt32());
                    var envDelay = r.ReadUInt32();
                    var envAttack = r.ReadUInt32();
                    var envHold = r.ReadUInt32();
                    var envDecay = r.ReadUInt32();
                    var envSustain = r.ReadInt32();
                    var envRelease = r.ReadUInt32();
                    var envAmount = r.ReadInt32();
                    var lfoDelay = r.ReadUInt32();
                    var lfoAttack = r.ReadUInt32();
                    var lfoAmount = r.ReadInt32();
                    var lfoSpeed = r.ReadUInt32();
                    var lfoShape = r.ReadByte();
                    var attackTension = r.ReadInt32();
                    var decayTension = r.ReadInt32();
                    var releaseTension = r.ReadInt32();
                    genData.Envelopes[_curChanEnvLfo] = new ChannelEnvelopeLfo
                    {
                        EnvEnabled = envEnabled,
                        EnvDelay = envDelay,
                        EnvAttack = envAttack,
                        EnvHold = envHold,
                        EnvDecay = envDecay,
                        EnvSustain = envSustain,
                        EnvRelease = envRelease,
                        EnvAmount = envAmount,
                        EnvAttackTension = attackTension,
                        EnvDecayTension = decayTension,
                        EnvReleaseTension = releaseTension,
                        LFODelay = lfoDelay,
                        LFOAttack = lfoAttack,
                        LFOAmount = lfoAmount,
                        LFOSpeed = lfoSpeed,
                        LFOShape = (Enums.ChannelLfoShape)lfoShape,
                        Flags = (Enums.ChannelEnvLfoFlags)envLfoFlags
                    };
                    _curChanEnvLfo += 1;
                    break;
                case Enums.Event.DataBasicChanParams:
                    if (genData != null)
                    {
                        genData.Panning = r.ReadUInt32();
                        genData.Volume = r.ReadUInt32();
                        genData.PitchShiftInCents = r.ReadUInt32();
                        _ = r.ReadUInt32();
                        _ = r.ReadUInt32();
                        _ = r.ReadUInt32();
                    }
                    else
                    {
                        if (layerData == null) break;
                        layerData.Panning = r.ReadUInt32();
                        layerData.Volume = r.ReadUInt32();
                        layerData.PitchShiftInCents = r.ReadUInt32();
                        _ = r.ReadUInt32();
                        _ = r.ReadUInt32();
                        _ = r.ReadUInt32();
                    }
                    break;
                case Enums.Event.DataChanPolyphony:
                    if (genData != null)
                    {
                        genData.MaxPolyphony = r.ReadUInt32();
                        genData.PolyphonySlideTime = r.ReadUInt32();
                        genData.PolyphonyType = (Enums.PolyphonyFlags)r.ReadByte();
                    }
                    break;
                case Enums.Event.DataPatternNotes:
                    while (r.BaseStream.Position < dataEnd)
                    {
                        var pos = r.ReadUInt32();
                        _ = r.ReadInt16();
                        var ch = r.ReadByte();
                        _ = r.ReadByte();
                        var length = r.ReadUInt32();
                        var key = r.ReadUInt16();
                        _ = r.ReadInt16();
                        var finePitch = r.ReadByte();
                        _ = r.ReadByte();
                        var release = r.ReadByte();
                        var midiChannel = r.ReadByte();
                        var pan = r.ReadByte();
                        var velocity = r.ReadByte();
                        var modx = r.ReadByte();
                        var mody = r.ReadByte();

                        var channel = _project.Channels[ch];
                        if (!_curPattern.Notes.ContainsKey(channel)) _curPattern.Notes.Add(channel, new List<Note>());
                        _curPattern.Notes[channel].Add(new Note
                        {
                            Position = pos,
                            Length = length,
                            Key = key,
                            FinePitch = finePitch,
                            Release = release,
                            MidiChannel = midiChannel,
                            Pan = pan,
                            Velocity = velocity,
                            ModX = modx,
                            ModY = mody
                        });
                    }
                    break;
                case Enums.Event.DataInsertParams:
                    while (r.BaseStream.Position < dataEnd)
                    {
                        var startPos = r.BaseStream.Position;
                        _ = r.ReadInt32(); // always 0?
                        var messageId = (Enums.InsertParam)r.ReadByte();
                        _ = r.ReadByte();  // 31 or 0
                        var channelData = r.ReadUInt16();
                        var messageData = r.ReadInt32();

                        var slotId = channelData & 0x3F;
                        var insertId = (channelData >> 6) & 0x7F;
                        var insertType = channelData >> 13;

                        var insert = _project.Inserts[insertId];

                        switch (messageId)
                        {
                            case Enums.InsertParam.SlotEnabled:
                                insert.Slots[slotId].Enabled = Convert.ToBoolean(messageData);
                                break;
                            case Enums.InsertParam.SlotVolume:
                                insert.Slots[slotId].Volume = messageData;
                                break;
                            case Enums.InsertParam.Volume:
                                insert.Volume = messageData;
                                break;
                            case Enums.InsertParam.Pan:
                                insert.Pan = messageData;
                                break;
                            case Enums.InsertParam.StereoSep:
                                insert.StereoSep = messageData;
                                break;
                            case Enums.InsertParam.LowLevel:
                                insert.LowLevel = messageData;
                                break;
                            case Enums.InsertParam.BandLevel:
                                insert.BandLevel = messageData;
                                break;
                            case Enums.InsertParam.HighLevel:
                                insert.HighLevel = messageData;
                                break;
                            case Enums.InsertParam.LowFreq:
                                insert.LowFreq = messageData;
                                break;
                            case Enums.InsertParam.BandFreq:
                                insert.BandFreq = messageData;
                                break;
                            case Enums.InsertParam.HighFreq:
                                insert.HighFreq = messageData;
                                break;
                            case Enums.InsertParam.LowWidth:
                                insert.LowWidth = messageData;
                                break;
                            case Enums.InsertParam.BandWidth:
                                insert.BandWidth = messageData;
                                break;
                            case Enums.InsertParam.HighWidth:
                                insert.HighWidth = messageData;
                                break;
                            default:
                                if ((int)messageId >= (int)Enums.InsertParam.SendLevelToTrack && (int)messageId <= 64 + Project.MaxInsertCount)
                                {
                                    var insertDest = (int)messageId - (int)Enums.InsertParam.SendLevelToTrack;
                                    insert.RouteVolumes[insertDest] = messageData;
                                    OutputLine($"{startPos:X4} insert send from {insertId} to {insertDest} volume: {messageData:X8}");
                                }
                                else
                                {
                                    OutputLine($"{startPos:X4} insert param: {messageId} {insertId}-{slotId}, data: {messageData:X8}");
                                }
                                break;
                        }
                    }
                    break;
                case Enums.Event.DataAutomationChannels:
                    while (r.BaseStream.Position < dataEnd)
                    {
                        _ = r.ReadUInt16();
                        var automationChannel = r.ReadByte();
                        _ = r.ReadUInt32();
                        _ = r.ReadByte();
                        var param = r.ReadUInt16();
                        var paramDestination = r.ReadInt16();
                        _ = r.ReadUInt64();

                        var channel = _project.Channels[automationChannel];

                        if ((paramDestination & 0x2000) == 0)  // Automation on channel
                        {
                            // Automation on project tempo
                            if (paramDestination > _project.Channels.Count)
                            {
                                channel.Data = new AutomationData { Parameter = param & 0x7FFF };
                            }
                            else
                            {
                                channel.Data = new AutomationData
                                {
                                    TargetChannel = _project.Channels[paramDestination],
                                    Parameter = param & 0x7fff,
                                    VstParameter = (param & 0x8000) > 0
                                };
                            }
                        }
                        else
                        {
                            channel.Data = new AutomationData // automation on insert slot
                            {
                                Parameter = param & 0x7fff,
                                InsertId = (paramDestination & 0x0FF0) >> 6,  // seems to be out by one
                                SlotId = paramDestination & 0x003F
                            };
                        }
                    }
                    break;
                case Enums.Event.DataChanTracking:
                    if (genData != null)
                    {
                        var i = _chanTrackingVolDone ? 1 : 0;
                        genData.TrackingMidValue[i] = Convert.ToByte(r.ReadUInt32());
                        genData.TrackingPan[i] = Convert.ToInt16(r.ReadInt32());
                        genData.TrackingModX[i] = Convert.ToInt16(r.ReadInt32());
                        genData.TrackingModY[i] = Convert.ToInt16(r.ReadInt32());
                    }
                    break;
                case Enums.Event.DataChanLevelOffsets:
                    if (genData != null)
                    {
                        genData.PanOffset = r.ReadUInt32();
                        genData.OffsetVolMultiplier = r.ReadUInt32();
                        _ = r.ReadUInt32();
                        genData.OffsetModX = r.ReadUInt32();
                        genData.OffsetModY = r.ReadUInt32();
                        
                    }
                    break;
                case Enums.Event.DataPlayListItems:
                    while (r.BaseStream.Position < dataEnd)
                    {
                        var startTime = r.ReadInt32();
                        var patternBase = r.ReadUInt16();   // 20480
                        var clipSource = r.ReadUInt16();    // Patterns: patternBase + Pattern.Id; Samples/Automation: 0, 1, 2, 3...
                        var length = r.ReadInt32();
                        var track = r.ReadUInt16();
                        track = (ushort)(Project.MaxTrackCount - 1 - track);
                        var group = r.ReadUInt16();
                        _ = r.ReadUInt16();
                        var itemFlags = r.ReadUInt16();
                        _ = r.ReadUInt32();
                        bool muted = (itemFlags & 0x2000) > 0;   // flag determines if item is muted

                        if (clipSource <= patternBase)
                        {
                            var startOffset = (uint)(r.ReadSingle() * _project.Ppq);
                            var endOffset = (uint)(r.ReadSingle() * _project.Ppq);

                            _project.Tracks[track].Items.Add(new ChannelPlaylistItem
                            {
                                Position = startTime,
                                Length = length,
                                StartOffset = startOffset,
                                EndOffset = endOffset,
                                Channel = _project.Channels[clipSource],
                                Muted = muted,
                                Group = group
                            });
                        }
                        else
                        {
                            var startOffset = r.ReadUInt32();
                            var endOffset = r.ReadUInt32();

                            _project.Tracks[track].Items.Add(new PatternPlaylistItem
                            {
                                Position = startTime,
                                Length = length,
                                StartOffset = startOffset,
                                EndOffset = endOffset,
                                Pattern = _project.Patterns[clipSource - patternBase - 1],
                                Muted = muted,
                                Group = group
                            });
                        }
                    }
                    break;
                case Enums.Event.DataAutomationData:
                    {
                        _ = r.ReadUInt32(); // always 1?
                        _ = r.ReadUInt32(); // always 64?
                        _ = r.ReadByte();
                        _ = r.ReadUInt16();
                        _ = r.ReadUInt16(); // always 0?
                        _ = r.ReadUInt32();
                        var keyCount = r.ReadUInt32();

                        if (autData == null) break;
                        autData.Keyframes = new AutomationKeyframe[keyCount];

                        for (var i = 0; i < keyCount; i++)
                        {
                            var startPos = r.BaseStream.Position;

                            var keyPos = r.ReadDouble();
                            var keyVal = r.ReadDouble();
                            var keyTension = r.ReadSingle();
                            _ = r.ReadUInt32(); // seems linked to tension?

                            var endPos = r.BaseStream.Position;
                            r.BaseStream.Position = startPos;
                            var byteData = r.ReadBytes((int)(endPos - startPos));
                            OutputLine($"Key {i} data: {string.Join(" ", byteData.Select(x => x.ToString("X2")))}");

                            autData.Keyframes[i] = new AutomationKeyframe
                            {
                                Position = (int)(keyPos * _project.Ppq),
                                Tension = keyTension,
                                Value = keyVal
                            };
                        }
                        // remaining data is unknown
                    }
                    break;
                case Enums.Event.DataInsertRoutes:
                    for (var i = 0; i < Project.MaxInsertCount; i++)
                    {
                        if (r.ReadBoolean())
                            _ = _curInsert.Routes.Add(Convert.ToByte(i));
                    }

                    // This code is wrong, an insert doesn't end after routing
                    /*var newIndex = _curInsert.Id + 1;
                    if (newIndex < _project.Inserts.Length)
                    {
                        _curInsert = _project.Inserts[newIndex];
                    }*/

                    break;
                // Insert begins here
                case Enums.Event.DataInsertFlags:
                    _ = r.ReadUInt32();
                    var flags = (Enums.InsertFlags)r.ReadUInt32();
                    if (dataLen >= 12) _ = r.ReadUInt32();
                    _curInsert = new Insert { Flags = flags };
                    _curSlot = new InsertSlot();    // New insert route, create new slot
                    _mixerMode = true;  // This is required as generators and effect use same events
                    _curChannel = null; // No more channels after this
                    break;
                // Project Save Time Stamp, thanks to @RoadCrewWorker
                case Enums.Event.DataSaveTimestamp:
                    _project.startDateOffset = (uint)r.BaseStream.Position;
                    var projectStartTime = r.ReadDouble();
                    _project.workTimeOffset = (uint)r.BaseStream.Position;
                    var projectWorkTime = r.ReadDouble();
                    var delphiOrigin = new DateTime(1899, 12, 30);
                    _project.StartDate = delphiOrigin + TimeSpan.FromDays(projectStartTime);
                    _project.WorkTime = TimeSpan.FromDays(projectWorkTime);
                    break;
                // Teack starts here
                case Enums.Event.DataPlaylistTrackInfo:
                    ushort trackNum;
                    // only 16b version as well
                    if (dataLen >= 16)
                    {
                        if (dataLen >= 22)
                        {
                            trackNum = Convert.ToUInt16(r.ReadUInt32());
                            _curTrack = new Track { Name = $"Track {trackNum}" };
                            _curTrack.Id = trackNum;
                            _curTrack.Color = Color.FromArgb(r.ReadInt32());
                            _curTrack.Icon = r.ReadInt32();
                            _curTrack.Enabled = r.ReadByte();
                            _curTrack.Height = r.ReadSingle();
                            _curTrack.LockedHeight = r.ReadSingle();
                            _curTrack.LockedToContent = r.ReadByte();
                            if (dataLen >= 47)
                            {
                                _curTrack.Motion = r.ReadInt32();
                                _curTrack.Press = r.ReadInt32();
                                _curTrack.TriggerSync = r.ReadInt32();
                                _curTrack.Queued = r.ReadInt32();
                                _curTrack.Tolerant = r.ReadInt32();
                                _curTrack.PositionSync = r.ReadInt32();
                                _curTrack.GroupedWithAbove = r.ReadByte();
                                // There's a 49b, 62b, & 66b version as well
                            }
                            _project.Tracks[trackNum] = _curTrack;
                        } 
                    }
                    break;
            }

            // make sure cursor is at end of data - important
            r.BaseStream.Position = dataEnd;
        }

        private Plugin ParsePluginChunk(byte[] chunk)
        {
            var plugin = new Plugin();

            using (var r = new BinaryReader(new MemoryStream(chunk)))
            {
                var pluginType = (Enums.PluginType)r.ReadInt32();

                if (pluginType != Enums.PluginType.Vst)
                {
                    return null;
                }

                while (r.BaseStream.Position < r.BaseStream.Length)
                {
                    var eventId = (Enums.PluginChunkId)r.ReadInt32();
                    var length = (int)r.ReadInt64();

                    switch (eventId)
                    {
                        case Enums.PluginChunkId.VendorName:
                            plugin.VendorName = Encoding.ASCII.GetString(r.ReadBytes(length));
                            break;
                        case Enums.PluginChunkId.Filename:
                            plugin.FileName = Encoding.ASCII.GetString(r.ReadBytes(length));
                            break;
                        case Enums.PluginChunkId.Name:
                            plugin.Name = Encoding.ASCII.GetString(r.ReadBytes(length));
                            break;
                        case Enums.PluginChunkId.State:
                            plugin.State = r.ReadBytes(length);
                            break;
                        default:
                            OutputLine($"Event {eventId}, data: {string.Join(" ", r.ReadBytes(length).Select(x => x.ToString("X2")))}");
                            break;
                    }
                }

                return plugin;
            }
        }

        private void Output(string value)
        {
            if (_verbose)
                Console.Write(value);
        }

        private void OutputLine(string value)
        {
            if (_verbose)
                Console.WriteLine(value);
        }
    }
}
