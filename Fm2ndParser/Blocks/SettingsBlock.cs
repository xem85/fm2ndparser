namespace Fm2ndParser
{
    public class SettingsBlock : Block
    {
        public uint Level { get; set; }
        public SettingsType SettingsType { get; set; }
        public HitMarkPosition Position { get; set; }
        public byte NumberWidth { get; set; }
        public uint Time { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public bool ConnectLtRt { get; set; }
        public bool ConnectUpDw { get; set; }
        public bool WidthEnabled { get; set; }
        public bool HeightEnabled { get; set; }
    }
}