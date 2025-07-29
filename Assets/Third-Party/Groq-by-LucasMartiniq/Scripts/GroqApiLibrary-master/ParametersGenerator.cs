using System;
using System.Collections.Generic;
using System.Reflection;

namespace GroqApiLibrary
{
    /// <summary>
    /// Utility to generate a Parameters object from a model class with static description and required fields.
    /// </summary>
    public static class ParametersGenerator
    {
        /// <summary>
        /// Generates a Parameters object for the given model type.
        /// The model type must define static fields:
        ///   - Dictionary<string, string> PropertyDescriptions
        ///   - string[] RequiredFields
        /// </summary>
        /// <param name="modelType">The model class type.</param>
        /// <returns>A Parameters object describing the model's properties.</returns>
        public static Parameters GenerateParameters(Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));

            // Get static PropertyDescriptions
            var descField = modelType.GetField("PropertyDescriptions", BindingFlags.Public | BindingFlags.Static);
            if (descField == null)
                throw new InvalidOperationException($"{modelType.Name} must define a public static PropertyDescriptions field.");
            var descriptions = descField.GetValue(null) as Dictionary<string, string>;
            if (descriptions == null)
                throw new InvalidOperationException($"PropertyDescriptions must be a Dictionary<string, string>.");

            // Get static RequiredFields
            var reqField = modelType.GetField("RequiredFields", BindingFlags.Public | BindingFlags.Static);
            if (reqField == null)
                throw new InvalidOperationException($"{modelType.Name} must define a public static RequiredFields field.");
            var required = reqField.GetValue(null) as string[];
            if (required == null)
                throw new InvalidOperationException($"RequiredFields must be a string array.");

            // Build properties dictionary
            var properties = new Dictionary<string, Property>();
            foreach (var prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var name = prop.Name;
                var type = GetParameterTypeString(prop.PropertyType);
                var desc = descriptions.ContainsKey(name) ? descriptions[name] : "";
                properties[name] = new Property { Type = type, Description = desc };
            }

            // Determine Parameters.Type (object or array)
            string parametersType = GetParametersType(modelType);

            return new Parameters
            {
                Type = parametersType,
                Properties = properties,
                Required = required
            };
        }

        /// <summary>
        /// Determines the Parameters.Type value ("object" or "array") based on the model type.
        /// </summary>
        private static string GetParametersType(Type modelType)
        {
            if (modelType.IsArray)
                return "array";
            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(List<>))
                return "array";
            // Default to object
            return "object";
        }

        /// <summary>
        /// Maps C# types to parameter type strings ("string", "boolean", etc.).
        /// </summary>
        private static string GetParameterTypeString(Type t)
        {
            if (t == typeof(string)) return "string";
            if (t == typeof(bool) || t == typeof(bool?)) return "boolean";
            if (t == typeof(int) || t == typeof(int?)) return "integer";
            if (t == typeof(float) || t == typeof(float?)) return "number";
            if (t == typeof(double) || t == typeof(double?)) return "number";
            // Add more as needed
            return "string"; // fallback
        }
    }
}
