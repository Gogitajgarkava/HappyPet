using HappyPet.Application.Interfaces.Code;
using QRCoder;

namespace HappyPet.Infrastructure.Code
{
    public sealed class QrCodeGenerator : IQrCodeGenerator
    {
        public byte[] GeneratePng(string content, int pixelsPerModule = 10)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("QR კოდის შიგთავსი ცარიელია.", nameof(content));
            }

            var moduleSize = Math.Clamp(pixelsPerModule, 1, 50);

            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data);
            return png.GetGraphic(moduleSize);
        }
    }
}
