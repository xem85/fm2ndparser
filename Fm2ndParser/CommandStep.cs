namespace Fm2ndParser
{
    public class CommandStep : BlockCommandStep
    {
        public CommandStepType Type { get; set; }
        public ushort Amount { get; set; }
    }
}