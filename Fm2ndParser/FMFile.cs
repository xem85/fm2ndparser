using System.Collections.Generic;

namespace Fm2ndParser
{
    public class FMFile
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<byte[]> MovementEntries { get; set; }
        public ICollection<byte[]> ChoiceEntries { get; set; }
        public ICollection<ImageResource> Images { get; set; }
        public ICollection<byte[]> GlobalPalettes { get; set; }
        public ICollection<SoundResource> Sounds { get; set; }
        public IList<Command> Commands { get; set; }
        public PlayerSettings Settings { get; set; }
        public StoryMode StoryMode { get; internal set; }
        internal ICollection<CpuCommand> Cpu { get; set; }
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
        public ushort Unknown { get; set; }
        public byte[] Data { get; set; }
    }
}
