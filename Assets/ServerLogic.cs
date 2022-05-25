using System;
using System.Collections;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class ServerLogic : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public bool IsGame;

    public InputField Name;
    public InputField Company;
    public InputField Email;
    public Button start;
    
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

    /*public override void OnPlayerEnteredRoom(Player newPlayer)
    {
    SendData(Encoding.ASCII.GetBytes(name), SendOptions.SendReliable);
    }*/

    public void SendData(byte[] data, SendOptions sendoptions, RaiseEventOptions raiseEventOptions = null)
    {
        PhotonNetwork.RaiseEvent(eventID, data, raiseEventOptions, sendoptions);
    }

    public void SendMessage()
    {
        if (Name.text == "" ||
            Company.text == "" ||
            Email.text == "")
            return;

        string text = $"{Name.text};{Company.text};{Email.text}";

        start.interactable = false;
        
        SendData(Encoding.ASCII.GetBytes(text), SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code != eventID) return;

        var data = (byte[]) photonEvent.CustomData;

        string text = Encoding.ASCII.GetString(data);

        Debug.LogError(text);

        if (text == "1")
        {
            Name.text = "";
            Company.text = "";
            Email.text = "";

            start.interactable = true;
        }
        else StartCoroutine(WaitMessage());
    }
    
    private IEnumerator WaitMessage()
    {
        yield return new WaitForSeconds(2f);

        SendMessage();
    }
    
    #endregion
}