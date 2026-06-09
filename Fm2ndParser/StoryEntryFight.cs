namespace Fm2ndParser
{
    class StoryEntryFight : StoryEntry
    {
        public SkillReference Stage { get; set; }
        public byte NumbOfRounds { get; set; }
        public StoryFirstLife FirstLife { get; set; }
        public byte LifeRecover { get; set; }
        public StoryIfDefeated IfDefeated { get; set; }
        public StoryStartingRound StartingRound { get; set; }
        public ushort Time { get; set; }
        public uint PlayerStartPos { get; set; }
        public bool ShowRoundSkill { get; set; }
        public bool ShowFightSkill { get; set; }
        public bool WL { get; set; }
        public CPU IfTimeIsOverCpu { get; set; }
        public byte IfTimeIsOverValue { get; set; }
        public StoryCpuWinsPoints CpuWinPoints { get; set; }
        public byte CpuWinPointsValue { get; set; }
    }
}