using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Cinemachine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class PlayerScript : MonoBehaviourPun, IPunObservable
{
    public PhotonView pv;

    public enum State
    {
        None, Citizen, Murder, Ghost
    }
    public State state;

    [HideInInspector]public bool is_Dead = false;
    private bool is_Grounded = true;
    private bool has_picked = false;
    private bool interactionable = false;

    GameManager Gamemanager;
    UIManager UImanager;

    PlayerInput playerInput;
    Rigidbody2D rb2D;
    SpriteRenderer spriteren;
    Animator animator;

    Axe axe;

    TextMeshProUGUI nicknameText;

    Vector3 curPos;

    public bool can_move; //게임매니저가 플레이어를 멈출때 사용한다.

    int jumpForce = 350;
    public int jumpCount = 0;
    public int jumpLimit;

    int ii = 0;

    float speed;

    int diretion;
    int lastdiretion;

    public string playername; //플레이어의 닉네임이다.

    public bool is_FirstBlood = false;

    public bool is_spawned = false;

    bool testFB = false;

    public float survive_time;

    void Start()
    {
        PhotonPeer.RegisterType(typeof(Vector3), (byte)'W', SerializationVec3.SerializeVector2, SerializationVec3.DeserializeVector2);

        playerInput = GetComponent<PlayerInput>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteren = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        Gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        
        UImanager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        if(pv.IsMine)
        {
            UImanager.jump.GetComponent<Button>().onClick.AddListener(Jump);
            UImanager.attack.GetComponent<Button>().onClick.AddListener(Attack);
            UImanager.interaction.GetComponent<Button>().onClick.AddListener(Pick);
        }

        Transform canvas = gameObject.transform.GetChild(0);
        nicknameText = canvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        state = State.None; //시작할 때에는 아무것도 아님

        nicknameText.text =  pv.IsMine ? playername : pv.Owner.NickName;
        nicknameText.color = pv.IsMine ? Color.yellow : Color.white;

        if (pv.IsMine)
        {
            jumpLimit = 2;
            speed = 5;
        }

        if (pv.IsMine)
        {
            CinemachineVirtualCamera cam = FindObjectOfType<CinemachineVirtualCamera>();

            cam.Follow = transform;
        }
        else
            playername = pv.Owner.NickName;

        can_move = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(gameObject.transform.position);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
        } 
    }

    void Update()
    {
        animator.SetInteger("move", playerInput.move);

        diretion = playerInput.move;

        is_Grounded = 
            Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Tilemap")) ||
            (Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(-0.5f, -0.3f), 0.07f, 1 << LayerMask.NameToLayer("Tilemap")) && spriteren.flipX) ||
            (Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0.5f, -0.3f), 0.07f, 1 << LayerMask.NameToLayer("Tilemap")) && !spriteren.flipX);

        animator.SetBool("is_Grounded", is_Grounded);

        if (is_Grounded)
            jumpCount = 0;

        if (playerInput.move != 0)
            lastdiretion = diretion;
        else
            diretion = lastdiretion;
    }

    public void Attack()
    {
        if (!pv.IsMine)
            return;

        if (axe.coend && state == State.Murder) //도끼도 안던지고 있고 술래이면
        {
            axe.BeforeVisible1(false);

            axe.BeforeVisible(true);

            axe.BeforeThrow(diretion); //지금 바라보고 있는 방향을 전달하고 코루틴 실행
        }
    }
        
    void FixedUpdate()
    {
        if (!pv.IsMine)
        {
            if ((transform.position - curPos).sqrMagnitude >= 500) transform.position = curPos;
            else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);

            return;
        }
            
#if UNITY_ANDROID

        if (can_move)
        {
            Move();
        }

#elif UNITY_STANDALONE

        if (can_move)
        {
            if (playerInput.moving) Move();
            if (playerInput.jumping) Jump();
            if (playerInput.attacking)
            {
                if (Gamemanager.play_game) Attack();
                else Pick();
            }
        }

