using GeoAPI.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Diagnostics.Test
{
    static class NaturalEarthData
    {
        public enum DataSetType { Cultural, Physical }

        private static Dictionary<string, HashSet<string>> naturalEarthTables = new Dictionary<string, HashSet<string>>();
        private const string naturalEarthCulturalconnectionString = @"Data Source=.\MSSQL2014;Initial Catalog=NaturalEarth_CulturalVectors_110;Integrated Security=True;Connection Timeout=5";
        private const string naturalEarthPhysicalconnectionString = @"Data Source=.\MSSQL2014;Initial Catalog=NaturalEarth_PhysicalVectors_110;Integrated Security=True;Connection Timeout=5";

        public static List<string> GetNaturalEarthTables(DataSetType dataSet)
        {
            if (dataSet == DataSetType.Cultural)
            {
                return GetNaturalEarthTables(naturalEarthCulturalconnectionString);
            }
            else if (dataSet == DataSetType.Physical)
            {
                return GetNaturalEarthTables(naturalEarthPhysicalconnectionString);
            }
            else
                return null;
        }
        private static List<string> GetNaturalEarthTables(string connectionString)
        {
            if (!naturalEarthTables.ContainsKey(connectionString))
            {
                naturalEarthTables.Add(connectionString, new HashSet<string>());

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand com = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", con))
                    {
                        using (SqlDataReader dr = com.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                naturalEarthTables[connectionString].Add(dr.GetString(0));
                            }
                        }
                    }
                }
            }

            return naturalEarthTables[connectionString].ToList();
        }

        public static List<NaturalEarthRow> GetNaturalEarthTableRows(DataSetType dataSet, string tableName)
        {
            if (dataSet == DataSetType.Cultural)
            {
                return GetNaturalEarthTableRows(tableName, naturalEarthCulturalconnectionString);
            }
            else if (dataSet == DataSetType.Physical)
            {
                return GetNaturalEarthTableRows(tableName, naturalEarthPhysicalconnectionString);
            }
            else
                return null;
        }
        private static List<NaturalEarthRow> GetNaturalEarthTableRows(string tableName, string connectionString)
        {
            WKBReader wKBReader = new WKBReader();
            wKBReader.HandleSRID = true;
            List<NaturalEarthRow> rows = new List<NaturalEarthRow>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand(string.Format("SELECT * FROM [{0}]", tableName), con))
                {
                    using (SqlDataReader dr = com.ExecuteReader())
                    {
                        List<ColInfo> colInfos = GetColumnsInfo(dr);
                        ColInfo nameCol = FindColumnByNameOrType(colInfos, "name", "String"); // find first column named "name" or else first string column
                        ColInfo geomCol = FindColumnByNameOrType(colInfos, "geom", null); // find first "geom" column

                        while (dr.Read())
                        {
                            NaturalEarthRow row = new NaturalEarthRow();


                            wKBReader.Read(dr.GetSqlBytes(geomCol.Index).Stream);



                            if (nameCol != null)
                            {
                                row.name = dr.GetString(nameCol.Index);
                            }

                            rows.Add(row);
                        }
                    }
                }
            }

            return rows;
        }


        private static ColInfo FindColumnByNameOrType(List<ColInfo> colInfos, string colName, string typeName = null)
        {
            ColInfo nameCol = colInfos.FirstOrDefault(c => c.Name.ToLower() == colName);
            if (nameCol != null)
                return nameCol;
            else if (typeName != null)
            {
                ColInfo typeCol = colInfos.FirstOrDefault(c => c.TypeName == typeName);
                if (typeCol != null)
                {
                    return typeCol;
                }
            }
            return null;
        }

        private static List<ColInfo> GetColumnsInfo(SqlDataReader reader)
        {
            List<ColInfo> colInfos = new List<ColInfo>();
            int rowIndex = 0;
            foreach (DataRow colInfo in reader.GetSchemaTable().Rows)
            {
                colInfos.Add(new ColInfo(rowIndex, colInfo["ColumnName"].ToString(), ((Type)colInfo["DataType"]).Name));
                rowIndex++;
            }
            return colInfos;
        }

        private class ColInfo
        {
            public int Index;
            public string Name;
            public string TypeName;
            public ColInfo(int index, string name, string typeName)
            {
                Index = index;
                Name = name;
                TypeName = typeName;
            }
        }
    }





    public class NaturalEarthRow
    {
        public IGeometry Geometry;
        public string name;
    }
}
