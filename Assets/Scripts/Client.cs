using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Data;

[RequireComponent (typeof(NetworkView))]
public class Client : MonoBehaviour 
{
	//public
	public GameObject main;
	public GameObject info;

	public Text textServerState;
	public Text textConnections;
	public Text textMessageLog;
	
	//Private
	private PlayerData _player;
	private NetworkView _net;
	private string _playerName;
	private string _serverIP = "127.0.0.1";
	private int _serverPort = 1234;
	private string _password = "";
	private bool _isConnected = false;
	private string _curState = "Disconnected";
	private List<string> _messageLog = new List<string>();

	//Getters Setters
	public string playerName 
	{ 
		get
		{
			return _playerName;
		}
		set
		{
			_playerName = value;
		}
	}
	public string serverIP
	{ 
		get
		{
			return _serverIP;
		}
		set
		{
			_serverIP = value;
		}
	}
	public string serverPort 
	{ 
		get
		{
			return _serverPort.ToString();
		}
		set
		{
			int.TryParse(value, out _serverPort);
		}
	}
	public string password 
	{ 
		get
		{
			return _password;
		}
		set
		{
			_password = value;
		}
	}
	
	//Connect to Server
	public void Connect()
	{
		Application.runInBackground = true;
		_net = GetComponent<NetworkView>();
		Network.Connect(_serverIP, _serverPort, _password);
		_player = new PlayerData(_playerName);
		main.SetActive(false);
		info.SetActive(true);
		_curState = "Connecting...";
		UpdateGUI();
	}

	//Send Player Data
	void OnConnectedToServer()
	{
		_net.RPC("PlayerData", RPCMode.Server, _player);
		_curState = "Authenthicating...";
		UpdateGUI();
	}

	void OnDisconnectedFromServer()
	{
		_curState = "Disconnected";
		UpdateGUI();
		Application.LoadLevel(Application.loadedLevel);
	}

	//Update GUI
	void UpdateGUI()
	{
		textServerState.text = "State: " + _curState;
		textConnections.text = "Connections: " + Network.connections.Length.ToString();
		string log = "";
		for (int i = 0; i < _messageLog.Count; i++)
		{
			if (i < _messageLog.Count - 1)
				log += _messageLog[i] + "\n";
			else
				log += _messageLog[i];
		}
		textMessageLog.text = log;
	}

	//Recieve connection confirmation
	[RPC]
	public void Connected()
	{
		_isConnected = true;
		info.SetActive(true);
		_curState = "Connected";
		UpdateGUI();
	}

	//Log Message
	void logNetworkMessage(string message)
	{
		_net.RPC("logLocalMessage", RPCMode.All, message);
	}

	[RPC]
	void logLocalMessage(string message)
	{
		_messageLog.Add(message);
	}
}
