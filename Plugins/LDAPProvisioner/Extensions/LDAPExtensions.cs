using System.Text;

namespace LDAPProvisioner.Extensions;

public static class LDAPExtensions
{
    public static bool TryConvertLdapValue(string name, string raw, out object value)
    {
        value = raw;

        switch (name.ToLowerInvariant())
        {
            case "useraccountcontrol":
                if (raw.Equals("Enabled",  StringComparison.OrdinalIgnoreCase))
                {
                    value = "512";
                }
                else if (raw.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                {
                    value = "514";
                }
                else
                {
                    value = raw;
                }

                return true;
            case "unicodepwd":
                string quoted = $"\"{raw}\"";
                value = Encoding.Unicode.GetBytes(quoted);
                return true;
            case "accountexpires":
                if (raw.Equals("never", StringComparison.OrdinalIgnoreCase) || raw == "0" || raw == "9223372036854775807")
                {
                    value = "9223372036854775807";
                    return true;
                }

                if (DateTime.TryParse(raw, out DateTime dateTime))
                {
                    DateTime minFileTime = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    if (dateTime < minFileTime)
                    {
                        dateTime = new(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                    }

                    value = dateTime.ToUniversalTime().ToFileTimeUtc().ToString();
                    return true;
                }
                return false;
        }
        return true;
    }
}