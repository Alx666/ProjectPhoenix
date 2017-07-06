using UnityEngine;
using System.Collections.Generic;
using Pathfinding.RVO;
using Pathfinding;
using System.Linq;
using UnityEngine.Networking;
using System;

public class ControllerAIHumanoid : NetworkBehaviour, IDamageable
{
    [SyncVar]
    Vector3 syncPos;
    [SyncVar]
    float syncRotY;
    Vector3 lastPos;
    Quaternion lastRot;
    Transform myTransform;
    float lerpRate = 3.5f;
    float posThershold = 0.5f;
    float rotThershold = 5f;


    public float Speed;
    public float NextWaypointDistance;
    public GameObject Ragdoll;
    public GameObject WeaponRagdoll;

    internal bool NOTURN = false;

    internal GameObject CurrentEnemy { get; private set; }
    internal RVOController RVOController { get; private set; }
    internal float MaxSpeedRVO { get; private set; }
    bool canMove;

    internal AIPath AI;

    List<IControllerPlayer> NearEnemies;
    float fieldOfView = 110f;
    SphereCollider hearingCollider;
    CapsuleCollider capsuleCollider;
    Animator animator;
    float turnValue;
    float forwardValue;
    int forwardHash;
    int turnHash;
    int toChaseHash;
    int rollLayer;
    int rollingHash;
    Transform weaponLocator;
    IWeapon weapon;
    Seeker seeker;
    Path currentPath;
    int currentWaypoint = 0;
    Vector3 m_GroundNormal;
    public void EndRolling()
    {
        animator.SetBool(rollingHash, false);
    }
    public override void PreStartClient()
    {
        this.weapon = GetComponent<IWeapon>();
        if (!isServer)
        {
            Destroy(GetComponent<AIPath>());
            Destroy(GetComponent<SphereCollider>());
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(GetComponent<Seeker>());
            Destroy(GetComponent<RVOController>());
            Destroy(GetComponent<FunnelModifier>());
        }
    }
    public override void OnStartServer()
    {
        this.AI = GetComponent<AIPath>();
        this.hearingCollider = GetComponent<SphereCollider>();
        this.capsuleCollider = GetComponent<CapsuleCollider>();
        this.forwardHash = Animator.StringToHash("Forward");
        this.turnHash = Animator.StringToHash("Turn");
        this.toChaseHash = Animator.StringToHash("ToChase");
        this.NearEnemies = new List<IControllerPlayer>();
        this.seeker = GetComponent<Seeker>();
        this.RVOController = GetComponent<RVOController>();
        this.MaxSpeedRVO = RVOController.maxSpeed;
    }
    void Start()
    {
        this.weaponLocator = GetComponent<IKControllerHumanoid>().WeaponLocator;
        this.rollLayer = Animator.StringToHash("Base Layer.Roll");
        this.rollingHash = Animator.StringToHash("Rolling");

        this.animator = GetComponent<Animator>();
        this.myTransform = transform;

    }
   
