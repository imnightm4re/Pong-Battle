﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{

    public float normalSpeedBackup = 0.2f;
    public float normalSpeed = 0.2f;
    public float runSpeed = 0.4f;
    public float topBotSize = 23.5f;
    public float leftRightSize = 23.5f;
    public float pushRadius = 5f;
    public TextMesh playerName;

    public float timerPush = 7f;
    public float cdPush = 7f;

    Rigidbody rb;

    public bool isPlayer = false;

    private void Awake()
    {

        if (photonView.IsMine)
        {
            isPlayer = true;
            rb = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame 
    void Update()
    {
        if (timerPush < cdPush)
        {
            timerPush += Time.deltaTime;
        }
        //if (!isLocalPlayer)
        //{
        //    return;
        //}
        if (isPlayer == true)
        {

            if ((Input.GetKey(KeyCode.W)) && (transform.position.y >= topBotSize == false))
            {
                transform.Translate(new Vector3(0, normalSpeed, 0));
            }


            if ((Input.GetKey(KeyCode.S)) && (transform.position.y <= -topBotSize == false))
            {
                transform.Translate(new Vector3(0, -normalSpeed, 0));
            }


            if ((Input.GetKey(KeyCode.D)) && (transform.position.x >= leftRightSize == false))
            {

                transform.Translate(new Vector3(normalSpeed, 0, 0));

            }

            if ((Input.GetKey(KeyCode.A)) && (transform.position.x <= -leftRightSize == false))
            {

                transform.Translate(new Vector3(-normalSpeed, 0, 0));

            }

            if (Input.GetKeyDown(KeyCode.F) && timerPush >= cdPush)
            {
                timerPush = 0;
                Debug.Log("PULSANDO F");
                Push();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isPlayer)
        {
            Debug.Log("COLLIDING");

            switch (collision.gameObject.tag)
            {
                case "ball":

                    Die();
                    break;
            }
        }
    }



    void Die()
    {
        Destroy(this.gameObject);
        photonView.RPC("DestroyPlayer", RpcTarget.OthersBuffered);
    }

    void Push()
    {
        Debug.Log("BUSCANDO OBJETOS");
        Collider[] inArea = Physics.OverlapSphere(transform.position, pushRadius);
        Debug.Log("Objetos en INAREA "+inArea.Length);
        int[] idArray = new int[4];
        int contador = 0;



        for (int i = 0; i < inArea.Length; i++)
        {
            Debug.Log(i+". "+inArea[i].gameObject.name);


            if (inArea[i].gameObject.tag == "player")
            {
                Debug.Log("Añadimos este");
                idArray[contador] = inArea[i].gameObject.GetComponent<PhotonView>().ViewID;
                contador++;
            }

        }

        Debug.Log("Objetos en Array antes de enviar " + idArray.Length);

        if (idArray != null)
        {
            this.transform.position = new Vector3(0, 0, 0);
            photonView.RPC("pushBack", RpcTarget.All, idArray, gameObject.transform.position, "");
        }
                
    }
    
    [PunRPC]
    void DestroyPlayer(PhotonMessageInfo info)
    {
        Destroy(info.photonView.gameObject);

    }

    [PunRPC]
    void pushBack(int[] idArray, Vector3 position, string lol)
    {

        Debug.Log("RPC RECIBIDO POR "+photonView.ViewID+" QUE CORRESPONDE A "+gameObject.name+" Y EL ARRAY TIENE "+idArray.Length+" objetos");

        bool shouldIPush = false;

        for (int i = 0; i < idArray.Length; i++)
        {
            Debug.Log("Mi ID es " + photonView.ViewID + " y el ID a comparar es " + idArray[i]);
            if (idArray[i] == photonView.ViewID)
            {
                shouldIPush = true;
            }
        }
        
        if (shouldIPush)
        {
            Debug.LogError("AÑADIENDO FUERZA A RB");
            rb.AddExplosionForce(15, position, pushRadius);
            
        }
    }

    void OnDrawGizmos()
    {
        // Display the explosion radius when selected
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, pushRadius);
    }



}
