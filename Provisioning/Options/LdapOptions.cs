namespace Provisioning.Options
{
    public sealed class LdapOptions
    {
        public required string HostUrl { get; init; }
        public required int Port { get; init; } = 389;
        public required string UserDn { get; init; }
        public required string Password { get; init; }
    }
}
