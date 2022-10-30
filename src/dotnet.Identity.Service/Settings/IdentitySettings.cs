namespace dotnet.Identity.Service.Settings
{
    public class IdentitySettings
    {
        public string AdminUserEmail { get; init; }
        public string AdminUserPassword { get; init; }
        public decimal StartingOkubo { get; init; }
        public string PathBase { get; init; }
        public string CertificateCerFilePath { get; init; }
        public string CertificateKeyFilePath { get; init; }
    }
}