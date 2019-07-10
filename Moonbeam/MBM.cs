using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Moonbeam
{
    static class MBM
    {
        public class Entry
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        static Encoding sjis = Encoding.GetEncoding("sjis");
        static Dictionary<char, char> dicFW2HW, dicHW2FW;
        static string Transform(string str, Dictionary<char, char> dic) => string.Concat(str.Select(c => dic.TryGetValue(c, out var d) ? d : c));

        public static void LoadDictionary(params string[] source)
        {
            var pairs = source.Where(s => s.Length == 2).ToList();
            dicFW2HW = pairs.ToDictionary(p => p[0], p => p[1]);
            dicHW2FW = pairs.ToDictionary(p => p[1], p => p[0]);
        }

        public static List<Entry> FromByteArray(byte[] bytes)
        {
            using (var br = new BinaryReader(new MemoryStream(bytes)))
            {
                br.ReadBytes(16);
                var entryCount = br.ReadInt32();
                br.ReadBytes(12);
                var entries = Enumerable.Range(0, 9999)
                              .Select(_ => (id: br.ReadInt32(), size: br.ReadInt32(), offset: (int)br.ReadInt64()))
                              .Where(x => x.offset != 0)
                              .Take(entryCount)
                              .ToList();
                return entries.Select(entry => new Entry { Id = entry.id, Text = ConvertBytesToString(br.ReadBytes(entry.size)) }).ToList();
            }
        }

        public static byte[] ToByteArray(this IEnumerable<Entry> entries)
        {
            var lst = entries.Select(entry => (id: entry.Id, bytes: ConvertStringToBytes(entry.Text))).ToList();

            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                // Write header
                bw.Write(0);
                bw.Write(0x3247534D); // MSG2
                bw.Write(0x10000);
                bw.Write(lst.Sum(x => x.bytes.Length + 16) + 32);
                bw.Write(lst.Count);
                bw.Write(0x20);
                bw.Write(0L);

                // Write entry table
                int prevId = -1;
                var stringOffset = lst.LastOrDefault().id * 16 + 48L;
                foreach (var (id, bytes) in lst)
                {
                    bw.Write(new byte[16 * (id - prevId - 1)]);
                    bw.Write(id);
                    bw.Write(bytes.Length);
                    bw.Write(stringOffset);

                    prevId = id;
                    stringOffset += bytes.Length;
                }

                // Write string data
                foreach (var (id, bytes) in lst)
                    bw.Write(bytes);
            }
            return ms.ToArray();
        }

        public static List<Entry> FromXElement(XElement root)
        {
            return root.Elements("entry").Select(el => new Entry { Id = (int)el.Attribute("id"), Text = el.Element("target").Value }).ToList();
        }

        public static XElement ToXElement(this IEnumerable<Entry> entries)
        {
            return new XElement("mbm", from entry in entries
                                       let idattr = new XAttribute("id", entry.Id)
                                       let source = new XElement("source", entry.Text)
                                       let target = new XElement("target", entry.Text)
                                       select new XElement("entry", idattr, source, target));
        }

        // Very basic control code parsing
        static int[] GetCodeSizes(byte byte0, byte byte1)
        {
            if (byte0 == 0x80)
            {
                switch (byte1)
                {
                    case 1: case 2: case 0x12: case 0x16: case 0x17: case 0x18: case 0xFD: case 0xFE: return new int[0];
                    default: return new[] { 1 };
                }
            }
            else
            {
                switch (byte1)
                {
                    case 1: case 2: case 0x12: case 0x16: case 0x17: case 0x18: return new int[0];
                    case 0x14: case 0x7C: return new[] { 1, 1 };
                    case 0x13: return new[] { 2, 2 };
                    case 0x7B: return new[] { 1, 1, 1, 1 };
                    default: return new[] { 1 };
                }
            }
        }

        static byte[] ConvertStringToBytes(string str)
        {
            return Regex.Split(str.Replace(@"\0", "{0000}") + "{FFFF}", "{([0-9A-F]{4}(?:,-?[0-9]+)*)}")
                .SelectMany((s, i) =>
                {
                    if (i % 2 == 0) return sjis.GetBytes(Transform(s, dicHW2FW));
                    var byte0 = Convert.ToByte(s.Substring(0, 2), 16);
                    var byte1 = Convert.ToByte(s.Substring(2, 2), 16);
                    var sizes = GetCodeSizes(byte0, byte1);
                    return new[] { byte0, byte1 }.Concat(s.Split(',').Skip(1).Select(int.Parse).Zip(sizes, (num, size) =>
                        size == 1 ? BitConverter.GetBytes((short)num) : BitConverter.GetBytes(num)).SelectMany(x => x));
                }).ToArray();
        }

        static string ConvertBytesToString(byte[] bytes)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bytes.Length / 2 - 1; i++)
            {
                var byte0 = bytes[2 * i];

                if (byte0 == 0x80 || byte0 == 0xf8)
                {
                    var byte1 = bytes[2 * i + 1];
                    sb.Append($"{{{byte0:X2}{byte1:X2}");
                    foreach (var size in GetCodeSizes(byte0, byte1))
                    {
                        sb.Append($",{(size == 1 ? BitConverter.ToInt16(bytes, 2 * i + 2) : BitConverter.ToInt32(bytes, 2 * i + 2))}");
                        i += size;
                    }
                    sb.Append("}");
                }
                else
                {
                    sb.Append(byte0 == 0 ? @"\0" : Transform(sjis.GetString(bytes, 2 * i, 2), dicFW2HW));
                }
            }
            return sb.ToString();
        }
    }
}
