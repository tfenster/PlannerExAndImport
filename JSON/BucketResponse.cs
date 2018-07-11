using System;
using System.Collections.Generic;

using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
namespace PlannerExAndImport.JSON {
    public partial class BucketResponse
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.count")]
        public long OdataCount { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        [JsonProperty("value")]
        public Bucket[] Buckets { get; set; }
    }

    public partial class Bucket
    {
        [JsonIgnoreSerialization]
        [JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("orderHint")]
        public string OrderHint { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonIgnoreSerialization]
        public PlannerTask[] Tasks { get; set; }
        /*
        public override HttpContent GetContent()
        {
            HttpContent content = new StringContent(
        }*/
    }
}