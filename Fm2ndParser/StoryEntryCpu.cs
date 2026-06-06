namespace Fm2ndParser
{
    internal class StoryEntryCpu
    {
        public bool ShowLife { get; set; }
        public bool CpuIgnoresPlayer { get; set; }
        public StoryEntryCpuMethod Method { get; set; }
        public StoryEntryCpuEffect Effect { get; set; }
        public StoryEntryWinPause WinPause { get; set; }
        public SkillReference Character { get; set; }
        public byte CpuLevel { get; set; }
        public bool PlayerIsEnemy { get; set; }
        public bool Cpu1IsEnemy { get; set; }
        public bool Cpu2IsEnemy { get; set; }
        public bool Cpu3IsEnemy { get; set; }
        public bool Cpu4IsEnemy { get; set; }
        public bool Cpu5IsEnemy { get; set; }
        public bool Cpu6IsEnemy { get; set; }
        public bool Cpu7IsEnemy { get; set; }
        public ushort StartPosition { get; set; }
        public byte MethodTimeSec { get; set; }
        public byte MethodTimeNumber { get; set; }
        public StoryPlayerToCheck MethodLifeToCheck { get; set; }
        public byte MethodLifeToCheckValue { get; set; }
        public byte VictoryPoints { get; set; }
        public sbyte LifeEffectValue { get; set; }
        public sbyte SpecialEffectValue { get; set; }
        public StoryCpuWinsPoints VictoryPointsAssignee { get; set; }
        public StoryPlayerToCheck WhenTime { get; set; }
        public byte WhenTimeValue { get; set; }
    }
}