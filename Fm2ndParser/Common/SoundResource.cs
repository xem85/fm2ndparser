namespace Fm2ndParser.Common
{
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
