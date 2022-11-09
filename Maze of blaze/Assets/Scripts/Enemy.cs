using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static MazeData;
public class Enemy : MonoBehaviour
{
    protected NavMeshAgent myAgent;

    //index is not used for anything at the moment
    [HideInInspector]
    public int index;

    //State enum for a state machine
    enum State { 
        CHASE,
        FLEE,
        STOP,
    };

    //current state of the enemy
    State state;

    [Header("Enemy characteristics")]
    public bool dealsDamage = true;
    public bool chasesThePlayer = true;
    public float trigerDistance = 15.0f;
    public float damage = 1.0f;
    public float speed = 3f;
    public float health = 2f;

    //reference to a data object containing information about the maze
    protected MazeData mazeData;

    protected virtual void Start()
    {
        if (myAgent == null)
            myAgent = GetComponent<NavMeshAgent>();
        if (GameManager.instance != null)
            mazeData = GameManager.instance.mazeData;
        myAgent.speed = speed;
    }

    protected float RemainingDistance(Vector3[] points)
    {
        if (points.Length < 2) return 0;
        float distance = 0;
        for (int i = 0; i < points.Length - 1; i++)
        {
            distance += Vector3.Distance(points[i], points[i + 1]);
            Debug.DrawLine(points[i], points[i + 1], Color.red, 0.1f);
        }
        return distance;
    }

    /// <summary>
    /// The function which is called in response to the player changing position
    /// </summary>
    /// <param name="newDest"></param>
    public virtual void UpdateDestination(Vector3 newDest)
    {
        ResetState(newDest);
        switch (state)
        {
            case State.STOP:
                myAgent.isStopped = true;
                break;
            case State.CHASE:
                myAgent.isStopped = false;
                myAgent.SetDestination(newDest);
                break;
            case State.FLEE:
                myAgent.isStopped = false;
                myAgent.SetDestination(mazeData.GetFleePosition(transform.position, newDest));
                break;
        }
    }

    public float timeBeforeColliderReset = 0.2f;

    //Some situations will make enemy flee from the player until certain point in time
    protected float timeToStopFleeing = 0;
    public virtual void Retreat(float duration = 1f)
    {
        GetComponent<Collider>().enabled = false;
        StartCoroutine(ResetColliderDelay(timeBeforeColliderReset));

        state = State.FLEE;
        timeToStopFleeing = Time.time + duration > timeToStopFleeing ? Time.time + duration : timeToStopFleeing;
    }

    public virtual void ResetState(Vector3 newDest)
    {
        if (state == State.FLEE)
            if (timeToStopFleeing > Time.time)
                return;
        var path = myAgent.path;
        NavMeshPath newPath = new NavMeshPath();
        myAgent.CalculatePath(newDest, newPath);
        if (RemainingDistance(newPath.corners) < trigerDistance)
            state = State.CHASE;
        else 
            state = State.STOP;
    }

    protected IEnumerator ResetColliderDelay(float wait)
    {
        yield return new WaitForSeconds(wait);
        GetComponent<Collider>().enabled = true;
    }

    public virtual void Die()
    {
        GameManager.instance.enemyManager.RemoveEnemy(this);
        Destroy(this.gameObject);
    }
}
