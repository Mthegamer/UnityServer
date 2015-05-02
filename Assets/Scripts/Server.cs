using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(NetworkView))]
public class Server : MonoBehaviour 
{
	//Public
	public GameObject main;
	public GameObject info;
	public float timeout = 2f;

	public Text textIP;
	public Text textConnections;


	//Private
	private NetworkView _net;
	private bool _isRunning = false;
	private int _serverPort = 1234;
	private int _maxConnections = 5;
	private string _password = "";
	private NetworkPlayer[] _connections;
	private Dictionary<NetworkPlayer, PendingPlayer> _pendingPlayers = new Dictionary<NetworkPlayer,PendingPlayer>();
	private Dictionary<NetworkPlayer, PlayerData> _activePlayers = new Dictionary<NetworkPlayer, PlayerData>();
	private List<string> _messageLog = new List<string>();

	//Getter Setters
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

	//Update GUI
	void UpdateGUI()
	{
		textIP.text = "Server IP: " + Network.player.ipAddress + ":" + serverPort;
		_connections = Network.connections;
		textConnections.text = "Connections: " + _connections.Length + "/" + maxConnections;
	}

	//Listen for Connections
	void OnPlayerConnected(NetworkPlayer player)
	{
		_pendingPlayers.Add(player, new PendingPlayer(player, Time.time + timeout));
		UpdateGUI();
	}
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		_activePlayers.Remove(player);
		UpdateGUI();
	}

	//Receive Player Data
	[RPC]
	public void PlayerData(object playerData)
	{
		PlayerData data = playerData as PlayerData;
		NetworkPlayer p = data.getNetPlayer();
		if(_pendingPlayers.ContainsKey(p) && !_activePlayers.ContainsValue(data))
		{
			_pendingPlayers.Remove(p);
			_activePlayers.Add(p, data);
			_net.RPC("Connected", p);
		}else
		{
			Network.CloseConnection(p, true);
		}
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
}
