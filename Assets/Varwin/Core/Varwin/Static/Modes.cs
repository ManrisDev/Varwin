namespace Varwin
{
    public enum ExecutionMode
    {
        Undefined = 0,
        RMS = 1,
        EXE = 2
    }

    public enum GameMode
    {
        Undefined = 1,
        Edit = 2,
        Preview = 3,
        View = 0
    }

    public enum PlatformMode
    {
        Vr = 1,
        Desktop = 2,
        Undefined = 3,
        NettleDesk = 4,
        Spectator = 5,
        Ar = 6
    }

    public enum MultiplayerMode
    {
        Undefined = 0,
        Single = 1,
        Host = 2,
        Client = 3
    }
}