using System.Xml.Serialization;

namespace Domain.Connection;

public static class XmlConnectionDefinitionLoader
{
    public static ConnectionDefinition? Load(string pluginBasePath)
    {
        string path = Path.Combine(pluginBasePath, "connection.definition.xml");

        if (!File.Exists(path))
        {
            return null;
        }

        FileStream fileStream = File.OpenRead(path);
        try
        {
            XmlSerializer serializer = new(typeof(ConnectionDefinition));
            ConnectionDefinition connectionDefinition = (ConnectionDefinition)serializer.Deserialize(fileStream)!;
            return connectionDefinition;
        }
        finally
        {
            fileStream.Dispose();
        }
    }
}