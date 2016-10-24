using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerSpatial.Toolkit.Test
{
	static class NaturalEarthData
	{
		private static List<string> naturalEarthTables = null;
		private const string naturalEarthconnectionString = @"Data Source=.\MSSQL2014;Initial Catalog=NaturalEarth_CulturalVectors_110;Integrated Security=True";
		public static List<string> GetNaturalEarthTables()
		{
			if (naturalEarthTables == null)
			{
				naturalEarthTables = new List<string>();

				using (SqlConnection con = new SqlConnection(naturalEarthconnectionString))
				{
					con.Open();
					using (SqlCommand com = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", con))
					{
						using (SqlDataReader dr = com.ExecuteReader())
						{
							while (dr.Read())
							{
								naturalEarthTables.Add(dr.GetString(0));
							}
						}
					}
				}
			}

			return naturalEarthTables;
		}

		public static List<NaturalEarthRow> GetNatualEarthTableRows(string tableName)
		{
			List<NaturalEarthRow> rows = new List<NaturalEarthRow>();
			using (SqlConnection con = new SqlConnection(naturalEarthconnectionString))
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

							using (BinaryReader binReader = new BinaryReader(dr.GetSqlBytes(geomCol.Index).Stream))
							{
								row.Geometry = new SqlGeometry();
								row.Geometry.Read(binReader);
							}
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
		public SqlGeometry Geometry;
		public string name;
	}
}
