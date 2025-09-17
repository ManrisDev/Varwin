using TMPro;
using UnityEngine;

public class VarwinVersionInfo : MonoBehaviour
{
    public static string VarwinVersion => string.Format(VarwinVersionInfoContainer.VersionString, VarwinVersionInfoContainer.VersionNumber);

    public static string VersionNumber => VarwinVersionInfoContainer.VersionNumber;

    public TMP_Text VersionTextObject;

    public static bool Exists => !string.IsNullOrEmpty(VersionNumber);

    public const string RequiredUnityVersion = "2021.3.0f1";
    
    private void Start()
    {
        if (VersionTextObject != null)
        {
            VersionTextObject.text = Exists ? VarwinVersion : string.Format(VarwinVersionInfoContainer.VersionString, Application.version);
        }
    }
}
