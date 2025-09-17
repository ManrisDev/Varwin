using System;
using UnityEngine;
using UnityEngine.Serialization;
using Varwin.Public;

[DisallowMultipleComponent]
public class WorldDescriptor : MonoBehaviour
{
    [HideInInspector] public Transform PlayerSpawnPoint;
    
    public LocalizationDictionary LocalizedName;
    public LocalizationDictionary LocalizedDescription;

    [HideInInspector] [Obsolete("Use LocalizedName")]
    public string Name;

    [HideInInspector] [Obsolete("Use LocalizedDescription")]
    public string Description = "Scene template description";

    public string Guid;
    public string RootGuid;

    [HideInInspector] public string Image;
    [HideInInspector] public string AssetBundleLabel;
    [HideInInspector] public string[] DllNames;
    [HideInInspector] public string[] AsmdefNames;

    [HideInInspector] public string AuthorName;
    [HideInInspector] public string AuthorEmail;
    [HideInInspector] public string AuthorUrl;

    [HideInInspector] public string LicenseCode;
    [HideInInspector] public string LicenseVersion;

    [HideInInspector] public string BuiltAt;
    [HideInInspector] public bool SourcesIncluded;
    [HideInInspector] public bool MobileReady;

    [HideInInspector] public string SceneGuid;

    [HideInInspector] public bool CurrentVersionWasBuilt;
    [HideInInspector] public bool CurrentVersionWasBuiltAsMobileReady;
    
    [HideInInspector] public string Changelog;
    [HideInInspector] public bool UseScripts;

    public bool IsFirstVersion => string.Equals(Guid, RootGuid);

    private void Reset()
    {
        Validate();
    }

    private void OnValidate()
    {
        Validate();
    }

    public void Validate()
    {
        LockTransform();

        if (string.IsNullOrWhiteSpace(Guid))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(RootGuid))
        {
            if (string.IsNullOrWhiteSpace(Guid))
            {
                Debug.LogError($"Scene Template {gameObject.name} has empty guid. New guid was generated.", gameObject);
                Guid = System.Guid.NewGuid().ToString();
            }

            RootGuid = Guid;
        }

        if (string.IsNullOrWhiteSpace(AuthorName))
        {
            AuthorName = "Anonymous";
        }

        if (string.IsNullOrWhiteSpace(LicenseCode))
        {
            LicenseCode = "cc-by";
        }

        if (string.IsNullOrWhiteSpace(LicenseVersion))
        {
            LicenseVersion = "4.0";
        }
    }

    private void OnDrawGizmos()
    {
        LockTransform();
    }

    public void RegenerateGuid()
    {
        Guid = System.Guid.NewGuid().ToString();
        RootGuid = Guid;
    }

    public void CleanBuiltInfo()
    {
        BuiltAt = null;
        CurrentVersionWasBuilt = false;
        CurrentVersionWasBuiltAsMobileReady = false;
    }

    private void LockTransform()
    {
        if (Application.isPlaying)
        {
            return;
        }

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}