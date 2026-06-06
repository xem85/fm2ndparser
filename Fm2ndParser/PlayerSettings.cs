namespace Fm2ndParser
{
    internal class PlayerSettings
    {
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public ushort SideHPYPos { get; set; }
        public ushort Interval { get; set; }
        public byte HRatio { get; set; }
        public byte StartPos { get; set; }
        public byte Correct { get; set; }
        public byte Combo { get; set; }
        public Button GuardButton { get; set; }
        public uint LifeGaugeMax { get; set; }
        public uint SpecialGaugeMax { get; set; }
        public uint SpecialMaxStock { get; set; }
        public bool NeutralGuard { get; set; }
        public bool SkyGuard { get; set; }
        public bool GuardWithButton { get; set; }
        public short PlayerAttacks { get; set; }
        public short EnemyAttacks { get; set; }
        public uint StartStock { get; set; }
    }
}