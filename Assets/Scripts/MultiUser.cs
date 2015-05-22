using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NetworkData;

[RequireComponent(typeof(NetworkView))]
public class MultiUser : MonoBehaviour
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
	private string _errorColor = "#ff1111";
	private string _optionalColor = "#44ff44";
	private string _requiredColor = "#4444ff";
	private string _commandColor = "#ffff44";
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
	private Dictionary<NetworkPlayer, PendingPlayer> _pendingPlayers = new Dictionary<NetworkPlayer, PendingPlayer>();
	private Dictionary<NetworkPlayer, PlayerData> _activePlayers = new Dictionary<NetworkPlayer, PlayerData>();
	private Dictionary<string, NetworkPlayer> _netPlayers = new Dictionary<string, NetworkPlayer>();
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
		}
		else
		{
			textServerState.text = "State: " + _curState;
			textConnections.text = "Connections: " + Network.connections.Length.ToString();
		}
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

	public void Command(string msg)
	{
		Debug.Log(msg);
		if (msg.Length == 0)
			return;
		if (msg[0] == '/')
		{
			msg = msg.Substring(1);
			Debug.Log(msg);
			string[] args = msg.Split(' ');
			switch (args[0])
			{
				case "list":
					if (args.Length >= 1) //List connections
					{
						logLocalMessage(getConnectionList());
					}
					else if (args.Length == 2) //List connections and broadcast
					{
						if (mode == NetMode.Server)
						{
							if (args[1] == "true")
								logNetworkMessage("<color=" + _commandColor + ">" + getConnectionList() + "</color>");
							else if (args[1] == "false")
								logLocalMessage("<color=" + _commandColor + ">" + getConnectionList() + "</color>");
							else //List show error message
							{
								logLocalMessage("<b><color=" + _errorColor + ">Invalid Sytax</color></b>");
								logLocalMessage("<b><color=" + _commandColor + ">/list</color> <i><color=" + _optionalColor + "><broadcast?>[true/false]</color></i></b>");
							}
						}
						else
							logLocalMessage("<b><color=" + _errorColor + ">You don't have permission to use this command</color></b>");
					}
					break;
				case "kick":
					if (mode == NetMode.Server)
					{
						if (args.Length >= 2) //kick player
						{
							if (args.Length == 2) //Just kick player
							{
								if (_netPlayers.ContainsKey(args[1]))
								{
									Network.CloseConnection(_netPlayers[args[1]], true);
									logLocalMessage("<color=" + _commandColor + ">Player " + args[1] + ", was kicked</color>");
								}
								else
								{
									logLocalMessage("<color=" + _errorColor + ">Player not found.</color>");
								}
							}
							else if (args.Length == 3) //Kick and send reason
							{
								string reason = string.Join("", args, 2, args.Length - 3);
								Network.CloseConnection(_netPlayers[args[2]], true);
								logNetworkMessage("<color=" + _commandColor + ">Player " + args[2] + ", was kicked for " + reason + "</color>");
							}
						}
						else
						{
							logLocalMessage("<color=" + _errorColor + ">Invalid Sytax</color>");
							logLocalMessage("<b><color=" + _commandColor + ">/kick</color> <color=" + _requiredColor + "><playerName></color> <color=" + _optionalColor + "><i><reason></i></color></b>");
						}
					}
					else
						logLocalMessage("<b><color=" + _errorColor + ">You don't have permission to use this command</color></b>");
					break;
			}
		}
		else
		{
			logNetworkMessage(msg);
		}
	}

	//Listen for Connections
	void OnPlayerConnected(NetworkPlayer player)
	{
		if (mode == NetMode.Server)
		{
			_pendingPlayers.Add(player, new PendingPlayer(player, Time.time + timeout));
			logLocalMessage(player.ipAddress + " attempting connection.");
		}
		UpdateGUI();
	}
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		if (mode != NetMode.Server)
			return;
		logNetworkMessage(_activePlayers[player].playerName + " has disconneted.");
		_netPlayers.Remove(_activePlayers[player].playerName);
		_activePlayers.Remove(player);
		UpdateGUI();
	}

	//Send Player Data
	void OnConnectedToServer()
	{
		if (mode != NetMode.Client)
			return;
		_net.RPC("PlayerData", RPCMode.Server, ProtoLoader.serializeData<PlayerData>(_player), Network.player);
		_curState = "Authenthicating...";
		UpdateGUI();
	}

	//Receive Player Data
	[RPC]
	public void PlayerData(byte[] playerData, NetworkPlayer netPlayer)
	{
		if (mode != NetMode.Server)
			return;
		PlayerData data = ProtoLoader.deserializeData<PlayerData>(playerData);
		logLocalMessage(data.playerName + "'s playerData received.");
		if (_pendingPlayers.ContainsKey(netPlayer) && !_activePlayers.ContainsValue(data))
		{
			_pendingPlayers.Remove(netPlayer);
			_activePlayers.Add(netPlayer, data);
			_netPlayers.Add(data.playerName, netPlayer);
			_net.RPC("Connected", netPlayer);
			logLocalMessage(data.playerName + "'s connection was accepted.");
		}
		else
		{
			Network.CloseConnection(netPlayer, true);
			logLocalMessage(data.playerName + "'s connection was rejected.");
		}
		UpdateGUI();
	}

	//Connection loss
	void OnDisconnectedFromServer()
	{
		_curState = "Disconnected";
		UpdateGUI();
		Application.LoadLevel(Application.loadedLevel);
	}
	void OnFailedToConnect()
	{
		_curState = "Failed to Connect";
		UpdateGUI();
		Application.LoadLevel(Application.loadedLevel);
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
		if (connectionList.Length == 0)
			return "There are no players online.";
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
		UpdateGUI();
	}
}
