using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;

public class UIManager : MonoBehaviourPun //원래는 네트워크 포팅을 하면 안되지만 마감을 앞당기기 위해 어쩔 수 없이 한다.
{
    public GameObject notice; //게임 중 화면 위에 뜰 공지다.
    public Transform noticeCanvas; //Canvas의 자식 오브젝트가 되기 위해 필요하다.

    public GameObject Info; //술래가 풀려난 뒤 뜰 정보다.
    public Animator Infoanimator;

    public Animator startUIanimator; 
    public GameObject startUI;

    public GameObject sceneLoader; //씬 변환 효과를 위한 UI다.
    public Animator sceneLoaderanimator;

    public GameObject attack; //술래의 공격버튼이다.
    public GameObject jump; //점프 버튼이다.
    public GameObject interaction; //상호작용 버튼이다.
    public GameObject none; //그냥 버튼이다.

    public GameObject button1; //실제로 사용되는 버튼이다.
    public GameObject button2;

    Joystick joystick; //조이스틱이다.
    public GameObject Phone_Pad; //이것도 그에 해당한다.

    public GameObject PC_Pad; //이것도 그에 해당한다.

    public GameObject nickname;
    TextMeshProUGUI nicknameText;

    public GameObject lasttime;
    TextMeshProUGUI lasttimeText;

    public GameObject remainSurvivors;

    public GameObject URMURDER;

    public GameObject playerUI;

    public GameObject ChatUI;

    public GameObject Others;

    public GameObject EndUI;

    public GameObject GhostUI;

    public Sprite NoneImage;
    public Sprite PlayerImage;

    public GameObject resultText, resultImage;

    GameObject player;

    GameManager gameManager; //UI매니저가 접근해도 되는건가.?
    //허나 어쩔 수 없다. 희생자와 술래의 이름을 얻어야한다..
    
    public bool is_interacted = false;

    public float time;

    void Awake()
    {
        lasttimeText = lasttime.GetComponent<TextMeshProUGUI>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        notice.SetActive(false);
        lasttime.SetActive(false);
        URMURDER.SetActive(false);

        sceneLoader.SetActive(true);
        startUI.SetActive(true);

        joystick = Phone_Pad.transform.GetChild(0).GetComponent<Joystick>();

#if UNITY_ANDROID

        Phone_Pad.SetActive(true);
        PC_Pad.SetActive(false);

        joystick.enabled = true;

        jump.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = ""; //키 이름을 나타내는 텍스트를 활성화
        attack.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
        interaction.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";


#elif UNITY_STANDALONE

        Phone_Pad.SetActive(false);
        PC_Pad.SetActive(true);

        joystick.enabled = false;

        jump.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "SPACE"; //키 이름을 나타내는 텍스트를 활성화
        attack.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "P";
        interaction.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "P";

#endif

        remainSurvivors.SetActive(false);
        Info.SetActive(false);

        attack.SetActive(false);
        jump.SetActive(false);
        interaction.SetActive(false);
        none.SetActive(false);

        button1 = jump;
        button2 = none;

        button1.SetActive(true);
        button2.SetActive(true);
    }

    void Start()
    {
        
    }

    void Update()
    {
       
    }
    
    public IEnumerator Notice(int noticeCode)
    {
        //DontDestroyOnLoad(notice);

        GameObject noticeprefeb = Instantiate(notice) as GameObject;
        TextMeshProUGUI noticeprefebText = noticeprefeb.GetComponent<TextMeshProUGUI>();

        noticeprefeb.SetActive(true);
        noticeprefeb.transform.SetParent(noticeCanvas, false);

        switch (noticeCode)
        {
            case 2:
                noticeprefebText.color = Color.red;
                noticeprefebText.text = "15초 남았습니다!";
                break;
            case 3:
                noticeprefebText.color = Color.white;
                noticeprefebText.text = "게임 종료!";
                break;
            case 4:
                noticeprefebText.color = Color.red;
                noticeprefebText.text = "술래가 풀려났습니다!";
                break;
        }

        yield return new WaitForSeconds(7f);

        Destroy(noticeprefeb); //7초 후 제거
    }

    IEnumerator NoticeWithName(int noticeCode, string name)
    {
        GameObject noticeprefeb = Instantiate(notice) as GameObject;
        TextMeshProUGUI noticeprefebText = noticeprefeb.GetComponent<TextMeshProUGUI>();

        noticeprefeb.SetActive(true);
        noticeprefeb.transform.SetParent(noticeCanvas, false);

        switch (noticeCode)
        {
            case 1:
                noticeprefebText.color = Color.red;
                noticeprefebText.text = name + "님을 피해 1분 동안 살아남으세요!";
                break;
            case 5:
                noticeprefebText.color = Color.red;
                noticeprefebText.text = name + "님이 잡히고 말았습니다!";
                break;
        }

        yield return new WaitForSeconds(7f);

        Destroy(noticeprefeb); //7초 후 제거
    }

    public void StartNoticeWithName(int noticeCode, string name)
    {
        StartCoroutine(NoticeWithName(noticeCode, name));
    }

    public void RenewalCountUI(int var)
    {
        remainSurvivors.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = var + "";
    }

