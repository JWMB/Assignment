using GeoLibrary.Model;
using System.Text.Json;
using System.Text.Json.Serialization;
using static WebApi.CoordinateToZoneService;

namespace WebApi
{
    public interface ICoordinateToZoneService
    {
        Task<string?> Get(Coordinate coordinate);
    }
    public record Coordinate(decimal Longitude, decimal Latitude);


    public interface IZoneDefinitionProvider
    {
        Task<List<Converted>> Get();
    }
    public class GeoJsonZoneDefinitionProvider : IZoneDefinitionProvider
    {
        private List<Converted>? converted;

        public async Task<List<Converted>> Get()
        {
            return await AssertData();
        }

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
    }

    public class CoordinateToZoneService : ICoordinateToZoneService
    {
        private readonly IZoneDefinitionProvider zoneDefinitionProvider;

        public CoordinateToZoneService(IZoneDefinitionProvider zoneDefinitionProvider)
        {
            this.zoneDefinitionProvider = zoneDefinitionProvider;
        }

        public async Task<string?> Get(Coordinate coordinate)
        {
            var converted = await zoneDefinitionProvider.Get();
            var found = converted.Where(o => o.IsInside(new((double)coordinate.Longitude, (double)coordinate.Latitude))).ToList();
            if (found.Count > 1)
                throw new Exception($"Polygons overlapping at {coordinate}"); // should be pre-validation step

            return found.Any() ? found.Single().Id : null;
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
                    data.Select(o => CreatePolygon(o.OfType<JsonElement>().Select(o => o.EnumerateArray().Select(p => p as object).ToList()).ToList())).ToList();

                Polygon CreatePolygon(List<List<object>> points) => 
                    new Polygon(AssertClosedPolygon(points.Select(CreatePoint)));

                Point CreatePoint(List<object> values)
                {
                    var cast = values.Cast<JsonElement>().ToList();
                    return new Point(cast[0].GetDouble(), cast[1].GetDouble());
                }
                IEnumerable<Point> AssertClosedPolygon(IEnumerable<Point> points) => points.Any() ? (points.First() != points.Last() ? points.Append(points.First()) : points) : points;
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
