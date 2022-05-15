using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Connector : MonoBehaviourPunCallbacks
{
    private Library.Player Player { get => PhotonNetwork.IsMasterClient ? Library.Player.Green : Library.Player.Red; }
    private bool IsGameStarted { get; set; } = false;

    [SerializeField]
    private Transform desk;

    private GameObject[] levels;

    public UnityEvent OnPhotonConnected;
    public UnityEvent<Library.Player> OnRoomJoined;
    public UnityEvent OnRoomFilled;
    public UnityEvent<Board> OnGameStarted;
    public UnityEvent OnOtherPlayerLeftGame;

    private void Awake() => levels = Resources.LoadAll<GameObject>("Boards/Balanced");

    private void Start() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        OnPhotonConnected.Invoke();
        PhotonNetwork.JoinRandomOrCreateRoom(null, 2, MatchmakingMode.FillRoom, TypedLobby.Default, null, null, new RoomOptions { MaxPlayers = 2, PublishUserId = true });
    }

    public override void OnJoinedRoom()
    {
        OnRoomJoined.Invoke(Player);

        if (!PhotonNetwork.IsMasterClient)
            OnRoomFilled.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            OnRoomFilled.Invoke();
            PhotonNetwork.CurrentRoom.IsVisible = false;
            StartCoroutine(LateStartCall());
        }
    }

    public void Restart() => PhotonNetwork.LeaveRoom();

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!IsGameStarted)
            Restart();
        else
            OnOtherPlayerLeftGame.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message) => PhotonNetwork.Disconnect();

    public override void OnLeftRoom()
    {
        IsGameStarted = false;
        SceneManager.LoadScene("Multiplayer");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        IsGameStarted = false;
        SceneManager.LoadScene("Start Menu");
    }

    private IEnumerator LateStartCall()
    {
        yield return new WaitForSeconds(Settings.MultiPlayerDelayTime);
        byte level = (byte)Random.Range(0, levels.Length);
        photonView.RPC(nameof(LoadLevel), RpcTarget.All, level);
    }

    [PunRPC]
    private void LoadLevel(byte id)
    {
        if (id < 0 || id >= levels.Length)
            return;

        IsGameStarted = true;

        // create new desk
        Board board = Instantiate(levels[id], desk).GetComponent<Board>();
        StartCoroutine(LateLoad(board));
    }

    private IEnumerator LateLoad(Board board)
    {
        yield return new WaitForEndOfFrame();
        OnGameStarted.Invoke(board);
    }
}
