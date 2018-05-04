using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using LibSosHub;

namespace SosHubDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            // This is a quick hacky thing, message me if you want an actual interface for it made

            bool doBuild = false;

            if (!doBuild)
            {
                string inPath = @"";
                string outPath = Path.ChangeExtension(inPath, null);
                Directory.CreateDirectory(outPath);

                List<InstallItem> items = SosHubManip.LoadInstallItems(inPath);
                for (int i = 0; i < items.Count; ++i)
                {
                    var item = items[i];
                    if (item.Payload != null && item.Payload.Length > 0)
                    {
                        item.PayloadName = SosHubManip.MakePayloadName(i, item.ContentType, 0);
                        File.WriteAllBytes(Path.Combine(outPath, item.PayloadName), item.Payload);
                    }
                    if (item.SponsorBanner != null && item.SponsorBanner.Length > 0)
                    {
                        item.SponsorBannerName = SosHubManip.MakePayloadName(i, item.ContentType, 1);
                        File.WriteAllBytes(Path.Combine(outPath, item.SponsorBannerName), item.SponsorBanner);
                    }
                }
                File.WriteAllText(Path.Combine(outPath, "items.json"), JsonConvert.SerializeObject(items, Formatting.Indented));
            }
            else
            {
                string stubPath = @"";
                string inPath = @"";
                string outPath = @"";

                List<InstallItem> items = JsonConvert.DeserializeObject<List<InstallItem>>(File.ReadAllText(Path.Combine(inPath, "items.json")));
                SosHubManip.BuildHub(stubPath, items, inPath, outPath);
            }
        }
    }
}
