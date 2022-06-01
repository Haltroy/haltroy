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
                Console.WriteLine("USAGE: wyamfix [wyam output path] [output path] [-v|--verbose]");
                Console.WriteLine("Example: wyamfix \"C:\\Users\\haltroy\\Desktop\\htalt\\\"");
            }
            else
            {
                bool verbose = args.Contains("--verbose") || args.Contains("-v");
                string workingDir = args[0];
                string workingDir2 = args[1];
                // Real work starts now
                if (Directory.Exists(workingDir))
                {
                    Stopwatch sw = new Stopwatch();
                    List<FileDirInfo> list = new List<FileDirInfo>();
                    sw.Start();
                    RecursiveFileSearch(workingDir, workingDir2, 0, ref list);

                    for (int i = 0; i < list.Count; i++)
                    {
                        var file = list[i];
                        if (verbose) { Console.WriteLine((file.IsDir ? "| " : "-\\ ") + file.FullPath); }
                        if (!file.IsDir && file.FullPath.EndsWith(".html"))
                        {
                            string text = HTAlt.Tools.ReadFile(file.FullPath, System.Text.Encoding.UTF8);
                            for (int _i = 0; _i < list.Count; _i++)
                            {
                                list[_i].FixPath(file.Depth);
                                text = text.Replace("href\"" + list[_i].Replace + "\"", "href\"" + list[_i].Path + "\"");
                                text = text.Replace("src\"" + list[_i].Replace + "\"", "src\"" + list[_i].Path + "\"");
                            }
                            HTAlt.Tools.WriteFile(file.FullPath.Replace(workingDir, workingDir2), text, System.Text.Encoding.UTF8);
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
            static void RecursiveFileSearch(string path, string opath, int depth, ref List<FileDirInfo> list, string workDir = "")
            {
                if (list.FindAll(it => it.FullPath == (string.IsNullOrWhiteSpace(workDir) ? path : workDir)).Count > 0)
                {
                    var info = new FileDirInfo
                    {
                        FullPath = string.IsNullOrWhiteSpace(workDir) ? path : workDir,
                        IsDir = true,
                        WorkDir = string.IsNullOrWhiteSpace(workDir) ? path : workDir
                    };
                    info.FixPath();
                    info.Depth = depth;
                    list.Add(info);
                }
                var folders = System.IO.Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < folders.Length; i++)
                {
                    var info = new FileDirInfo
                    {
                        FullPath = folders[i],
                        IsDir = true,
                        WorkDir = string.IsNullOrWhiteSpace(workDir) ? path : workDir
                    };
                    info.FixPath();
                    info.Depth = depth;
                    list.Add(info);
                    RecursiveFileSearch(folders[i], opath, depth + 1, ref list, string.IsNullOrWhiteSpace(workDir) ? path : workDir);
                }
                var files = System.IO.Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    var info = new FileDirInfo
                    {
                        FullPath = files[i],
                        IsDir = false,
                        WorkDir = string.IsNullOrWhiteSpace(workDir) ? path : workDir
                    };
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
            public bool IsDir { get; set; }
            public string WorkDir { get; set; }
            public string Replace { get; set; }
            public int Depth { get; set; }

            public void FixPath(int depth = 0)
            {
                WorkDir = (WorkDir.EndsWith("/") || WorkDir.EndsWith("\\")) ? WorkDir[..^1] : WorkDir;
                Path = FullPath.Replace(WorkDir + "/", "").Replace(WorkDir + "\\", "");
                Replace = !IsDir
                    ? (System.IO.Path.GetExtension(FullPath).ToLowerEnglish().EndsWith("html") ? ("/" + Path[..^System.IO.Path.GetExtension(FullPath).Length].Replace("\\", "/")) : ("/" + Path.Replace("\\", "/")))
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
                Path = x + Path.Replace("\\", "/") + (!IsDir ? (!Path.EndsWith(System.IO.Path.GetExtension(FullPath)) ? System.IO.Path.GetExtension(FullPath) : "") : "/index.html");
            }
        }
    }
}