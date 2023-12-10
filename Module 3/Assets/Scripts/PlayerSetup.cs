using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;


public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera camera;

    public GameObject playerUiPrefab;

    private Shooting shooting;

    [SerializeField]
    TextMeshProUGUI playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
        {
            GetComponent<VehicleMovement>().enabled = photonView.IsMine;
            GetComponent<LapController>().enabled = photonView.IsMine;
            camera.enabled = photonView.IsMine;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            GetComponent<VehicleMovement>().enabled = photonView.IsMine;

            shooting = this.GetComponent<Shooting>();

            playerNameText.text = photonView.Owner.NickName;

            if (photonView.IsMine)
            {
                GameObject playerUi = Instantiate(playerUiPrefab);
                camera.enabled = true;
                playerUi.transform.Find("FireButton1").GetComponent<Button>().onClick.AddListener(() => shooting.Fire1());
                playerUi.transform.Find("FireButton2").GetComponent<Button>().onClick.AddListener(() => shooting.Fire2());
                playerUi.transform.Find("QuitGame").GetComponent<Button>().onClick.AddListener(LeaveRoomAndReturnToLobby);
            }
            else
            {
                camera.enabled = false;
            }
        }
    }

    void LeaveRoomAndReturnToLobby()
    {
        PhotonNetwork.Disconnect();

        PhotonNetwork.LoadLevel("LobbyScene");

        PhotonNetwork.ReconnectAndRejoin();
    }
}
