using System.Collections.Generic;

namespace Fm2ndParser
{
    public class Player
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<byte[]> MovementEntries { get; set; }
        public ICollection<byte[]> ChoiceEntries { get; set; }
        public ICollection<PlayerImageResource> Images { get; set; }
        public ICollection<byte[]> GlobalPalettes { get; set; }
        public ICollection<PlayerSoundResource> Sounds { get; set; }
    }

    public class PlayerImageResource
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint PaletteType { get; set; }
        public uint PackedSize { get; set; }
        public uint Offset { get; set; }
        public byte[] Data { get; set; }
    }

    public class PlayerSoundResource
    {
        public string Name { get; set; }
        public uint Size { get; set; }
        public ushort Unknown { get; set; }
        public byte[] Data { get; set; }
    }
}