#endif
    }

    [PunRPC]
    public void Dead() //호스트에서 실행될 수 밖에 없다.
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("Dead", RpcTarget.Others);

            Gamemanager.GetPlayers();

            for (int i = 0; i < Gamemanager.players.Count; i++)
            {
                if (Gamemanager.players[i].GetComponent<PlayerScript>().is_FirstBlood) //퍼블이 있다면
                {
                    testFB = true;
                }
            }
            //루프를 빠져나오고
            if (!testFB) //퍼블이 없다면
            {
                pv.RPC("SetFirstBlood", RpcTarget.All, true); //내가 퍼블이네..
            }
        }

        if (!is_Dead && state == State.Citizen && Gamemanager.play_game) //시민인데 죽었으면
        { //단, 죽지 않은 상태이고 게임이 진행중이여야 한다.
            //
            spriteren.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            GetComponent<BoxCollider2D>().enabled = false;
            nicknameText.enabled = false;
            //자신을 감춘다.

            is_Dead = true; //난 죽었어요.

            pv.RPC("ImGhost", RpcTarget.All);

            survive_time = Mathf.Ceil(Gamemanager.time);

            Gamemanager.playerDied(playername);
        }
    }

    [PunRPC]
    void FlipXRPC(int dir) => spriteren.flipX = (dir < 0); //좌우 변환을 결정한다.

    public void GetStart()
    {
        if (!pv.IsMine)
            return;

        //범인이 됨과 동시에 다른 사람은 시민이 되어야한다.
        //범인은 크기과 색깔이 달라진다.
        jumpLimit = 1; //술래는 점프 기회가 최대 1번이다.
        speed = 6.75f;
        UImanager.changeButton(2, "Attack");

        pv.RPC("SetMurderNick", RpcTarget.All);

        Gamemanager.BeforeGameStart();
    }

    [PunRPC] public void ImMurder() => state = State.Murder; //나는 범인이다.

    [PunRPC] void ImCitizen() => state = State.Citizen; //시민임을 설정.

    public void RPCImCitizen() => pv.RPC("ImCitizen", RpcTarget.All);

    [PunRPC] void SetFirstBlood(bool bo) => is_FirstBlood = bo;

    [PunRPC]
    public void ImGhost()
    {
        state = State.Ghost; //유령(관전자)임을 설정.

        if (pv.IsMine && Gamemanager.play_game)
        {
            UImanager.SetObserverMode();

            UImanager.GhostUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(ObservePrevious);
            UImanager.GhostUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(ObserveNext);

            SetCamera(0);
        }
    }

    [PunRPC]
    public void SetMurderNick()
    {
        nicknameText.color = Color.red;
    }

    void Move()
    {
        Vector2 vector2 = new Vector2(playerInput.move, 0);

        transform.Translate(vector2 * Time.deltaTime * speed);

        if(playerInput.move != 0)
            pv.RPC("FlipXRPC", RpcTarget.All, diretion);
    }

    public void Jump()
    {
        if (!pv.IsMine)
            return;

        if (jumpCount < jumpLimit)     
        {
            jumpCount++;

            rb2D.velocity = Vector2.zero;

            rb2D.AddForce(new Vector2(0, jumpForce)); 
        }
    }

    public void Pick() //도끼와 상호작용 했을 시 발동된다.
    {
        if (!pv.IsMine)
            return;

        if (!interactionable)
            return;

        axe = GameObject.FindGameObjectWithTag("Weapons").GetComponent<Axe>();

        axe.BeforeVisible(false); //도끼를 감춘다.
        pv.RPC("ImMurder", RpcTarget.All); //그 사람이 술래다.

        GetStart();
    }

    void SetCamera(int ii)
    {
        Gamemanager.GetPlayers_Observed();

        if (pv.IsMine)
        {
            CinemachineVirtualCamera cam = FindObjectOfType<CinemachineVirtualCamera>();

            cam.Follow = Gamemanager.players_observed[ii].transform;
        }

        UImanager.SetObserverUI(Gamemanager.players_observed[ii].GetComponent<PlayerScript>().playername);
    }

    public void ObservePrevious()
    {
        ii--;

        if (ii <= 0)
            ii = 0;

        SetCamera(ii);
    }

    public void ObserveNext()
    {
        ii++;

        if (ii >= Gamemanager.players_observed.Count)
            ii = Gamemanager.players_observed.Count;

        SetCamera(ii);
    }

    public string GetKeyword(State state)
    {
        if (state == State.Ghost)
            return "유령 ";
        else if (state == State.Citizen)
            return "";
        else if (state == State.Murder)
            return "<color=red>" + "술래 ";
        else
            return "";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!pv.IsMine)
            return;

        if (collision.tag == "Weapons") //닿은게 무기면
        {
            if (state == State.None) //누군가가 처음으로 잡았으면
            {
                //여기서 버튼이 '상호작용'으로 바뀌어야한다.
                UImanager.changeButton(2, "Interaction"); //ㅋㅋㅋㅋㅋㅋ ㅇㄴ
                //이 스크립트는 UImanager를 취하지 않는다.
                interactionable = true;

            //이 뒤는 버튼에 맡긴다.
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!pv.IsMine)
            return;

        if (collision.tag == "Weapons") //떨어진게 무기면
        {
            if (state == State.None)
            {
                UImanager.changeButton(2, "None");

                interactionable = false;
            }
        }
    }
}
