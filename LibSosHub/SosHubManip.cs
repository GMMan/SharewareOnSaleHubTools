using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LibSosHub
{
    public static class SosHubManip
    {
        // http://delphidabbler.com/articles?article=7
        static readonly byte[] Watermark = new Guid("9FABA105-EDA8-45C3-89F4-369315A947EB").ToByteArray();

        public static List<InstallItem> LoadInstallItems(string compiledPath)
        {
            using (FileStream fs = File.OpenRead(compiledPath))
            {
                fs.Seek(-Watermark.Length, SeekOrigin.End);
                BinaryReader br = new BinaryReader(fs);
                long pos = PatternSearchEnd(br, Watermark);
                if (pos == -1) return null;

                fs.Seek(pos + Watermark.Length, SeekOrigin.Begin);
                int exeSize = br.ReadInt32();
                int dataSize = br.ReadInt32();
                fs.Seek(exeSize, SeekOrigin.Begin);

                List<InstallItem> items = new List<InstallItem>();
                int count = br.ReadInt32();
                int chunksLengthRead = 4;
                for (int i = 0; i < count; ++i)
                {
                    InstallItem chunk = new InstallItem();
                    int chunkLength = br.ReadInt32();
                    long chunkHeadPos = fs.Position;

                    chunk.Name = ReadUnicodeString(br);
                    chunk.DownloadUrl = ReadUnicodeString(br);
                    chunk.Description = ReadUnicodeString(br);
                    chunk.Sponsor = ReadUnicodeString(br);
                    chunk.ContentType = (ContentType)br.ReadByte();
                    chunk.ContentVersion = br.ReadInt32();
                    chunk.AuxData = ReadUnicodeString(br);
                    chunk.CommandLineArgs = ReadUnicodeString(br);
                    chunk.Reserved = ReadUnicodeString(br);
                    chunk.Payload = ReadBuffer(br);
                    chunk.SponsorBanner = ReadBuffer(br);

                    if (fs.Position - chunkHeadPos != chunkLength) throw new InvalidDataException(string.Format("Chunk {0} length mismatch", i));
                    items.Add(chunk);
                    chunksLengthRead += chunkLength;
                }

                if (chunksLengthRead != dataSize) throw new InvalidDataException("Total chunks size mismatch");
                return items;
            }
        }

        public static void BuildHub(string stubPath, List<InstallItem> items, string workingDir, string outputPath)
        {
            using (FileStream fs = File.Create(outputPath))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                using (FileStream stub = File.OpenRead(stubPath))
                {
                    stub.CopyTo(fs);
                }
                int dataOffset = (int)fs.Position;

                bw.Write(items.Count);
                foreach (var item in items)
                {
                    long chunkStart = fs.Position;
                    bw.Write(0); // dummy length
                    WriteUnicodeString(bw, item.Name);
                    WriteUnicodeString(bw, item.DownloadUrl);
                    WriteUnicodeString(bw, item.Description);
                    WriteUnicodeString(bw, item.Sponsor);
                    bw.Write((byte)item.ContentType);
                    bw.Write(item.ContentVersion);
                    WriteUnicodeString(bw, item.AuxData);
                    WriteUnicodeString(bw, item.CommandLineArgs);
                    WriteUnicodeString(bw, item.Reserved);

                    if (!string.IsNullOrEmpty(item.PayloadName) && item.Payload == null)
                    {
                        byte[] data = File.ReadAllBytes(Path.Combine(workingDir, item.PayloadName));
                        WriteBuffer(bw, data);
                    }
                    else
                    {
                        WriteBuffer(bw, item.Payload);
                    }

                    if (!string.IsNullOrEmpty(item.SponsorBannerName) && item.SponsorBanner == null)
                    {
                        byte[] data = File.ReadAllBytes(Path.Combine(workingDir, item.SponsorBannerName));
                        WriteBuffer(bw, data);
                    }
                    else
                    {
                        WriteBuffer(bw, item.SponsorBanner);
                    }

                    int chunkSize = (int)(fs.Position - chunkStart - 4);
                    fs.Seek(chunkStart, SeekOrigin.Begin);
                    bw.Write(chunkSize);
                    fs.Seek(chunkSize, SeekOrigin.Current);
                }

                int dataSize = (int)fs.Position - dataOffset - 4 * items.Count; // Item length field is not counted???
                bw.Write(Watermark);
                bw.Write(dataOffset);
                bw.Write(dataSize);
            }
        }

        public static string MakePayloadName(int index, ContentType type, int member)
        {
            switch (member)
            {
                case 0:
                    return string.Format(type == ContentType.Notifier ? "notifier{0}.zip" : "image{0}.png", index);
                case 1:
                    return string.Format("banner{0}.bin", index);
                default:
                    throw new ArgumentOutOfRangeException("member");
            }
        }

        static void WriteUnicodeString(BinaryWriter bw, string s)
        {
            if (s == null)
            {
                bw.Write(0);
                return;
            }

            bw.Write(s.Length);
            bw.Write(Encoding.Unicode.GetBytes(s));
        }

        static void WriteBuffer(BinaryWriter bw, byte[] buf)
        {
            if (buf == null)
            {
                bw.Write(0);
                return;
            }

            bw.Write(buf.Length);
            bw.Write(buf);
        }

        static string ReadUnicodeString(BinaryReader br)
        {
            int len = br.ReadInt32();
            if (len == 0) return null;
            byte[] buf = br.ReadBytes(2 * len);
            return Encoding.Unicode.GetString(buf);
        }

        static byte[] ReadBuffer(BinaryReader br)
        {
            int len = br.ReadInt32();
            if (len == 0) return null;
            return br.ReadBytes(len);
        }

        static long PatternSearchEnd(BinaryReader br, byte[] target)
        {
            byte buffer;
            int pos = 0;

            try
            {
                do
                {
                    buffer = br.ReadByte();
                    if (buffer != target[pos])
                    {
                        br.BaseStream.Seek(-pos - 2, SeekOrigin.Current);
                        if (br.BaseStream.Position < 0) return -1;
                        pos = 0;
                        // byte may not match the current target byte, but it may match the first target byte
                    }
                    if (buffer == target[pos]) ++pos;
                }
                while (pos != target.Length);
            }
            catch (EndOfStreamException)
            {
                return -1;
            }

            return br.BaseStream.Position - target.Length;
        }
    }
}
