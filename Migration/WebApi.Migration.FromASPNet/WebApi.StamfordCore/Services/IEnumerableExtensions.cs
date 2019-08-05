using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace WebApi.StamfordCore.Services
{
    public static class IEnumerableExtensions
    {
        public static DataTable AsDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                // table.Columns.Add(prop.Name, typeof(string));
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                }
                table.Rows.Add(row);
            }

            return table;
        }
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            value = value.Trim();
            if (value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }
            return value;
        }
    }
}