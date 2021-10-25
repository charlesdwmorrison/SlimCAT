
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Reflection;
/// <summary>
/// This is another way to load the appsettings.json file, but probably the  most correct approach is:
/// https://dejanstojanovic.net/aspnet/2018/december/setting-up-kestrel-port-in-configuration-file-in-aspnet-core/
/// </summary>
public static class Extensions
{
    public static string GetConfiguration(string configValue)
    {
        string jsonConfigStr = "";
        string execPath = Assembly.GetExecutingAssembly().Location;
        string executionDir = Path.GetDirectoryName(execPath);
        string fileNameAndPath = Path.Combine(executionDir, "appsettings.json");
        if (File.Exists(fileNameAndPath))
        {
            jsonConfigStr = File.ReadAllText(fileNameAndPath);
        }

        var result = (string)JObject.Parse(jsonConfigStr)
                    .DescendantsAndSelf()
                    .OfType<JProperty>()
                    .Single(x => x.Name.Equals(configValue))
                    .Value;

        return result;
    }
}