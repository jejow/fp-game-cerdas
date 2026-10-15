using Unity.NetCode;

public class CommonAutoConnect : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 0;
        NetworkStreamReceiveSystem.DriverConstructor = new OfflineDriverConstructor();
        // The menu owns match startup; never create an automatic remote client or server.
        return false;
    }
}




