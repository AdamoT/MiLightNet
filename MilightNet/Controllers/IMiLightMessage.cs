namespace MiLightNet.Controllers
{
    public interface IMiLightMessage
    {
        byte[] Data { get; }

        void CalculateChecksum();
    }    
}
