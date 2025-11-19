namespace MADProvisioner.Settings;

public sealed record MADSettings(string Host, int Port, bool UseSsl, string BindDn, string Password, string BaseDn, string Domain, string AuthType);