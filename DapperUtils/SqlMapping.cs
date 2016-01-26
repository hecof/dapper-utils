using System;
using System.Collections.Generic;
using System.Linq;

namespace DapperUtils
{
    public class SqlMapping<T>
    {
        private IDictionary<string, IEnumerable<string>> columnMapping = new Dictionary<string, IEnumerable<string>>();

        private ISet<string> notMap = new HashSet<string>();

        private string idField = "Id";

        private string tableName;

        private IDictionary<string, Func<T, object>> columnValueGenerator = new Dictionary<string, Func<T, object>>();

        private Func<T, object> sqlValuesBuilder;

        public SqlMapping<T> MapColumn(string objectField, IEnumerable<string> sqlFields)
        {
            columnMapping.Add(objectField, sqlFields);
            return this;
        }

        public SqlMapping<T> MapColumn(string objectField, string sqlField)
        {
            columnMapping.Add(objectField, new string[] { sqlField });
            return this;
        }

        public SqlMapping<T> NotMap(string objectField)
        {
            notMap.Add(objectField);
            return this;
        }

        private bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }

            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(DateTime))
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }

        public SqlMapping<T> UseColumnValue(string column, Func<T, object> builder)
        {
            columnValueGenerator.Add(column, builder);
            return this;
        }

        public IEnumerable<string> GetColumns()
        {
            var result = new List<string>();
            var type = typeof(T);

            foreach (var property in type.GetProperties())
            {
                var getMethod = property.GetGetMethod();
                if (getMethod == null || getMethod.IsStatic)
                    continue;

                if (notMap.Contains(property.Name))
                    continue;

                IEnumerable<string> cols;
                if (columnMapping.TryGetValue(property.Name, out cols))
                    result.AddRange(cols);
                else if (IsSimple(property.PropertyType))
                    result.Add(property.Name);
            }

            return result;
        }

        public IEnumerable<string> GetColumns(IEnumerable<string> partialFields)
        {
            if (partialFields == null || !partialFields.Any())
                return GetColumns();

            var result = new List<string>();
            var type = typeof(T);

            foreach (var property in type.GetProperties())
            {
                var getMethod = property.GetGetMethod();
                if (getMethod == null || getMethod.IsStatic)
                    continue;

                if (notMap.Contains(property.Name))
                    continue;

                if (!partialFields.Contains(property.Name))
                    continue;

                IEnumerable<string> cols;
                if (columnMapping.TryGetValue(property.Name, out cols))
                    result.AddRange(cols);
                else if (IsSimple(property.PropertyType))
                    result.Add(property.Name);
            }

            return result;
        }

        public SqlMapping<T> SetIdField(string field)
        {
            idField = field;
            return this;
        }

        public string IdField
        {
            get { return idField; }
        }

        public SqlMapping<T> SetTableName(string name)
        {
            tableName = name;
            return this;
        }

        public string TableName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    tableName = typeof(T).Name;

                return tableName;
            }
        }

        public object IdValue(T t)
        {
            return typeof(T).GetProperties().Where(p => p.Name == IdField).FirstOrDefault().GetMethod.Invoke(t, new object[] { });
        }

        public IDictionary<string, object> GetColumnValues(T t, IEnumerable<string> columns)
        {
            var result = new Dictionary<string, object>();

            Func<T, object> builder;
            var properties = typeof(T).GetProperties();

            foreach (var column in columns)
            {

                if (columnValueGenerator.TryGetValue(column, out builder))
                    result.Add(column, builder(t));
                else
                {
                    var property = properties.Where(p => p.Name == column).FirstOrDefault();
                    if (property != null)
                        result.Add(column, property.GetMethod.Invoke(t, new object[] { }));
                }
            }

            return result;
        }

        public SqlMapping<T> SetSqlValuesBuilder(Func<T, object> builder)
        {
            sqlValuesBuilder = builder;
            return this;
        }

        public Func<T, object> SqlValuesBuilder
        {
            get { return sqlValuesBuilder; }
        }
    }
}
