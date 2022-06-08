using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonDiffPatchDotNet;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using System.Linq;
using System.Reflection.PortableExecutable;

namespace MakeJSONPatch
{
    public class Program
    {
        public struct Config
        {
            public string Name;
            public string DB_Old;
            public string DB_New;
            public string index;
            public string repoName;
            public string branch;
        }
        public static void Main(string[] args)
        {
            ConvertJson(args[0]);
        }
        public static void ConvertJson(string cn)
        {
            var conf = JArray.Parse(File.ReadAllText("config.json")).ToObject<List<Config>>();
            var jsed = new JsonDiffPatch(new Options()
            {
                ArrayDiff = ArrayDiffMode.Efficient,
                TextDiff = TextDiffMode.Efficient,
                DiffBehaviors = DiffBehavior.None,
                ExcludePaths = new List<string>() { "/DBPatchVer" }
            });
            var cfg = conf.Where(x => x.Name == cn).First();
            var Dir = new FileInfo(cfg.index).DirectoryName;
            var DBOld = JObject.Parse(File.ReadAllText(cfg.DB_Old));
            var DBConv = JObject.Parse(File.ReadAllText(cfg.DB_New));
            var patchIndex = JArray.Parse(File.ReadAllText(cfg.index));
            var Formatter = new JsonDeltaFormatter();
            var diff = Formatter.Format(jsed.Diff(DBOld, DBConv));
            File.WriteAllText(cfg.DB_Old, JsonConvert.SerializeObject(DBConv, Formatting.Indented));
            File.WriteAllText(Path.Combine(Dir, $"{patchIndex.Count}.json"), JsonConvert.SerializeObject(diff, Formatting.Indented));
            patchIndex.Add($"https://raw.githubusercontent.com/{cfg.repoName}/{cfg.branch}/{cfg.Name}/{patchIndex.Count}.json");
            File.WriteAllText(cfg.index, JsonConvert.SerializeObject(patchIndex, Formatting.Indented));
        }
    }
}