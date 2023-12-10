using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject hitEffectPrefab;

    [Header("Bullet")]
    public Transform bulletOutlet;
    public GameObject bulletPrefab;

    [Header("HP")]
    public float startHealth = 100;
    private float health;
    public Image healthBar;

    private Text killedText;
    private Text winnerText;

    private int playersAlive;
    private string lastAlivePlayer;

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
        killedText = GameObject.Find("Killed Text").GetComponent<Text>();
        winnerText = GameObject.Find("Winner Text").GetComponent<Text>();

        playersAlive = PhotonNetwork.PlayerList.Length;
        lastAlivePlayer = "";
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    public void Fire1()
    {
        RaycastHit hit;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.47f));

        if (Physics.Raycast(ray, out hit, 200))
        {
            Debug.Log(hit.collider.gameObject.name);
            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);
            
            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 10);
            }
        }
    }

    public void Fire2()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletOutlet.transform.position, transform.rotation);
        StartCoroutine(TravelBullet(bullet));
    }

    IEnumerator TravelBullet(GameObject bullet)
    {
        float distanceTraveled = 0f;
        float maxDistance = 500f;

        while (distanceTraveled < maxDistance)
        {
            distanceTraveled += bullet.GetComponent<BulletMovement>().speed * Time.deltaTime;
            bullet.transform.Translate(Vector3.forward * bullet.GetComponent<BulletMovement>().speed * Time.deltaTime);
            yield return null;
        }

        Destroy(bullet);
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

            playersAlive--;
            if (playersAlive == 1)
            {
                lastAlivePlayer = info.Sender.NickName;
                photonView.RPC("GameWin", RpcTarget.AllBuffered, lastAlivePlayer);
            }

            string killedInfo = info.Sender.NickName + " killed " + info.photonView.Owner.NickName;
            photonView.RPC("DisplayKilledInfo", RpcTarget.AllBuffered, killedInfo);
        }
    }

    [PunRPC]
    private void GameWin(string winnerName)
    {
        if (string.IsNullOrEmpty(lastAlivePlayer))
        {
            winnerText.text = "No winner!";
        }
        else
        {
            winnerText.text = "The winner is " + winnerName + "!";
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
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }

    public void Die()
    {
        if (photonView.IsMine)
        {
            transform.GetComponent<VehicleMovement>().enabled = false;
        }
    }
}
