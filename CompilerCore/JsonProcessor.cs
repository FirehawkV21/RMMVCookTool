using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CompilerCore
{
    public class JsonProcessor
    {
        public string JsonData;
        public static string BuildJson(string appName, string gameId, string fileLocation, bool nodeJsEnabled, string chromiumFlags, string jsFlags, string iconLocation, int windowWidth, int windowHeight, int windowMinWidth, int windowMinHeight)
        {
            JObject gameMetadata = new JObject(
                new JProperty("app_name", appName),
                new JProperty("name", gameId),
                new JProperty("main", fileLocation),
                new JProperty("nodejs", nodeJsEnabled),
                new JProperty("chromium-flags", chromiumFlags),
                new JProperty("js-flags", jsFlags),
                new JProperty("window",
                    new JObject(
                        new JProperty("icon", iconLocation),
                        new JProperty("width", windowWidth),
                        new JProperty("height", windowHeight),
                        new JProperty("min_width", windowMinWidth),
                        new JProperty("min_height", windowMinHeight))));
            using (StreamWriter settingsFile = new StreamWriter("C:\\Users\\acemo\\package.json"))
            settingsFile.Write(gameMetadata);
            return gameMetadata.ToString();
        }

        //public static async string ReadJson(string FileLocation)
        //{
        //    char[] JsonIn;
        //    using (StreamReader settingsLoader = new StreamReader("C:\\Users\\acemo\\packge.json"))
        //    {
        //        JsonIn = new Char[(int)settingsLoader.BaseStream.Length];
        //        await settingsLoader.ReadAsync(JsonIn, 0, (int) settingsLoader.BaseStream.Length);
        //    }

        //    string JsonOut = new string(JsonIn);
        //    return JsonOut;
        //}
    }
}
