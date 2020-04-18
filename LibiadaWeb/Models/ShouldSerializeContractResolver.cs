using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace LibiadaWeb.Models
{
    public class ShouldSerializeContractResolver:DefaultContractResolver
    {
        private IEnumerable<string> ignoredProperties;
        public ShouldSerializeContractResolver(IEnumerable<string> propNamesToIgnore)
        {
            ignoredProperties = propNamesToIgnore;
        }
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = (x) => { return !ignoredProperties.Contains(property.PropertyName); };
            return property;
        }
    }
}