namespace Varwin.Data.ServerData
{
    public class ServerConfig : IJsonSerializable
    {
        public string appVersion { get; set; }
        public string remoteAddr { get; set; }
        public string remoteAddrPort { get; set; }
        public bool defaultUserAuthorizationAllowed { get; set; }
        public AppLicenseLicense appLicenseInfo { get; set; }
    }
}