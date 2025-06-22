namespace LDAPProvisioner.Settings;

public sealed record LDAPSettings(string Host, int Port, bool UseSsl, string BindDn, string Password, string BaseDn, string Domain, string AuthType);