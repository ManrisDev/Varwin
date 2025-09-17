using System;

public static class CoreErrorManager
{
    public static event Action<Exception> OnError;
    public static void Error(Exception e)
    {
        OnError?.Invoke(e);
    }
}
