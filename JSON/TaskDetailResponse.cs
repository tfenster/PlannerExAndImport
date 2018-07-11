using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
namespace PlannerExAndImport.JSON
{
    public partial class TaskDetailResponse
    {
        [JsonIgnoreSerialization]
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("previewType")]
        public string PreviewType { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("references")]
        public Dictionary<string, Reference> References { get; set; }

        [JsonProperty("checklist")]
        public Dictionary<string, Checklist> Checklist { get; set; }
    }

    public partial class Checklist
    {
        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }

        [JsonProperty("isChecked")]
        public bool IsChecked { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("orderHint")]
        public string OrderHint { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("lastModifiedDateTime")]
        public DateTimeOffset LastModifiedDateTime { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("lastModifiedBy")]
        public LastModifiedBy LastModifiedBy { get; set; }
    }

    public partial class LastModifiedBy
    {
        [JsonProperty("user")]
        public User User { get; set; }
    }

    public partial class User
    {
        [JsonProperty("displayName")]
        public object DisplayName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Reference
    {
        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("previewPriority")]
        public string PreviewPriority { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("lastModifiedDateTime")]
        public DateTimeOffset LastModifiedDateTime { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("lastModifiedBy")]
        public LastModifiedBy LastModifiedBy { get; set; }
    }
}
