﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public enum BehaviourID { idle, wander, chase, investigate, patrol, alerted }

public class AI : MonoBehaviour
{
    
    [Header("AI Settings")]
    public float walkSpeed = 1.2f;
    public float chaseSpeed = 1.7f;
    public BehaviourID initialState = BehaviourID.idle;
    [Header("Wander Behaviour")]
    public Bounds boundBox;
    [Header("Detection Settings")]
    public float viewRadius;
    public float viewAngle = 45f;
    public float soundDetectionRadius;
    public LayerMask sightDontIgnore;
    [Header("Patrol Settings")]
    public int points = 5;

    [SerializeField]
    private Vector3[] storedPatrolPoints = new Vector3[0];
    [Header("Target Settings")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Vector3 targetPosition = Vector3.zero;
    private NavMeshAgent agent;
    private Animator animator;
    [Header("State")]
    [SerializeField]
    private BehaviourID currentState;
    private float timeIdle = 0f;
    private Vector3 debugMiddle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (storedPatrolPoints.Length == 0)
        {
            storedPatrolPoints = new Vector3[points];
            for (int i = 0; i < storedPatrolPoints.Length; i++)
            {
                storedPatrolPoints[i] = GetRandomPosInBounds();
            }
        }
        SetState(initialState);
    }

    // Update is called once per frame
    void Update()
    {
        debugMiddle = new Vector3(transform.position.x, agent.height / 2, transform.position.z);
        CanSeePlayer();
        // Check line of sight
        if (target != null)
        {
            targetPosition = target.position;
        }
        float targetDistance = Vector3.Distance(transform.position, targetPosition);
        switch (currentState)
        {
            case BehaviourID.idle:
                animator.SetBool("Walking", false);
                if (Time.time >= timeIdle)
                {
                    SetState(BehaviourID.wander);
                }
                break;
            case BehaviourID.wander:
                animator.SetBool("Walking", true);
                if (targetDistance <= agent.stoppingDistance)
                {
                    GetNewWanderPoint();
                }
                break;
            case BehaviourID.chase:
                animator.SetBool("Walking", true);
                if (target != null)
                {
                    agent.SetDestination(targetPosition);
                    if (targetDistance <= agent.stoppingDistance)
                    {
                        SetState(BehaviourID.idle);
                    }
                }
                else
                {
                    SetState(BehaviourID.wander);
                }
                break;
        }
    }

    private void CheckLineOfSight()
    {

    }

    private bool CanSeePlayer()
    {
        
        Vector3 playerPosition = Camera.main.transform.position;
        Vector3 vectorToPlayer = playerPosition - transform.position;

        if (Vector3.Distance(debugMiddle, playerPosition) <= viewRadius)
        {
            if (Vector3.Angle(transform.forward, vectorToPlayer) <= viewAngle)
            {
                if (Physics.Linecast(transform.position, playerPosition, out RaycastHit hit, sightDontIgnore))
                {
                    if (hit.transform.CompareTag("Player") == true)
                    {
                        Debug.DrawLine(debugMiddle, playerPosition, Color.red);
                        return true;
                    }
                    Debug.DrawLine(debugMiddle, playerPosition, Color.yellow);
                    return false;
                }
            }
            else
            {
                Debug.DrawLine(debugMiddle, playerPosition, Color.green);
                return false;
            }
        }
        return false;
    }

    private Vector3 GetNextPatrolPoint()
    {
        if (targetPosition == Vector3.zero)
        {
            return storedPatrolPoints[0];
        }
        else
        {
            Vector3 point = targetPosition;
            for (int i = 0; i < storedPatrolPoints.Length; i++)
            {
                if (storedPatrolPoints[i] == point)
                {
                    if (i + 1 >= storedPatrolPoints.Length)
                    {
                        point = storedPatrolPoints[0];
                        break;
                    }
                    else
                    {
                        point = storedPatrolPoints[i + 1];
                        break;
                    }
                }
            }
            return point;
        }
    }

    public void SetState(BehaviourID stateID)
    {
        if (currentState != stateID)
        {
            //set our current state id to the one being passed
            //initialize behaviour according to state id
            if (stateID == BehaviourID.idle)
            {
                animator.SetBool("Walking", false);
                timeIdle = Time.time + Random.Range(1f, 7f);
                agent.isStopped = true;
            }
            else if (stateID == BehaviourID.wander)
            {
                animator.SetBool("Walking", true);
                agent.speed = walkSpeed;
                agent.isStopped = false;
                GetNewWanderPoint();
            }
            else if (stateID == BehaviourID.chase)
            {
                agent.speed = chaseSpeed;
                agent.isStopped = false;
            }
            else if (stateID == BehaviourID.investigate)
            {
                agent.speed = 1.5f;
                agent.isStopped = false;
            }
            else if (stateID == BehaviourID.patrol)
            {
                animator.SetBool("Walking", true);
                agent.speed = walkSpeed;
                agent.isStopped = false;
                targetPosition = storedPatrolPoints[0];
                agent.SetDestination(targetPosition);
            }
            else if (stateID == BehaviourID.alerted)
            {
                agent.speed = 0.4f;
                agent.isStopped = false;
            }
            currentState = stateID;
        }
    }

    void GetNewWanderPoint()
    {
        targetPosition = GetRandomPosInBounds();
        agent.SetDestination(targetPosition);
    }

    private Vector3 GetRandomPosInBounds()
    {
        return new Vector3(Random.Range(-boundBox.extents.x + boundBox.center.x, boundBox.extents.x + boundBox.center.x), transform.position.y,
            Random.Range(-boundBox.extents.z + boundBox.center.z, boundBox.extents.z + boundBox.center.z));
    }

    private void OnDrawGizmos()
    {
        Vector3 middlePosition = new Vector3(transform.position.x, 0.87f, transform.position.z);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boundBox.center, boundBox.size);
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(targetPosition, 0.2f);
        //Gizmos.color = Color.magenta;
        //Gizmos.DrawWireSphere(transform.position, viewRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(middlePosition, soundDetectionRadius);
        Handles.color = Color.white;
        Handles.DrawWireArc(middlePosition, Vector3.up, Vector3.forward, 360, viewRadius);
        Handles.color = new Color(1, 0, 0, 0.2f);
        Handles.DrawSolidArc(middlePosition, Vector3.up, transform.forward, viewAngle, viewRadius);
        Handles.DrawSolidArc(middlePosition, Vector3.up, transform.forward, -viewAngle, viewRadius);

        // patrol point drawing
        if (storedPatrolPoints.Length > 0)
        {
            foreach (Vector3 point in storedPatrolPoints)
            {
                if (targetPosition != point)
                {
                    Gizmos.color = new Color(1, 1, 0, 0.5f);
                    Gizmos.DrawWireSphere(point, 0.2f);
                }
                else
                {
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                    Gizmos.DrawWireSphere(point, 0.2f);
                }
            }
        }
    }
}
