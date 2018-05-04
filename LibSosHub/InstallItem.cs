using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LibSosHub
{
    public class InstallItem
    {
        public string Name { get; set; }
        public string DownloadUrl { get; set; }
        public string Description { get; set; }
        public string Sponsor { get; set; }
        public ContentType ContentType { get; set; }
        public int ContentVersion { get; set; }
        public string AuxData { get; set; }
        public string CommandLineArgs { get; set; }
        public string Reserved { get; set; }
        [JsonIgnore]
        public byte[] Payload { get; set; }
        [JsonIgnore]
        public byte[] SponsorBanner { get; set; }
        public string PayloadName { get; set; }
        public string SponsorBannerName { get; set; }
    }
}
