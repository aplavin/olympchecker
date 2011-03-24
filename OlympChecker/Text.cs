namespace OlympChecker
{
    using Nini.Config;
    using System;
    using System.IO;

    internal class Text
    {
        private static string fileName = "locale.ini";
        private static IniConfigSource configSource;
        public static string lang { get; set; }

        static Text()
        {
            try
            {
                configSource = new IniConfigSource(fileName);
            }
            catch (IOException) { }
        }

        public static string Get(string name, string defaultValue = "")
        {
            try
            {
                return configSource.Configs[lang.ToUpper()].Get(name, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string TestingComplete { get { return Get("TestingComplete", "Testing completed:"); } }

        public static string CompilerNotFound { get { return Get("CompilerNotFound", "Compiler not found:"); } }

        public static string NotExist { get { return Get("NotExists", "Doesn't exist:"); } }

        public static string Compiling { get { return Get("Compiling", "Compiling"); } }

        public static string Test { get { return Get("Test", "Test"); } }

        public static string Testing { get { return Get("Testing", "Testing..."); } }

        public static string NoAccess { get { return Get("NoAccess", "No access to"); } }

        public static string ConfigCreated { get { return Get("ConfigCreated", "Configuration file was created. Edit it and run OlympChecker again."); } }

        public static string Error { get { return Get("Error", "ERROR"); } }

        public static string LookingForTests { get { return Get("LookingForTests", "Looking for tests..."); } }

        public static string RemovingTemp { get { return Get("RemovingTemp", "Removing temporary files..."); } }

        public static string MaxTime { get { return Get("MaxTime", "Maximum time (w/o TL):"); } }

        public static string AllFound { get { return Get("AllFound", "All specified files found."); } }

        public static string Copyright { get { return "Original version: (c) Medvednikov Alexander\nThis modification: (c) Plavin Alexander"; } }
    }

}

