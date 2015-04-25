using UnityEngine;
using System.Collections;

public class PendingPlayer
{
	private float _timeout;
	private NetworkPlayer _player;

	public PendingPlayer(NetworkPlayer player, float timeout)
	{
		_timeout = timeout;
		_player = player;
	}

	//Get _player
	public NetworkPlayer getPlayer()
	{
		return _player;
	}
	//Get _timeout
	public float getTimeout()
	{
		return _timeout;
	}
}