    public IEnumerator UpdateTime()
    {
        lasttime.SetActive(true);
        lasttimeText.color = Color.white;

        time = 0;

        while (time <= 6.9) //이 부분은 술래가 풀려나기 전이다.
        {
            time += Time.deltaTime; //시간은 흐른다

            lasttimeText.text = "술래가 " + (7 - (int)time) + "초 후에 풀려납니다";

            yield return null;
        }

        time = 1;

        while (time <= 60) //이제 술래가 풀려나고 본격적인 시작이다. 60초가 되기 전,
        {
            time += Time.deltaTime; //시간은 흐른다

            lasttimeText.text = "" + (int)time;

            if (time >= 55) //55초가 지났다 = 5초가 남았다.
                lasttimeText.color = Color.red; //시간을 빨간색으로 표시한다.

            yield return null;
        }

        //timeover = true;
    }

    IEnumerator OnGameOver()
    {
        yield return new WaitForSeconds(1f);

        playerUI.SetActive(false);
        ChatUI.SetActive(false);
        Others.SetActive(false);
        GhostUI.SetActive(false);
        EndUI.SetActive(true);
        button1.SetActive(false);
        button2.SetActive(false);

        if (gameManager.player.GetComponent<PlayerScript>().state == PlayerScript.State.Citizen ||
            gameManager.player.GetComponent<PlayerScript>().state == PlayerScript.State.Ghost)
        {
            resultText.GetComponent<TextMeshProUGUI>().text
            = gameManager.winner == PlayerScript.State.Citizen ? "승리하였습니다!" : "패배하였습니다.";
        }
        else
        {
            resultText.GetComponent<TextMeshProUGUI>().text
            = gameManager.winner == PlayerScript.State.Murder ? "승리하였습니다!" : "패배하였습니다.";
        }

        resultText.SetActive(true);

        yield return new WaitForSeconds(2f);

        resultText.SetActive(false);
        resultImage.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            PlayerScript mur = gameManager.murder.GetComponent<PlayerScript>();

            if (mur != null)
            {
                photonView.RPC("SetEndUI_Murder", RpcTarget.All, false, mur.playername, (gameManager.victims + "킬"));
            }
            else
            {
                photonView.RPC("SetEndUI_Murder", RpcTarget.All, true, "없음", "");
            }

            for (int i = 0; i < 5; i++)
            {
                if (i >= gameManager.citizensCount)
                {
                    photonView.RPC("SetEndUI_Citizens", RpcTarget.All, i, true, "없음", "", "");
                }
                else
                {
                    PlayerScript pl = gameManager.citizens[i].GetComponent<PlayerScript>();

                    string text1 = pl.is_Dead ? pl.survive_time + "초" : "생존";

                    string text2 = pl.is_FirstBlood ? "퍼블ㅋ" : "";

                    photonView.RPC("SetEndUI_Citizens", RpcTarget.All, i, false, pl.playername, text1, text2);
                }
            }
        }

        int endtime = 0;

        while (endtime < 5)
        {
            resultImage.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = (5 - endtime) + "초 후 로비로 이동합니다.";

            yield return new WaitForSeconds(1f);
            endtime++;
        }

        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void SetEndUI_Citizens(int i, bool IsNone, string name, string t1, string t2)
    {
        Transform citizens = resultImage.transform.GetChild(1);

        citizens.GetChild(i).GetComponent<Image>().sprite = IsNone ? NoneImage : PlayerImage;

        citizens.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;

        citizens.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = t1;

        citizens.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text = t2;
    }

    [PunRPC]
    void SetEndUI_Murder(bool IsNone, string name, string kills)
    {
        Transform murderImage = resultImage.transform.GetChild(0);

        murderImage.GetComponent<Image>().sprite = IsNone ? NoneImage : PlayerImage;

        murderImage.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;

        murderImage.GetChild(1).GetComponent<TextMeshProUGUI>().text = kills;
    }

    public void SetGameOverUI() => photonView.RPC("RPCOnGameOver", RpcTarget.All);

    [PunRPC] void RPCOnGameOver() => StartCoroutine(OnGameOver());

    public void SetObserverMode()
    {
        playerUI.SetActive(false);
        button1.SetActive(false);
        button2.SetActive(false);
        GhostUI.SetActive(true);
    }

    public void SetObserverUI(string name)
    {
        GhostUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
    }

    void invokeNotice4()
    {
        StartCoroutine("Notice", 4);
    }

    public void interactable(bool param)
    {
        button1.SetActive(true);
        button2.SetActive(true);

        button1.GetComponent<Button>().interactable = param;
        button2.GetComponent<Button>().interactable = param;
        joystick.interactable = param;
    }

    public void changeButton(int num, string str) //이 로직은 대체 무엇일까
    {
        if (num == 1)
        {
            button1.SetActive(false);

            if (str == "None")
                button1 = none;
            if (str == "Interaction")
                button1 = interaction;
            if (str == "Attack")
                button1 = attack;

            button1.SetActive(true);
        }
        else if (num == 2)
        {
            button2.SetActive(false);

            if (str == "None")
                button2 = none;
            if (str == "Interaction")
                button2 = interaction;
            if (str == "Attack")
                button2 = attack;

            button2.SetActive(true);
        }
    }

    public void vanishInfo()
    {
        Infoanimator.SetTrigger("Vanish");

        Destroy(Info, 3f);
    }

    public void appearInfo()
    {
        Info.SetActive(true);
    }

    public void startUIFade()
    {
        startUIanimator.SetTrigger("fade");

        Invoke("afterFade", 5f);
    }

    void afterFade()
    {
        startUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}
