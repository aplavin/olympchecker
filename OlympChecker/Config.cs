
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Nini.Config;

namespace OlympChecker
{
	static class Config
    {
        private static string fileName = "OlympChecker.ini";
        private static IniConfigSource configSource;

        public static void Load()
        {
            try
            {
                configSource = new IniConfigSource(fileName);
            }
            catch (FileNotFoundException)
            {
                configSource = new IniConfigSource();
                IConfig settings = configSource.AddConfig("Settings");
                settings.Set("Lang", "ru");
                IConfig solutionConfig = configSource.AddConfig("Solution");
                solutionConfig.Set("FileName", "");
                solutionConfig.Set("Precompiled", "false");
                solutionConfig.Set("Compiler", "C:\\MinGW\\bin\\g++.exe");
                solutionConfig.Set("CompileOptions", "-O2 -s");
                solutionConfig.Set("Source", "");
                solutionConfig.Set("TestsDir", "");
                solutionConfig.Set("TimeLimit", "1000");
                IConfig checkerConfig = configSource.AddConfig("Checker");
                checkerConfig.Set("Standart", "true");
                checkerConfig.Set("Exact", "false");
                checkerConfig.Set("Precompiled", "false");
                checkerConfig.Set("Compiler", "C:\\MinGW\\bin\\g++.exe");
                checkerConfig.Set("CompileOptions", "-O2 -s");
                checkerConfig.Set("Name", "");

                configSource.Save(fileName);

                Utils.PrintLine(Text.ConfigCreated);
                Utils.Exit();
            }

            Text.lang = configSource.Configs["Settings"].Get("Lang");

            configSource.Configs["Checker"].Set("Source", Path.Combine(Utils.programDir, "checkers\\" + configSource.Configs["Checker"].Get("Name")));

            if (configSource.Configs["Solution"].Get("FileName").Trim() == "")
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(configSource.Configs["Solution"].Get("Source"));
                configSource.Configs["Solution"].Set("FileName", name);
            }
        }

        #region Settings
        public static string Lang { get { return GetString("Settings", "Lang"); } }
        #endregion

        #region Solution
        public static string SolutionFileName { get { return GetString("Solution", "FileName"); } }
        public static string SolutionFileIn
        {
            get
            {
                if (SolutionFileName.Contains("*"))
                {
                    return SolutionFileName.Replace("*", "in");
                }
                else
                {
                    return SolutionFileName + ".in";
                }
            }
        }
        public static string SolutionFileOut
        {
            get
            {
                if (SolutionFileName.Contains("*"))
                {
                    return SolutionFileName.Replace("*", "out");
                }
                else
                {
                    return SolutionFileName + ".out";
                }
            }
        }
        public static bool SolutionPrecompiled { get { return GetBoolean("Solution", "Precompiled"); } }
        public static string SolutionCompiler { get { return GetString("Solution", "Compiler"); } }
        public static string SolutionCompileOptions { get { return GetString("Solution", "CompileOptions"); } }
        public static string SolutionSource { get { return GetString("Solution", "Source"); } }
        public static string SolutionTestsDir { get { return GetString("Solution", "TestsDir"); } }
        public static int SolutionTimeLimit { get { return GetInt("Solution", "TimeLimit"); } }
        #endregion

        #region Checker
        public static bool CheckerStandart { get { return GetBoolean("Checker", "Standart"); } }
        public static bool CheckerExact { get { return GetBoolean("Checker", "Exact"); } }
        public static bool CheckerPrecompiled { get { return GetBoolean("Checker", "Precompiled"); } }
        public static string CheckerCompiler { get { return GetString("Checker", "Compiler"); } }
        public static string CheckerCompileOptions { get { return GetString("Checker", "CompileOptions"); } }
        public static string CheckerName { get { return GetString("Checker", "Name"); } }
        public static string CheckerSource { get { return GetString("Checker", "Source"); } }
        #endregion

        #region Helpers
        private static string GetString(string configName, string name)
        {
            if (configSource.Configs[configName] == null || !configSource.Configs[configName].Contains(name))
            {
                Utils.PrintError(Text.NotExist + " '" + name + "' (" + fileName + ")", true);
            }

            return configSource.Configs[configName].Get(name);
        }

        private static int GetInt(string configName, string name)
        {
            if (configSource.Configs[configName] == null || !configSource.Configs[configName].Contains(name))
            {
                Utils.PrintError(Text.NotExist + " '" + name + "' (" + fileName + ")", true);
            }

            return configSource.Configs[configName].GetInt(name);
        }

        private static bool GetBoolean(string configName, string name)
        {
            if (configSource.Configs[configName] == null || !configSource.Configs[configName].Contains(name))
            {
                Utils.PrintError(Text.NotExist + " '" + name + "' (" + fileName + ")", true);
            }

            return configSource.Configs[configName].GetBoolean(name);
        } 
        #endregion

	}

}
