namespace HappyPet.Application.Interfaces.Code
{
    public interface IQrCodeGenerator
    {
        byte[] GeneratePng(string content, int pixelsPerModule = 10);
    }
}
