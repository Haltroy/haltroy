using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace HTAlt
{
    /// <summary>
    /// Haltroy UPDATE class.
    /// </summary>
    public class HTUPDATE
    {
        #region HT Info

        private readonly Uri wikiLink = new Uri("https://haltroy.com/htalt/api/HTAlt.Standart/HTUPDATE");
        private readonly Version firstHTAltVersion = new Version("0.1.7.0");
        private readonly string description = "Haltroy UPDATE class.";

        /// <summary>
        /// This control's wiki link.
        /// </summary>
        [Bindable(false)]
        [Category("HTAlt")]
        [Description("This control's wiki link.")]
        public Uri WikiLink => wikiLink;

        /// <summary>
        /// This control's first appearance version for HTAlt.
        /// </summary>
        [Bindable(false)]
        [Category("HTAlt")]
        [Description("This control's first appearance version for HTAlt.")]
        public Version FirstHTAltVersion => firstHTAltVersion;

        /// <summary>
        /// This control's description.
        /// </summary>
        [Bindable(false)]
        [Category("HTAlt")]
        [Description("This control's description.")]
        public string Description => description;

        #endregion HT Info

        /// <summary>
        /// Creates a new HTUPDATE from <paramref name="uri"/>.
        /// </summary>
        /// <param name="name">Name of your project.</param>
        /// <param name="uri"><see cref="Uri"/> as <seealso cref="string"/></param>
        /// <param name="workFolder"><see cref="string"/> as path to working directory.</param>
        /// <param name="arch">Current Processor Architecture.</param>
        /// <param name="tempFolder">Temporary folder for temporary files such as downloaded packages.</param>
        /// <param name="version">Current version of your project.</param>
        public HTUPDATE(string name, string uri, string workFolder, string tempFolder, int version, string arch)
        {
            Name = name;
            URL = uri;
            WorkFolder = workFolder;
            CurrentVer = version;
            TempFolder = tempFolder;
            Arch = arch;
        }

        /// <summary>
        /// Loads HTUPDATE information from <see cref="URL"/>.
        /// </summary>
        public void LoadFromUrl()
        {
            if (DoTaskAsAsync)
            {
                LoadUrlAsync();
            }
            else
            {
                LoadUrlSync();
            }
        }

        /// <summary>
        /// Loads URL without sync.
        /// </summary>
        public async void LoadUrlAsync()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                LoadUrlSync();
            });
        }

        #region Zip

        private static class HTUGZip
        {
            public static void CompressFile(string sDir, string sRelativePath, System.IO.Compression.GZipStream zipStream)
            {
                //Compress file name
                char[] chars = sRelativePath.ToCharArray();
                zipStream.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
                foreach (char c in chars)
                    zipStream.Write(BitConverter.GetBytes(c), 0, sizeof(char));

                //Compress file content
                byte[] bytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(sDir, sRelativePath));
                zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
                zipStream.Write(bytes, 0, bytes.Length);
            }

            public static bool DecompressFile(string sDir, System.IO.Compression.GZipStream zipStream)
            {
                //Decompress file name
                byte[] bytes = new byte[sizeof(int)];
                int Readed = zipStream.Read(bytes, 0, sizeof(int));
                if (Readed < sizeof(int))
                    return false;

                int iNameLen = BitConverter.ToInt32(bytes, 0);
                bytes = new byte[sizeof(char)];
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < iNameLen; i++)
                {
                    zipStream.Read(bytes, 0, sizeof(char));
                    char c = BitConverter.ToChar(bytes, 0);
                    sb.Append(c);
                }
                string sFileName = sb.ToString();

                bytes = new byte[sizeof(int)];
                zipStream.Read(bytes, 0, sizeof(int));
                int iFileLen = BitConverter.ToInt32(bytes, 0);

                bytes = new byte[iFileLen];
                zipStream.Read(bytes, 0, bytes.Length);

                string sFilePath = System.IO.Path.Combine(sDir, sFileName);
                string sFinalDir = System.IO.Path.GetDirectoryName(sFilePath);
                if (!System.IO.Directory.Exists(sFinalDir))
                    System.IO.Directory.CreateDirectory(sFinalDir);

                using (System.IO.FileStream outFile = new System.IO.FileStream(sFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
                    outFile.Write(bytes, 0, iFileLen);

                return true;
            }

            public static void CompressDirectory(string sInDir, string sOutFile)
            {
                string[] sFiles = System.IO.Directory.GetFiles(sInDir, "*.*", System.IO.SearchOption.AllDirectories);
                int iDirLen = sInDir[sInDir.Length - 1] == System.IO.Path.DirectorySeparatorChar ? sInDir.Length : sInDir.Length + 1;

                using (System.IO.FileStream outFile = new System.IO.FileStream(sOutFile, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
                using (System.IO.Compression.GZipStream str = new System.IO.Compression.GZipStream(outFile, System.IO.Compression.CompressionMode.Compress))
                    foreach (string sFilePath in sFiles)
                    {
                        string sRelativePath = sFilePath.Substring(iDirLen);
                        CompressFile(sInDir, sRelativePath, str);
                    }
            }

            public static void DecompressToDirectory(string sCompressedFile, string sDir)
            {
                using (System.IO.FileStream inFile = new System.IO.FileStream(sCompressedFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None))
                    DecompressToDirectory(inFile, sDir);
            }

            public static void DecompressToDirectory(System.IO.Stream fileStream, string sDir)
            {
                using (System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(fileStream, System.IO.Compression.CompressionMode.Decompress, true))
                    while (DecompressFile(sDir, zipStream)) ;
            }

            public static long Seek(System.IO.Stream mainStream, byte search, long startPos = 0)
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    mainStream.CopyTo(stream);
                    for (long i = startPos; i < stream.Length; i++)
                    {
                        stream.Position = i;
                        byte v = (byte)stream.ReadByte();
                        if (search == v)
                        {
                            return i;
                        }
                    }
                    return -1;
                }
            }

            public static void ApplyDelta(string workFolder, System.IO.Stream deltaFileStream)
            {
                using (var deltaStream = new System.IO.MemoryStream())
                {
                    using (var gzipStream = new System.IO.Compression.GZipStream(deltaFileStream, System.IO.Compression.CompressionMode.Decompress, true))
                    {
                        gzipStream.CopyTo(deltaStream);
                    }
                    long processlength = 1;
                    for (long i = 0; i < deltaStream.Length - 1;)
                    {
                        deltaStream.Position = i;
                        var bayt = (byte)deltaStream.ReadByte();

                        switch (bayt)
                        {
                            case 0x01: // Change
                                long fileNameStart = i + 2;
                                deltaStream.Position = fileNameStart;
                                long fileNameEnd = Seek(deltaStream, 0x03, 1) + fileNameStart;
                                var fileNameList = new System.Collections.Generic.List<byte>();
                                for (long ı = fileNameStart; ı < fileNameEnd; ı++)
                                {
                                    deltaStream.Position = ı;
                                    fileNameList.Add((byte)deltaStream.ReadByte());
                                }
                                string fileName = System.Text.Encoding.Unicode.GetString(fileNameList.ToArray());
                                var startArray = new byte[10];
                                var startStrStart = fileNameEnd + 2;
                                deltaStream.Position = startStrStart;
                                deltaStream.Read(startArray, 0, 9);
                                var startIndex = BitConverter.ToInt64(startArray, 0);
                                var lengthArray = new byte[10];
                                var lengthStrStart = startStrStart + 9;
                                deltaStream.Position = lengthStrStart;
                                deltaStream.Read(lengthArray, 0, 9);
                                var length = BitConverter.ToInt64(lengthArray, 0);
                                var trimArray = new byte[10];
                                var trimStrStart = lengthStrStart + 9;
                                deltaStream.Position = trimStrStart;
                                deltaStream.Read(trimArray, 0, 9);
                                var trim = BitConverter.ToInt64(trimArray, 0);
                                var diffStart = trimStrStart + 9;
                                using (var diffs = new System.IO.MemoryStream())
                                {
                                    for (long _i = 0; _i < length; _i++)
                                    {
                                        diffs.Position = _i;
                                        deltaStream.Position = diffStart + _i;
                                        diffs.WriteByte((byte)deltaStream.ReadByte());
                                    }
                                    diffs.Position = 0;
                                    using (var targetFileStream = new System.IO.FileStream(System.IO.Path.Combine(workFolder, fileName), System.IO.FileMode.Open, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
                                    {
                                        // Trim file
                                        if (trim != 0 && trim > startIndex && trim >= (startIndex + length))
                                        {
                                            targetFileStream.SetLength(trim);
                                        }
                                        // apply changes
                                        for (long ı = 0; ı < diffs.Length; ı++)
                                        {
                                            diffs.Position = ı;
                                            var diffByte = (byte)diffs.ReadByte();
                                            targetFileStream.Position = startIndex + ı;
                                            targetFileStream.WriteByte(diffByte);
                                        }
                                    }
                                }
                                processlength = diffStart + length + 1;
                                break;

                            case 0x02: // Remove
                                long removefileStart = i + 2;
                                deltaStream.Position = removefileStart;
                                long removefileEnd = Seek(deltaStream, 0x03, 1) + removefileStart;
                                var removefileList = new System.Collections.Generic.List<byte>();
                                for (long ı = removefileStart; ı < removefileEnd; ı++)
                                {
                                    deltaStream.Position = ı;
                                    var removebayt = (byte)deltaStream.ReadByte();
                                    removefileList.Add(removebayt);
                                }
                                string removeFileName = System.Text.Encoding.Unicode.GetString(removefileList.ToArray());
                                if (System.IO.File.Exists(System.IO.Path.Combine(workFolder, removeFileName)))
                                {
                                    System.IO.File.Delete(System.IO.Path.Combine(workFolder, removeFileName));
                                }
                                processlength = removefileEnd + 2;
                                break;

                            case 0x03: // Create
                                long createfileNameStart = i + 2;
                                deltaStream.Position = createfileNameStart;
                                long createfileNameEnd = Seek(deltaStream, 0x03, 1) + createfileNameStart;
                                var createfileNameList = new System.Collections.Generic.List<byte>();
                                for (long ı = createfileNameStart; ı < createfileNameEnd; ı++)
                                {
                                    deltaStream.Position = ı;
                                    var createbayt = (byte)deltaStream.ReadByte();
                                    createfileNameList.Add(createbayt);
                                }
                                string createfileName = System.Text.Encoding.Unicode.GetString(createfileNameList.ToArray());
                                var createstartArray = new byte[10];
                                var createstartStrStart = createfileNameEnd + 2;
                                deltaStream.Position = createstartStrStart;
                                deltaStream.Read(createstartArray, 0, 8);
                                var createLength = BitConverter.ToInt64(createstartArray, 0);
                                var creatediffStart = createstartStrStart + 9;
                                using (var createStream = new System.IO.MemoryStream())
                                {
                                    createStream.SetLength(createLength);
                                    for (long _i = 0; _i < createLength; _i++)
                                    {
                                        createStream.Position = _i;
                                        deltaStream.Position = creatediffStart + _i;
                                        createStream.WriteByte((byte)deltaStream.ReadByte());
                                    }
                                    createStream.Position = 0;
                                    var workFile = System.IO.Path.Combine(workFolder, createfileName);
                                    if (!System.IO.File.Exists(workFile))
                                    {
                                        System.IO.File.Create(workFile).Close();
                                    }
                                    using (var targetFileStream = new System.IO.FileStream(workFile, System.IO.FileMode.Open, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
                                    {
                                        createStream.CopyTo(targetFileStream);
                                    }
                                }
                                processlength = creatediffStart + createLength + 1;
                                break;

                            case 0x04: // Rename
                                long renamefile1Start = i + 2;
                                deltaStream.Position = renamefile1Start;
                                long renamefile1End = Seek(deltaStream, 0x03, 1) + renamefile1Start;
                                var renamefileList = new System.Collections.Generic.List<byte>();
                                for (long ı = renamefile1Start; ı < renamefile1End; ı++)
                                {
                                    deltaStream.Position = ı;
                                    var r1bayt = (byte)deltaStream.ReadByte();
                                    renamefileList.Add(r1bayt);
                                }
                                string renameFile1Name = System.Text.Encoding.Unicode.GetString(renamefileList.ToArray());
                                long renamefile2Start = renamefile1End + 2;
                                deltaStream.Position = renamefile2Start;
                                long renamefile2End = Seek(deltaStream, 0x03, 1) + renamefile2Start;
                                renamefileList.Clear();
                                for (long ı = renamefile2Start; ı < renamefile2End; ı++)
                                {
                                    deltaStream.Position = ı;
                                    var r2bayt = (byte)deltaStream.ReadByte();
                                    renamefileList.Add(r2bayt);
                                }
                                string renameFile2Name = System.Text.Encoding.Unicode.GetString(renamefileList.ToArray());
                                if (System.IO.File.Exists(System.IO.Path.Combine(workFolder, renameFile1Name)))
                                {
                                    System.IO.File.Move(System.IO.Path.Combine(workFolder, renameFile1Name), System.IO.Path.Combine(workFolder, renameFile2Name));
                                }
                                processlength = renamefile2End + 2;
                                break;
                        }
                        i = processlength;
                    }
                }
            }
        }

        #endregion Zip

        /// <summary>
        /// Loads URL with sync.
        /// </summary>
        public void LoadUrlSync()
        {
            Log("Loading info from URL...", LogLevel.Info);
            System.Net.WebClient webC = new System.Net.WebClient();
            if (string.IsNullOrWhiteSpace(URL)) { Log("URL was either null, empty or just whitespaces.", LogLevel.Error); return; }
            string htu = string.Empty;
            try
            {
                htu = webC.DownloadString(URL);
            }
            catch (Exception ex)
            {
                Log("Cannot get information, exception caught: " + ex.ToString(), LogLevel.Error); return;
            }
            if (string.IsNullOrWhiteSpace(htu)) { Log("The information received was either null, empty or just white spaces. ", LogLevel.Error); return; }
            webC.Dispose();
            webC = null;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(htu);
                XmlNode rootNode = doc.FindRoot();
                List<string> applied = new List<string>();
                for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                {
                    bool exitLoop = false;
                    XmlNode node = rootNode.ChildNodes[i];
                    switch (node.Name.ToLowerEnglish())
                    {
                        case "mirror":
                            if (applied.Contains(node.Name.ToLowerEnglish()))
                            {
                                break;
                            }
                            applied.Add(node.Name.ToLowerEnglish());
                            Log("Mirror found.");
                            if (node.Attributes["URL"] == null)
                            {
                                Log("Mirror link was null.", LogLevel.Error);
                            }
                            else
                            {
                                URL = node.Attributes["URL"].Value.XmlToString();
                                Log("Mirrored to \"" + URL + "\".");
                                LoadFromUrl();
                                exitLoop = true;
                                break;
                            }
                            return;

                        case "version":
                            if (applied.Contains(node.Name.ToLowerEnglish()))
                            {
                                break;
                            }
                            applied.Add(node.Name.ToLowerEnglish());
                            if (string.IsNullOrWhiteSpace(node.InnerXml))
                            {
                                Log("Version InnerXML is empty.", LogLevel.Error);
                                return;
                            }
                            LatestVer = int.Parse(node.InnerXml.XmlToString());
                            break;

                        case "versions":
                            if (applied.Contains(node.Name.ToLowerEnglish()))
                            {
                                break;
                            }
                            applied.Add(node.Name.ToLowerEnglish());
                            for (int _i = 0; _i < node.ChildNodes.Count; _i++)
                            {
                                HTUPDATE_Version ver = new HTUPDATE_Version(node.ChildNodes[_i], this);
                                if (!Versions.Contains(ver))
                                {
                                    Versions.Add(ver);
                                }
                            }
                            Versions.Sort((x, y) => x.ID.CompareTo(y.ID));
                            break;

                        default:
                            if (!node.IsComment())
                            {
                                ThrownNodes.Add(node);
                            }
                            break;
                    }
                    if (exitLoop) { break; }
                }
            }
            catch (XmlException xe)
            {
                Log("XML configuration has error(s): " + xe.ToString(), LogLevel.Error);
            }
            catch (Exception ex)
            {
                Log("Cannot work on information, exception caught: " + ex.ToString(), LogLevel.Error); return;
            }
            Versions = Versions.OrderBy(it => it.ID).ToList(); // Sorts lists (increasing)
        }

        /// <summary>
        /// Updates the packages
        /// </summary>
        /// <param name="force">Forces to do update even when it is already in the latest version.</param>
        public void Update(bool force = false)
        {
            if (DoTaskAsAsync)
            {
                DoAsyncUpdate();
            }
            else
            {
                DoSyncUpdate();
            }
        }

        private class HTU_Download
        {
            public HTUPDATE_Arch arch { get; set; }
            public HTUPDATE_Version version => arch.Version;
            public string fileName { get; set; }
            public string fileURl => arch.Url;

            public int errorCount { get; set; } = 0;
        }

        private HTU_Download[] getDownloadList(List<HTU_Download> rList = null, HTUPDATE_Version ver = null)
        {
            if (rList == null) { rList = new List<HTU_Download>(); }
            HTUPDATE_Version lver = ver != null ? ver : LatestVersion;
            if (lver.BasedVersion != null)
            {
                if (lver.BasedVersion is int)
                {
                    lver.BasedVersion = (int)lver.BasedVersion != 0
                        ? GetVersion((int)lver.BasedVersion) is null
                            ? throw new Exception("Cannot find the based version with ID=\"" + (int)lver.BasedVersion + "\" for version with ID=\"" + lver.ID + "\".")
                            : GetVersion((int)lver.BasedVersion)
                        : throw new Exception("Illegal based version ID for version ID=\"" + (int)lver.BasedVersion + "\".");
                }
                getDownloadList(rList, (HTUPDATE_Version)lver.BasedVersion);
            }
            HTU_Download download = new HTU_Download();
            if (Arch.Contains("noarch"))
            {
                download.arch = lver.FindNoArch();
            }
            else
            {
                download.arch = lver.FindArch(Arch)[0];
            }
            rList.Add(download);
            return rList.ToArray(); // NOTE: The most based version should be in the front of this list.
        }

        /// <summary>
        /// Skips the error on the backup creation.
        /// </summary>
        public bool SkipBackupError { get; set; } = false;

        /// <summary>
        /// Skips the backup creation.
        /// </summary>
        public bool SkipBackup { get; set; } = false;

        private void downloadAndInstall(HTU_Download download)
        {
            download.fileName = System.IO.Path.Combine(TempFolder, Name + "-" + download.version.Name + "-" + download.arch.Arch + "." + (download.arch.isDelta ? "d" : "") + "hup");
            System.Net.WebClient webC = new System.Net.WebClient();
            try
            {
                bool error = false;
                using (var downloadStream = new System.IO.MemoryStream(webC.DownloadData(download.fileURl)))
                {
                    if (download.arch.Hashes.Count > 0)
                    {
                        for (int i = 0; i < download.arch.Hashes.Count; i++)
                        {
                            Log("Verifying file with hashes " + (i + 1) + "/" + download.arch.Hashes.Count + "...", LogLevel.Info);
                            try
                            {
                                download.arch.Hashes[i].Verify(downloadStream);
                            }
                            catch (Exception ex)
                            {
                                Log("Verification error on package \"" + (Name + "-" + download.version.Name + "-" + (download.arch.isDelta ? "d" : "") + ".hup") + "\", an error occurred on try " + (download.errorCount++) + "/" + RetryCount + ":" + ex.ToString(), LogLevel.Error);
                                if (download.errorCount < (RetryCount + 1))
                                {
                                    Log("Retry installing package :" + (Name + "-" + download.version.Name + "-" + download.arch.Arch + "." + (download.arch.isDelta ? "d" : "") + "hup"), LogLevel.Info);
                                    if (webC.IsBusy)
                                    {
                                        webC.CancelAsync();
                                    }
                                    downloadAndInstall(download);
                                    error = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!error)
                    {
                        try
                        {
                            Log("Applying " + (download.arch.isDelta ? "delta " : "") + "package \"" + (Name + "-" + download.version.Name + "-" + download.arch.Arch + "." + (download.arch.isDelta ? "d" : "") + "hup") + "\"...", LogLevel.Info);
                            if (download.arch.isDelta)
                            {
                                HTUGZip.ApplyDelta(WorkFolder, downloadStream);
                            }
                            else
                            {
                                HTUGZip.DecompressToDirectory(downloadStream, WorkFolder);
                            }
                            Log("Applying package \"" + (Name + "-" + download.version.Name + "-" + download.arch.Arch + "." + (download.arch.isDelta ? "d" : "") + "hup") + "\" successful.", LogLevel.Info);
                        }
                        catch (Exception ex)
                        {
                            Log("Error on package \"" + (Name + "-" + download.version.Name + "-" + download.arch.Arch + "." + (download.arch.isDelta ? "d" : "") + "hup") + "\", an error occurred on try " + (download.errorCount++) + "/" + RetryCount + ":" + ex.ToString(), LogLevel.Error);
                            if (download.errorCount < (RetryCount + 1))
                            {
                                Log("Retry installing package :" + (Name + "-" + download.version.Name + "-" + download.arch.Arch + "." + (download.arch.isDelta ? "d" : "") + "hup"), LogLevel.Info);
                                if (webC.IsBusy)
                                {
                                    webC.CancelAsync();
                                }
                                downloadAndInstall(download);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error on downloading package \"" + (Name + "-" + download.version.Name + "-" + download.arch.Arch + "." + (download.arch.isDelta ? "d" : "") + "hup") + "\", an error occurred on try " + (download.errorCount++) + "/" + RetryCount + ":" + ex.ToString(), LogLevel.Error);
                if (download.errorCount < (RetryCount + 1))
                {
                    Log("Retry installing package :" + (Name + "-" + download.version.Name + "-" + download.arch.Arch + "." + (download.arch.isDelta ? "d" : "") + "hup"), LogLevel.Info);
                    if (webC.IsBusy)
                    {
                        webC.CancelAsync();
                    }
                    downloadAndInstall(download);
                }
            }
        }

        /// <summary>
        /// Updates the packages with sync.
        /// </summary>
        /// <param name="force">Forces to do update even when it is already in the latest version.</param>
        public void DoSyncUpdate(bool force = false)
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            Log("Syncing...", LogLevel.Info);
            LoadFromUrl();
            Log("Sync complete. Starting update...", LogLevel.Info);
            if (OnBeforeUpdate != null)
            {
                Log("OnBeforeUpdate() started...", LogLevel.Info);
                OnBeforeUpdate(this, new EventArgs());
                Log("OnBeforeUpdate() ended...", LogLevel.Info);
            }
            // Determine what should be downloaded, then download & apply all.
            HTU_Download[] downloads = getDownloadList();
            string downloadList = "HTUPDATE will download these: ";
            for (int i = 0; i < downloads.Length; i++)
            {
                downloadList += (Name + "-" + downloads[i].version.Name + "-" + downloads[i].arch.Arch + "." + (downloads[i].arch.isDelta ? "d" : "") + "hup") + " ";
            }
            Log(downloadList, LogLevel.Info);
            if (force || downloads.Length > 0)
            {
                // Get backup before doing anything.
                string backupFile = System.IO.Path.Combine(TempFolder, Name + "-backup.hup");
                if (!SkipBackup)
                {
                    try
                    {
                        Log("Getting a backup...");
                        if (System.IO.File.Exists(backupFile)) { throw new Exception("Backup file \"" + backupFile + "\" already exists."); }
                        HTUGZip.CompressDirectory(WorkFolder, backupFile);
                        Log("Backup created.");
                    }
                    catch (Exception ex)
                    {
                        Log("Error on backup creation, exception caught: " + ex.ToString(), LogLevel.Critical);
                        if (!SkipBackupError)
                        {
                            return;
                        }
                        else
                        {
                            Log("Backup creation skipped." + ex.ToString(), LogLevel.Info);
                        }
                    }
                }
                else { Log("Skipped backup creation.", LogLevel.Info); }
                for (int i = 0; i < downloads.Length; i++)
                {
                    downloadAndInstall(downloads[i]);
                }
            }
            else
            {
                Log("Package already up-to-date or latest version doesn't include the current architecture.", LogLevel.Info);
            }
            if (OnAfterUpdate != null)
            {
                Log("OnAfterUpdate() started...", LogLevel.Info);
                OnAfterUpdate(this, new EventArgs());
                Log("OnAfterUpdate() ended...", LogLevel.Info);
            }
        }

        /// <summary>
        /// Updates the packages without sync.
        /// </summary>
        /// <param name="force">Forces to do update even when it is already in the latest version.</param>
        public async void DoAsyncUpdate(bool force = false)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                DoSyncUpdate();
            });
        }

        /// <summary>
        /// Event used for logging.
        /// </summary>
        /// <param name="x">Message</param>
        /// <param name="level"><see cref="LogLevel"/></param>
        public void Log(string x, LogLevel level = LogLevel.None)
        {
            if (OnLogEntry != null) { OnLogEntry(this, new OnLogEntryEventArgs(x, level)); }
        }

        /// <summary>
        /// The amount of retries after a package receives an error on download or extract.
        /// </summary>
        public int RetryCount { get; set; } = 10;

        /// <summary>
        /// Latest Version number.
        /// </summary>
        public int LatestVer { get; set; } = 1;

        /// <summary>
        /// Latest LTS version number.
        /// </summary>
        public int LatestLTSVer => LatestLTSVersion.ID;

        /// <summary>
        /// Current version number.
        /// </summary>
        public int CurrentVer { get; set; } = 1;

        /// <summary>
        /// Folder where HTUPDATE store temporary files such as downloaded packages.
        /// </summary>
        public string TempFolder { get; private set; }

        /// <summary>
        /// Current processor architecture. This architecture is going to be picked over "noarch". If this architecture doesn't exists, "noarch" packages will be installed.
        /// </summary>
        public string Arch { get; private set; }

        /// <summary>
        /// The current operating system information. Used to determine which package should be downloaded.
        /// </summary>
        public HTUPDATE_OS OperatingSystem => HTUPDATE_Default_OS.Parse(Arch);

        /// <summary>
        /// A list of nodes that are thrown away while gathering information.
        /// </summary>
        public List<XmlNode> ThrownNodes { get; set; } = new List<XmlNode>();

        /// <summary>
        /// Event raised before updating.
        /// </summary>
        public event EventHandler OnBeforeUpdate;

        /// <summary>
        /// EVent raised after updating.
        /// </summary>
        public event EventHandler OnAfterUpdate;

        /// <summary>
        /// Delegante for <see cref="OnLogEntry"/> event.
        /// </summary>
        /// <param name="sender"><see cref="HTUPDATE"/></param>
        /// <param name="e"></param>
        public delegate void OnLogEntryDelegate(object sender, OnLogEntryEventArgs e);

        public event OnLogEntryDelegate OnLogEntry;

        /// <summary>
        /// <c>true</c> to make all tasks asynchronous, otherwise <c>false</c>.
        /// </summary>
        public bool DoTaskAsAsync { get; set; }

        /// <summary>
        /// Returns <c>true</c> if it's currently checking for updates, otherwise <c>false</c>.
        /// </summary>
        public bool isCheckingForUpdates { get; set; }

        /// <summary>
        /// Checks if the packages are up to date.
        /// </summary>
        public bool isUpToDate => LatestLTSVer == CurrentVer || LatestVer == CurrentVer;

        /// <summary>
        /// Details of update error.
        /// </summary>
        public Exception UpdateError { get; set; }

        /// <summary>
        /// Codename of the HTUPDATE component.
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Folder used for working on updates by HTUPDATE.
        /// </summary>
        public string WorkFolder { get; set; }

        /// <summary>
        /// Location of the HTUPDATE file on Web.
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Versions of this product that are detected by HTUPDATE.
        /// </summary>
        public List<HTUPDATE_Version> Versions { get; set; } = new List<HTUPDATE_Version>();

        /// <summary>
        /// Gets the current version of this product.
        /// </summary>
        public HTUPDATE_Version CurrentVersion => Versions.FindAll(it => it.ID == CurrentVer).Count > 0 ? Versions.FindAll(it => it.ID == CurrentVer)[0] : null;

        /// <summary>
        /// Gets the current version of this product.
        /// </summary>
        public HTUPDATE_Version LatestVersion => Versions.FindAll(it => it.ID == LatestVer).Count > 0 ? Versions.FindAll(it => it.ID == LatestVer)[0] : null;

        /// <summary>
        /// Gets a list of LTS versions.
        /// </summary>
        public List<HTUPDATE_Version> LTSVersions => Versions.FindAll(it => it.LTS);

        /// <summary>
        /// Gets the latest LTS version.
        /// </summary>

        public HTUPDATE_Version LatestLTSVersion => LTSVersions.Count > 0 ? LTSVersions[LTSVersions.Count - 1] : null;
        /// <summary>
        /// Checks if the <paramref name="ver"/> is revoked.
        /// </summary>
        /// <param name="ver"><see cref="HTUPDATE_Version"/></param>
        /// <returns><see cref="bool"/></returns>

        public bool isLTSRevoked(HTUPDATE_Version ver)
        {
            return DateTime.Now.CompareTo(DateTime.ParseExact(ver.LTSRevokeDate, "yyyy-MM-dd", null)) > 0;
        }

        /// <summary>
        /// Name of your product.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets version by its number.
        /// </summary>
        /// <param name="v">Version number.</param>
        /// <returns><see cref="HTUPDATE_Version"/></returns>
        public HTUPDATE_Version GetVersion(int v)
        {
            HTUPDATE_Version ver = null;
            for (int i = 0; i < Versions.Count; i++)
            {
                if (Versions[i].ID == v)
                {
                    ver = Versions[i];
                    break;
                }
            }
            return ver;
        }
    }

    public class OnLogEntryEventArgs : EventArgs
    {
        public OnLogEntryEventArgs(string newLog, LogLevel level = LogLevel.None)
        {
            Level = level;
            LogEntry = newLog;
        }

        public LogLevel Level { get; internal set; }
        public string LogEntry { get; internal set; }
    }

    /// <summary>
    /// Version class for <see cref="HTUPDATE"/>.
    /// </summary>
    public class HTUPDATE_Version
    {
        /// <summary>
        /// Creates a new version with XML node.
        /// </summary>
        /// <param name="vernode">The node that stores information about this version.</param>
        /// <param name="htu"><see cref="HTUPDATE"/></param>
        public HTUPDATE_Version(XmlNode vernode, HTUPDATE htu)
        {
            if (htu == null) { throw new ArgumentNullException(nameof(htu)); }
            HTUPDATE = htu;
            if (vernode != null && vernode.ChildNodes.Count > 0)
            {
                try
                {
                    List<string> applied = new List<string>();
                    for (int i = 0; i < vernode.ChildNodes.Count; i++)
                    {
                        XmlNode node = vernode.ChildNodes[i];
                        if (!applied.Contains(node.Name.ToLowerEnglish()))
                        {
                            applied.Add(node.Name.ToLowerEnglish());
                            switch (node.Name.ToLowerEnglish())
                            {
                                case "name":
                                    Name = node.InnerXml.XmlToString();
                                    break;

                                case "id":
                                    ID = int.Parse(node.InnerXml.XmlToString());
                                    break;

                                case "flags":
                                    Flags = node.InnerXml.XmlToString().Split(';');
                                    break;

                                case "based":
                                    BasedVersion = HTUPDATE.GetVersion(int.Parse(node.InnerXml.XmlToString())) != null ? HTUPDATE.GetVersion(int.Parse(node.InnerXml.XmlToString())) : (object)int.Parse(node.InnerXml.XmlToString());
                                    break;

                                case "architectures":
                                case "archs":
                                    for (int _i = 0; _i < node.ChildNodes.Count; _i++)
                                    {
                                        XmlNode subnode = node.ChildNodes[_i];
                                        if (subnode.Name.ToLowerEnglish() == "arch")
                                        {
                                            HTUPDATE_Arch arch = new HTUPDATE_Arch(this);
                                            for (int ai = 0; ai < subnode.ChildNodes.Count; ai++)
                                            {
                                                XmlNode subsubnode = subnode.ChildNodes[ai];
                                                switch (subsubnode.Name.ToLowerEnglish())
                                                {
                                                    case "hash":
                                                        if (subsubnode.Attributes["Algorithm"] != null && !string.IsNullOrWhiteSpace(subsubnode.InnerXml))
                                                        {
                                                            HTUPDATE_Hash hash = new HTUPDATE_Hash
                                                            {
                                                                Hash = subsubnode.InnerXml.XmlToString()
                                                            };
                                                            switch (subsubnode.Attributes["Algorithm"].Value.XmlToString().ToLowerEnglish())
                                                            {
                                                                case "md5": hash.Algorithm = System.Security.Cryptography.MD5.Create(); break;
                                                                case "sha256": hash.Algorithm = System.Security.Cryptography.SHA256.Create(); break;
                                                                case "sha1": hash.Algorithm = System.Security.Cryptography.SHA1.Create(); break;
                                                                case "sha384": hash.Algorithm = System.Security.Cryptography.SHA384.Create(); break;
                                                                case "SHA512": hash.Algorithm = System.Security.Cryptography.SHA512.Create(); break;
                                                                case "hmacmd5": hash.Algorithm = System.Security.Cryptography.HMACMD5.Create(); break;
                                                                case "hmacsha1": hash.Algorithm = System.Security.Cryptography.HMACSHA1.Create(); break;
                                                                case "hmacsha256": hash.Algorithm = System.Security.Cryptography.HMACSHA256.Create(); break;
                                                                case "hmacsha384": hash.Algorithm = System.Security.Cryptography.HMACSHA384.Create(); break;
                                                                case "hmacsha512": hash.Algorithm = System.Security.Cryptography.HMACSHA512.Create(); break;
                                                            }
                                                            arch.Hashes.Add(hash);
                                                        }
                                                        break;

                                                    case "arch":

                                                        arch.Arch = subsubnode.InnerXml.XmlToString();
                                                        break;

                                                    case "url":

                                                        arch.Url = subsubnode.InnerXml.XmlToString();
                                                        break;
                                                }
                                            }
                                            arch.isDelta = BasedVersion != null;
                                            Archs.Add(arch);
                                        }
                                    }
                                    break;

                                case "lts":
                                    LTS = node.InnerXml.XmlToString() == "true";
                                    break;

                                case "ltsrevokedate":
                                    LTSRevokeDate = node.InnerXml.XmlToString();
                                    break;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Creates a new epty version.
        /// </summary>
        public HTUPDATE_Version() { }

        /// <summary>
        /// The version which this version is based on.
        /// </summary>
        public object BasedVersion { get; set; }

        /// <summary>
        /// HTUPDATE associated with this version.
        /// </summary>
        public HTUPDATE HTUPDATE { get; set; }

        /// <summary>
        /// Number of the version. This property is important.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Display name of the version. This property is important.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Flags associated with this version.
        /// </summary>
        public string[] Flags { get; set; }

        /// <summary>
        /// Determines if this version is supported until revoke time.
        /// </summary>
        public bool LTS { get; set; }

        /// <summary>
        /// Date of this Long Term Supported version's end-of-support. Must be in dd/MM/yyyy format. This propery is only important when LongTerm is true.
        /// </summary>
        public string LTSRevokeDate { get; set; }

        /// <summary>
        /// Processor architectures supported in this version. Can also include environemnt information such as Win-x86. This property is important.
        /// </summary>
        public List<HTUPDATE_Arch> Archs { get; set; } = new List<HTUPDATE_Arch>();

        /// <summary>
        /// Finds an architecture.
        /// </summary>
        /// <param name="arch">Processor Architecture.</param>
        /// <returns>A list of <see cref="HTUPDATE_Arch"/></returns>
        public List<HTUPDATE_Arch> FindArch(string arch)
        {
            return Archs.FindAll(it => it.Arch.ToLowerEnglish() == arch.ToLowerEnglish() && it.OS.isCompatible(HTUPDATE.OperatingSystem));
        }

        /// <summary>
        /// Finds a package for no-architecture.
        /// </summary>
        /// <returns><see cref="HTUPDATE_Arch"/></returns>
        public HTUPDATE_Arch FindNoArch()
        {
            return Archs.FindAll(it => it.Arch.ToLowerEnglish() == "noarch" && it.OS.isCompatible(HTUPDATE.OperatingSystem)).Count > 0 ? Archs.FindAll(it => it.Arch.ToLowerEnglish() == "noarch" && it.OS.isCompatible(HTUPDATE.OperatingSystem))[0] : null;
        }
    }

    /// <summary>
    /// HTUPDATE VErsion Architecture.
    /// </summary>
    public class HTUPDATE_Arch
    {
        /// <summary>
        /// Creates a new <see cref="HTUPDATE_Arch"/>.
        /// </summary>
        /// <param name="version"><see cref="HTUPDATE_Version"/></param>
        public HTUPDATE_Arch(HTUPDATE_Version version) : this()
        {
            if (version != null) { Version = version; } else { throw new ArgumentNullException(nameof(version)); }
        }

        /// <summary>
        /// Creates a new <see cref="HTUPDATE_Arch"/>.
        /// </summary>
        public HTUPDATE_Arch()
        {
        }

        /// <summary>
        /// Version of this architecture.
        /// </summary>
        public HTUPDATE_Version Version { get; set; }

        /// <summary>
        /// Name of the architecture.
        /// </summary>
        public string Arch { get; set; }

        /// <summary>
        /// The operating system mentioned in the <see cref="Arch"/>.
        /// </summary>
        public HTUPDATE_OS OS => HTUPDATE_Default_OS.Parse(Arch);

        /// <summary>
        /// Download location for the files
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Determines if this package hs binary changes or not.
        /// </summary>
        public bool isDelta { get; set; }

        /// <summary>
        /// A list of file hashes.
        /// </summary>
        public List<HTUPDATE_Hash> Hashes { get; set; } = new List<HTUPDATE_Hash>();

        /// <summary>
        /// Checks if a version is compatible or not.
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        public bool isCompatible()
        {
            Arch = Arch.ToLowerEnglish();
            if (Arch == "noarch")
            {
                return true;
            }
            else
            {
                if (Arch == "arm" && (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64))
                {
                    return true;
                }
                if (Arch == "arm64" && System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
                {
                    return true;
                }
                if (Arch == "x86" && (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X86 || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X64))
                {
                    return true;
                }
                if (Arch == "x86" && System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X64)
                {
                    return true;
                }
                return false;
            }
        }
    }

    /// <summary>
    /// File Hash.
    /// </summary>
    public class HTUPDATE_Hash
    {
        /// <summary>
        /// Hash of the file.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Algorithm of the hash.
        /// </summary>
        public System.Security.Cryptography.HashAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Verifys the file.
        /// </summary>
        /// <param name="stream"><see cref="System.IO.Stream"/></param>
        /// <returns><see cref="bool"/></returns>
        public bool Verify(System.IO.Stream stream)
        {
            return Tools.VerifyFile(Algorithm, stream, Hash);
        }
    }

    public static class HTUPDATE_Default_OS
    {
        public static HTUPDATE_OS Parse(string arch)
        {
            // No OS, Just processor
            if (arch.StartsWith("noarch"))
            {
                return new Any() { CurrentProcArch = "noarch" };
            }
            else if (arch.StartsWith("x86"))
            {
                return new Any() { CurrentProcArch = "x86" };
            }
            else if (arch.StartsWith("x64"))
            {
                return new Any() { CurrentProcArch = "x64" };
            }
            else if (arch.StartsWith("arm64"))
            {
                return new Any() { CurrentProcArch = "arm64" };
            }
            else if (arch.StartsWith("arm"))
            {
                return new Any() { CurrentProcArch = "arm" };
            }
            // OS
            else
            {
                string os = arch.Substring(0, arch.IndexOf('-'));
                string osinfo = arch.Replace(os + "-", "").Substring(0, os.IndexOf('-'));
                switch (os)
                {
                    default:
                        return new Any() { CurrentProcArch = osinfo };
                    // Windows
                    case "win":
                        return new Windows() { CurrentProcArch = osinfo };

                    case "win7":
                        return new Windows7() { CurrentProcArch = osinfo };

                    case "win7sp1":
                        return new Windows7SP1() { CurrentProcArch = osinfo };

                    case "win8":
                        return new Windows8() { CurrentProcArch = osinfo };

                    case "win81":
                        return new Windows81() { CurrentProcArch = osinfo };

                    case "win10":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new Windows10() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new Windows10() { UpdateVersion = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "win11":
                        return new Windows11() { CurrentProcArch = osinfo };

                    case "winserver":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new WindowsServer() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new WindowsServer() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    // Unix
                    case "mac":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new macOS() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new macOS() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "unix":
                        return new Unix() { CurrentProcArch = osinfo };

                    case "linux":
                        return new Linux() { CurrentProcArch = osinfo };

                    case "freebsd":
                        return new FreeBSD() { CurrentProcArch = osinfo };

                    case "bsd":
                        return new BSD() { CurrentProcArch = osinfo };
                    // Linux
                    case "alpine":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new Alpine() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new Alpine() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "android":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new Android() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new Android() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "arch":
                        return new ArchLinux() { CurrentProcArch = osinfo };

                    case "centos":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new CentOS() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new CentOS() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "debian":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new Debian() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new Debian() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "exherbo":
                        return new Exherbo() { CurrentProcArch = osinfo };

                    case "fedora":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new Fedora() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new Fedora() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "gentoo":
                        return new Gentoo() { CurrentProcArch = osinfo };

                    case "linuxmint":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new LinuxMint() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new LinuxMint() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "ol":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new OracleLinux() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new OracleLinux() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "opensuse":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new OpenSUSE() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new OpenSUSE() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "rhel":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new RHEL() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new RHEL() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "sles":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new SLES() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new SLES() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "solaris":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new Solaris() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new Solaris() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "tizen":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new Tizen() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new Tizen() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "void":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new VoidLinux() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new VoidLinux() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                    case "ubuntu":
                        if (osinfo == "noarch" || osinfo == "x86" || osinfo == "x64" || osinfo == "arm" || osinfo == "arm64")
                        {
                            return new Ubuntu() { CurrentProcArch = osinfo };
                        }
                        else
                        {
                            return new Ubuntu() { Version = osinfo, CurrentProcArch = arch.Replace(osinfo + "-", "").Substring(0, osinfo.IndexOf('-')) };
                        }
                }
            }
        }

        public class Any : HTUPDATE_OS
        {
            public override string Name => "Any OS";
            public override string Version => "";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return true;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        #region Unix

        public class Unix : Any
        {
            public override string Name => "Unix";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Unix;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class BSD : Unix
        {
            public override string Name => "BSD";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is BSD;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class FreeBSD : BSD
        {
            public override string Name => "FreeBSD";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is FreeBSD;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class macOS : FreeBSD
        {
            public override string Name => "macOS";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is macOS && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return (int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) > int.Parse(version.Substring(0, version.IndexOf('.') - 1))) || (int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) < int.Parse(version.Substring(0, version.IndexOf('.') - 1))) ? false : int.Parse((base.Version.Substring(base.Version.IndexOf('.') - 1)).Substring(0, base.Version.Substring(base.Version.IndexOf('.') - 1).IndexOf('.') - 1)) >= int.Parse((version.Substring(version.IndexOf('.') - 1)).Substring(0, version.Substring(version.IndexOf('.') - 1).IndexOf('.') - 1));
            }
        }

        #region GNU/Linux

        public class Linux : Unix
        {
            public override string Name => "Linux";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Linux && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return (int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) > int.Parse(version.Substring(0, version.IndexOf('.') - 1))) || (int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) < int.Parse(version.Substring(0, version.IndexOf('.') - 1))) ? false : int.Parse((base.Version.Substring(base.Version.IndexOf('.') - 1)).Substring(0, base.Version.Substring(base.Version.IndexOf('.') - 1).IndexOf('.') - 1)) >= int.Parse((version.Substring(version.IndexOf('.') - 1)).Substring(0, version.Substring(version.IndexOf('.') - 1).IndexOf('.') - 1));
            }
        }

        #region Independent Family

        public class Alpine : Linux
        {
            public override string Name => "Alpine Linux";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Alpine && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return int.Parse(Version.Replace(".", "")) >= int.Parse(version.Replace(".", ""));
            }
        }

        public class Solaris : Linux
        {
            public override string Name => "Solaris";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Solaris && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return int.Parse(Version.Replace(".", "")) >= int.Parse(version.Replace(".", ""));
            }
        }

        public class Tizen : Linux
        {
            public override string Name => "Tizen";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Tizen && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return int.Parse(Version.Replace(".", "")) >= int.Parse(version.Replace(".", ""));
            }
        }

        public class Android : Linux
        {
            public override string Name => "Android";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Android && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return int.Parse(Version.Replace(".", "")) >= int.Parse(version.Replace(".", ""));
            }
        }

        public class ArchLinux : Linux
        {
            public override string Name => "Arch Linux";
            public override string[] SupportedArchs => new string[] { "noarch", "x64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is ArchLinux;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class VoidLinux : Linux
        {
            public override string Name => "Void Linux";
            public override string[] SupportedArchs => new string[] { "noarch", "x64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is VoidLinux;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class Gentoo : Linux
        {
            public override string Name => "Gentoo";
            public override string[] SupportedArchs => new string[] { "noarch", "x64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Gentoo;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class Exherbo : Linux
        {
            public override string Name => "Exherbo";
            public override string[] SupportedArchs => new string[] { "noarch", "x64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Exherbo;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        #endregion Independent Family

        #region Red Hat Family

        public class Fedora : Linux
        {
            public override string Name => "Fedora";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Fedora && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) <= int.Parse(version.Substring(0, version.IndexOf('.') - 1)) && int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) >= int.Parse(version.Substring(0, version.IndexOf('.') - 1));
            }
        }

        public class CentOS : Fedora
        {
            public override string Name => "CentOS";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };
        }

        public class RHEL : Fedora
        {
            public override string Name => "Red hat Enterprise Linux";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };
        }

        public class OracleLinux : Fedora
        {
            public override string Name => "Oracle Linux";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };
        }

        #endregion Red Hat Family

        #region SUSE Famliy

        public class OpenSUSE : Linux
        {
            public override string Name => "OpenSUSE";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Fedora && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) <= int.Parse(version.Substring(0, version.IndexOf('.') - 1)) && int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) >= int.Parse(version.Substring(0, version.IndexOf('.') - 1));
            }
        }

        public class SLES : OpenSUSE
        {
            public override string Name => "SLES";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };
        }

        #endregion SUSE Famliy

        #region Debian Family

        public class Debian : Linux // DO NOT INCLUDE CODENAMES IN VERSION SUCH AS buster, sid etc.
        {
            public override string Name => "Debian";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Debian && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return (int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) > int.Parse(version.Substring(0, version.IndexOf('.') - 1))) || (int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) < int.Parse(version.Substring(0, version.IndexOf('.') - 1))) ? false : int.Parse((base.Version.Substring(base.Version.IndexOf('.') - 1)).Substring(0, base.Version.Substring(base.Version.IndexOf('.') - 1).IndexOf('.') - 1)) >= int.Parse((version.Substring(version.IndexOf('.') - 1)).Substring(0, version.Substring(version.IndexOf('.') - 1).IndexOf('.') - 1));
            }
        }

        public class Ubuntu : Debian
        {
            public override string Name => "Ubuntu";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Ubuntu && isCompatibleWithVersion(otherOS.Version);
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return (int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) > int.Parse(version.Substring(0, version.IndexOf('.') - 1))) || (int.Parse(base.Version.Substring(0, base.Version.IndexOf('.') - 1)) < int.Parse(version.Substring(0, version.IndexOf('.') - 1))) ? false : int.Parse((base.Version.Substring(base.Version.IndexOf('.') - 1)).Substring(0, base.Version.Substring(base.Version.IndexOf('.') - 1).IndexOf('.') - 1)) >= int.Parse((version.Substring(version.IndexOf('.') - 1)).Substring(0, version.Substring(version.IndexOf('.') - 1).IndexOf('.') - 1));
            }
        }

        public class LinuxMint : Ubuntu
        {
            public override string Name => "Linux Mint";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };
        }

        #endregion Debian Family

        #endregion GNU/Linux

        #endregion Unix

        #region Windows

        public class Windows : Any
        {
            public override string Name => "Microsoft Windows";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Windows;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class Windows7 : Windows
        {
            public override string Name => "Microsoft Windows 7";
            public override string Version => "6.1";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Windows;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class Windows7SP1 : Windows
        {
            public override string Name => "Microsoft Windows 7 SP1";
            public override string Version => "6.1sp1";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                return otherOS is Windows;
            }

            public override bool isCompatibleWithVersion(string version)
            {
                return true;
            }
        }

        public class Windows8 : Windows
        {
            public override string Name => "Microsoft Windows 8";
            public override string Version => "6.2";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                if (otherOS is Windows)
                {
                    return isCompatibleWithVersion(otherOS.Version);
                }
                else
                {
                    return false;
                }
            }

            public override bool isCompatibleWithVersion(string version)
            {
                if (version.Length > 2)
                {
                    string ver_mn = version.Substring(2);
                    if (version.StartsWith("6"))
                    {
                        return !ver_mn.StartsWith("0") && !ver_mn.StartsWith("1");
                    }
                    else if (version.StartsWith("7"))
                    {
                        return false;
                    }
                    else if (version.StartsWith("8") || version.StartsWith("10") || version.StartsWith("11"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return version.StartsWith("10") || version.StartsWith("6") || version.StartsWith("7") || version.StartsWith("8") || version.StartsWith("8.1") || version.StartsWith("11");
                }
            }
        }

        public class Windows81 : Windows8
        {
            public override string Name => "Microsoft Windows 8.1";
            public override string Version => "6.2.9200.00";
        }

        public class Windows10 : Windows
        {
            public override string Name => "Microsoft Windows 10";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public string UpdateVersion { get; set; }
            public override string Version { get => UpdateVersion; set => UpdateVersion = value.StartsWith("10.") ? value.Substring(3) : value; }

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                if (otherOS is Windows)
                {
                    return isCompatibleWithVersion(otherOS.Version);
                }
                else
                {
                    return false;
                }
            }

            public override bool isCompatibleWithVersion(string version)
            {
                if (version.Length > 2)
                {
                    string ver_mn = version.Substring(3);
                    if (version.StartsWith("10"))
                    {
                        return int.Parse(ver_mn.Replace("H", "0")) >= int.Parse(UpdateVersion.Replace("H", "0"));
                    }
                    else if (version.StartsWith("7") || version.StartsWith("6"))
                    {
                        return false;
                    }
                    else if (version.StartsWith("11"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return version.StartsWith("10") || version.StartsWith("6") || version.StartsWith("7") || version.StartsWith("8") || version.StartsWith("8.1") || version.StartsWith("11");
                }
            }
        }

        public class Windows11 : Windows
        {
            public override string Name => "Microsoft Windows 11";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public string UpdateVersion { get; set; }
            public override string Version { get => UpdateVersion; set => UpdateVersion = value.StartsWith("11.") ? value.Substring(3) : value; }

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                if (otherOS is Windows)
                {
                    return isCompatibleWithVersion(otherOS.Version);
                }
                else
                {
                    return false;
                }
            }

            public override bool isCompatibleWithVersion(string version)
            {
                if (version.Length > 2)
                {
                    string ver_mn = version.Substring(3);
                    if (version.StartsWith("10"))
                    {
                        return int.Parse(ver_mn.Replace("H", "0")) >= int.Parse(UpdateVersion.Replace("H", "0"));
                    }
                    else if (version.StartsWith("7") || version.StartsWith("6"))
                    {
                        return false;
                    }
                    else if (version.StartsWith("11"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return version.StartsWith("10") || version.StartsWith("6") || version.StartsWith("7") || version.StartsWith("8") || version.StartsWith("8.1") || version.StartsWith("11");
                }
            }
        }

        public class WindowsServer : Windows
        {
            public override string Name => "Microsoft Windows Server";
            public override string[] SupportedArchs => new string[] { "noarch", "x86", "x64", "arm", "arm64" };

            public override bool isCompatible(HTUPDATE_OS otherOS)
            {
                if (otherOS is Windows)
                {
                    return isCompatibleWithVersion(otherOS.Version);
                }
                else
                {
                    return false;
                }
            }

            public override bool isCompatibleWithVersion(string version)
            {
                if (version.Length > 2)
                {
                    string ver_mn = version.Substring(3);
                    if (version.StartsWith("10"))
                    {
                        return int.Parse(ver_mn.Replace("H", "0")) >= int.Parse(Version.Replace("H", "0"));
                    }
                    else if (version.StartsWith("7") || version.StartsWith("6"))
                    {
                        return false;
                    }
                    else if (version.StartsWith("11"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return version.StartsWith("10") || version.StartsWith("6") || version.StartsWith("7") || version.StartsWith("8") || version.StartsWith("8.1") || version.StartsWith("11");
                }
            }
        }

        #endregion Windows
    }

    public class HTUPDATE_OS
    {
        public virtual string Name { get; set; }
        public virtual string CurrentProcArch { get; set; }
        public virtual string Version { get; set; }
        public virtual string[] SupportedArchs { get; set; } = new string[] { "noarch" };

        public virtual bool isCompatible(HTUPDATE_OS otherOS)
        {
            return otherOS.Name == Name;
        }

        public virtual bool isCompatibleWithVersion(string version)
        {
            return version == Version;
        }
    }
}