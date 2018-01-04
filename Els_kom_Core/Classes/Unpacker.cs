namespace Els_kom_Core.Classes
{
    /// <summary>
    /// Unpacker Class for Els_kom that unpacks KOM files.
    /// </summary>
    public class Unpacker
    {
        private static System.Collections.Generic.Dictionary<int, int> KeyMap { get; set; } = new System.Collections.Generic.Dictionary<int, int>();

        /// <summary>
        /// Makes KOM V2 Entries for unpacking.
        /// </summary>
        private static System.Collections.Generic.List<EntryVer2> Make_entries_v2(int size, System.IO.BinaryReader reader)
        {
            System.Collections.Generic.List<EntryVer2> entries = new System.Collections.Generic.List<EntryVer2>();
            //int relative_offseterr = size;
            for (int i = 0; i < size; i++)
            {
                string key;
                ReadInFile(reader, out key, 60, System.Text.Encoding.ASCII);
                int originalsize;
                ReadInFile(reader, out originalsize);
                int compressedSize;
                ReadInFile(reader, out compressedSize);
                int offset;
                ReadInFile(reader, out offset);
                //SubFiles.Add(GetSafeString(key), new KOMSubFile(this, reader, 0x3C + size * 0x48));
                var entry = new EntryVer2(GetSafeString(key), originalsize, compressedSize, offset);
                entries.Add(entry);
                //relative_offseterr += offset;
            }
            return entries;
        }

        /// <summary>
        /// Makes KOM V3 Entries for unpacking.
        /// </summary>
        private static System.Collections.Generic.List<EntryVer3> Make_entries_v3(string xmldata, int entry_count)
        {
            System.Collections.Generic.List<EntryVer3> entries = new System.Collections.Generic.List<EntryVer3>();
            var xml = System.Xml.Linq.XElement.Parse(xmldata);
            foreach (var fileElement in xml.Elements("File"))
            {
                var nameAttribute = fileElement.Attribute("Name");
                var name = nameAttribute?.Value ?? "no value";
                var sizeAttribute = fileElement.Attribute("Size");
                var size = sizeAttribute == null ? -1 : System.Convert.ToInt32(sizeAttribute.Value);
                var CompressedSizeAttribute = fileElement.Attribute("CompressedSize");
                var CompressedSize = CompressedSizeAttribute == null ? -1 : System.Convert.ToInt32(CompressedSizeAttribute.Value);
                var ChecksumAttribute = fileElement.Attribute("Checksum");
                var Checksum = ChecksumAttribute == null ? -1 : int.Parse(ChecksumAttribute.Value, System.Globalization.NumberStyles.HexNumber);
                var FileTimeAttribute = fileElement.Attribute("FileTime");
                var FileTime = FileTimeAttribute == null ? -1 : int.Parse(FileTimeAttribute.Value, System.Globalization.NumberStyles.HexNumber);
                var AlgorithmAttribute = fileElement.Attribute("Algorithm");
                var Algorithm = AlgorithmAttribute == null ? -1 : System.Convert.ToInt32(AlgorithmAttribute.Value);
                var entry = new EntryVer3(name, size, CompressedSize, Checksum, FileTime, Algorithm);
                entries.Add(entry);
            }
            return entries;
        }

        /// <summary>
        /// Decrypt XML Header data in KOM V4 for KOM V3 Entry maker works out of the box.
        /// </summary>
        private static void DecryptCRCXml(int key, ref byte[] data, int length, System.Text.Encoding encoding)
        {
            if (!KeyMap.ContainsKey(key))
                return;

            string keyStr = KeyMap[key].ToString();
            string sha1Key = System.BitConverter.ToString(new System.Security.Cryptography.SHA1CryptoServiceProvider().ComputeHash(encoding.GetBytes(keyStr))).Replace("-", "");

            BlowFish blowfish = new BlowFish(sha1Key);
            data = blowfish.Decrypt(data, System.Security.Cryptography.CipherMode.ECB);
        }

        /// <summary>
        /// Common Unpack Code for KOM V3 and KOM V4.
        /// </summary>
        private static void Kom_v3_v4_unpack(string in_path, string out_path, int version)
        {
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(in_path), System.Text.Encoding.ASCII);
            byte[] entry_count_buffer = new byte[System.Convert.ToInt32(KOM_DATA.KOM_ENTRY_COUNT_SIZE)];
            reader.BaseStream.Position += 52;
            entry_count_buffer = reader.ReadBytes(System.Convert.ToInt32(KOM_DATA.KOM_ENTRY_COUNT_SIZE));
            int compressed = 0;
            if (version > 3)
            {
                // do some reading for the CRC XML Data decompression or something?
            }
            int entry_count = System.BitConverter.ToInt32(entry_count_buffer, 0);
            byte[] file_timer_buffer = new byte[System.Convert.ToInt32(KOM_DATA.KOM_FILE_TIMER_SIZE)];
            reader.BaseStream.Position += 4;
            file_timer_buffer = reader.ReadBytes(System.Convert.ToInt32(KOM_DATA.KOM_FILE_TIMER_SIZE));
            byte[] xml_size_file_buffer = new byte[System.Convert.ToInt32(KOM_DATA.KOM_XML_SIZE_FILE_SIZE)];
            xml_size_file_buffer = reader.ReadBytes(System.Convert.ToInt32(KOM_DATA.KOM_XML_SIZE_FILE_SIZE));
            byte[] xmldatabuffer = new byte[System.BitConverter.ToInt32(xml_size_file_buffer, 0)];
            xmldatabuffer = reader.ReadBytes(System.BitConverter.ToInt32(xml_size_file_buffer, 0));
            if (version > 3)
            {
                DecryptCRCXml(compressed, ref xmldatabuffer, System.BitConverter.ToInt32(xml_size_file_buffer, 0), System.Text.Encoding.ASCII);
            }
            string xmldata = System.Text.Encoding.ASCII.GetString(xmldatabuffer);
            System.Collections.Generic.List<EntryVer3> entries = Make_entries_v3(xmldata, entry_count);
            foreach (var entry in entries)
            {
                // we iterate through every entry here and unpack the data.
                if (!System.IO.Directory.Exists(out_path))
                {
                    System.IO.Directory.CreateDirectory(out_path);
                }
                byte[] entrydata = reader.ReadBytes(entry.compressed_size);
                byte[] dec_entrydata;
                //entry.uncompressed_size
                //entry.compressed_size
                //entry.checksum
                //entry.file_time
                if (entry.algorithm == 0)
                {
                    System.IO.FileStream entryfile = System.IO.File.Create(out_path + "\\" + entry.name);
                    MessageManager.ShowInfo(entry.uncompressed_size.ToString(), "Debug!");
                    ZlibHelper.DecompressData(entrydata, out dec_entrydata, entry.uncompressed_size);
                    entryfile.Write(dec_entrydata, 0, entry.uncompressed_size);
                    entryfile.Close();
                    entryfile.Dispose();
                }
                else
                {
                    if (entry.algorithm == 3)
                    {
                        // algorithm 3 code.
                        // this possible where plugin algorithm 3 unpack support be called?
                    }
                    else
                    {
                        // algorithm 2 code.
                        // this possible where plugin algorithm 2 unpack support be called?
                    }
                    System.IO.FileStream entryfile;
                    if (entrydata.Length == entry.uncompressed_size)
                    {
                        entryfile = System.IO.File.Create(out_path + "\\" + entry.name);
                    }
                    else
                    {
                        // data was not decompressed properly so lets just dump it as is.
                        entryfile = System.IO.File.Create(out_path + "\\" + entry.name + "." + entry.uncompressed_size + "." + entry.algorithm);
                    }
                    // for now until I can decompress this crap.
                    entryfile.Write(entrydata, 0, entry.compressed_size);
                    entryfile.Close();
                    entryfile.Dispose();
                }
            }
            reader.Close();
            reader.Dispose();
        }

        /// <summary>
        /// Unpacks V4 KOM Files.
        /// Note: V4 is V3 but with a encrypted CRC XML Data.
        /// </summary>
        public static void Kom_v4_unpack(string in_path, string out_path)
        {
            Kom_v3_v4_unpack(in_path, out_path, 4);
        }

        /// <summary>
        /// Unpacks V3 KOM Files.
        /// </summary>
        public static void Kom_v3_unpack(string in_path, string out_path)
        {
            Kom_v3_v4_unpack(in_path, out_path, 3);
        }

        /// <summary>
        /// Unpacks V2 KOM Files.
        /// </summary>
        public static void Kom_v2_unpack(string in_path, string out_path)
        {

            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(in_path), System.Text.Encoding.ASCII);
            reader.BaseStream.Position = 56;
            int size;
            ReadInFile(reader, out size);
            //System.Collections.Generic.List<EntryVer2> entries = Make_entries_v2(0x3C + size * 0x48, reader);
            // 0x3C + size and 0x3C + size * 0x48 seems to both overflow the actual entry count?
            // Need a better way for getting the entry count here.
            System.Collections.Generic.List<EntryVer2> entries = Make_entries_v2(0x3C + size, reader);
            foreach (var entry in entries)
            {
                // iterate through every item in KOM V2.
                MessageManager.ShowInfo("Entry File Name: " + entry.name, "Debug!");
                MessageManager.ShowInfo("Entry File Original Size: " + entry.uncompressed_size.ToString(), "Debug!");
                MessageManager.ShowInfo("Entry File Compressed Size Size: " + entry.compressed_size.ToString(), "Debug!");
                //MessageManager.ShowInfo(entry.relative_offset.ToString(), "Debug!");
            }
            reader.Close();
            reader.Dispose();
        }

        private static string GetSafeString(string source)
        {
            if (source.Contains(new string(char.MinValue, 1)))
                return source.Substring(0, source.IndexOf(char.MinValue));

            return source;
        }

        internal static bool ReadInFile(System.IO.BinaryReader binaryReader, out string destString, int length, System.Text.Encoding encoding)
        {
            long position = binaryReader.BaseStream.Position;
            byte[] readBytes = binaryReader.ReadBytes(length);
            if ((binaryReader.BaseStream.Position - position) == length)
            {
                destString = encoding.GetString(readBytes);
                return true;
            }
            destString = null;
            return false;
        }

        internal static bool ReadInFile(System.IO.BinaryReader binaryReader, out int destInt)
        {
            long position = binaryReader.BaseStream.Position;
            int readInt = binaryReader.ReadInt32();
            if ((binaryReader.BaseStream.Position - position) == sizeof(int))
            {
                destInt = readInt;

                return true;
            }
            destInt = int.MinValue;
            return false;
        }
    }
}
