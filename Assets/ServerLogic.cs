using System;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerLogic : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public bool IsGame;
    
    private const byte eventID = 42;

    public event Action<byte[]> OnReceived;
    
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Pun Callbacks

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Failed to connect: {cause}");
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to photon!");
        
        if (IsGame)
            PhotonNetwork.JoinRandomRoom();
        else
            PhotonNetwork.CreateRoom("ChatRoom");
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError(message);
        
        PhotonNetwork.CreateRoom("My First Room");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.CurrentRoom.Name} joined!");
        
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("joined!");

        string name = "Передал данные";
        
        SendData(Encoding.ASCII.GetBytes(name), SendOptions.SendReliable);
    }

    public void SendData(byte[] data, SendOptions sendoptions, RaiseEventOptions raiseEventOptions = null)
    {
        PhotonNetwork.RaiseEvent(eventID, data, raiseEventOptions, sendoptions);
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code != eventID) return;

        var data = (byte[]) photonEvent.CustomData;

        OnReceived?.Invoke(data);
    }
    
    #endregion
}
