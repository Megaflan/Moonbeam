using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Moonbeam
{
    class Program
    {
        static void Main(string[] args)
        {
            MBM.LoadDictionary(File.ReadAllLines("dic.txt"));
            PO PO = new PO();
            switch (args[0])
            {
                case "-e":
                    switch (Path.GetExtension(args[1]).ToLower())
                    {
                        case ".mbm":
                            File.WriteAllText(args[1] + ".xml", MBM.FromByteArray(File.ReadAllBytes(args[1])).ToXElement().ToString());
                            break;
                        default:
                            try
                            {
                                foreach (var filefound in Directory.GetFiles(args[1], "*.mbm", SearchOption.AllDirectories))
                                {
                                    File.WriteAllText(filefound + ".xml", MBM.FromByteArray(File.ReadAllBytes(filefound)).ToXElement().ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ERROR");
                                Console.WriteLine(ex);
                                Console.ReadLine();
                            }
                            //recursive search
                            break;
                    }
                    break;
                case "-i":
                    switch (Path.GetExtension(args[1]).ToLower())
                    {
                        case ".xml":
                            File.WriteAllBytes(args[1] + ".mbm", MBM.FromXElement(XElement.Load(args[1])).ToByteArray());
                            break;
                        default:
                            try
                            {
                                foreach (var filefound in Directory.GetFiles(args[1], "*.xml", SearchOption.AllDirectories))
                                {
                                    File.WriteAllBytes(filefound + ".mbm", MBM.FromXElement(XElement.Load(filefound)).ToByteArray());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ERROR");
                                Console.WriteLine(ex);
                                Console.ReadLine();
                            }
                            //recursive search
                            break;
                    }
                    break;
                case "-v":
                    var root = new XElement("mbms", Directory.GetFiles(args[1], "*.mbm", SearchOption.AllDirectories)
                        .Select(path => new XElement("mbm", new XAttribute("path", path),
                        from entry in MBM.FromByteArray(File.ReadAllBytes(path))
                        let idattr = new XAttribute("id", entry.Id)
                        select new XElement("entry", idattr, entry.Text))));
                    File.WriteAllText(args[1] + ".xml", root.ToString());
                    break;
                case "-xml2po":                    
                    switch (Path.GetExtension(args[1]).ToLower())
                    {
                        case ".xml":
                            PO.XML2PO(args[1]);
                            break;
                        default:
                            foreach (var filefound in Directory.GetFiles(args[1], "*.xml", SearchOption.AllDirectories))
                                PO.XML2PO(filefound);
                            break;
                    }
                    break;
                case "-po2xml":
                    switch (Path.GetExtension(args[1]).ToLower())
                    {
                        case ".po":
                            File.WriteAllText(args[1].Replace(".po", ".xml"), PO.PO2XML(args[1]).ToString());
                            break;
                        default:
                            foreach (var filefound in Directory.GetFiles(args[1], "*.po", SearchOption.AllDirectories))
                                File.WriteAllText(filefound.Replace(".po", ".xml"), PO.PO2XML(filefound).ToString());
                            break;
                    }
                    break;
            }
            
        }

        // Enter a root directory to test all MBMs in the directory
        static void UnitTest(string rootDirectory)
        {
            foreach (var path in Directory.GetFiles(rootDirectory, "*.mbm", SearchOption.AllDirectories))
            {
                Console.WriteLine(path);
                var original = File.ReadAllBytes(path);
                var modified = MBM.ToByteArray(MBM.FromXElement(MBM.ToXElement(MBM.FromByteArray(original))));
                if (!original.SequenceEqual(modified)) throw new Exception("Byte sequences are not the same");

                System.Diagnostics.Debug.WriteLine(MBM.FromByteArray(original).ToXElement());
            }
        }
    }
}
