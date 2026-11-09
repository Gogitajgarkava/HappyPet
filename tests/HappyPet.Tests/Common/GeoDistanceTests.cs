using HappyPet.Application.Common;

namespace HappyPet.Tests.Common;

public class GeoDistanceTests
{
    [Fact]
    public void CalculateKm_ForSamePoint_ReturnsZero()
    {
        var distance = GeoDistance.CalculateKm(41.7151, 44.8271, 41.7151, 44.8271);

        Assert.Equal(0, distance, 3);
    }

    [Fact]
    public void CalculateKm_BetweenTbilisiAndBatumi_IsApproximatelyCorrect()
    {
        // Straight-line distance is roughly 265 km.
        var distance = GeoDistance.CalculateKm(41.7151, 44.8271, 41.6168, 41.6367);

        Assert.InRange(distance, 255, 275);
    }

    [Fact]
    public void GetBoundingBox_ContainsOriginAndClampsLatitude()
    {
        var box = GeoDistance.GetBoundingBox(41.7151, 44.8271, 25);

        Assert.True(box.MinLatitude < 41.7151);
        Assert.True(box.MaxLatitude > 41.7151);
        Assert.True(box.MinLongitude < 44.8271);
        Assert.True(box.MaxLongitude > 44.8271);
        Assert.False(box.CrossesAntimeridian);
    }

    [Fact]
    public void GetBoundingBox_NearAntimeridian_ReportsWrap()
    {
        var box = GeoDistance.GetBoundingBox(0, 179.9, 100);

        Assert.True(box.CrossesAntimeridian);
    }
}
