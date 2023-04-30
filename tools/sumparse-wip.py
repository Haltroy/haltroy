def Main(args):
    if len(args) <= 1:
    print("Not Enough Arguments.")
    return
    output = args[0]
    hashes = []
      i = 1
    while i < len(args):
        file = args[i]
        if not File.Exists(file):
            print("Cannot find file: " + file)
            i += 1
            continue
        print("Read: " + file)
        reader = StreamReader(file)
        while not reader.EndOfStream:
            _line = reader.ReadLine()
            print("Line: \"" + _line + "\"")
            line = _line.split("  ")
            filename = line[1]
            _hash = line[0]
            hash = None
            if hashes.FindAll(lambda it : it.FileName == filename):
                hash = hashes.FindAll(lambda it : it.FileName == filename)[0]
            else:
                print("Cannot find: " + filename + " (created)")
                hash = Hash()
                hash.FileName = filename
                hashes.append(hash)
            if file.EndsWith("md5", StringComparison.InvariantCultureIgnoreCase) or file.EndsWith("md5.txt", StringComparison.InvariantCultureIgnoreCase):
                print("Add MD5 ('{0:s}') to {1:s}".format(_hash, hash.FileName))
                hash.Md5 = _hash
            if file.EndsWith("sha1", StringComparison.InvariantCultureIgnoreCase) or file.EndsWith("sha1.txt", StringComparison.InvariantCultureIgnoreCase):
                print("Add SHA1 ('{0:s}') to {1:s}".format(_hash, hash.FileName))
                hash.Sha1 = _hash
            if file.EndsWith("sha256", StringComparison.InvariantCultureIgnoreCase) or file.EndsWith("sha256.txt", StringComparison.InvariantCultureIgnoreCase):
                print("Add SHA256 ('{0:s}') to {1:s}".format(_hash, hash.FileName))
                hash.Sha256 = _hash
            if file.EndsWith("sha512", StringComparison.InvariantCultureIgnoreCase) or file.EndsWith("sha512.txt", StringComparison.InvariantCultureIgnoreCase):
                print("Add SHA512 ('{0:s}') to {1:s}".format(_hash, hash.FileName))
                hash.Sha512 = _hash
        reader.Close()
        reader.Dispose()
        i += 1
    writer = StreamWriter(output, False, System.Text.Encoding.UTF8)
    print("Write: " + output)
    for i, unusedItem in enumerate(hashes):
        hash = hashes[i]
        print("Work on: " + hash.FileName)
        _h = "**{0:s}**:{1:s}- SHA1: `{2:s}` {3:s}- SHA256: `{4:s}` {5:s}- SHA512: `{6:s}` {7:s}- MD5: `{8:s}` {9:s}".format(hash.FileName, Environment.NewLine, hash.Sha1, Environment.NewLine, hash.Sha256, Environment.NewLine, hash.Sha512, Environment.NewLine, hash.Md5, Environment.NewLine)
        writer.WriteLine(_h)
    writer.Close()
    writer.Dispose()
  class Hash:
      def __init__(self):
      self.FileName = None
      self.Sha1 = None
      self.Sha256 = None
      self.Sha512 = None
      self.Md5 = None

if __name__ == "__main__":
    main()