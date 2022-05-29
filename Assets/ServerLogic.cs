using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
    public TextMeshProUGUI InfoPanel;
    public Toggle TryConnect;
    public Toggle SaveIP;

    private Dictionary<InputField, Text> GUI;
    private WebSocket webSocket;
    private IPAddress correctIP;
    
    private bool TryConnectB => TryConnect.isOn;
    
    private int Port = 4649;
    
    void Start()
    {
        InfoPanel.enabled = false;

        IpnutIP.text = LoadURL();
        
        GUI = new Dictionary<InputField, Text>
        {
            {Name, NameH},
            {Company, CompanyH},
            {Email, EmailH}
        };
    }

    public void InitConnect()
    {
        StopAllCoroutines();
        
        if (!IPAddress.TryParse(IpnutIP.text, out correctIP))
            return;

        TryConnectToServer(correctIP);
    }
    
    private void TryConnectToServer(IPAddress ip)
    {
        InfoPanel.enabled = true;
        InfoPanel.text = $"Attempt to connect to the [{ip}]...";

        using(TcpClient tcpClient = new TcpClient())
        {
            try
            {
                tcpClient.Connect(ip, Port);
                
                if (SaveIP.isOn)
                    SaveURL(ip.ToString());
            }
            catch (Exception)
            {
                if (TryConnect.isOn)
                    StartCoroutine(Reconnect());
                else
                    InfoPanel.text =
                        "Critical connection problem, please check your internet connection on both devices and try again.";

                return;
            }
        }
        
        webSocket = new WebSocket($"ws://{ip}:{Port}/Echo");
        
        webSocket.OnOpen += WebSocketOnOnOpen;

        webSocket.OnMessage += OnEvent;

        webSocket.OnError += WebSocketOnOnError;

        webSocket.OnClose += WebSocketOnOnClose;
        
        webSocket.Connect();

        if (webSocket.IsAlive)
        {
            InfoPanel.text = $"Connected successfully !";
            
            StartCoroutine(CloseInfo());
        }
        else StartCoroutine(Reconnect());
    }

    private void SaveURL(string URL)
    {
        PlayerPrefs.SetString("URL", URL);
    }

    private string LoadURL()
    {
        if (PlayerPrefs.HasKey("URL"))
            return PlayerPrefs.GetString("URL");

        return "";
    }

    private IEnumerator CloseInfo()
    {
        yield return new WaitForSeconds(2f);

        InfoPanel.enabled = !InfoPanel.enabled;
    }
    
    private IEnumerator Reconnect()
    {
        yield return new WaitForSeconds(2f);

        TryConnectToServer(correctIP);
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
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (e.Code == 1005)
                StartCoroutine(Reconnect());

            if(!TryConnectB) IPField.SetActive(true);
        });
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
        });
    }

    private IEnumerator WaitMessage()
    {
        yield return new WaitForSeconds(2f);

        SendMessage();
    }
}