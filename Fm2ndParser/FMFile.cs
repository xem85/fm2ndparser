using System.Collections.Generic;

namespace Fm2ndParser
{
    public class FMFile
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<ImageResource> Images { get; set; }
        public ICollection<byte[]> GlobalPalettes { get; set; }
        public ICollection<SoundResource> Sounds { get; set; }
        public SkillReference BGM { get; internal set; }
    }

    public class ImageResource
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint PaletteType { get; set; }
        public uint PackedSize { get; set; }
        public uint Offset { get; set; }
        public byte[] Data { get; set; }
    }

    public class SoundResource
    {
        public string Name { get; set; }
        public uint Size { get; set; }
        public byte[] Data { get; set; }
        public bool EndlessLoop { get; set; }
        public byte CDDATrack { get; set; }
        public SoundType Type { get; set; }
    }
}
