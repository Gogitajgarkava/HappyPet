using HappyPet.Infrastructure.Code;

namespace HappyPet.Tests.Services;

public class QrCodeGeneratorTests
{
    [Fact]
    public void GeneratePng_ReturnsPngBytes()
    {
        var generator = new QrCodeGenerator();

        var bytes = generator.GeneratePng("https://api.example.com/api/v1/public/pets/demo-token");

        Assert.NotEmpty(bytes);
        Assert.True(bytes.Length > 8);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, bytes.Take(4).ToArray());
    }

    [Fact]
    public void GeneratePng_WithEmptyContent_Throws()
    {
        var generator = new QrCodeGenerator();

        Assert.Throws<ArgumentException>(() => generator.GeneratePng("  "));
    }
}
