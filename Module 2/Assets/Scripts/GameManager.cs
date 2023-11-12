using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public bool gameEnded = false;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.Instance.GetRandomSpawnPoint();

        if (spawnPoint != null)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError("No valid spawn point found for player!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EndGame()
    {
        if (!gameEnded)
        {
            gameEnded = true;

            StartCoroutine(ReturnToLobby());
        }
    }
    
    private IEnumerator ReturnToLobby()
    {
        yield return new WaitForSeconds(5.0f);

        PhotonNetwork.Disconnect();

        PhotonNetwork.LoadLevel("LobbyScene");
    }
}
