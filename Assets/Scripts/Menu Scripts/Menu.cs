using UnityEngine;
using System.Collections;
using Photon;
using UnityEngine.UI;

public class Menu : PunBehaviour {

	// references to GUI game objects, so they can be enabled/disabled and
	// used to show useful messages to player
	public Transform btStart;
	public Transform nickPanel;
	public Transform gamesPanel;
    public Transform playersPanel;

	public Sprite[] carTextures;
	public Sprite noCar;
	public RoomJoiner[] roomButtons;
	public Transform[] playerMenus;
	public InputField edtNickname;
	public Text messages;
	public Sprite[] trackTextures;
	public Transform[] trackButtons;
	public Image trackSprite;

	// connect to photon
	void Start () {
		messages.text = "";
		nickPanel.gameObject.SetActive(true);
	}

	// for each listed room, set a join button from the available button/slots
	void OnGUI () {
		if (!gamesPanel.gameObject.GetActive ())
			return;

		foreach (RoomJoiner bt in roomButtons) {
			bt.gameObject.SetActive(false);
		}

		int index = 0;
		foreach (RoomInfo game in PhotonNetwork.GetRoomList())
		{
			if (index >= roomButtons.Length || !game.open)
				break;
			RoomJoiner button = roomButtons[index++];
			button.gameObject.SetActive(true);
			button.RoomName = game.name;
			string info = game.name.Trim() + " (" + game.playerCount + "/" + game.maxPlayers + ")";
			button.GetComponentInChildren<Text>().text = info;
		}
	}

	// called when finished editing nickname (which will also serve as 
	// room name - if player creates one)
	public void EnteredNickname() {
		PhotonNetwork.player.name = edtNickname.text;
		PhotonNetwork.ConnectUsingSettings("v1.0");
		messages.text = "Connecting...";
	}

    // when connected to Photon, enable nickname editing
    public override void OnConnectedToMaster()
    {
		PhotonNetwork.JoinLobby ();
		messages.text = "Entering lobby...";
    }

	// when connected to Photon Lobby, disable nickname editing, enables game list
	public override void OnJoinedLobby () {
		nickPanel.gameObject.SetActive(false);
		messages.gameObject.SetActive(false);
        gamesPanel.gameObject.SetActive(true);
	}

	// called from UI
	public void CreateGame () {
		RoomOptions options = new RoomOptions();
		options.maxPlayers = 4;
		PhotonNetwork.CreateRoom(edtNickname.text, options, TypedLobby.Default);
	}

	// if we join (or create) a room, no need for the create button anymore
	public override void OnJoinedRoom () {
        gamesPanel.gameObject.SetActive(false);
        playersPanel.gameObject.SetActive(true);
        SetCustomProperties(PhotonNetwork.player, 0, PhotonNetwork.playerList.Length - 1);
	}

	// (masterClient only) enables start race button
	public override void OnCreatedRoom () {
		btStart.gameObject.SetActive(true);
		SetCustomProperties(PhotonNetwork.player, 0, PhotonNetwork.playerList.Length - 1);
	}

	// if master client, for every newly connected player, sets the custom properties for him
	// car = 0, position = last (size of player list)
	public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer) {
		if (PhotonNetwork.isMasterClient) {
			SetCustomProperties (newPlayer, 0, PhotonNetwork.playerList.Length - 1);
		}
	}

	// when a player disconnects from the room, update the spawn/position order for all
	public override void OnPhotonPlayerDisconnected(PhotonPlayer disconnetedPlayer) {
		if (PhotonNetwork.isMasterClient) {
			int playerIndex = 0;
			foreach (PhotonPlayer p in PhotonNetwork.playerList) {
				SetCustomProperties(p, (int) p.customProperties["car"], playerIndex++);
			}
		}
	}

	public override void OnPhotonPlayerPropertiesChanged (object[] playerAndUpdatedProps) {
		UpdatePlayerList ();
	}

	// updates the players list on the players panel
	public void UpdatePlayerList() {
		Debug.Log ("updating");
		ClearPlayersGUI ();
		int playerIndex = 0;
		foreach (PhotonPlayer p in PhotonNetwork.playerList) {
			Transform playerMenu = playerMenus[playerIndex++];
			// updates icon based on car index (protected for early calls before a new player has own properties set)
			if (p.customProperties.ContainsKey("car")) {
				playerMenu.FindChild("Text").GetComponent<Text>().text = p.name.Trim();
			}
		}
	}

	private void ClearPlayersGUI() {
		foreach (Transform t in playerMenus) {
			t.FindChild("Image").GetComponent<Image>().sprite = noCar;
			t.FindChild("Text").GetComponent<Text>().text = "";
		}
	}

	// masterClient only. Calls an RPC to start the race on all clients. Called from GUI
	public void CallLoadGame() {
		PhotonNetwork.room.open = false;
		photonView.RPC("LoadGame", PhotonTargets.All);
	}

    // loads game level (called once from masterClient).
    // Use LoadLevel from Photon, otherwise it messes up
    // the GOs created in between level changes
    [PunRPC]
	public void LoadGame () {
		PhotonNetwork.LoadLevel("InGame");
	}

	// sets and syncs custom properties on a network player (including masterClient)
	private void SetCustomProperties(PhotonPlayer player, int car, int position) {
		ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() { { "spawn", position }, {"car", car} };
		player.SetCustomProperties(customProperties);
	}
}
