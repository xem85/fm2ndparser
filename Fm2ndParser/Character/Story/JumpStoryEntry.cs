namespace Fm2ndParser.Character.Story
{
    class JumpStoryEntry : StoryEntry
    {
        public StoryEntryJump If { get; set; }
        public byte Value { get; set; }
        public sbyte GoToEvent { get; set; }
    }
}