namespace Els_kom_Core.Classes
{
    /// <summary>
    /// Unpacker Class for Els_kom that unpacks KOM files.
    /// </summary>
    public class Unpacker
    {
        /// <summary>
        /// Makes KOM V3 Entries for unpacking.
        /// </summary>
        private static System.Tuple<System.Collections.Generic.List<EntryVer3>, int> Make_entries_v3(string xmldata, int entry_count)
        {
            System.Collections.Generic.List<EntryVer3> entries = new System.Collections.Generic.List<EntryVer3>();
            int relative_offseterr = 0;
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
                var entry = new EntryVer3(name, size, CompressedSize, Checksum, relative_offseterr, FileTime, Algorithm);
                entries.Add(entry);
                relative_offseterr += entry.compressed_size;
            }
            return System.Tuple.Create(entries, relative_offseterr);
        }

        /// <summary>
        /// Unpacks V4 KOM Files.
        /// Note: V4 is V3 but with a encrypted CRC XML Data.
        /// </summary>
        public static void Kom_v4_unpack(string in_path, string out_path)
        {
            // not implemented yet due to lack of information on v4 koms.
        }

        /// <summary>
        /// Unpacks V3 KOM Files.
        /// </summary>
        public static void Kom_v3_unpack(string in_path, string out_path)
        {
            // not fully implemented yet due to crash in the actual testing code.
            /*
            if you dont understand why we dont do the header check here,
            the check is actually in the KOM Manager that invokes these packers / unpackers anyway.
            and would be redundant to have here as well so they are left out.
            */
            int offset = 0;
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(in_path), System.Text.Encoding.ASCII);
            byte[] entry_count_buffer = new byte[System.Convert.ToInt32(KOM_DATA.KOM_ENTRY_COUNT_SIZE)];
            offset += 52;
            reader.BaseStream.Position += 52;
            entry_count_buffer = reader.ReadBytes(System.Convert.ToInt32(KOM_DATA.KOM_ENTRY_COUNT_SIZE));
            int entry_count = System.BitConverter.ToInt32(entry_count_buffer, 0);
            //MessageManager.ShowInfo(entry_count.ToString(), "Debug!");
            offset += 12;
            byte[] file_timer_buffer = new byte[System.Convert.ToInt32(KOM_DATA.KOM_FILE_TIMER_SIZE)];
            reader.BaseStream.Position += 4;
            file_timer_buffer = reader.ReadBytes(System.Convert.ToInt32(KOM_DATA.KOM_FILE_TIMER_SIZE));
            //MessageManager.ShowInfo(System.BitConverter.ToInt32(file_timer_buffer, 0).ToString(), "Debug!");
            byte[] xml_size_file_buffer = new byte[System.Convert.ToInt32(KOM_DATA.KOM_XML_SIZE_FILE_SIZE)];
            offset += 4;
            xml_size_file_buffer = reader.ReadBytes(System.Convert.ToInt32(KOM_DATA.KOM_XML_SIZE_FILE_SIZE));
            //MessageManager.ShowInfo(System.BitConverter.ToInt32(xml_size_file_buffer, 0).ToString(), "Debug!");
            offset += 4;
            byte[] xmldatabuffer = new byte[System.BitConverter.ToInt32(xml_size_file_buffer, 0)];
            xmldatabuffer = reader.ReadBytes(System.BitConverter.ToInt32(xml_size_file_buffer, 0));
            string xmldata = System.Text.Encoding.UTF8.GetString(xmldatabuffer);
            //System.IO.StreamWriter debugfile = System.IO.File.CreateText(System.Windows.Forms.Application.StartupPath + "\\debug.log");
            //debugfile.Write(xmldata);
            //debugfile.Close();
            //debugfile.Dispose();
            System.Tuple<System.Collections.Generic.List<EntryVer3>, int> entries = Make_entries_v3(xmldata, entry_count);
            foreach (var entry in entries.Item1)
            {
                // we iterate through every entry here and unpack the data.
                if (!System.IO.Directory.Exists(out_path))
                {
                    System.IO.Directory.CreateDirectory(out_path);
                }
                System.IO.FileStream entryfile = System.IO.File.Create(out_path + "\\" + entry.name);
                byte[] entrydata = reader.ReadBytes(entry.compressed_size);
                //entry.uncompressed_size
                //entry.compressed_size
                //entry.checksum
                //entry.relative_offset
                //entry.file_time
                if (entry.algorithm == 0)
                {
                    // TODO: Use Embeded Zlib stuff.
                    entryfile.Write(entrydata, 0, entry.compressed_size);
                }
                else if (entry.algorithm == 2)
                {
                    // algorithm 3 code.

                    // for now until I can decompress this crap.
                    entryfile.Write(entrydata, 0, entry.compressed_size);
                }
                else if (entry.algorithm == 3)
                {
                    // algorithm 3 code.

                    // for now until I can decompress this crap.
                    entryfile.Write(entrydata, 0, entry.compressed_size);
                }
                entryfile.Close();
                entryfile.Dispose();
            }
            reader.Close();
            reader.Dispose();
        }

        /// <summary>
        /// Unpacks V2 KOM Files.
        /// </summary>
        public static void Kom_v2_unpack(string in_path, string out_path)
        {
            // not implemented yet due to lack of information on v4 koms.
        }
    }
}
