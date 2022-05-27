using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WebSocketSharp;

public class ServerLogic : MonoBehaviour
{
    public TMP_InputField IpnutIP;
    public InputField Name;
    public InputField Company;
    public InputField Email;
    public Text NameH;
    public Text CompanyH;
    public Text EmailH;
    public Button start;
    public GameObject IPField;

    private Dictionary<InputField, Text> GUI;
    private WebSocket webSocket;

    void Start()
    {
        GUI = new Dictionary<InputField, Text>
        {
            {Name, NameH},
            {Company, CompanyH},
            {Email, EmailH}
        };
    }

    public void InitConnect()
    {
        IPAddress correctIP;

        if (!IPAddress.TryParse(IpnutIP.text, out correctIP))
            return;

        TryConnectToServer(correctIP);
    }
    
    private void TryConnectToServer(IPAddress ip)
    {
        webSocket = new WebSocket($"ws://{ip}:4649/Echo");

        /*webSocket.OnOpen += (sender, e) => { UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogError($"open {e}")); };

        webSocket.OnMessage += (sender, e) => { UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogError($"data {e.Data}")); };

        webSocket.OnError += (sender, e) => { UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogError(e.Message)); };

        webSocket.OnClose += (sender, e) => { UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogWarning(e.Code)); };*/
        
        webSocket.OnOpen += WebSocketOnOnOpen;

        webSocket.OnMessage += OnEvent;

        webSocket.OnError += WebSocketOnOnError;

        webSocket.OnClose += WebSocketOnOnClose;
        
        webSocket.Connect();
    }

    private void OnDestroy()
    {
        if (webSocket == null)
            return;
        
        webSocket.Close();

        webSocket.OnOpen -= WebSocketOnOnOpen;

        webSocket.OnMessage -= OnEvent;

        webSocket.OnError -= WebSocketOnOnError;

        webSocket.OnClose -= WebSocketOnOnClose;
    }

    private void WebSocketOnOnClose(object sender, CloseEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => IPField.SetActive(true));
    }

    private void WebSocketOnOnError(object sender, ErrorEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogError(e.Message));
    }

    private void WebSocketOnOnOpen(object sender, EventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => IPField.SetActive(false));
    }

    public void PlaceHolder(InputField field)
    {
        if (field.text == "")
            GUI[field].enabled = true;
        else
            GUI[field].enabled = false;
    }

    public void SendMessage()
    {
        if (Name.text == "" ||
            Company.text == "" ||
            Email.text == "")
            return;

        string text = $"{Name.text};{Company.text};{Email.text}";

        start.interactable = false;
        webSocket.Send(text);

    }

    public void OnEvent(object sender, MessageEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (e.Data == "1")
            {
                Name.text = "";
                Company.text = "";
                Email.text = "";

                start.interactable = true;
            }
            else StartCoroutine(WaitMessage());
        });
    }

    private IEnumerator WaitMessage()
    {
        yield return new WaitForSeconds(2f);

        SendMessage();
    }
}