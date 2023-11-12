using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject hitEffectPrefab;

    [Header("HP Related Stuff")]
    public float startHealth = 100;
    private float health;
    public Image healthBar;

    private Animator animator;

    private int kills = 0;
    private Text killedText;
    private Text winnerText;

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
        animator = this.GetComponent<Animator>();
        killedText = GameObject.Find("Killed Text").GetComponent<Text>();
        winnerText = GameObject.Find("Winner Text").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire()
    {
        RaycastHit hit;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out hit, 200))
        {
            Debug.Log(hit.collider.gameObject.name);

            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 25);
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        this.health -= damage;
        this.healthBar.fillAmount = health / startHealth;

        if (health <= 0)
        {
            Die();
            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);

            // Display who killed who on the UI
            string killedInfo = info.Sender.NickName + " killed " + info.photonView.Owner.NickName;
            photonView.RPC("DisplayKilledInfo", RpcTarget.AllBuffered, killedInfo);

            kills++;
            if (kills >= 10)
            {
                string winnerName = info.Sender.NickName;
                photonView.RPC("GameWin", RpcTarget.AllBuffered, info.Sender.NickName);
            }
        }
    }

    [PunRPC]
    private void DisplayKilledInfo(string killedInfo)
    {
        StartCoroutine(ShowKilledInfo(killedInfo));
    }

    IEnumerator ShowKilledInfo(string killedInfo)
    {
        killedText.text = killedInfo;
        yield return new WaitForSeconds(5.0f);
        killedText.text = "";
    }

    [PunRPC]
    private void GameWin(string winnerName)
    {
        winnerText.text = "The winner is " + winnerName + "!";

        GameObject.Find("GameManager").GetComponent<GameManager>().EndGame();
    }

    [PunRPC]
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }

    public void Die()
    {
        if (photonView.IsMine)
        {
            animator.SetBool("isDead", true);
            StartCoroutine(RespawnCountdown());
        }
    }

    IEnumerator RespawnCountdown()
    {
        GameObject respawnText = GameObject.Find("Respawn Text");
        float respawnTime = 5.0f;

        while (respawnTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            transform.GetComponent<PlayerMovementController>().enabled = false;
            respawnText.GetComponent<Text>().text = "You are killed. Respawning in " + respawnTime.ToString(".00");
        }

        animator.SetBool("isDead", false);
        respawnText.GetComponent<Text>().text = "";

        Transform spawnPoint = SpawnManager.Instance.GetRandomSpawnPoint();

        this.transform.position = spawnPoint.position;
        transform.GetComponent<PlayerMovementController>().enabled = true;

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RegainHealth()
    {
        health = 100;
        healthBar.fillAmount = health / startHealth;
    }
}
