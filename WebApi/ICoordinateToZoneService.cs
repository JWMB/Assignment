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
            throw new NotImplementedException();
        }
    }
}
