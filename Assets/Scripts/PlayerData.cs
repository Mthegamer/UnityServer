using UnityEngine;
using System.Collections;
using ProtoBuf;

namespace NetworkData
{
	[ProtoContract]
	public class PlayerData
	{
		//Private
		[ProtoMember(1)]
		public string playerName { get; set; }

		public PlayerData()
		{
			this.playerName = "";
		}

		public PlayerData(string playerName)
		{
			this.playerName = playerName;
		}
	}
}
