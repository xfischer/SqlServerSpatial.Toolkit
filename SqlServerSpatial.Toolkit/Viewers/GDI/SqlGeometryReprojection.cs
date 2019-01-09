using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.CoordinateSystems.Transformations;
using GeoAPI.CoordinateSystems.Transformations;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;
using NetTopologySuite.Densify;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Precision;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;
using NetTopologySuite.Diagnostics.BaseLayer;

namespace NetTopologySuite.Diagnostics
{
    public static class IGeometryReprojection
    {

        public static IGeometry ReprojectTo(this IGeometry geom, int destinationEpsgCode)
        {

            IGeometry transformed = geom;

            if (geom.SRID != destinationEpsgCode)
            {
                GeoAPI.CoordinateSystems.ICoordinateSystem sourceCs = SridReader.GetCSbyID(geom.SRID);
                GeoAPI.CoordinateSystems.ICoordinateSystem destCs = SridReader.GetCSbyID(destinationEpsgCode);

                var transform = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory()
                    .CreateFromCoordinateSystems(sourceCs, destCs);


                transformed = NetTopologySuite.CoordinateSystems.Transformations.GeometryTransform.TransformGeometry(
                        GeometryFactory.Default, geom, transform.MathTransform);


            }
            return transformed;
        }
        public static IGeometry ReprojectToMercator(this IGeometry geom)
        {

            IGeometry transformed = GeometryTransform.TransformGeometry(GeometryFactory.Default, geom, new CustomTransformMercator());

            return transformed;
        }

    }

    internal class CustomTransformMercator : IMathTransform
    {
        public int DimSource => 2;

        public int DimTarget => 2;

        public string WKT => throw new NotImplementedException();

        public string XML => throw new NotImplementedException();

        public double[,] Derivative(double[] point)
        {
            throw new NotImplementedException();
        }

        public List<double> GetCodomainConvexHull(List<double> points)
        {
            throw new NotImplementedException();
        }

        public DomainFlags GetDomainFlags(List<double> points)
        {
            throw new NotImplementedException();
        }

        public bool Identity()
        {
            return false;
        }

        public IMathTransform Inverse()
        {
            throw new NotImplementedException();
        }

        public void Invert()
        {
            throw new NotImplementedException();
        }

        public double[] Transform(double[] point)
        {
            double projX = 0;
            double projY = 0;
            BingMapsTileSystem.LatLongToDoubleXY(point[1], point[0], out projX, out projY);
            //System.Diagnostics.Trace.TraceInformation("X: {0} / Y: {1}", projX, projY);
            return new double[] { projX, projY };
        }

        public ICoordinate Transform(ICoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public Coordinate Transform(Coordinate coordinate)
        {
            double projX = 0;
            double projY = 0;
            BingMapsTileSystem.LatLongToDoubleXY(coordinate.Y, coordinate.X, out projX, out projY);
            //System.Diagnostics.Trace.TraceInformation("X: {0} / Y: {1}", projX, projY);
            return new Coordinate(projX, projY );
    }

    public ICoordinateSequence Transform(ICoordinateSequence coordinateSequence)
    {
           return GeometryFactory.Default.CoordinateSequenceFactory.Create(coordinateSequence.ToCoordinateArray().Select(c => Transform(c)).ToArray());
    }

    public IList<double[]> TransformList(IList<double[]> points)
    {
        throw new NotImplementedException();
    }

    public IList<Coordinate> TransformList(IList<Coordinate> points)
    {
        throw new NotImplementedException();
    }
}


}
