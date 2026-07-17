using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fm2ndParser
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, List<string>> _propertyNameToExclude;

        public DynamicContractResolver()
        {
            _propertyNameToExclude = new Dictionary<Type, List<string>>();
        }

        public void AddPropertyToExclude(Type type, string name)
        {
            if (!_propertyNameToExclude.ContainsKey(type))
            {
                _propertyNameToExclude.Add(type, new List<string>());
            }
            _propertyNameToExclude[type].Add(name);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // only serializer properties that are not named after the specified property.
            properties =
                properties.Where(p =>
                    !_propertyNameToExclude.ContainsKey(type) ||
                    !_propertyNameToExclude[type].Contains(p.PropertyName)
                ).ToList();

            return properties;
        }
    }
}
