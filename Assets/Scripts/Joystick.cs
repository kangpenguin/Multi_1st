using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{ //왜 이 스크립트를 백그라운드에 주면 정가운데로 갈까?
    public bool is_dragged;
    public bool interactable;

    public RectTransform back_trasform;
    public RectTransform stick_transform;

    public Animator animator;

    Vector3 pos;
    float range;

    public int move;

    void Awake()
    {
        range = back_trasform.rect.width * 0.5f;
        is_dragged = false;
        interactable = true;
    }

    void Update()
    {
        if (is_dragged)
        {
            if (pos.x > 0)
                move = 1;
            else if (pos.x < 0)
                move = -1;
        }
        else
            move = 0;

        animator.SetInteger("direction", move);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interactable)
            return;

        pos = new Vector3(eventData.position.x - back_trasform.position.x, eventData.position.y - back_trasform.position.y, 0);
        pos = Vector3.ClampMagnitude(pos, range);

        stick_transform.localPosition = pos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        is_dragged = false;

        stick_transform.localPosition = Vector3.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable)
            return;

        is_dragged = true;

        pos = new Vector3(eventData.position.x - back_trasform.position.x, eventData.position.y - back_trasform.position.y, 0);
        pos = Vector3.ClampMagnitude(pos, range);

        stick_transform.localPosition = pos;
    }
}
