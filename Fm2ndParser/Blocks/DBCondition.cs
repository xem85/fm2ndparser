namespace Fm2ndParser
{
    public enum DBCondition : byte
    {
        None = 0,
        Guarding = 1,
        Standing = 2,
        Crouching = 3,
        ForwardIsTapped = 4,
        BackwardIsTapped = 5,
        UpIsTapped = 6,
        DownIsTapped = 7,
    }
}