using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


[RequireComponent (typeof(NetworkView))]
public class Server : MonoBehaviour 
{
	public enum NetMode
	{
		Client,
		Server
	}
	//Public
	public NetMode mode;
	public GameObject main;
	public GameObject info;
	public float timeout = 2f;

	public Text textIP;
	public Text textServerState;
	public Text textConnections;
	public Text textMessageLog;


	//Private
	private NetworkView _net;
	private PlayerData _player;
	private string _playerName = "playerName";
	private string _serverIP = "127.0.0.1";
	private int _serverPort = 1234;
	private int _maxConnections = 5;
	private string _password = "";
	private bool _isRunning = false;
	private bool _isConnected = false;
	private NetworkPlayer[] _connections;
	private Dictionary<NetworkPlayer, PendingPlayer> _pendingPlayers = new Dictionary<NetworkPlayer,PendingPlayer>();
	private Dictionary<NetworkPlayer, PlayerData> _activePlayers = new Dictionary<NetworkPlayer, PlayerData>();
	private List<string> _messageLog = new List<string>();
	private string _curState = "Disconnected";

	//Getter Setters
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
	public string maxConnections 
	{
		get
		{
			return _maxConnections.ToString();
		}
		set
		{
			int.TryParse(value, out _maxConnections);
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

	//Start Server
	public void StartServer()
	{
		if (mode != NetMode.Server)
			return;
		Application.runInBackground = true;
		Network.InitializeSecurity();
		bool useNat = !Network.HavePublicAddress();
		Network.incomingPassword = _password;
		Network.InitializeServer(_maxConnections, _serverPort, useNat);
		Debug.Log("Starting Server");
		main.SetActive(false);
		info.SetActive(true);
		_isRunning = true;
		_net = GetComponent<NetworkView>();
		UpdateGUI();
	}
	
	//Connect to Server
	public void Connect()
	{
		if (mode != NetMode.Client)
			return;
		Application.runInBackground = true;
		_net = GetComponent<NetworkView>();
		Network.Connect(_serverIP, _serverPort, _password);
		_player = new PlayerData(_playerName);
		main.SetActive(false);
		info.SetActive(true);
		_curState = "Connecting...";
		UpdateGUI();
	}


	//Update GUI
	void UpdateGUI()
	{
		if (mode == NetMode.Server)
		{
			textIP.text = "Server IP: " + Network.player.ipAddress + ":" + serverPort;
			_connections = Network.connections;
			textConnections.text = "Connections: " + _activePlayers.Count + "/" + maxConnections;
		}else
		{
			textServerState.text = "State: " + _curState;
			textConnections.text = "Connections: " + Network.connections.Length.ToString();
		}
		string log = "";
		for (int i = 0; i < _messageLog.Count; i++ )
		{
			if (i < _messageLog.Count - 1)
				log += _messageLog[i] + "\n";
			else
				log += _messageLog[i];
		}
		textMessageLog.text = log;
	}

	//Listen for Connections
	void OnPlayerConnected(NetworkPlayer player)
	{
		if (mode != NetMode.Server)
			return;
		_pendingPlayers.Add(player, new PendingPlayer(player, Time.time + timeout));
		logLocalMessage(player.ipAddress + " attempting connection.");
		UpdateGUI();
	}
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		if (mode != NetMode.Server)
			return;
		_activePlayers.Remove(player);
		UpdateGUI();
	}

	//Send Player Data
	void OnConnectedToServer()
	{
		if (mode != NetMode.Client)
			return;
		_net.RPC("PlayerData", RPCMode.Server, _player);
		_curState = "Authenthicating...";
		UpdateGUI();
	}

	void OnDisconnectedFromServer()
	{
		if (mode != NetMode.Client)
			return;
		_curState = "Disconnected";
		UpdateGUI();
		Application.LoadLevel(Application.loadedLevel);
	}

	//Receive Player Data
	[RPC]
	public void PlayerData(PlayerData playerDdata)
	{
		if (mode != NetMode.Server)
			return;
		PlayerData data = playerDdata as PlayerData;
		NetworkPlayer p = data.getNetPlayer();
		logLocalMessage(data.getPlayerName() + "'s playerData received.");
		if(_pendingPlayers.ContainsKey(p) && !_activePlayers.ContainsValue(data))
		{
			_pendingPlayers.Remove(p);
			_activePlayers.Add(p, data);
			_net.RPC("Connected", p);
			logLocalMessage(data.getPlayerName() + "'s connection was accepted.");
		}else
		{
			Network.CloseConnection(p, true);
			logLocalMessage(data.getPlayerName() + "'s connection was rejected.");
		}
		UpdateGUI();
	}

	//Recieve connection confirmation
	[RPC]
	public void Connected()
	{
		if (mode != NetMode.Client)
			return;
		_isConnected = true;
		info.SetActive(true);
		_curState = "Connected";
		UpdateGUI();
	}

	//Get Connection List
	string getConnectionList()
	{
		string connectionList = "";
		for (int i = 0; i < _connections.Length; i++)
		{
			NetworkPlayer cur = _connections[i];
			connectionList += cur.externalIP;
			connectionList += ":" + cur.externalPort;
			connectionList += "\n";
		}
		return connectionList;
	}

	//Log Message
	void logNetworkMessage(string message)
	{
		_net.RPC("logLocalMessage", RPCMode.All, message);
	}

	[RPC]
	void logLocalMessage(string message)
	{
		Debug.Log(message);
		_messageLog.Add(message);
	}
}
