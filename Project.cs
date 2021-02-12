using System;
using System.Collections.Generic;
using System.IO;

namespace Monad.FLParser
{
    public class Project
    {
        public static byte MaxInsertCount = 127;    // 104 before FL 12
        public static ushort MaxTrackCount = 502;   // 199 before FL 20

        public int MainVolume { get; set; } = 300;
        public int MainPitch { get; set; } = 0;
        public int Ppq { get; set; } = 0;
        public float Tempo { get; set; } = 140;
        public string ProjectTitle { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public string VersionString { get; set; } = string.Empty;
        public int Version { get; set; } = 0x100;
        public DateTime StartDate { get; set; } = new DateTime();
        public TimeSpan WorkTime { get; set; } = new TimeSpan();
        public List<Channel> Channels { get; set; } = new List<Channel>();
        public Track[] Tracks { get; set; } = new Track[0];
        public List<Pattern> Patterns = new List<Pattern>();
        public Insert[] Inserts { get; set; } = new Insert[0];
        public List<ChannelFilter> ChannelFilters { get; set; } = new List<ChannelFilter>();
        public List<TimeMarker> TimeMarkers { get; set; } = new List<TimeMarker>();
        public bool PlayTruncatedNotes { get; set; } = false;
        public bool APDC { get; set; } = true;
        public bool EEAutoMode { get; set; } = true;
        public string DataPath { get; set; } = string.Empty;
        public bool ShowInfoOnStartup { get; set; } = false;
        public ulong NumEvents { get; set; } = 0;
        public uint SongLoopPos { get; set; } = 0;

        public uint ppqOffset;
        public uint authorOffset, authorLength;
        public uint tempoOffset;
        public uint genreOffset, genreLength;
        public uint titleOffset, titleLength;
        public uint urlOffset, urlLength;
        public uint commentOffset, commentLength;
        public uint versionOffset, versionLength;
        public uint startDateOffset, workTimeOffset;
        public uint dataPathOffset, dataPathLength;
        public uint infoOnStartOffset;

        public static Project Load(string path, bool verbose)
        {
            using (var stream = File.OpenRead(path))
            {
                return Load(stream, verbose);
            }
        }

        public static Project Load(Stream stream, bool verbose)
        {
            using (var reader = new BinaryReader(stream))
            {
                return Load(reader, verbose);
            }
        }

        public static Project Load(BinaryReader reader, bool verbose)
        {
            var parser = new ProjectParser(verbose);
            return parser.Parse(reader);
        }
    }
}
