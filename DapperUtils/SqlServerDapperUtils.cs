using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DapperUtils
{
    public static class SqlServerDapperUtils
    {
        private static IDictionary<Type, object> dictionary = new Dictionary<Type, object>();

        public static void UseMapping<T>(SqlMapping<T> mapping)
        {
            dictionary.Add(typeof(T), mapping);
        }

        public static SqlMapping<T> GetMapping<T>()
        {
            object obj;
            if (dictionary.TryGetValue(typeof(T), out obj))
                return obj as SqlMapping<T>;

            return null;
        }

        public static void Update<T>(this IDbConnection cnx, T t, IEnumerable<string> fields = null)
        {
            var mapping = GetMapping<T>();
            if (mapping == null)
                throw new Exception("not_mapping_found");

            var columnNames = new List<string>(mapping.GetColumns(fields));
            columnNames.Remove(mapping.IdField);

            var sqlColumns = "";
            foreach (var column in columnNames)
                sqlColumns += column + " = @" + column + ", ";

            sqlColumns = sqlColumns.Remove(sqlColumns.Length - 2);

            var columnValues = mapping.GetColumnValues(t, columnNames);
            columnValues.Add(mapping.IdField, mapping.IdValue(t));

            var sql = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}", mapping.TableName, sqlColumns, mapping.IdField, "@" + mapping.IdField);

            var dbArgs = new DynamicParameters();
            foreach (var pair in columnValues)
                dbArgs.Add(pair.Key, pair.Value);
            
            cnx.Execute(sql, dbArgs);
        }

        public static void Add<T>(this IDbConnection cnx, T t)
        {
            var mapping = GetMapping<T>();
            if (mapping == null)
                throw new Exception("not_mapping_found");

            var columnNames = new List<string>(mapping.GetColumns());
            columnNames.Remove(mapping.IdField); //TODO: only for autonumeric values

            var columns = "";
            var values = "";
            foreach (var col in columnNames)
            {
                columns += col + ", ";
                values += "@" + col + ", ";
            }

            columns = columns.Remove(columns.Length - 2);
            values = values.Remove(values.Length - 2);

            var sql = @"
                INSERT INTO [{0}] ({1}) VALUES ({2})
                SELECT CAST(scope_identity() AS BigInt)"; //TODO: cast to the Id type
            sql = string.Format(sql, mapping.TableName, columns, values);

            var valuesBuilder = mapping.SqlValuesBuilder;
            if (valuesBuilder != null)
            {
                var obj = valuesBuilder(t);
                var id = cnx.Query<object>(sql, obj).FirstOrDefault();
            }
            else
            {
                var columnValues = mapping.GetColumnValues(t, columnNames);
                var dbArgs = new DynamicParameters();
                foreach (var pair in columnValues)
                    dbArgs.Add(pair.Key, pair.Value);
                var id = cnx.Query<object>(sql, dbArgs).FirstOrDefault(); //TODO: id only for server-generated id
            }
        }
    }
}
