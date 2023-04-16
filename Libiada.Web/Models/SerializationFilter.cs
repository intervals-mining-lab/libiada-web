namespace Libiada.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Serialization;
    using Newtonsoft.Json;
    using System.Reflection;

    public class SerializationFilter : DefaultContractResolver
    {
        private IEnumerable<string> ignoredProperties;

        public SerializationFilter(IEnumerable<string> propNamesToIgnore)
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