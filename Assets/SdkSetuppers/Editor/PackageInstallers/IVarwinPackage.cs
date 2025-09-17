#if UNITY_EDITOR
public interface IVarwinPackage
{
    public string PackageIdentified { get; }

    bool IsPackageInstalled(string manifest, string packagesLock);
}
#endif