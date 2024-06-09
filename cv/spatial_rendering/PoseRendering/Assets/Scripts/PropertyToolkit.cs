using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace PropertyToolkit
{
    public class Tools
    {
        // Set the property value of a serializable C# object
        public static void SetPropertyValue(object obj, string jsonPropertyName, object value)
        {
            var property = GetPropertyByJsonName(obj, jsonPropertyName);
            if (property != null)
            {
                property.SetValue(obj, value);
            }
        }

        // Set the property value of a serializable C# object
        public static object GetPropertyValue(object obj, string jsonPropertyName)
        {
            var property = GetPropertyByJsonName(obj, jsonPropertyName);
            //Debug.Log("GETTING THIS PROP: " + property);
            //Debug.Log("POSSIBLE THIS VALUE: " + property?.GetValue(obj));
            return property?.GetValue(obj);
        }

        // Get generic PropertyInfo property value of a serializable C# object
        private static PropertyInfo GetPropertyByJsonName(object obj, string jsonPropertyName)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttributes(typeof(JsonPropertyAttribute), false)
                                        .Cast<JsonPropertyAttribute>()
                                        .FirstOrDefault();
                //Debug.Log("Attribute: " + attribute.PropertyName + " Json property name: " + jsonPropertyName);
                if (attribute != null && attribute.PropertyName == jsonPropertyName)
                {
                    return property;
                }
            }
            return null;
        }

    }
}
