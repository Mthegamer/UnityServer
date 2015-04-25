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
	public Text textPeertype;

	//Private
	private string _playerName;
	private string _serverIP = "127.0.0.1";
	private int _serverPort = 1456;
	private string _password;
	private PlayerData _player;
	private NetworkView _net;
	private bool _isConnected = false;
	
	public void Connect()
	{
		_net = GetComponent<NetworkView>();
		Network.Connect(_serverIP, _serverPort, _password);
		_player = new PlayerData(_playerName);
		main.SetActive(false);
	}

	//Send Player Data
	void OnConnectedToServer()
	{
		_net.RPC("PlayerData", RPCMode.Server, _player);
	}

	//Update GUI
	void UpdateGUI()
	{

	}

	[RPC]
	public void Connected()
	{
		_isConnected = true;
		info.SetActive(true);
		UpdateGUI();
	}

	//Get Set _serverIP
	public string getServerIP()
	{
		return _serverIP;
	}
	public void setServerIP(string ip)
	{
		_serverIP = ip;
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

	//Get Set password
	public string getPassword()
	{
		return _password;
	}
	public void setPassword(string password)
	{
		this._password = password;
	}

	//Get Set playerName
	public string getPlayerName()
	{
		return _playerName;
	}
	public void setPlayerName(string playerName)
	{
		_playerName = playerName;
	}
}
