﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoccerGame;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class BallController : MonoBehaviour
{

    [System.Serializable]
    public class OffsetConfiguration
    {
        [SerializeField]
        [Range(-2.00f, 2.00f)]
        private float forward = 0.65f;
        public float Forwad { get { return forward; } }
        [SerializeField]
        [Range(-2.00f, 2.00f)]
        private float up = 0.25f;
        public float Up { get { return up; } }
        [SerializeField]
        [Range(-2.00f, 2.00f)]
        private float right = 0f;
        public float Right { get { return right; } }

        public Vector3 GetMult(Transform transform)
        {

            Vector3 target = transform.position + transform.forward * Forwad + transform.right * Right + transform.up * Up;
            return target;

        }
    }
    public delegate void OnSetOwner(PlayerController owner, PlayerController lasOwner);
    public delegate void OnRemoveOwner(PlayerController lasOwner);

    public static BallController instance;

    [Header("Ball Controller")]
    [SerializeField]
    private PlayerController owner;
    [SerializeField]
    private Collider fovTriger;
    [SerializeField]
    private float speed_return = 5.5f;
    [SerializeField]
    private float speed_forward = 5.5f;
    [SerializeField]
    private OffsetConfiguration iddleOffset;
    [SerializeField]
    private OffsetConfiguration runOffset;

    public bool HasMyOwner { get { return owner != null; } }

    public event OnSetOwner onSetMyOwner;
    public event OnRemoveOwner onRemoveMyOwner;

    private Animator animator;
    private new Rigidbody rigidbody;

    PlayerController lastOwner;
    private float timeToSetOwner = 0.0f;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        if (owner)
        {
            timeToSetOwner = 99f;
            PlayerController player = owner;
            UnsetmeOwner();
            SetmeOwner(player);
        }
    }

    void Update()
    {

        if (owner)
        {
            if (owner.isMovie == false)
            {
                //Posiciona a bola no offset predefinido em iddleOffset e
                //Rotaciona a bola para a mesma rotação do player, isto evita bugs com animações rootmotion da bola
                Vector3 targetPos = iddleOffset.GetMult(owner.transform);
                transform.position = Vector3.Lerp(transform.position, targetPos, speed_return * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, owner.transform.rotation, 3.2f * Time.deltaTime);
            }
            else
            {
                //Posiciona a bola no offset predefinido em runOffset e
                //Rotaciona a bola no sentido de movimento do jogador para dar a impreção de bola rolando
                Vector3 targetPos = runOffset.GetMult(owner.transform);
                float targetRot = (owner.transform.forward * 360 * Time.deltaTime).magnitude;


                transform.position = Vector3.Lerp(transform.position, targetPos, speed_forward * Time.deltaTime);
                transform.Rotate(owner.transform.right, targetRot * 1.8f, Space.World);


            }
        }
        timeToSetOwner += Time.deltaTime;
    }
    private void FixedUpdate()
    {

    }

    void OnAnimatorMove()
    {
        /*Usado para animação da bola em sincronia com a do jogador
        if (animator.GetBool("IdleDrible"))
        {
            transform.position = animator.rootPosition;
            transform.rotation = animator.rootRotation;
        }
        */
    }

    public void SetmeOwner(PlayerController player)
    {
        if (player == null)
            return;

        if (timeToSetOwner <= 0.5f)
            return;

        if (onSetMyOwner != null)
            onSetMyOwner(player, owner);

        lastOwner = owner;
        owner = player;

        SetKinematic();

        timeToSetOwner = 0.0f;
    }
    public void UnsetmeOwner()
    {
        lastOwner = owner;
        owner = null;

        if (onRemoveMyOwner != null)
            onRemoveMyOwner(lastOwner);

        UnsetKinematic();


    }
    public void SetmeKick()
    {

        if (owner == null)
            return;

        PlayerController playerFromKick = owner;
        UnsetmeOwner();
        rigidbody.AddForce(playerFromKick.transform.forward * 20, ForceMode.Impulse);
        rigidbody.AddForce(playerFromKick.transform.up * 8, ForceMode.Impulse);

    }
    public void ChangemeDirection()
    {
        if (owner == null)
            return;

        PlayerController old = owner;
        UnsetmeOwner();

        rigidbody.AddForce(old.transform.up * 3.5f, ForceMode.Impulse);
        rigidbody.AddForce(-old.transform.forward * 2.0f, ForceMode.Impulse);

    }
    public void SetBallProtected()
    {
        fovTriger.enabled = false;
    }
    public void SetDesprotectBall()
    {
        fovTriger.enabled = true;
    }
    public PlayerController GetMyOwner()
    {
        return owner;
    }
    public bool IsMyOwner(PlayerController player)
    {
        return player == owner;
    }

    public static void ChangeDirection()
    {
        instance.ChangemeDirection();
    }
    public static bool HasOwner()
    {
        return instance.HasMyOwner;
    }
    public static bool IsOwner(PlayerController player)
    {
        return instance.IsMyOwner(player);
    }
    public static void SetKick()
    {
        instance.SetmeKick();
    }
    public static void SetOwner(PlayerController player)
    {
        if (player != null)
        {
            instance.SetmeOwner(player);
        }
    }
    public static void UnsetOwner()
    {
        instance.UnsetmeOwner();
    }

    private void SetKinematic()
    {
        rigidbody.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;

    }
    private void UnsetKinematic()
    {
        GetComponent<Collider>().isTrigger = false;
        rigidbody.isKinematic = false;
    }

}
