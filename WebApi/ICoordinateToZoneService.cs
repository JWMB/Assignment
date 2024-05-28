using GeoLibrary.Model;

namespace WebApi
{
    public interface ICoordinateToZoneService
    {
        Task<string?> Get(Coordinate coordinate);
    }
    public record Coordinate(decimal Longitude, decimal Latitude);

    public class CoordinateToZoneService : ICoordinateToZoneService
    {
        public Task<string?> Get(Coordinate coordinate)
        {
            var polys = new[] {
                ("SE1", new Polygon(new[] { new Point(0, 0), new(0, 10), new(10, 10), new(10, 0), new(0, 0) }))
            };
            var found = polys.SingleOrDefault(o => o.Item2.IsPointInside(new((double)coordinate.Longitude, (double)coordinate.Latitude)));
            return Task.FromResult(found.Item2 == default ? null : found.Item1);
        }

        public record Converted(string Id, List<Polygon> Polygons)
        {
            public static Converted Create(Feature feature)
            {
                return new Converted(
                    $"SE{feature.Id}",
                    (feature.Geometry.Type == "MultiPolygon" 
                        ? feature.Geometry.Coordinates.SelectMany(CreateMultiPolygons)
                        : feature.Geometry.Coordinates.Select(CreatePolygon)
                    ).ToList()
                    );

                List<Polygon> CreateMultiPolygons(List<List<object>> data) =>
                    //data.Select(o => new Polygon(o.Select(p => p as List<object>).Select(p => new[] { p[0], p[1] }.Cast<JsonElement>().Cast<object>().ToList()).Select(x => CreatePoint(x)))).ToList();
                    data.Select(o => CreatePolygon(o.OfType<List<object>>().ToList())).ToList();

                Polygon CreatePolygon(List<List<object>> points) => 
                    new Polygon(points.Select(CreatePoint));

                Point CreatePoint(List<object> values)
                {
                    var cast = values.Cast<JsonElement>().ToList();
                    return new Point(cast[0].GetDouble(), cast[1].GetDouble());
                }
            }
            public bool IsInside(Point pt) => Polygons.Any(o => o.IsPointInside(pt));
        }

        // from https://json2csharp.com/
        public record Feature(
           [property: JsonPropertyName("type")] string Type,
           [property: JsonPropertyName("id")] int? Id,
           [property: JsonPropertyName("geometry")] Geometry Geometry,
           [property: JsonPropertyName("properties")] Properties Properties
       );

        public record Geometry(
            [property: JsonPropertyName("type")] string Type,
            [property: JsonPropertyName("coordinates")] IReadOnlyList<List<List<object>>> Coordinates // Stupid format...
        );

        public record Properties(
            [property: JsonPropertyName("OBJECTID")] int? OBJECTID
        );

        public record Root(
            [property: JsonPropertyName("type")] string Type,
            [property: JsonPropertyName("features")] IReadOnlyList<Feature> Features
        );
    }
}
