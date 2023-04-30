using System;
using System.IO;
using System.Collections.Generic;

namespace sumparse
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.WriteLine("Not Enough Arguments.");
                return;
            }
            string output = args[0];
            List<Hash> hashes = new List<Hash>();

            for (int i = 1; i < args.Length; i++)
            {
                string file = args[i];

                if (!File.Exists(file))
                {
                    Console.WriteLine("Cannot find file: " + file);
                    continue;
                }

                Console.WriteLine("Read: " + file);
                StreamReader reader = new(file);

                while (!reader.EndOfStream)
                {
                    string _line = reader.ReadLine();
                    Console.WriteLine("Line: \"" + _line + "\"");
                    string[] line = _line.Split("  ");
                    string filename = line[1];
                    string _hash = line[0];
                    Hash hash = null;
                    if (hashes.FindAll(it => it.FileName == filename).Count > 0)
                    {
                        hash = hashes.FindAll(it => it.FileName == filename)[0];
                    }
                    else
                    {
                        Console.WriteLine("Cannot find: " + filename + " (created)");
                        hash = new Hash() { FileName = filename };
                        hashes.Add(hash);
                    }

                    if (file.EndsWith("md5", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith("md5.txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Add MD5 ('{_hash}') to {hash.FileName}");
                        hash.Md5 = _hash;
                    }
                    if (file.EndsWith("sha1", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith("sha1.txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Add SHA1 ('{_hash}') to {hash.FileName}");
                        hash.Sha1 = _hash;
                    }
                    if (file.EndsWith("sha256", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith("sha256.txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Add SHA256 ('{_hash}') to {hash.FileName}");
                        hash.Sha256 = _hash;
                    }
                    if (file.EndsWith("sha512", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith("sha512.txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Add SHA512 ('{_hash}') to {hash.FileName}");
                        hash.Sha512 = _hash;
                    }
                }

                reader.Close();
                reader.Dispose();
            }

            StreamWriter writer = new(output, false, System.Text.Encoding.UTF8);
            Console.WriteLine("Write: " + output);
            for (int i = 0; i < hashes.Count; i++)
            {
                var hash = hashes[i];
                Console.WriteLine("Work on: " + hash.FileName);
                string _h = $"**{hash.FileName}**:{Environment.NewLine}- SHA1: `{hash.Sha1}` {Environment.NewLine}- SHA256: `{hash.Sha256}` {Environment.NewLine}- SHA512: `{hash.Sha512}` {Environment.NewLine}- MD5: `{hash.Md5}` {Environment.NewLine}";
                writer.WriteLine(_h);
            }
            writer.Close();
            writer.Dispose();
        }

        private class Hash
        {
            public string FileName { get; set; }
            public string Sha1 { get; set; }
            public string Sha256 { get; set; }
            public string Sha512 { get; set; }
            public string Md5 { get; set; }
        }
    }
}