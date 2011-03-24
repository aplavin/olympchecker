
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Nini.Config;

namespace OlympChecker
{

    internal static class Program
    {
        private const int OK = 0, WA = 1, PE = 2, FL = 3, RE = 4, TL = 5;
        private static string[] resultDirs = { "OK", "WA", "PE", "FL", "RE", "TL" };
        private static List<string> tests = new List<string>();
        private const string workDir = "work";
        private const string solutionExec= "olympSolution.exe";
        private const string checkerExec = "olympChecker.exe";
        private static int testsPassed;
        private static int maxTime;

        private static void Main(string[] args)
        {
            Thread thread = new Thread(Utils.CheckForUpdates);
            thread.Start();

            Utils.PrintLine(Text.Copyright + "\n", ConsoleColor.Cyan);


            Config.Load();

            Directory.CreateDirectory(workDir);
            Directory.SetCurrentDirectory(workDir);

            CheckFiles();
            CleanBefore();
            Utils.PrintLine();

            if (!Config.CheckerStandart)
            {
                CompileChecker();
            }
            CompileSolution();
            Utils.PrintLine();

            FindTests();
            PerformTesting();

            CleanAfter();

            Utils.Print("\n" + Text.TestingComplete + " ", false);
            ConsoleColor color = 0;
            int score = 100 * testsPassed / tests.Count;
            if (score == 100)
            {
                color = ConsoleColor.Green;
            }
            else if (score == 0)
            {
                color = ConsoleColor.Red;
            }
            else
            {
                color = ConsoleColor.Yellow;
            }
            Utils.PrintLine(testsPassed + "/" + tests.Count + " (" + score + "/100)", color);

            Utils.Print(Text.MaxTime, false);
            Utils.PrintLine(" " + maxTime + " ms",
                maxTime <= Config.SolutionTimeLimit * 3 / 4 ? ConsoleColor.Green : ConsoleColor.Yellow);

            Utils.Exit();
        }

        private static void CleanBefore()
        {
            Utils.Print("\n" + Text.RemovingTemp);


            Thread.Sleep(100);

            try
            {
                foreach (string file in Directory.GetFiles("."))
                {
                    Utils.WaitForFile(file);
                    File.Delete(file);
                }

                foreach (string dir in Directory.GetDirectories("."))
                {
                    Directory.Delete(dir, true);
                }
            }
            catch (IOException)
            {
                Utils.PrintError("[" + Text.Error + "]");
                Utils.Exit();
            }


            Utils.PrintLine("[OK]", ConsoleColor.Green);
        }
        
        private static void CleanAfter()
        {
            Utils.Print("\n" + Text.RemovingTemp);

            Thread.Sleep(100);

            try
            {
                foreach (string file in Directory.GetFiles("."))
                {
                    Utils.WaitForFile(file);
                    File.Delete(file);
                }

                foreach (string dir in Directory.GetDirectories("."))
                {
                    if (Directory.GetFiles(dir).Length == 0)
                    {
                        Directory.Delete(dir);
                    }
                }
            }
            catch (IOException)
            {
                Utils.PrintError("[" + Text.Error + "]");
                Utils.Exit();
            }

            Utils.PrintLine("[OK]", ConsoleColor.Green);
        }

        private static void CheckFiles()
        {
            bool ok = true;
            if (!File.Exists(Config.SolutionCompiler))
            {
                Utils.PrintError(Text.CompilerNotFound + " '" + Config.SolutionCompiler + "'");
                ok = false;
            }
            if (!File.Exists(Config.CheckerCompiler))
            {
                Utils.PrintError(Text.CompilerNotFound + " '" + Config.CheckerCompiler + "'");
                ok = false;
            }
            if (!File.Exists(Config.SolutionSource))
            {
                Utils.PrintError(Text.NotExist + " '" + Config.SolutionSource + "'");
                ok = false;
            }
            if (!Config.CheckerStandart && !File.Exists(Config.CheckerSource))
            {
                Utils.PrintError(Text.NotExist + " '" + Config.CheckerSource + "'");
                ok = false;
            }
            if (!Directory.Exists(Config.SolutionTestsDir))
            {
                Utils.PrintError(Text.NotExist + " '" + Config.SolutionTestsDir + "'");
                ok = false;
            }

            if (!ok)
            {
                Utils.Exit();
            }

            Utils.PrintLine(Text.AllFound);
        }

        private static void CompileChecker()
        {
            if (!Config.CheckerPrecompiled)
            {
                Utils.Print(Text.Compiling + " '" + Path.GetFileName(Config.CheckerSource) + "'...");
                if (Utils.Compile(Config.CheckerCompiler, Config.CheckerCompileOptions, Config.CheckerSource, checkerExec))
                {
                    Utils.PrintLine("[OK]", ConsoleColor.Green);
                }
                else
                {
                    Utils.PrintError("[" + Text.Error + "]");
                }
            }
            else
            {
                if (Path.GetFullPath(Config.CheckerSource) != Path.GetFullPath(checkerExec))
                {
                    File.Copy(Config.CheckerSource, checkerExec, true);
                }
            }
        }

