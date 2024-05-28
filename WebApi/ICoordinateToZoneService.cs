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
