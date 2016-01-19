using UnityEngine;
using System.Collections;

public class RoomJoiner : MonoBehaviour {

	public string RoomName { get; set; }
	
	public void Join () {
		PhotonNetwork.JoinRoom(RoomName);
	}
}
