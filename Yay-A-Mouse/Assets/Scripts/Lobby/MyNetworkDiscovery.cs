using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Overrides NetworkDiscovery base class
/// to handle reception of broadcast message 
/// from server on the host
/// </summary>

public class MyNetworkDiscovery : NetworkDiscovery{

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        // Gets the network lobby manager to start the client,
        // connecting to the given server address
        Debug.Log("frmoAddress" + fromAddress);
        Debug.Log("dsata is " + data);
        LobbyManager.singleton.networkAddress = fromAddress;
        LobbyManager.singleton.StartClient();
        StopBroadcast();
    }
}

