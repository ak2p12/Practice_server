using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : Player
{
    NetworkManager networkManager;
    // Start is called before the first frame update
    void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        StartCoroutine("CoSendPacket");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            C_Move movePacket = new C_Move();
            movePacket.posX = Random.Range(-50,50);
            movePacket.posY = 0;
            movePacket.posZ = Random.Range(-50,50);

            networkManager.Send(movePacket.Write());
        }
    }
}
