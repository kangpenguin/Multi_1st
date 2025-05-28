using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public GameObject nicknameButton;
    public GameObject nicknameButtonText;
    public GameObject nicknameInput;

    public GameObject roomButton;
    public GameObject roomButtonText;

    public GameObject roompasswordInput;
    public GameObject roomnameInput;

    public GameObject roompasswordInput2;
    public GameObject wrong;

    public GameObject cancelNicknameInputButton;
    public GameObject cancelRoommakeInputButton;

    public GameObject GameStartButton;

    public GameObject LeaveRoomButton;

    public GameObject RoomPanel;

    public GameObject RoomInto;

    public GameObject ChatInput, ChatSend, epilogue;
    public TextMeshProUGUI[] ChatText;

    public GameObject playerNick;

    public GameObject RoomInfo;
    public GameObject players;

    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;

    public Sprite HostImage, ClientImage, NoneImage;

    public string playername; //나중에 모든 씬에 통일되겠지.
    string A;
    string B;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    public PhotonView PV;

    public LevelManager levelManager;

    bool is_started = false;
    bool chain = false;
    bool chain2 = false;

    void Start()
    {
        PhotonNetwork.JoinLobby();

        PhotonNetwork.NickName = PlayerPrefs.GetInt("firstConnect") != 1 ? PlayerPrefs.GetString("name") : "player";

        playerNick.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.NickName;  
    }

    public void openNicknameInput()
    {
        cancelNicknameInputButton.SetActive(true);

        nicknameButton.GetComponent<Button>().interactable = false;
        nicknameButtonText.SetActive(false);
        nicknameInput.SetActive(true);
    }

    public void closeNicknameInput()
    {
        cancelNicknameInputButton.SetActive(false);

        string beforeNick = nicknameInput.GetComponent<TMP_InputField>().text;
        
        PhotonNetwork.NickName = beforeNick.Replace('@', '#').Replace(' ', '#').Substring(0, 6); 
        //비밀방에 사용될 @는 사전에 없애두고, 공백도 없앤다. 그 상태에서 6글자까지의 문자열만 가진다.

        playerNick.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.NickName;

        PlayerPrefs.SetString("name", playerNick.GetComponent<TextMeshProUGUI>().text);
        PlayerPrefs.SetInt("firstConnect", 1);

        nicknameInput.GetComponent<TMP_InputField>().text = "";

        nicknameButton.GetComponent<Button>().interactable = true;
        nicknameButtonText.SetActive(true);
        nicknameInput.SetActive(false);
    }

    public void cancelNicknameInput()
    {
        cancelNicknameInputButton.SetActive(false);

        nicknameInput.GetComponent<TMP_InputField>().text = "";

        nicknameButton.GetComponent<Button>().interactable = true;
        nicknameButtonText.SetActive(true);
        nicknameInput.SetActive(false);
    }

    public void openRoommakeInput()
    {
        roomnameInput.GetComponent<TMP_InputField>().text = "";
        roompasswordInput.GetComponent<TMP_InputField>().text = "";

        cancelRoommakeInputButton.SetActive(true);

        roomButton.GetComponent<Button>().interactable = false;
        roomButtonText.SetActive(false);
        roomnameInput.SetActive(true);
    }

    public void Sec_openRoommakeInput()
    {
        roomnameInput.SetActive(false);
        roompasswordInput.SetActive(true);
    }

    public void closeRoommakeInput()
    {
        cancelRoommakeInputButton.SetActive(false);

        roomButton.GetComponent<Button>().interactable = true;
        roomButtonText.SetActive(true);
        roompasswordInput.SetActive(false);
    }

    public void cancelRoommakeInput()
    {
        cancelRoommakeInputButton.SetActive(false);

        roomButton.GetComponent<Button>().interactable = true;
        roomButtonText.SetActive(true);
        roomnameInput.SetActive(false);
        roompasswordInput.SetActive(false);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        OnBacktoLobby();
    }

    public void MyListClick(int num)
    {
        if (num == -2) currentPage--;
        else if (num == -1) currentPage++;
        else
        {
            if(!myList[multiple + num].IsOpen)
            {
                wrong.GetComponent<TextMeshProUGUI>().text = "게임 진행 중입니다!";

                wrong.SetActive(true);

                Invoke("setactivewrong", 1.1f);

                return;
            }
            
            string[] str = myList[multiple + num].Name.Split('@');
            string RoomName = str[0];
            string Password = str[1];
            string isitPrivate = Password == " " ? "Public" : "Private";
            string HostName = str[2];

            if (isitPrivate == "Public")
            {
                PhotonNetwork.JoinRoom(myList[multiple + num].Name);
            }
            else
            {
                A = RoomName + '@';
                B = '@' + HostName;

                roomButton.GetComponent<Button>().interactable = false;
                roomButtonText.SetActive(false);
                roompasswordInput2.SetActive(true);
            }
        }

        MyListRenewal();
    }

    public void SetPassword()
    {
        PhotonNetwork.JoinRoom(A + roompasswordInput2.GetComponent<TMP_InputField>().text + B);

        roompasswordInput2.GetComponent<TMP_InputField>().text = "";
        roomButtonText.SetActive(true);
        roompasswordInput2.SetActive(false);
        roomButton.GetComponent<Button>().interactable = true;
    }

    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            if (multiple + i < myList.Count)
            {
                string[] str = myList[multiple + i].Name == null ? null : myList[multiple + i].Name.Split('@');
                string RoomName = str[0];
                string Password = str[1];
                string isitPrivate = Password == " " ? "Public" : "Private";
                string HostName = str[2];

                CellBtn[i].interactable = true;
                CellBtn[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = RoomName;
                CellBtn[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = HostName;
                CellBtn[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers;
                CellBtn[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = isitPrivate;
            }
            else
            {
                CellBtn[i].interactable = false;
                CellBtn[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
                CellBtn[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                CellBtn[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                CellBtn[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) // 로비에 있을 때, 방 리스트 업데이트 시
    {
        Debug.Log("Updated");

        base.OnRoomListUpdate(roomList);

        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }


    public void CreateRoom()
    {
        //비밀번호와 방장 이름을 붙여두면 방장 이름과 비밀번호를 다른 클라이언트에서 꺼낼 수 있다.

        string password = roompasswordInput.GetComponent<TMP_InputField>().text;
        password = password.Length > 10 ? password.Substring(0, 10) : password; //10자로 길이 제한한 방 이름

        string name = roomnameInput.GetComponent<TMP_InputField>().text;
        name = name.Length > 4 ? name.Substring(0, 4) : name; //4자로 길이 제한한 방 비밀번호
        name = name == "" ? " " : name.Replace('@', '#'); //비번이 없으면 공백, 있으면 골뱅이를 샵으로 바꾼다.
        name = "@" + name + "@" + PhotonNetwork.LocalPlayer.NickName; //방 이름에 붙일 비밀번호와 방장 이름이다.

        PhotonNetwork.CreateRoom(password == "" ? "Room" + Random.Range(0, 100) + name : password.Replace('@', '#') + name, new RoomOptions { MaxPlayers = 6 });
        //방 이름을 정해두지 않으면 임의로 정해준다.
    }

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom()
    {
        if (chain)
            return;

        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        RoomPanel.transform.GetChild(0).GetComponent<Animator>().SetTrigger("vanish"); //RoomCells
        RoomPanel.transform.GetChild(1).GetComponent<Animator>().SetTrigger("Ipop");

        ChatInput.GetComponent<TMP_InputField>().interactable = true;
        ChatSend.GetComponent<Button>().interactable = true;
        epilogue.SetActive(false);

        nicknameButton.SetActive(false);
        roomButton.SetActive(false);
        GameStartButton.SetActive(true);
        LeaveRoomButton.SetActive(true);

        ChatInput.GetComponent<TMP_InputField>().text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";

        RoomRenewal();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        RoomPanel.transform.GetChild(0).GetComponent<Animator>().SetTrigger("pop");
        RoomPanel.transform.GetChild(1).GetComponent<Animator>().SetTrigger("Ivanish");

        if (is_started)
        {
            StopCoroutine("GameStart");

            is_started = false;
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        PhotonNetwork.JoinLobby();
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        roomnameInput.GetComponent<TMP_InputField>().text = ""; CreateRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);

        roomnameInput.GetComponent<TMP_InputField>().text = ""; CreateRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        //비밀번호가 틀렸을 때 : Game dose not exist, 32758
        //방이 다 찼을 때 :
        //

        if(returnCode == 32758)
        {
            wrong.GetComponent<TextMeshProUGUI>().text = "비밀번호가 틀렸거나\n방이 사라졌습니다!";

            wrong.SetActive(true);

            Invoke("setactivewrong", 1.1f);
        }

        Debug.Log(returnCode);
    }

    void setactivewrong()
    {
        wrong.SetActive(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        RoomRenewal();

        Chat("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        RoomRenewal();

        Chat("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");

        if (is_started)
        {
            StopCoroutine("GameStart");

            Chat("<color=green>플레이어의 퇴장으로 인하여 취소되었습니다.</color>");

            PV.RPC("IsStarted", RpcTarget.AllBuffered, false);
            PV.RPC("Chain", RpcTarget.AllBuffered, false);

            chain2 = false;
            GameStartButton.GetComponent<Button>().interactable = true;

            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    [PunRPC]
    void Chat(string msg)
    {
        //if (ChatInput.GetComponent<TMP_InputField>().text.Length == 0)
                //return;

        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }

    public void Send()
    {
        PV.RPC("Chat", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.GetComponent<TMP_InputField>().text);

        ChatInput.GetComponent<TMP_InputField>().text = "";
    }

    void RoomRenewal()
    {
        string[] str = PhotonNetwork.CurrentRoom.Name.Split('@');

        RoomInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = str[0];
        RoomInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PhotonNetwork.PlayerList[0].NickName;
        RoomInfo.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
        RoomInfo.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = str[1];

        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            if (i < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                if(i == 0)
                    players.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().sprite = HostImage;
                else
                    players.transform.GetChild(i).transform.GetChild(0).GetComponent<Image>().sprite = ClientImage;

                players.transform.GetChild(i).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PhotonNetwork.PlayerList[i].NickName;
            }
            else
            {
                players.transform.GetChild(i).transform.GetChild(0).GetComponent<Image>().sprite = NoneImage;
                players.transform.GetChild(i).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }

        GameStartButton.GetComponent<Button>().interactable = PhotonNetwork.IsMasterClient ? true : false;
        
        if (PhotonNetwork.IsMasterClient)
        {
            GameStartButton.GetComponent<Button>().interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 2 ? true : false;
            GameStartButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                !GameStartButton.GetComponent<Button>().interactable ? "혼자선 게임 못합니다..ㅎ" : "";
        }
        else
        {
            GameStartButton.GetComponent<Button>().interactable = false;
            GameStartButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "방장이 시작합니다.";
        }
    }

    IEnumerator GameStart()
    {
        chain2 = true;

        if (chain2)
            GameStartButton.GetComponent<Button>().interactable = false;

        PhotonNetwork.CurrentRoom.IsOpen = false;

        int time = 0;
        PV.RPC("IsStarted", RpcTarget.AllBuffered, true);

        while (time <= 4)
        {
            PV.RPC("Chat", RpcTarget.All, "<color=green>" + (5 - time) + "초 후 시작합니다</color>");

            time++;

            yield return new WaitForSeconds(1f);
        }

        PV.RPC("Chat", RpcTarget.All, "<color=green>게임 시작! Have fun :)</color>");

        PV.RPC("Chain", RpcTarget.AllBuffered, true);
        
        PV.RPC("LoadLevelRPC", RpcTarget.All);

        yield return new WaitForSeconds(0.5f);

        PhotonNetwork.IsMessageQueueRunning = false;

        levelManager.StartphotonLoad();
    }

    [PunRPC] void LoadLevelRPC() => levelManager.LoadLevel();
    [PunRPC] void IsStarted(bool bo) => is_started = bo;
    [PunRPC] void Chain(bool bo) => chain = bo;

    public void OnClickGameStartButton()
    {
        StartCoroutine("GameStart");
    }

    void OnBacktoLobby()
    {
        myList.Clear(); //리스트를 비워야 오류가 생기지 않는다.

        ChatInput.GetComponent<TMP_InputField>().interactable = false;
        ChatSend.GetComponent<Button>().interactable = false;
        epilogue.SetActive(true);

        nicknameButton.SetActive(true);
        roomButton.SetActive(true);
        GameStartButton.SetActive(false);
        LeaveRoomButton.SetActive(false);

        nicknameButton.GetComponent<Button>().interactable = true;
        nicknameButtonText.SetActive(true);
        nicknameInput.SetActive(false);

        roomButtonText.SetActive(true);
        roomnameInput.SetActive(false);
        roompasswordInput.SetActive(false);

        ChatInput.GetComponent<TMP_InputField>().interactable = false;
        ChatSend.GetComponent<Button>().interactable = false;
        epilogue.SetActive(true);

        ChatInput.GetComponent<TMP_InputField>().text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";

        MyListRenewal();
    }
}
