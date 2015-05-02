using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof(NetworkView))]
public class Client : MonoBehaviour 
{
	//public
	public GameObject main;
	public GameObject info;

	public Text textServerState;
	public Text textConnections;
	
	//Private
	private PlayerData _player;
	private NetworkView _net;
	private string _playerName;
	private string _serverIP = "127.0.0.1";
	private int _serverPort = 1234;
	private string _password = "";
	private bool _isConnected = false;

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
		_net = GetComponent<NetworkView>();
		Network.Connect(_serverIP, _serverPort, _password);
		_player = new PlayerData(_playerName);
		main.SetActive(false);
		info.SetActive(true);
	}

	//Send Player Data
	void OnConnectedToServer()
	{
		UpdateGUI();
		_net.RPC("PlayerData", RPCMode.Server, _player as object);
	}

	//Update GUI
	void UpdateGUI()
	{
		textServerState.text = "State: " + Network.peerType.ToString();
		textConnections.text = "Connections: " + Network.connections.Length.ToString();
	}

	//Recieve connection confirmation
	[RPC]
	public void Connected()
	{
		_isConnected = true;
		info.SetActive(true);
		UpdateGUI();
	}
}
