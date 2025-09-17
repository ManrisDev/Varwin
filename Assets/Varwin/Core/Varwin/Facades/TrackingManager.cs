using System;
using Varwin.PlatformAdapter;

public static class TrackingManager
{
    public static event Action HmdOnHead;
    public static event Action HmdOffHead;

    public static bool ChangeHmdStateIsLocked;

    public static void OnHmdOnHead()
    {
        if (!ChangeHmdStateIsLocked)
        {
            HmdOnHead?.Invoke();
        }
    }

    public static void OnHmdOffHead()
    {
        if (!ChangeHmdStateIsLocked)
        {
            HmdOffHead?.Invoke();
        }
    }

    public static bool IsHmdOnHead() => InputAdapter.Instance != null && InputAdapter.Instance.PlayerController.Tracking.IsHeadsetOnHead();
}
