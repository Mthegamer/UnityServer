using UnityEngine;
using System.Collections;

public class PlayerData
{
	//Private
	private string _playerName;
	private NetworkPlayer _netPlayer;

	public PlayerData(string playerName, NetworkPlayer netPlayer)
	{
		_playerName = playerName;
		_netPlayer = netPlayer;
	}

	public PlayerData(string playerName)
	{
		_playerName = playerName;
		_netPlayer = Network.player;
	}

	//Get _playerName
	public string getPlayerName()
	{
		return _playerName;
	}
	
	//Get _netPlayer
	public NetworkPlayer getNetPlayer()
	{
		return _netPlayer;
	}
}
