using System;
using System.Reflection;
using WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider
{
    internal abstract class DocumentHeader : DynamicPropertyObject { }
    internal abstract class DocumentDetailLine : DynamicPropertyObject { }
    internal abstract class DynamicPropertyObject
    {
        protected const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.GetProperty;
        /// <summary>
        /// Set property value, property name is ignore case
        /// </summary>
        /// <param name="propertyName">Ignore case</param>
        /// <param name="value">The value of property</param>
        internal void SetPropertyValue(string propertyName, string value)
        {
            PropertyInfo property;
            try
            {
                property = GetType().GetProperty(propertyName, Flags);
            }
            catch (AmbiguousMatchException)
            {
                throw new Exception($"Property [{propertyName}] declared twice in [{GetType().Name}]");
            }
            if (property == null)
                throw new Exception($"Property [{propertyName}] does not exist in [{GetType().Name}]");
            var attrs = property.GetCustomAttributes(typeof(FormatPropertyAttribute), false);
            foreach (FormatPropertyAttribute attr in attrs)
            {
                value = attr.Format(value);
            }
            property.SetValue(this, Convert.ChangeType(value, property.PropertyType), null);
        }
    }
}