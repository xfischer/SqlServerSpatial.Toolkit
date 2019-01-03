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

namespace SqlServerSpatial.Toolkit
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

            IGeometry transformed = geom;


            GeoAPI.CoordinateSystems.ICoordinateSystem sourceCs = SridReader.GetCSbyID(geom.SRID);
            GeoAPI.CoordinateSystems.ICoordinateSystem destCs = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;

            var transform = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory()
                .CreateFromCoordinateSystems(sourceCs, destCs);


            transformed = NetTopologySuite.CoordinateSystems.Transformations.GeometryTransform.TransformGeometry(
                    GeometryFactory.Default, geom, transform.MathTransform);



            return transformed;
        }

    }


}
