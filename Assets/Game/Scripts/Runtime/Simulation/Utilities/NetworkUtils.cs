using Unity.Entities;

namespace GameNetwork.Utils
{
    public enum ClientConnectionState { NotConnected, Connecting, Connected }

    /// <summary>References to the simulations inside this application. No socket or cloud service is used.</summary>
    public static class ClientTransportHelper
    {
        public static ClientConnectionState State = ClientConnectionState.NotConnected;
        public static World ClientWorld { get; set; }
        public static World ServerWorld { get; set; }
    }
}
