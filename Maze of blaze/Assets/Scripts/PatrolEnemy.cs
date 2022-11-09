using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static MazeData;

/// <summary>
/// Patrol Enemy walks between certain points on the map, until he sees the player
/// When he loses sight he goes back to his patrol duties
/// </summary>
public class PatrolEnemy : Enemy
{
    enum State
    {
        CHASE,
        FLEE,
        PATROL,
    };

    public List<Vector3> patrolPoints;
    public int currentDest = 0;
    public int numOfPoints = 3;

    State stateP = State.PATROL;

    List<Vector3> GenerateRandomPoints()
    {
        var rand = new System.Random();
        var result = new List<Vector3>();
        result.Add(transform.position);
        for (int i = 0; i < numOfPoints; ++i)
        {
            Vector2Int point = new Vector2Int(rand.Next(0, mazeData.width), rand.Next(0, mazeData.height));
            result.Add(mazeData.GetCellPosition(point));
        }
        return result;
    }

    protected override void Start()
    {
        base.Start();
        patrolPoints = GenerateRandomPoints();
    }

    public override void UpdateDestination(Vector3 newDest)
    {
        ResetState(newDest);
        switch (stateP)
        {
            case State.PATROL:
                myAgent.isStopped = false;
                myAgent.SetDestination(patrolPoints[currentDest]);
                var path = new NavMeshPath();
                myAgent.CalculatePath(patrolPoints[currentDest], path);
                if (RemainingDistance(path.corners) < 0.05f)
                {
                    ++currentDest;
                    if (currentDest >= patrolPoints.Count)
                        currentDest = 0;
                }
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

    public override void ResetState(Vector3 newDest)
    {
        if (stateP == State.FLEE)
            if (timeToStopFleeing > Time.time)
                return;
        var path = myAgent.path;
        NavMeshPath newPath = new NavMeshPath();
        myAgent.CalculatePath(newDest, newPath);
        if (RemainingDistance(newPath.corners) < trigerDistance)
            stateP = State.CHASE;
        else
            stateP = State.PATROL;
    }

    public override void Retreat(float duration = 1f)
    {
        GetComponent<Collider>().enabled = false;
        StartCoroutine(ResetColliderDelay(timeBeforeColliderReset));

        stateP = State.FLEE;
        timeToStopFleeing = Time.time + duration;
    }
}
