using GeoLibrary.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApi
{
    public interface ICoordinateToZoneService
    {
        Task<string?> Get(Coordinate coordinate);
    }
    public record Coordinate(decimal Longitude, decimal Latitude);

    public class CoordinateToZoneService : ICoordinateToZoneService
    {
        private List<Converted>? converted;
        private async Task<List<Converted>> AssertData()
        {
            if (converted == null)
            {
                var data = JsonSerializer.Deserialize<Root>(await File.ReadAllTextAsync("Resources/EnergyAreas.geojson"));
                if (data == null)
                    throw new Exception("Could not parse data");
                converted = data.Features.Select(Converted.Create).ToList();
            }
            return converted;
        }

        public async Task<string?> Get(Coordinate coordinate)
        {
            var converted = await AssertData();
            var found = converted.Where(o => o.IsInside(new((double)coordinate.Longitude, (double)coordinate.Latitude))).ToList();
            if (found.Count > 1)
                throw new Exception($"Polygons overlapping at {coordinate}"); // should be pre-validation step

            return found.Any() ? $"SE{found.Single().Id}" : null;
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
