using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
namespace PlannerExAndImport.JSON
{
    
    public partial class TaskResponse
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.count")]
        public long OdataCount { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        [JsonProperty("value")]
        public PlannerTask[] Tasks { get; set; }
    }

    public partial class PlannerTask
    {
        [JsonIgnoreSerialization]
        [JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("bucketId")]
        public string BucketId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("orderHint")]
        public string OrderHint { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("assigneePriority")]
        public string AssigneePriority { get; set; }

        [JsonProperty("percentComplete")]
        public long PercentComplete { get; set; }

        [JsonProperty("startDateTime")]
        public object StartDateTime { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("createdDateTime")]
        public DateTimeOffset CreatedDateTime { get; set; }

        [JsonProperty("dueDateTime")]
        public object DueDateTime { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("hasDescription")]
        public bool HasDescription { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("previewType")]
        public string PreviewType { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("completedDateTime")]
        public DateTimeOffset? CompletedDateTime { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("completedBy")]
        public EdBy CompletedBy { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("referenceCount")]
        public long ReferenceCount { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("checklistItemCount")]
        public long ChecklistItemCount { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("activeChecklistItemCount")]
        public long ActiveChecklistItemCount { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("conversationThreadId")]
        public string ConversationThreadId { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("createdBy")]
        public EdBy CreatedBy { get; set; }

        [JsonIgnoreSerialization]
        [JsonProperty("appliedCategories")]
        public AppliedCategories AppliedCategories { get; set; }

        [JsonProperty("assignments")]
        public Dictionary<string, Assignment> Assignments { get; set; }

        [JsonIgnoreSerialization]
        public TaskDetailResponse TaskDetail { get; set;}
    }

    public partial class AppliedCategories
    {
    }

    public partial class Assignment
    {
        [JsonProperty("@odata.type")]
        public string OdataType { get; set; }

        [JsonProperty("assignedDateTime")]
        [JsonIgnoreSerialization]
        public DateTimeOffset AssignedDateTime { get; set; }

        [JsonProperty("orderHint")]
        public string OrderHint { get; set; }

        [JsonProperty("assignedBy")]
        [JsonIgnoreSerialization]
        public EdBy AssignedBy { get; set; }
    }

    public partial class EdBy
    {
        [JsonProperty("user")]
        public User User { get; set; }
    }
}
