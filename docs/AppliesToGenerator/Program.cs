using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace DocFXAppliesToGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write("Missing api list input");
                return 1;
            }
            var output = args[0];
            var fi = new FileInfo(output);
            if(!fi.Exists)
            {
                Console.Write("File not found: " + output);
                return 2;
            }

            var settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(AppliesToDataModel), settings);
            var list = (AppliesToDataModel)serializer.ReadObject(File.OpenRead(fi.FullName));

            var result = BuildApiList(list, fi.Directory.FullName);
            var outfile = new FileInfo(Path.Combine(fi.Directory.FullName, list.Output));
            if (!outfile.Directory.Exists) outfile.Directory.Create();
            using (StreamWriter sw = new StreamWriter(outfile.FullName))
            {
                foreach (var item in result.Values)
                {
                    sw.WriteLine("---");
                    sw.WriteLine($"uid: {item.Id}");
                    sw.WriteLine($"appliesTo:");
                    foreach(var p in item.AppliesTo.GroupBy(p=>p.Platform))
                    {
                        sw.WriteLine($"  - platform: {p.Key}");
                        if (p.Where(t => !string.IsNullOrEmpty(t.Version)).Any())
                            sw.WriteLine($"    versions: {string.Join(", ", p.Select(t => t.Version))}");
                    }
                    sw.WriteLine("---");
                }
            }
            return 0;
        }

        private static Dictionary<string, Api> BuildApiList(AppliesToDataModel manifestFolder, string rootFolder)
        {
            Dictionary<string, Api> apilist = new Dictionary<string, Api>();

            foreach(var platform in manifestFolder.Metadata)
            {
                foreach (var manifest in platform.Versions)
                {
                    var file = Path.Combine(rootFolder, manifest.Manifest);
                    if (File.Exists(file))
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            while (true)
                            {
                                var line = sr.ReadLine()?.Trim();
                                if (line == null)
                                    break;

                                if (line.StartsWith('"'))
                                {
                                    var name = line.Substring(1, line.IndexOf("\":") - 1);
                                    if (!apilist.ContainsKey(name))
                                        apilist[name] = new Api() { Id = name };
                                    apilist[name].AppliesTo.Add(new AppliesTo() { Platform = platform.PlatformName, Version = manifest.Name });
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Manifest file not found: " + file);
                    }
                }
            }
            return apilist;
        }

        public class Api
        {
            public string Id { get; set; }
            public List<AppliesTo> AppliesTo { get; } = new List<AppliesTo>();
        }
        public class AppliesTo
        {
            public string Version { get; set; }
            public string Platform { get; set; }
        }

        [DataContract]
        public class AppliesToDataModel
        {
            [DataMember]
            public string Output { get; set; }
            [DataMember]
            public Metadata[] Metadata { get; set; }
        }
        
        [DataContract]
        public class Metadata
        {
            [DataMember]
            public string PlatformName { get; set; }
            [DataMember]
            public ManifestVersion[] Versions { get; set; }
        }

        [DataContract]
        public class ManifestVersion
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public string Manifest { get; set; }
        }
    }
}
