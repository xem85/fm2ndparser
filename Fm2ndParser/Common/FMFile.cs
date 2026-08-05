using System.Collections.Generic;

namespace Fm2ndParser.Common
{
    public class FMFile
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<ImageResource> Images { get; set; }
        public ICollection<Palette> GlobalPalettes { get; set; }
        public ICollection<SoundResource> Sounds { get; set; }
        public SkillReference BGM { get; set; }
        public uint Time { get; set; }
        public bool SkipWithInput { get; set; }
    }

    public class ImageResource
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public PaletteType PaletteType { get; set; }
        public uint PackedSize { get; set; }
        public uint Offset { get; set; }
        public byte[] PackedData { get; set; }
        public byte[] Data { get; set; }
        public byte[] Pointer { get; set; }
    }

    public enum PaletteType : uint
    {
        Global = 0,
        Private = 1,
    }

    public class SoundResource
    {
        public string Name { get; set; }
        public uint Size { get; set; }
        public byte[] Data { get; set; }
        public bool EndlessLoop { get; set; }
        public byte CDDATrack { get; set; }
        public SoundType Type { get; set; }
        public byte[] Pointer { get; set; }
    }
}
