
using System;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Nini.Config;

namespace OlympChecker
{
	static class Utils
    {
        public static string programDir = Directory.GetCurrentDirectory();
        private const string version = "2.0";
        private const string downloadUrl = "";

        public static void CheckForUpdates()
        {
            try
            {
                byte[] data = new WebClient().DownloadData("https://olympchecker.googlecode.com/hg/version");
                string newestVer = Encoding.ASCII.GetString(data);

                if (newestVer != version)
                {
                    if (MessageBox.Show(Text.NewVersionAvailable, "", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        new WebClient().DownloadFile(downloadUrl, "OlympChecker.exe");
                        MessageBox.Show(Text.Downloaded);
                    }
                }
            }
            catch
            {
            }
        }

        public static Process StartProcess(string fileName, string args = "", bool oneProcessor = false)
        {
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = args;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.PriorityClass = ProcessPriorityClass.RealTime;
            if (oneProcessor)
            {
                process.ProcessorAffinity = new IntPtr(1 << 0);
            }
            return process;
        }

        public static bool Compile(string compiler, string options, string source, string output)
        {
            Process process = Utils.StartProcess(compiler, " " + options + " \"" + source + "\" -o \"" + output + "\"");
            process.WaitForExit();
            return (process.ExitCode == 0);
        }

        public static bool CompareFiles(string answerFile, string outFile, bool exact)
        {
            Utils.WaitForFile(answerFile);
            StreamReader answerReader = new StreamReader(answerFile);

            Utils.WaitForFile(outFile);
            StreamReader outReader = new StreamReader(outFile);

            bool result = true;
            while (!answerReader.EndOfStream)
            {
                string ansStr = answerReader.ReadLine();

                string outStr;
                try
                {
                    outStr = outReader.ReadLine();
                }
                catch (Exception) { outStr = ""; }

                if (!exact)
                {
                    try
                    {
                        while (ansStr.Contains("  "))
                        {
                            ansStr = ansStr.Replace("  ", " ");
                        }
                        ansStr = ansStr.Trim();
                    }
                    catch (NullReferenceException) { }

                    try
                    {
                        while (outStr.Contains("  "))
                        {
                            outStr = outStr.Replace("  ", " ");
                        }
                        outStr = outStr.Trim();
                    }
                    catch (NullReferenceException) { }
                }

                if (ansStr != outStr)
                {
                    result = false;
                }
            }
            answerReader.Close();
            outReader.Close();

            return result;
        }
        
        #region FS

        public static bool FileAvailable(string fileName)
        {
            FileStream fs;
            if (!File.Exists(fileName))
            {
                return false;
            }

            try
            {
                fs = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                fs.Dispose();
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }

        public static void WaitForFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                File.Create(fileName);
            }

            int cnt = 100;
            while (!FileAvailable(fileName) && cnt > 0)
            {
                Thread.Sleep(20);
                cnt--;
            }
            if (!FileAvailable(fileName))
            {
                PrintError(Text.NoAccess + " '" + fileName + "'");
            }
        }
        
        #endregion

        #region ConsoleOut
        public static void PrintError(string message, bool exitAfter = false)
        {
            PrintLine(message, ConsoleColor.Red);
            if (exitAfter)
            {
                Exit();
            }
        }

        public static void PrintLine(string s = "", ConsoleColor color = ConsoleColor.Gray)
        {
            Print(s + "\n", false, color);
        }

        public static void Print(string s, bool tab = true, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.Write(s + (tab ? "\t" : ""));
        } 
        #endregion

        public static void Exit()
        {
            Console.ReadKey(false);
            Environment.Exit(0);
        }
	}
}
