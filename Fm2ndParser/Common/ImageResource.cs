namespace Fm2ndParser.Common
{
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
}
