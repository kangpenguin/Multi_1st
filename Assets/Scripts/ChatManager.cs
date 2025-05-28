using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ChatManager : MonoBehaviourPunCallbacks
{
    PlayerScript playerScript;
    GameManager gameManager;

    public TMP_InputField ChatInput;
    public TextMeshProUGUI[] ChatText;

    public PhotonView PV;

    int i = 0;

    private void Awake()
    {
        ChatInput.text = "";
    }

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    [PunRPC]
    public void Chat(string msg)
    { 
        if (i <= (ChatText.Length - 1))
        {
            ChatText[i].text = msg;
            ChatInput.text = "";

            i++;
        } 
        else
        {
            for (int j = 0; j <= (ChatText.Length - 2); j++)
                ChatText[j].text = ChatText[j + 1].text;

            ChatText[i - 1].text = msg;
            ChatInput.text = "";
        }
    }

    public void Send()
    {
        playerScript = gameManager.player.GetComponent<PlayerScript>();

        PV.RPC("Chat", RpcTarget.All,  playerScript.GetKeyword(playerScript.state) + PhotonNetwork.NickName + " : " + ChatInput.text);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        Chat("<color=yellow>" + otherPlayer.NickName + "님이 방 나갔어요 ㄷㄷ</color>");
    }
}
