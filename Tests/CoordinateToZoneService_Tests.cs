using FakeItEasy;
using GeoLibrary.Model;
using Shouldly;
using WebApi;

namespace Tests
{
    public class CoordinateToZoneService_Tests
    {
        [Theory]
        [InlineData(59.31323397678888, 18.067266038822446, "SE3")] // Stockholm
        [InlineData(39.31323397678888, 18.067266038822446, null)]
        [InlineData(0, 0, null)]
        public async Task CoordinateToZoneService_FakeZones(double longitude, double latitude, string? expectedZone)
        {
            var zoneDefinitionProvider = A.Fake<IZoneDefinitionProvider>();
            A.CallTo(() => zoneDefinitionProvider.Get())
                .Returns(Task.FromResult(new[] { new CoordinateToZoneService.Converted("SE1", new[] { new Polygon(new[] { new Point(50, 17), new(60, 17), new(60, 19), new(50, 19), new(50, 17) }) }.ToList()) }.ToList() ));

            var sut = new CoordinateToZoneService(zoneDefinitionProvider);
            var zone = await sut.Get(new((decimal)longitude, (decimal)latitude));
            zone.ShouldBe(expectedZone);
        }

        [Theory]
        [InlineData(59.31323397678888, 18.067266038822446, "SE3")] // Stockholm
        [InlineData(55.597203911235034, 13.0017964889469, "SE4")] // Malmö
        [InlineData(67.12664979623472, 20.633174522636434, "SE1")] // Gällivare
        [InlineData(61.72033675223663, 17.051631721240916, "SE2")] // Hudiksvall
        [InlineData(0, 0, null)]
        public async Task CoordinateToZoneService_RealZones(double longitude, double latitude, string? expectedZone)
        {
            var zoneDefinitionProvider = new GeoJsonZoneDefinitionProvider();
            var sut = new CoordinateToZoneService(zoneDefinitionProvider);
            var zone = await sut.Get(new((decimal)longitude, (decimal)latitude));
            zone.ShouldBe(expectedZone);
        }
    }
}
