using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Axe : MonoBehaviourPun, IPunObservable
{
    public bool coend = true;

    public float bulletspeed;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    public GameObject[] SpawnPoint = new GameObject[4];
    Rigidbody2D rb2D = null;
    Animator animator = null;

    GameManager gameManager;

    public PhotonView pv;

    Vector3 curPos;

    float time;

    void Awake() //컴포넌트들을 채운다.
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D = GetComponent<Rigidbody2D>();
        
        animator = GetComponent<Animator>();

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        SpawnPoint = GameObject.FindGameObjectsWithTag("WeaponSpawnpoint");
    }

    void Start()
    {
        PhotonPeer.RegisterType(typeof(Vector3), (byte)'X', SerializationVec3.SerializeVector2, SerializationVec3.DeserializeVector2);

        gameObject.SetActive(true);

        if (pv.IsMine)
            transform.position = SpawnPoint[Random.Range(0, 3)].GetComponent<Transform>().position;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if ((transform.position - curPos).sqrMagnitude >= 500) transform.position = curPos;
            else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
        }
    }

    //이미 다른 도끼가 있으면 나머지 지우기 혹은 소환 불가
    [PunRPC]
    public void visible(bool bo)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("visible", RpcTarget.Others, bo);
        }

        gameObject.SetActive(bo);
    }

    public void BeforeVisible(bool bo)
    {
        pv.RPC("visible", RpcTarget.MasterClient, bo);
    }

    public void BeforeVisible1(bool bo)
    {
        pv.RPC("visible1", RpcTarget.MasterClient, bo);
    }

    [PunRPC]
    public void visible1(bool bo)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("visible1", RpcTarget.Others, bo);
        }

        if (bo)
            spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        else
            spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
    }

    public IEnumerator Throw(int diretion, GameObject player) //도끼를 날린다.
    {
        transform.position = player.transform.position;

        BeforeVisible1(true); //도끼를 보인다.

        time = 0f;
        coend = false;
        animator.SetBool("Throw", true);

        while (time < 1f) //던진 지 1초가 지나기 전까지
        {
            transform.Translate(new Vector2(diretion, 0) * Time.deltaTime * bulletspeed); //날라간다.

            time += Time.deltaTime;

            yield return null;
        }

        Vector3 playerPosition = player.transform.position;
        float sqrRemainingDistance = (transform.position - playerPosition).sqrMagnitude; //거리 계산을 한다.

        while (1f < time && sqrRemainingDistance > 0.1f) //1초가 지나고 도끼랑 거리가 0.1 정도로 가까워지기 전까지
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, playerPosition, 4f * Time.deltaTime * bulletspeed);

            rb2D.MovePosition(newPosition); //도끼를 움직인다.

            playerPosition = player.transform.position; //지금의 플레이어의 위치를 구한다. 플레이어는 계속 움직이기 때문이다.
            sqrRemainingDistance = (transform.position - playerPosition).sqrMagnitude; //이 로직 이후 바뀐 거리를 계산한다.

            time += Time.deltaTime;

            yield return null;
        }

        BeforeVisible(false); //도끼를 숨긴다.
        coend = true; //코루틴이 끝났다.
        animator.SetBool("Throw", false);
    }

    public void BeforeThrow(int dir)
    {
        if (!coend)
            return;

        pv.RPC("CallToMaster", RpcTarget.MasterClient, dir);
    }

    [PunRPC]
    public void CallToMaster(int dir)
    {
        StartCoroutine(Throw(dir, gameManager.murder));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!PhotonNetwork.IsMasterClient) //호스트만 판정한다.
            return;

        PlayerScript Target = collision.GetComponent<PlayerScript>(); //트리거에 들어온 사람의 스크립트를 넣고

        //Debug.Log(Target.state == PlayerScript.State.Citizen);
        //Debug.Log(Target.pv.IsMine);

        if (Target != null && Target.state == PlayerScript.State.Citizen) //넣기에 성공, 타겟이 도망자였을때면 (술래가 던진 것이므로)
        {
            Target.Dead();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.transform.position);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
        }
    }
}
