using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using HTAlt;

namespace wyamfix
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length <= 0 || string.IsNullOrWhiteSpace(args[0]) || args[0] == "--help" || args[0] == "-h" || args[0] == "?")
            {
                Console.WriteLine("WYAMFIX for HTAlt API Docs. ");
                Console.WriteLine("----------------------------");
                Console.WriteLine("USAGE: wyamfix [output path] [-v|--verbose]");
                Console.WriteLine("Example: wyamfix \"C:\\Users\\haltroy\\Desktop\\htalt\\\"");
            }
            else
            {
                bool verbose = args.Contains("--verbose") || args.Contains("-v");
                string workingDir = args[0];
                // Real work starts now
                if (Directory.Exists(workingDir))
                {
                    Stopwatch sw = new Stopwatch();
                    List<FileDirInfo> list = new List<FileDirInfo>();
                    RecursiveFileSearch(workingDir, 0, ref list);
                    sw.Start();
                    for (int i = 0; i < list.Count; i++)
                    {
                        var file = list[i];
                        if (verbose) { Console.WriteLine((file.isDir ? "| " : "-\\ ") + file.FullPath); }
                        if (!file.isDir && file.FullPath.EndsWith(".html"))
                        {
                            string text = HTAlt.Tools.ReadFile(file.FullPath, System.Text.Encoding.UTF8);
                            for (int _i = 0; _i < list.Count; _i++)
                            {
                                list[_i].FixPath(file.Depth);
                                text = text.Replace("href\"" + list[_i].Replace + "\"", "href\"" + list[_i].Path + "\"");
                                text = text.Replace("src\"" + list[_i].Replace + "\"", "src\"" + list[_i].Path + "\"");
                            }
                            HTAlt.Tools.WriteFile(file.FullPath, text, System.Text.Encoding.UTF8);
                        }
                    }

                    sw.Stop();
                    if (verbose) Console.WriteLine("[Done in " + sw.ElapsedMilliseconds + " ms.]");
                }
                else
                {
                    Console.WriteLine("Wyam Output directory doesn't exists. Argument(s): " + string.Join(' ', args));
                }
            }
            static void RecursiveFileSearch(string path, int depth, ref List<FileDirInfo> list, string workDir = "")
            {
                if (list.FindAll(it => it.FullPath == (string.IsNullOrWhiteSpace(workDir) ? path : workDir)).Count > 0)
                {
                    var info = new FileDirInfo();
                    info.FullPath = string.IsNullOrWhiteSpace(workDir) ? path : workDir;
                    info.isDir = true;
                    info.WorkDir = string.IsNullOrWhiteSpace(workDir) ? path : workDir;
                    info.FixPath();
                    info.Depth = depth;
                    list.Add(info);
                }
                var folders = System.IO.Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < folders.Length; i++)
                {
                    var info = new FileDirInfo();
                    info.FullPath = folders[i];
                    info.isDir = true;
                    info.WorkDir = string.IsNullOrWhiteSpace(workDir) ? path : workDir;
                    info.FixPath();
                    info.Depth = depth;
                    list.Add(info);
                    RecursiveFileSearch(folders[i], depth + 1, ref list, string.IsNullOrWhiteSpace(workDir) ? path : workDir);
                }
                var files = System.IO.Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    var info = new FileDirInfo();
                    info.FullPath = files[i];
                    info.isDir = false;
                    info.WorkDir = string.IsNullOrWhiteSpace(workDir) ? path : workDir;
                    info.FixPath();
                    info.Depth = depth;
                    list.Add(info);
                }
            }
        }

        public class FileDirInfo
        {
            public string FullPath { get; set; }
            public string Path { get; set; }
            public bool isDir { get; set; }
            public string WorkDir { get; set; }
            public string Replace { get; set; }
            public int Depth { get; set; }

            public void FixPath(int depth = 0)
            {
                WorkDir = (WorkDir.EndsWith("/") || WorkDir.EndsWith("\\")) ? WorkDir.Substring(0, WorkDir.Length - 1) : WorkDir;
                Path = FullPath.Replace(WorkDir + "/", "").Replace(WorkDir + "\\", "");
                Replace = !isDir
                    ? (System.IO.Path.GetExtension(FullPath).ToLowerEnglish().EndsWith("html") ? ("/" + Path.Substring(0, Path.Length - System.IO.Path.GetExtension(FullPath).Length).Replace("\\", "/")) : ("/" + Path.Replace("\\", "/")))
                    : "/" + Path.Replace("\\", "/");

                string x = string.Empty;
                if (depth == 0)
                {
                    x = "./";
                }
                else if (depth > 0)
                {
                    for (int i = 0; i < depth; i++)
                    {
                        x += "../";
                    }
                }
                Path = x + Path.Replace("\\", "/") + (!isDir ? (!Path.EndsWith(System.IO.Path.GetExtension(FullPath)) ? System.IO.Path.GetExtension(FullPath) : "") : "/index.html");
            }
        }
    }
}