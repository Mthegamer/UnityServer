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
	public Text textConnectionList;

	//Private
	private NetworkView _net;
	private int _serverPort = 1456;
	private int _maxConnections = 20;
	private string _password = "";
	private bool _isRunning = false;
	private NetworkPlayer[] _connections;
	private List<PendingPlayer> _pendingPlayers = new List<PendingPlayer>();
	private List<PlayerData> _activePlayers = new List<PlayerData>();
	private List<string> _messageLog = new List<string>();

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
		textIP.text = "Server IP: " + Network.natFacilitatorIP + ":" + _serverPort;
		_connections = Network.connections;
		textConnections.text = "Connections: " + _connections.Length + "/" + _maxConnections;
		if (_connections.Length == 0)
		{
			textConnectionList.text = "No connections";
			return;
		}
	}

	//Listen for Connections
	void OnPlayerConnected(NetworkPlayer player)
	{
		_pendingPlayers.Add(new PendingPlayer(player, Time.time + timeout));
		UpdateGUI();
	}
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		UpdateGUI();
	}

	//Receive Player Data
	[RPC]
	public void PlayerData(PlayerData playerData)
	{
		NetworkPlayer p = playerData.getNetPlayer();
		int index = -1;
		for(int i = 0; i < _pendingPlayers.Count; i++)
		{
			if (p == _pendingPlayers[i].getPlayer())
				index = i;
		}
		if(index == -1)
		{
			Network.CloseConnection(p, true);
			return;
		}
		_activePlayers.Add(playerData);
		_pendingPlayers.RemoveAt(index);
		_net.RPC("Connected", p);
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

	//Get Set serverPort
	public int getServerPort()
	{
		return _serverPort;
	}
	public void setServerPort(string port)
	{
		int.TryParse(port, out _serverPort);
	}
	//Get Set maxConnections
	public int getMaxConnections()
	{
		return _maxConnections;
	}
	public void setMaxConnections(string max)
	{
		int.TryParse(max, out _maxConnections);
	}
	//Get Set password
	public string getPassword()
	{
		return _password;
	}
	public void setPassword(string password)
	{
		_password = password;
	}
}
