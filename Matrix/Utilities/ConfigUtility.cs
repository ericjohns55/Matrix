namespace Matrix.Utilities;

public class ConfigUtility
{
    public static Dictionary<string, object?> GetConfig(IConfiguration configuration)
    {
        var configSettings = new Dictionary<string, object?>();
        
        foreach (var configurationSection in configuration.GetChildren())
        {
            IterateConfigurationSection(configurationSection, configSettings);
        }

        return configSettings;
    }

    private static void IterateConfigurationSection(
        IConfigurationSection? section,
        Dictionary<string, object?> sections)
    {
        if (section != null)
        {
            sections.Add(section.Path, section.Value);

            foreach (var child in section.GetChildren())
            {
                IterateConfigurationSection(child, sections);
            }
        }
    }
}