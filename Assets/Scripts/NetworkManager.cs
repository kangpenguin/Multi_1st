using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "TestOnSeolnal_2";

    public TextMeshProUGUI connectionInfo;

    public LevelManager levelManager;

    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion; //게임 버전 설정

        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.ConnectUsingSettings(); //해당 설정대로 서버 접속

        connectionInfo.text = "서버에 접속중입니다. . .";
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        connectionInfo.text = "서버 접속에 성공하였습니다.";

        levelManager.LoadLevel();

        levelManager.StartsceneLoad();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        connectionInfo.text = "접속 실패 : 네트워크 연결 여부를 확인하십시오.";
    }
}
