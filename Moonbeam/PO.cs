using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Yarhl.FileFormat;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace Moonbeam
{
    class PO
    {
        Po POHeader = new Po
        {
            Header = new PoHeader("Shin Megami Tensei IV", "smtivesp@gmail.com", "es")
            {
                LanguageTeam = "TraduSquare (SMTIVesp)",
            }
        };

        public void XML2PO(string file)
        {
            XElement XML = XElement.Load(file);
            int c = 0;
            foreach (XElement entry in XML.Descendants("entry"))
            {
                string source = entry.Nodes().ElementAt(0).ToString();
                string target = entry.Nodes().ElementAt(1).ToString();
                POExport(source.Replace("<source>", "").Replace("</source>", ""), target.Replace("<target>", "").Replace("</target>", ""), c);
                c++;
            }
            POWrite(file.Replace(".xml", ""));
        }

        public XElement PO2XML(string file)
        {
            var POBuffer = new BinaryFormat(new DataStream(file, FileOpenMode.Read)).ConvertTo<Po>();
            return new XElement("mbm", from entry in POBuffer.Entries
                                       let idattr = new XAttribute("id", entry.Context)
                                       let source = new XElement("source", entry.Original)
                                       let target = new XElement("target", entry.Text)
                                       select new XElement("entry", idattr, source, target));
        }

        public void POExport(string source, string target, int i)
        {
            POHeader.Add(new PoEntry(source) { Translated = target, Context = i.ToString() });
        }

        public void POWrite(string file)
        {
            POHeader.ConvertTo<BinaryFormat>().Stream.WriteTo(file + ".po");
        }

    }
}
