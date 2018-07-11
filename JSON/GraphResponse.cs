using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
namespace PlannerExAndImport.JSON 
{
    // provides generic convenience methods to make get, post and patch requests
    public class GraphResponse<Type> {
        private static Type FromJson(string json) => JsonConvert.DeserializeObject<Type>(json, Converter.Settings);

        public static async Task<Type> Get(string url, HttpClient httpClient) {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return FromJson(await response.Content.ReadAsStringAsync());
        }

        public static async Task<Type> Post(string url, HttpClient httpClient, Type data) 
        {
            string content = Serialize.ToJsonWithIgnore(data);
            var response = await httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            return FromJson(await response.Content.ReadAsStringAsync());
        }

        public static async Task<Type> Patch(string url, HttpClient httpClient, Type data, string ifMatch) 
        {
            string content = Serialize.ToJsonWithIgnore(data);
            var req = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            req.Headers.TryAddWithoutValidation("If-Match", ifMatch);
            var response = await httpClient.SendAsync(req);
            response.EnsureSuccessStatusCode();
            return FromJson(await response.Content.ReadAsStringAsync());
        }
    }

    public static class Serialize
    {
        public static string ToJsonWithIgnore(this object self) => JsonConvert.SerializeObject(self, Converter.IgnoreSettings);
        public static string ToJson(this object self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };

        public static readonly JsonSerializerSettings IgnoreSettings = new JsonSerializerSettings
        {
            ContractResolver = new JsonPropertiesResolver(),
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    // credit for this goes to https://stackoverflow.com/users/3174275/jraco11 because of https://stackoverflow.com/a/35234883
    public class JsonIgnoreSerializationAttribute : Attribute { }

    class JsonPropertiesResolver : DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            //Return properties that do NOT have the JsonIgnoreSerializationAttribute
            return objectType.GetProperties()
                            .Where(pi => !Attribute.IsDefined(pi, typeof(JsonIgnoreSerializationAttribute)))
                            .ToList<MemberInfo>();
        }
    }
}