namespace Fm2ndParser
{
    public class CommandStep
    {
        public ComDirection Direction { get; set; }
        public bool A { get; internal set; }
        public bool B { get; internal set; }
        public bool C { get; internal set; }
        public bool D { get; internal set; }
        public bool E { get; internal set; }
        public bool F { get; internal set; }
        public bool Continue { get; internal set; }
        public bool Active { get; internal set; }
    }
}