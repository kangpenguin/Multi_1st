using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInput : MonoBehaviourPun
{
    public int move;

    Joystick joystick;

    Transform PCpad;

    public bool moving; //움직이는 입력 유뮤에 대한 불
    public bool jumping; //점프하는 입력 유무에 대한 불
    public bool attacking; //공격하는 입력 유무에 대한 불

    public PhotonView pv;

    void Awake()
    {
#if UNITY_ANDROID

        joystick = GameObject.FindGameObjectWithTag("Joystick").GetComponent<Joystick>();

#elif UNITY_STANDALONE

        PCpad = GameObject.FindGameObjectWithTag("PCPad").transform;

        PCpad.GetChild(0).GetComponent<Animator>().SetInteger("direction", move);

#endif
    }

    void Update()
    {
#if UNITY_ANDROID


#elif UNITY_STANDALONE //키보드를 통한 입력이 필요하다.

        moving = Input.GetButton("Horizontal");
        jumping = Input.GetButtonDown("Jump");
        attacking = Input.GetButtonDown("Attack");

#endif
    }

    void FixedUpdate()
    {
        if (!pv.IsMine)
            return;

#if UNITY_ANDROID

        move = joystick.move;

#elif UNITY_STANDALONE

        move = (int)Input.GetAxisRaw("Horizontal");

#endif
    }
}
