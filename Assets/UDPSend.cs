 
 
/*
 
    -----------------------
    UDP-Send
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
    // > gesendetes unter
    // 127.0.0.1 : 8050 empfangen
   
    // nc -lu 127.0.0.1 8050
 
        // todo: shutdown thread at the end
*/
using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
public class UDPSend : MonoBehaviour
{
    private static int localPort;
   
    // prefs
    [SerializeField] private string IP = "127.0.0.1";  // define in init
    [SerializeField] private int port = 8051;  // define in init
   
    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;
   
    // gui
    string strMessage="";
   
       
    // call it from shell (as program)
    private static void Main()
    {
        UDPSend sendObj=new UDPSend();
        sendObj.init();
       
        // testing via console
        // sendObj.inputFromConsole();
       
        // as server sending endless
        sendObj.sendEndless(" endless infos \n");
       
    }
    // start from unity3d
    public void Start()
    {
        init();
    }
   
    // OnGUI
    void OnGUI()
    {
        Rect rectObj=new Rect(40,380,200,400);
            GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj,"# UDPSend-Data\n " +IP+":"+port+" #\n"
                    + "shell> nc -lu "+IP+" "+port+" \n"
                ,style);
       
        // ------------------------
        // send it
        // ------------------------
        strMessage=GUI.TextField(new Rect(40,420,140,20),strMessage);
        if (GUI.Button(new Rect(190,420,40,20),"send"))
        {
            //sendString(strMessage+"\n");
        }      
    }
   
    // init
    public void init()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("UDPSend.init()");
       
        // define
        //IP="127.0.0.1"; //defined in serializable field above
        //port=8051;
       
        // ----------------------------
        // Senden
        // ----------------------------
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
        
 
        // Den message zum Remote-Client senden.
        //client.Send(data, data.Length, remoteEndPoint);
        //sendDouble(111.3);
   
    }
 
    // inputFromConsole
    private void inputFromConsole()
    {
        try
        {
            string text;
            do
            {
                text = Console.ReadLine();
 
                // Den Text zum Remote-Client senden.
                if (text != "")
                {
 
                    // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
                    byte[] data = Encoding.UTF8.GetBytes(text);
 
                    // Den Text zum Remote-Client senden.
                    client.Send(data, data.Length, remoteEndPoint);
                }
            } while (text != "");
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
 
    }
 
    // sendData
public void sendString(string message)
    {
        try
        {
                //if (message != "")
                //{
 
                    // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
                    byte[] data = Encoding.UTF8.GetBytes(message);
 
                    // Den message zum Remote-Client senden.
                    client.Send(data, data.Length, remoteEndPoint);
                //}
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }
   
    public void sendDouble(double message)
    {
        try
        {
            //if (message != "")
            //{
 
            // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
            byte[] data = BitConverter.GetBytes(message);
 
            // Den message zum Remote-Client senden.
            client.Send(data, data.Length, remoteEndPoint);
            //}
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }
   
    // endless test
    private void sendEndless(string testStr)
    {
        do
        {
            //sendString(testStr);
           
           
        }
        while(true);
       
    }
    
    
   
}