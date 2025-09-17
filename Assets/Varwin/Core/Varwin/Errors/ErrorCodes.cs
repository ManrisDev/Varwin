namespace Varwin.Log
{
    public static class ErrorCode
    {
        public const int ServerNoConnectionError = 101;
        public const int LicenseKeyError = 102;
        public const int RabbitNoArgsError = 103;
        public const int SaveSceneError = 104;
        public const int RabbitCannotReadArgsError = 105;
        public const int ApiAndClientVersionMismatchError = 106;
        
        public const int PhotonServerDisconnectError = 201;
        
        public const int LogicExecuteError = 301;
        public const int LogicInitError = 302;

        public const int CompileCodeError = 401;
        public const int RuntimeCodeError = 402;
        
        public const int ReadLaunchArgsError = 501;
        public const int LoadObjectError = 502;
        public const int LoadSceneError = 503;
        public const int SpawnPointNotFoundError = 504;
        public const int LoadWorldConfigError = 505;
        public const int EnvironmentNotFoundError = 506;
        public const int ProjectConfigNullError = 507;

        public const int ExceptionInObject = 601;
        public const int CannotPreview = 602;
        public const int NotForCommercialUse = 604;
        public const int MobileVRIsNotAvailable = 605;
        
        public const int UnknownError = 900;
    }
}