    internal bool IsInFieldOfView(GameObject obj)
    {
        //Setting Height
        Vector3 direction = (obj.transform.position  - (this.transform.position + Vector3.up));

        Debug.DrawRay(transform.position + Vector3.up, direction,Color.black);

        float angle = Vector3.Angle(this.transform.forward, direction);
        if (angle <= fieldOfView * 0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, direction.normalized, out hit, hearingCollider.radius))
            {
                Debug.DrawRay(transform.position + Vector3.up, direction);
                if (hit.collider.gameObject == CurrentEnemy.transform.parent.gameObject)
                {
                    return true;
                }
            }
        }
        return false;
    }
    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.GetComponentInParent<IControllerPlayer>() != null || coll.gameObject.tag == "Enemy")
        {
            RpcDie(coll.relativeVelocity);
        }
    }
    public void Damage(IDamageSource hSource)
    {
        RpcDie(Vector3.up);
    }
    void OnTriggerEnter(Collider other)
    {


        IControllerPlayer enemy = other.gameObject.GetComponent<IControllerPlayer>();

        if (enemy != null || /* enemy is ControllerPlayerWheels || enemy is ControllerSpiderMech ||*/ other.tag == "Enemy")
        {
            if (!NearEnemies.Contains(enemy))
                this.NearEnemies.Add(enemy);

            if (!animator.GetBool(toChaseHash))
            {
                //Prende la transform del veicolo per settare la mira correttamente.  Trovare un altro sistema!!!
                CurrentEnemy = other.gameObject.GetComponent<ControllerWheels>().CentralPoint;

                RpcSetCurrentEnemy(other.gameObject.GetComponent<NetworkIdentity>());
                animator.SetBool(toChaseHash, true);
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        IControllerPlayer enemy = other.gameObject.GetComponent<IControllerPlayer>();

        if (enemy != null || /*enemy is ControllerPlayerWheels || enemy is ControllerSpiderMech ||*/ other.tag == "Enemy")
        {
            if (NearEnemies.Contains(enemy))
                this.NearEnemies.Remove(enemy);

            if (animator.GetBool(toChaseHash) && NearEnemies.Count == 0)
            {
                AI.enabled = false;
                RVOController.Move(Vector3.zero);
                animator.SetBool(toChaseHash, false);
            }
        }
    }

    void SetTransformRagdoll(GameObject ragdoll)
    {
        Transform[] currentTransform = this.GetComponentsInChildren<Transform>();
        Transform[] ragdollTranform = ragdoll.GetComponentsInChildren<Transform>();
        for (int i = 0; i < currentTransform.Length; i++)
        {
            for (int j = 0; j < ragdollTranform.Length; j++)
            {
                if (currentTransform[i].name.GetHashCode() == ragdollTranform[j].name.GetHashCode())
                {
                    ragdollTranform[j].position = currentTransform[i].position;
                    ragdollTranform[j].rotation = currentTransform[i].rotation;

                }
            }
        }
    }
    #region ClientRPC
    [ClientRpc]
    internal void RpcDie(Vector3 force)
    {
        GameObject ragdoll = Instantiate(Ragdoll, this.transform.position, this.transform.rotation) as GameObject;
        SetTransformRagdoll(ragdoll);
        Instantiate(WeaponRagdoll, weaponLocator.position, weaponLocator.rotation);
        ragdoll.GetComponentsInChildren<Rigidbody>().ToList().ForEach(r => r.AddForce(force, ForceMode.VelocityChange));
        this.gameObject.SetActive(false);
    }
    [ClientRpc]
    internal void RpcStopShooting()
    {

        weapon.Release();
    }
    [ClientRpc]
    internal void RpcStartShooting()
    {
        weapon.Press();
    }
    [ClientRpc]
    internal void RpcSetCurrentEnemy(NetworkIdentity id)
    {
        //Prende la transform della torretta del veicolo per settare la mira correttamente. Trovare un altro sistema!!!
        this.CurrentEnemy = ClientScene.FindLocalObject(id.netId).GetComponent<ControllerWheels>().CentralPoint;
    }

    #endregion
    internal void ManuallyStopAgent()
    {
        canMove = false;
        currentPath = null;
        RVOController.Move(Vector3.zero);

    }
    #region VANGUARD MOVE
    internal bool ManuallyMoveAgent()
    {
        if (canMove)
        {
            Vector3 dir = (currentPath.vectorPath[currentWaypoint] - transform.position).normalized;
            dir *= Speed * Time.deltaTime;
            RVOController.Move(dir);
            if (Vector3.Distance(transform.position, currentPath.vectorPath[currentWaypoint]) < NextWaypointDistance)
            {
                currentWaypoint++;

            }
            if (currentWaypoint >= currentPath.vectorPath.Count)
            {
                canMove = false;
                Debug.Log("End Of Path Reached");
                return true;
            }
            return false;
        }
        else
        {
            Debug.LogWarning("Path is not ready!!");
            return false;

        }
    }
    internal void ManuallySetPath(Path path)
    {
        seeker.StartPath(path, OnPathComplete);

    }
    internal void ManuallySetPath(Vector3 pos)
    {
        seeker.StartPath(this.transform.position, pos, OnPathComplete);

    }
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            currentPath = p;
            currentWaypoint = 0;
            canMove = true;
        }

    }
    void OnAnimatorMove()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == rollLayer)
            animator.ApplyBuiltinRootMotion();
    }


    #endregion
    void SendMotion()
    {
        if (!isServer)
            return;
        if (Vector3.Distance(myTransform.position, lastPos) > posThershold || Quaternion.Angle(myTransform.rotation, lastRot) > rotThershold)
        {
            lastPos = myTransform.position;
            lastRot = myTransform.rotation;
            syncPos = myTransform.position;
            syncRotY = myTransform.eulerAngles.y;
        }
    }
    void LerpMotion()
    {
        if (isServer)
            return;

        myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate);
        Vector3 newRotation = new Vector3(0f, syncRotY, 0f);
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.Euler(newRotation), Time.deltaTime * lerpRate);

    }
    void Update()
    {
        SendMotion();
        LerpMotion();

        if (isServer)
        {
            Move(RVOController.velocity);
        }
    }

    internal void Move(Vector3 move)
    {
        //if (move.magnitude > 1f)
        //    move.Normalize();

        move = transform.InverseTransformDirection(move);
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 0.1f))
        {
            m_GroundNormal = hitInfo.normal;
        }

        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        turnValue = Mathf.Atan2(move.x, move.z);
        forwardValue = move.z;
        SetAnimator(move);
        ApplyExtraTurnRotation();
    }
    void ApplyExtraTurnRotation()
    {
        float turnSpeed = Mathf.Lerp(180f, 360f, forwardValue);
        transform.Rotate(0f, turnValue * turnSpeed * Time.deltaTime, 0f);
    }

    void SetAnimator(Vector3 move)
    {
        animator.SetFloat(forwardHash, forwardValue, 0.1f, Time.deltaTime);
        animator.SetFloat(turnHash, turnValue, 0.1f, Time.deltaTime);
        if (move.magnitude > 0)
        {
            animator.speed = 1;
        }
    }

   
}
