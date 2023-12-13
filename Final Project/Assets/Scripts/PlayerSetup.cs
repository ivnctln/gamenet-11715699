using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        FindPlayerCamera(transform); // Start the search for the camera within the hierarchy
        GetComponent<CharacterControls>().enabled = photonView.IsMine;
        GetComponent<WinTrigger>().enabled = photonView.IsMine;

        if (playerCamera != null)
        {
            playerCamera.enabled = photonView.IsMine;
            playerCamera.tag = "MainCamera";
        }
    }

    // Recursive function to find the camera within the hierarchy
    void FindPlayerCamera(Transform currentTransform)
    {
        foreach (Transform child in currentTransform)
        {
            if (child.CompareTag("MainCamera"))
            {
                playerCamera = child.GetComponent<Camera>();
                return; // Stop searching once the camera is found
            }
            else
            {
                FindPlayerCamera(child); // Continue searching in the child's hierarchy
            }
        }
    }
}
