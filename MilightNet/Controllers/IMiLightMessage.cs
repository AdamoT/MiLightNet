namespace MiLightNet.Controllers
{
    public interface IMiLightMessage
    {
        byte[] GetData();

        void CalculateChecksum();
    }
}
