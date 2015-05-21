using UnityEngine;
using System.Collections;
using ProtoBuf;

namespace Data
{
	[ProtoContract]
	public class PlayerData
	{
		//Private
		[ProtoMember(1)]
		public string playerName;

		public PlayerData()
		{

		}

		public PlayerData(string playerName)
		{
			this.playerName = playerName;
		}
	}
}
