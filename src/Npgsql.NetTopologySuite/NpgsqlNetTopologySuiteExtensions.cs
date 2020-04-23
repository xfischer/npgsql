using System;
using System.Data;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using EnterpriseDB.EDBClient.NetTopologySuite;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient{
    /// <summary>
    /// Extension allowing adding the NetTopologySuite plugin to an EDB type mapper.
    /// </summary>
    public static class EDBNetTopologySuiteExtensions
    {
        static readonly Type[] ClrTypes =
        {
            typeof(Geometry),
            typeof(Point),
            typeof(LineString),
            typeof(Polygon),
            typeof(MultiPoint),
            typeof(MultiLineString),
            typeof(MultiPolygon),
            typeof(GeometryCollection),
        };

        /// <summary>
        /// Sets up NetTopologySuite mappings for the PostGIS types.
        /// </summary>
        /// <param name="mapper">The type mapper to set up (global or connection-specific).</param>
        /// <param name="coordinateSequenceFactory">The factory which knows how to build a particular implementation of ICoordinateSequence from an array of Coordinates.</param>
        /// <param name="precisionModel">Specifies the grid of allowable points.</param>
        /// <param name="handleOrdinates">Specifies the ordinates which will be handled. Not specified ordinates will be ignored.
        /// If <see cref="F:GeoAPI.Geometries.Ordiantes.None" /> is specified, an actual value will be taken from
        /// the <see cref="P:GeoAPI.Geometries.ICoordinateSequenceFactory.Ordinates"/> property of <paramref name="coordinateSequenceFactory"/>.</param>
        /// <param name="geographyAsDefault">Specifies that the geography type is used for mapping by default.</param>
        public static IEDBTypeMapper UseNetTopologySuite(
            this IEDBTypeMapper mapper,
            CoordinateSequenceFactory? coordinateSequenceFactory = null,
            PrecisionModel? precisionModel = null,
            Ordinates handleOrdinates = Ordinates.None,
            bool geographyAsDefault = false)
        {
            if (coordinateSequenceFactory == null)
                coordinateSequenceFactory = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;

            if (precisionModel == null)
                precisionModel = NtsGeometryServices.Instance.DefaultPrecisionModel;

            if (handleOrdinates == Ordinates.None)
                handleOrdinates = coordinateSequenceFactory.Ordinates;

            var typeHandlerFactory = new NetTopologySuiteHandlerFactory(
                new PostGisReader(coordinateSequenceFactory, precisionModel, handleOrdinates),
                new PostGisWriter());

            return mapper
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "geometry",
                    EDBDbType = EDBDbType.Geometry,
                    ClrTypes = geographyAsDefault ? Type.EmptyTypes : ClrTypes,
                    InferredDbType = DbType.Object,
                    TypeHandlerFactory = typeHandlerFactory
                }.Build())
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "geography",
                    EDBDbType = EDBDbType.Geography,
                    ClrTypes = geographyAsDefault ? ClrTypes : Type.EmptyTypes,
                    InferredDbType = DbType.Object,
                    TypeHandlerFactory = typeHandlerFactory
                }.Build());
        }
    }
}
