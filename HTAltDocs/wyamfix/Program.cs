using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using HTAlt;

namespace wyamfix
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <= 0 || string.IsNullOrWhiteSpace(args[0]) || args[0] == "--help" || args[0] == "-h" || args[0] == "?")
            {
                Console.WriteLine("WYAMFIX for HTAlt API Docs. ");
                Console.WriteLine("----------------------------");
                Console.WriteLine("USAGE: wyamfix [output path]");
                Console.WriteLine("Example: wyamfix \"F:\\Website\\HTAlt\\output");
            }
            else
            {
                bool verbose = args.Contains("--verbose") || args.Contains("-v");
                string workingDir = args[0];
                List<string> ReplaceList = new List<string>();
                //Read the doc, add all nodes
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Properties.Resources.Files);
                XmlNode rootnode = doc.FindRoot();
                for (int i =0; i < rootnode.ChildNodes.Count;i++)
                {
                    ReplaceList.Add(rootnode.ChildNodes[i].Attributes["Text"].Value);
                }
                // Real work starts now
                if (Directory.Exists(workingDir))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var files = Directory.GetFiles(workingDir, "*.html", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length;i++)
                    {
                        if (verbose) Console.WriteLine("[----------]");
                        var str = Tools.ReadFile(files[i], System.Text.Encoding.UTF8);
                        for (int i1 = 0; i1 < ReplaceList.Count; i1++)
                        {
                            if (verbose) Console.WriteLine("| \"" + ReplaceList[i1] + "\" -> \"" + ReplaceList[i1] + ".html\"");
                            str = str.Replace(ReplaceList[i1], ReplaceList[i1] + ".html");
                        }
                        str.WriteToFile(files[i],System.Text.Encoding.UTF8);
                        if (verbose) Console.WriteLine(">>" + files[i]);
                    }
                    sw.Stop();
                    if (verbose) Console.WriteLine("[Done in " + sw.ElapsedMilliseconds + " ms.]");
                }
                else
                {
                    Console.WriteLine("Wyam Output directory doesn't exists. Argument(s): " + string.Join(' ', args));
                }
            }
        }
    }
}
