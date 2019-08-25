using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CompilerCore
{
    public static class JsonProcessor
    {
        public static string JsonString;

        /// <summary>
        /// The JSON Formatter and saving code.
        /// </summary>
        /// <param name="appName">The game's name. Fills the app_name field.</param>
        /// <param name="gameId">The game's ID. Fills the name field.</param>
        /// <param name="gameVersion">The version of the game. Fills the version field.</param>
        /// <param name="fileLocation">The location of the HTML file. THis must be in the context of the nwjs/Game executable's location. Fills the main field.</param>
        /// <param name="nodeJsEnabled">Enable or disable Node.js . Fills the nodejs field.</param>
        /// <param name="chromiumFlags">Sets the Chromium flags for the game. Fills the chromium-flags field.</param>
        /// <param name="jsFlags">Sets V8's flags for the game. Fills the js-flags field.</param>
        /// <param name="windowId">Set the ID of the game's window. Fills the id field in the window section.</param>
        /// <param name="iconLocation">Sets the location of the icon. Fills the icon field in the window section.</param>
        /// <param name="windowTitle">The title that shows up when the game starts. Fills the title field in the window section.</param>
        /// <param name="windowWidth">The window's width. Fills the width field in the window section.</param>
        /// <param name="windowHeight">The window's height. Fills the height field in the window section.</param>
        /// <param name="windowMinWidth">The minimum width of the window. Fills the min_width field in the window section.</param>
        /// <param name="windowMinHeight">The minimum height of the window. Fills the min_height field in the window section.</param>
        /// <param name="resizable">The window is resizable when this is true. Fills the resizable field in the window section.</param>
        /// <param name="fullscreen">The game starts in Fullscreen when it's true. Fills the fullscreen field in the window section.</param>
        /// <param name="kioskMode">Turns on or off kiosk mode. Fills the kiosk filed in the window section.</param>
        /// <param name="windowLocation">The location of the window when the game starts up. Fills the position field in the window section.</param>
        /// <param name="packageFileLocation">The folder where the package.json will be saved.</param>
        public static void BuildJson(in string appName, in string gameId, in string gameVersion, in string fileLocation, in bool nodeJsEnabled, in string chromiumFlags, in string jsFlags, in string windowId, in string iconLocation, in string windowTitle, in int windowWidth, in int windowHeight, in int windowMinWidth, in int windowMinHeight, in bool resizable, in bool fullscreen, in bool kioskMode, in string windowLocation, in string packageFileLocation)
        {
            JObject gameMetadata = new JObject(
                new JProperty("app_name", appName),
                new JProperty("name", gameId),
                new JProperty("version", gameVersion),
                new JProperty("main", fileLocation),
                new JProperty("nodejs", nodeJsEnabled),
                new JProperty("chromium-args", chromiumFlags),
                new JProperty("js-flags", jsFlags),
                new JProperty("window",
                    new JObject(
                        new JProperty("id", windowId),
                        new JProperty("icon", iconLocation),
                        new JProperty("title", windowTitle),
                        new JProperty("position", windowLocation),
                        new JProperty("resizable", resizable),
                        new JProperty("fullscreen", fullscreen),
                        new JProperty("kiosk", kioskMode),
                        new JProperty("width", windowWidth),
                        new JProperty("height", windowHeight),
                        new JProperty("min_width", windowMinWidth),
                        new JProperty("min_height", windowMinHeight))));
            using (StreamWriter settingsFile = new StreamWriter(Path.Combine(packageFileLocation, "package.json")))
                settingsFile.Write(gameMetadata);
        }


        public static string ReadJson(in string fileLocation)
        {
            char[] JsonIn;
            using (StreamReader settingsLoader = new StreamReader(fileLocation))
            {
                JsonIn = new Char[(int)settingsLoader.BaseStream.Length];
                settingsLoader.Read(JsonIn, 0, (int)settingsLoader.BaseStream.Length);
            }

            JsonString = new string(JsonIn);
            return JsonString;
        }
    }
}