        private static void CompileSolution()
        {
            if (!Config.SolutionPrecompiled)
            {
                Utils.Print(Text.Compiling + " '" + Path.GetFileName(Config.SolutionSource) + "'...");
                if (Utils.Compile(Config.SolutionCompiler, Config.SolutionCompileOptions, Config.SolutionSource, solutionExec))
                {
                    Utils.PrintLine("[OK]", ConsoleColor.Green);
                }
                else
                {
                    Utils.PrintError("[" + Text.Error + "]");
                }
            }
            else
            {
                File.Copy(Config.SolutionSource, solutionExec);
            }
        }

        #region Testing
        private static int CheckAnswer(string inputFile, string answerFile, string outFile)
        {
            if (Config.CheckerStandart)
            {
                return Utils.CompareFiles(answerFile, outFile, Config.CheckerExact) ? OK : WA;
            }
            else
            {
                Process process = Utils.StartProcess(checkerExec, inputFile + " " + answerFile + " " + outFile);
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        private static void PrintTestResult(int code, int time)
        {
            switch (code)
            {
                case OK:
                    Utils.PrintLine("[OK - " + time + " ms]", ConsoleColor.Green);
                    break;
                case WA:
                    Utils.PrintLine("[WA - " + time + " ms]", ConsoleColor.Red);
                    break;
                case PE:
                    Utils.PrintLine("[PE - " + time + " ms]", ConsoleColor.Red);
                    break;
                case FL:
                    Utils.PrintLine("[FAIL - " + time + " ms]", ConsoleColor.Red);
                    break;
                case RE:
                    Utils.PrintLine("[RE - " + time + " ms]", ConsoleColor.Red);
                    break;
                case TL:
                    Utils.PrintLine("[TL - >" + Config.SolutionTimeLimit + " ms]", ConsoleColor.Red);
                    break;
            }
        }

        private static void PrepareTest(string test)
        {
            if (File.Exists(Config.SolutionFileIn))
            {
                Utils.WaitForFile(Config.SolutionFileIn);
                File.Delete(Config.SolutionFileIn);
            }
            if (File.Exists("correct.out"))
            {
                Utils.WaitForFile("correct.out");
                File.Delete("correct.out");
            }
            File.Copy(test, Config.SolutionFileIn);
            File.Copy(test + ".a", "correct.out");
        }

        private static void PerformTest(string test)
        {
            Process process = Utils.StartProcess(solutionExec, "", true);
            while (!process.HasExited && process.UserProcessorTime.TotalMilliseconds <= Config.SolutionTimeLimit)
            {
                Thread.Sleep(20);
            }
            if (!process.HasExited)
            {
                process.Kill();
                process.WaitForExit();
            }

            // process exited, gather results

            bool gotRE = (process.ExitCode != 0); // if we killed process, it will appear too

            int time = (int)process.UserProcessorTime.TotalMilliseconds;
            bool gotTL = (time > Config.SolutionTimeLimit);
            if (!gotTL)
            {
                maxTime = Math.Max(maxTime, time);
            }


            int code = -1;
            if (gotTL)
            {
                code = TL;
            }
            else if (gotRE)
            {
                code = RE;
            }
            else
            {
                code = CheckAnswer(Config.SolutionFileIn, "correct.out", Config.SolutionFileOut);
            }

            Directory.CreateDirectory(resultDirs[code]);
            File.Copy(test, Path.Combine(resultDirs[code], Path.GetFileName(test)));
            File.Copy(test + ".a", Path.Combine(resultDirs[code], Path.GetFileName(test) + ".a"));

            PrintTestResult(code, time);
            if (code == OK)
            {
                testsPassed++;
            }
        }

        private static void PerformTesting()
        {
            Utils.PrintLine(Text.Testing + "\n");

            foreach (string test in tests)
            {
                Utils.Print(Text.Test + " '" + Path.GetFileNameWithoutExtension(test) + "': ", false);
                try
                {
                    PrepareTest(test);
                    PerformTest(test);
                }
                catch (Exception)
                {
                    Utils.PrintError(Text.Error, true);
                }
            }
        }

        private static void FindTests()
        {
            string[] filesList = Directory.GetFiles(Config.SolutionTestsDir);
            Array.Sort(filesList);
            foreach (string file in filesList)
            {
                if (file.IndexOf('.') == -1 && File.Exists(file + ".a"))
                {
                    tests.Add(file);
                }
            }
            Utils.Print(Text.LookingForTests);
            Utils.PrintLine("[" + tests.Count + "]", ConsoleColor.Green);
        } 
        #endregion
    }
}
