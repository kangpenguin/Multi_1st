using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public UIManager UImanager;

    PlayerScript playerScript;

    public LevelManager levelManager;

    public GameObject playerPrefab;
    public GameObject player;
    public GameObject Axe;

    public GameObject[] SpawnPoint = new GameObject[6];

    public PhotonView pv;

    public bool play_game = false;

    public int count_survivor = 99; //방에 접속한 사람의 수에 1을 뺀 수를 할당하고 차례차례 줄인다.

    public PlayerScript.State winner; //승리 진영이 어느쪽인지를 할당한다.

    public GameObject murder;

    public string murderName = "murder";
    public string victimName = "victim";
    public List<GameObject> citizens = new List<GameObject>();

    public int victims = 99;
    public int citizensCount = 99;

    bool notice2 = false; //한번만 좀 띄우게 하자. Update 함수가 싫어지는 순간이다.

    public List<GameObject> players = new List<GameObject>();
    public List<GameObject> players_observed = new List<GameObject>();

    bool IsMurderExist = true;

    public bool is_first;

    public float time;

    public int ID;

    void Awake()
    {
        PhotonNetwork.IsMessageQueueRunning = true;

        is_first = true;

        SpawnPoint = GameObject.FindGameObjectsWithTag("PlayerSpawnpoint");

        player = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);

        playerScript = player.GetComponent<PlayerScript>(); //게임매니저 넷은 서로 자기 씬의 플레이어를 할당한다.

        playerScript.is_spawned = true;

        playerScript.playername = PhotonNetwork.NickName;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].NickName == player.GetComponent<PlayerScript>().playername)
                ID = i;
        }

        player.gameObject.transform.position = SpawnPoint[ID].GetComponent<Transform>().position;

        Transform nicknameCanvas = player.transform.GetChild(0);
        UImanager.nickname = nicknameCanvas.GetChild(0).GetComponent<GameObject>();

        UImanager.startUIFade();

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Instantiate(Axe.name, Vector3.zero, Quaternion.identity);
    }

    void Update()
    {
        time = UImanager.time;

        if (!play_game)
            return;

        if (time >= 60 && play_game) //타임오버 시 *도망자 전멸로 인한 게임 오버는 RenewalCount() 에서 관리한다.
        {
            if (pv.IsMine)
            {
                play_game = false;
                GameOver(PlayerScript.State.Citizen);
            }//동기화 되므로 호스트만 실행한다.
        }
        if (time >= 45 && !notice2) //15초 남았으면
        {
            notice2 = true;
            UImanager.StartCoroutine("Notice", 2);
        }
    }
    public void GetPlayers()
    {
        players.Clear();

        GameObject[] pl = GameObject.FindGameObjectsWithTag("Player") as GameObject[]; 

        for (int i = 0; i < pl.Length; i++)
            players.Add(pl[i]);
    }

    public void GetPlayers_Observed()
    {
        players_observed.Clear();

        GameObject[] pl = GameObject.FindGameObjectsWithTag("Player") as GameObject[];

        for (int i = 0; i < pl.Length; i++)
        {
            if(pl[i] != player || pl[i].GetComponent<PlayerScript>().state != PlayerScript.State.Ghost)
                players_observed.Add(pl[i]);
        }
           
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(count_survivor);
        }
        else
        {
            count_survivor = (int)stream.ReceiveNext();
            UImanager.RenewalCountUI(count_survivor);
        }
    }

    [PunRPC]
    public void GameStart() //술래가 생길 때 게임 시작이다.
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("GameStart", RpcTarget.Others);
        }

        is_first = false;
        play_game = true;

        UImanager.remainSurvivors.SetActive(true);
        UImanager.Invoke("appearInfo", 7f); //사실 info라는 건 술래 안내만 존재하는게 아니다. 종류를 다양하게 하려면 이건 수정해야한다.
        UImanager.Invoke("invokeNotice4", 7f);
        
        if (playerScript.state == PlayerScript.State.Murder)
        {
            playerScript.can_move = false; //술래를 잠그면서 대기 시간을 준다.

            UImanager.interactable(false); //모든 버튼을 잠근다.
            UImanager.URMURDER.SetActive(true); //7초동안 이 창을 띄워준다.

            Invoke("playerUnChain", 7f); //7초 뒤 술래를 풀어준다.
        }
        if (playerScript.state == PlayerScript.State.None)
        {
            playerScript.RPCImCitizen();
        }

        if(PhotonNetwork.IsMasterClient)
        {
            GetPlayers();

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].GetComponent<PlayerScript>().state == PlayerScript.State.Murder)
                    murder = players[i];
            }

            pv.RPC("SetMurderName", RpcTarget.All, murder.GetComponent<PlayerScript>().playername);
        }

        RenewalCount();

         //머지않아 코드를 자세히 구현한다. 1이 무엇인지.
        UImanager.StartCoroutine("UpdateTime");
    }

    public void BeforeGameStart()
    {
        pv.RPC("GameStart", RpcTarget.MasterClient);
    }

    public void RPCVictimName(string name) => pv.RPC("SetVictimName", RpcTarget.All, name);

    [PunRPC]
    void SetMurderName(string name)
    {
        murderName = name;
        UImanager.StartNoticeWithName(1, murderName);
    }

    [PunRPC]
    public void GameOver(PlayerScript.State win) //시간이 다 되었거나 모든 생존자가 사망했을 경우
    {
        if (play_game)
            return;

        if (PhotonNetwork.IsMasterClient)
            pv.RPC("GameOver", RpcTarget.Others, win);

        winner = win;

        play_game = false;

        UImanager.StartCoroutine("Notice", 3);

        if (PhotonNetwork.IsMasterClient)
        {
            GetPlayers();

            victims = 0;

            citizens = players;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].GetComponent<PlayerScript>().is_Dead)
                {
                    victims++;
                    pv.RPC("RPCVictims", RpcTarget.All, victims);
                }

                if (players[i].GetComponent<PlayerScript>().state == PlayerScript.State.Murder)
                {
                    citizens.Remove(players[i]);
                    citizensCount = citizens.Count;
                    pv.RPC("RPCCitizensCount", RpcTarget.All, citizensCount);
                }
            }

            UImanager.SetGameOverUI();
        }
    }

    [PunRPC] void RPCVictims(int vic) => victims = vic;

    [PunRPC] void RPCCitizensCount(int cit) => citizensCount = cit;

    public void playerUnChain() //술래를 풀 때 사용한다.
    {
        UImanager.URMURDER.SetActive(false); //7초 동안 띄운 창을 감춘다.

        playerScript.can_move = true; //술래는 움직일 수 있게 된다.
        UImanager.interactable(true);
    }

    //RPC가 필요없다.
    public void playerDied(string victimname) //이 메소드를 실행하는 함수는 RPC 호출을 하므로 동기화가 된다.
    {
        UImanager.StartNoticeWithName(5, victimname);

        RenewalCount();
    }

    //RPC가 필요없다.
    public override void OnPlayerLeftRoom(Player otherPlayer) //모든 플레이어에 발동되는 콜백이므로 동기화가 된다.
    {
        base.OnPlayerLeftRoom(otherPlayer);

        RenewalCount();
    }

    void RenewalCount()
    {
        if (PhotonNetwork.IsMasterClient) //count_survivor는 자동 동기화 되므로 호스트만 실행한다.
        {
            IsMurderExist = false;
            count_survivor = PhotonNetwork.CurrentRoom.PlayerCount;

            GetPlayers(); //비우고 다시 채운다

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].GetComponent<PlayerScript>().state == PlayerScript.State.Murder)
                {
                    count_survivor--;
                    IsMurderExist = true;
                }
                else if (players[i].GetComponent<PlayerScript>().state == PlayerScript.State.Ghost)
                    count_survivor--;
            }

            if (!IsMurderExist || count_survivor <= 0)
            {
                count_survivor = 0; //음수가 됨을 방지한다.

                play_game = false;
                StartCoroutine(GameOverdelay(1));
            }

            UImanager.RenewalCountUI(count_survivor);
        }
    }

    IEnumerator GameOverdelay(float time)
    {
        yield return new WaitForSeconds(time);

        GameOver(PlayerScript.State.Murder);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        levelManager.LoadLevel();

        levelManager.StartsceneLoad();
    }
}
