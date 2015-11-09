using Microsoft.SqlServer.Types;
using Microsoft.Win32;
using SqlServerSpatialTypes.Toolkit.Viewers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SqlServerSpatialTypes.Toolkit.Viewer
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

#if DEBUG
            DebugPanel.Visibility = Visibility.Visible;
            viewer.Visibility = Visibility.Visible;
#else
			DebugPanel.Visibility = Visibility.Collapsed;
			viewer.Visibility = Visibility.Collapsed;
#endif


        }

        void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                LaunchTraceViewer(files[0]);
                e.Handled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //TestTrace();
            TestDepartements();
            //TestCommunes();

            TestTraceViewer();
        }

        private void TestTrace()
        {
            SpatialTrace.Enable();

            SpatialTrace.TraceText("Simple geometries");
            SpatialTrace.Indent();

            SqlGeometry points = Sample_Points(4326);
            SpatialTrace.TraceGeometry(points, "Points");

            SqlGeometry poly = Sample_Polygon(4326);
            SpatialTrace.TraceGeometry(poly, "Polygon");

            SqlGeography geog = Sample_PolygonGeography();
            SpatialTrace.TraceGeometry(geog, "Polygon (geography)");

            SpatialTrace.Unindent();

            SpatialTrace.TraceText("Composite geometries");
            SpatialTrace.Indent();

            SpatialTrace.TraceGeometry(poly.STUnion(points), "poly.STUnion(points)");

            SpatialTrace.TraceGeometry(poly.STUnion(points).STConvexHull(), "poly.STUnion(points).STConvexHull()");

            SpatialTrace.Unindent();

            SpatialTrace.Disable();
        }

        #region Geometry samples

        private SqlGeometry Sample_Points(int srid)
        {
            SqlGeometryBuilder builder = new SqlGeometryBuilder();
            builder.SetSrid(srid);
            builder.BeginGeometry(OpenGisGeometryType.MultiPoint);

            Random rnd = new Random();
            for (int i = 0; i < 100; i++)
            {
                builder.BeginGeometry(OpenGisGeometryType.Point);
                builder.BeginFigure(rnd.Next(-50, 200), rnd.Next(-50, 200));
                builder.EndFigure();
                builder.EndGeometry();
            }


            builder.EndGeometry();
            return builder.ConstructedGeometry;
        }

        private SqlGeometry Sample_Polygon(int srid)
        {
            SqlGeometryBuilder builder = new SqlGeometryBuilder();
            builder.SetSrid(srid);
            builder.BeginGeometry(OpenGisGeometryType.Polygon);
            builder.BeginFigure(10, 110);
            builder.AddLine(60, 10);
            builder.AddLine(110, 110);
            builder.AddLine(10, 110);
            builder.EndFigure();
            builder.EndGeometry();
            return builder.ConstructedGeometry;
        }

        private SqlGeography Sample_PolygonGeography()
        {
            SqlGeographyBuilder geoBuilder = new SqlGeographyBuilder();
            geoBuilder.SetSrid(4326);
            geoBuilder.BeginGeography(OpenGisGeographyType.Polygon);
            geoBuilder.BeginFigure(40, -10);
            geoBuilder.AddLine(40, 10);
            geoBuilder.AddLine(50, 0);
            geoBuilder.AddLine(40, -10);
            geoBuilder.EndFigure();
            geoBuilder.EndGeography();
            return geoBuilder.ConstructedGeography;
        }

        #endregion

        #region Examples

        private void TestDepartements()
        {
            List<SqlGeometry> geom = new List<SqlGeometry>();

            SpatialTrace.Enable();
            SpatialTrace.TraceText("Open DB connection");
            SpatialTrace.Indent();

            using (SqlConnection con = new SqlConnection(@"Data Source=.;Initial Catalog=SampleSpatialData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False"))
            {
                con.Open();

                using (SqlCommand com = new SqlCommand("SELECT geom, CODE_DEPT + ' ' + NOM_DEPT FROM dbo.DEPARTEMENT --WHERE geom2154.STNumInteriorRing() > 0", con))
                {
                    int i = 0;

                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        SpatialTrace.TraceText("Reading results DB\t\t connection");
                        SpatialTrace.Indent();
                        while (reader.Read())
                        {
                            i++;

                            // workaround https://msdn.microsoft.com/fr-fr/library/ms143179(v=sql.120).aspx
                            // In version 11.0 only
                            SqlGeometry curGeom = SqlGeometry.Deserialize(reader.GetSqlBytes(0));

                            //// In version 10.0 or 11.0
                            //curGeom = new SqlGeometry();
                            //curGeom.Read(new BinaryReader(reader.GetSqlBytes(0).Stream));

                           
                            geom.Add(curGeom);

                            SpatialTrace.SetFillColor(GetRandomColor());
                            SpatialTrace.SetLineColor(GetRandomColor());
                            SpatialTrace.SetLineWidth(GetRandomStrokeWidth());
                            SpatialTrace.TraceGeometry(curGeom, reader[1].ToString());
                        }

                        SpatialTrace.Unindent();
                    }
                }
            }

            SpatialTrace.Unindent();

            ((ISpatialViewer)viewer).SetGeometry(SqlGeomStyledFactory.Create(geom,null));
        }
        private void TestCommunes()
        {
            List<SqlGeometry> geom = new List<SqlGeometry>();

            SpatialTrace.Enable();
            SpatialTrace.TraceText("Open DB connection");
            SpatialTrace.Indent();

            using (SqlConnection con = new SqlConnection(@"Data Source=.;Initial Catalog=SampleSpatialData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False"))
            {
                con.Open();

                using (SqlCommand com = new SqlCommand("SELECT geom, INSEE_COM + ' ' + NOM_COM FROM dbo.COMMUNE --WHERE geom2154.STNumInteriorRing() > 0", con))
                {
                    int i = 0;

                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        SpatialTrace.TraceText("Reading results DB\t\t connection");
                        SpatialTrace.Indent();
                        while (reader.Read())
                        {
                            i++;

                            // workaround https://msdn.microsoft.com/fr-fr/library/ms143179(v=sql.120).aspx
                            // In version 11.0 only
                            SqlGeometry curGeom = SqlGeometry.Deserialize(reader.GetSqlBytes(0));

                            //// In version 10.0 or 11.0
                            //curGeom = new SqlGeometry();
                            //curGeom.Read(new BinaryReader(reader.GetSqlBytes(0).Stream));


                            geom.Add(curGeom);

                            SpatialTrace.SetFillColor(GetRandomColor());
                            SpatialTrace.SetLineColor(GetRandomColor());
                            SpatialTrace.SetLineWidth(GetRandomStrokeWidth());
                            SpatialTrace.TraceGeometry(curGeom, reader[1].ToString());
                        }

                        SpatialTrace.Unindent();
                    }
                }
            }

            SpatialTrace.Unindent();

						((ISpatialViewer)viewer).SetGeometry(SqlGeomStyledFactory.Create(geom, "Sample"));
        }

        static Random rnd = new Random();
        private Color GetRandomColor()
        {
            return Color.FromArgb((byte)rnd.Next(128, 255), (byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255));
        }
        private int GetRandomStrokeWidth()
        {
            return rnd.Next(1, 5);
        }

        #endregion

        private void btnTraceView_Click(object sender, RoutedEventArgs e)
        {
            TestTraceViewer();
        }
        private void ResetViewButton_Click(object sender, RoutedEventArgs e)
        {
            ((ISpatialViewer)viewer).ResetView();
        }

        private void TestTraceViewer()
        {
            string traceFilePath = SpatialTrace.TraceFilePath;
            if (traceFilePath == null)
            {
                MessageBox.Show("No current trace");
                return;
            }
            LaunchTraceViewer(traceFilePath);
        }

        private void LaunchTraceViewer(string traceFilePath)
        {
            SpatialTraceViewerControl ctlTraceViewer = new SpatialTraceViewerControl();
            ctlTraceViewer.Initialize(traceFilePath);
            Window wnd = new Window();
            wnd.Title = "SqlServer Spatial Trace Viewer";
            wnd.Content = ctlTraceViewer;
            wnd.Closed += (o, e) => { ctlTraceViewer.Close(); };
            wnd.Show();
        }


        private void btnTraceLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Trace files (SpatialTrace*.txt)|SpatialTrace*.txt";
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                LaunchTraceViewer(dlg.FileName);
            }
        }



    }
}
