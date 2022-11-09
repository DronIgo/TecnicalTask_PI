using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Bonus Theif enemies run away from the player, when caught they drop bonuses and teleport away
/// </summary>
public class BonusThiefEnemy : Enemy
{
    [Tooltip("Bonus theif should drop")]
    public GameObject bonusPrefab;
    enum State
    {
        FLEE,
        STOP,
    };

    State state;

    System.Random rand = new System.Random();
    /// <summary>
    /// Teleports theif to a random location on the maze
    /// </summary>
    void Teleport()
    {
        Vector2Int point = new Vector2Int(rand.Next(0, mazeData.width), rand.Next(0, mazeData.height));
        float yPos = transform.position.y;
        Vector3 newPos = mazeData.GetCellPosition(point);
        transform.position = new Vector3(newPos.x, yPos, newPos.z);
    }
    public override void UpdateDestination(Vector3 newDest)
    {
        ResetState(newDest);
        switch (state)
        {
            case State.STOP:
                myAgent.isStopped = true;
                break;
            case State.FLEE:
                myAgent.isStopped = false;
                myAgent.SetDestination(mazeData.GetFleePosition(transform.position, newDest));
                break;
        }
    }
    public virtual void Retreat(float duration = 1f)
    {
        GetComponent<Collider>().enabled = false;
        StartCoroutine(ResetColliderDelay(timeBeforeColliderReset));

        state = State.FLEE;
        timeToStopFleeing = Time.time + duration > timeToStopFleeing ? Time.time + duration : timeToStopFleeing;
    }

    public virtual void ResetState(Vector3 newDest)
    {
        NavMeshPath newPath = new NavMeshPath();
        myAgent.CalculatePath(newDest, newPath);
        if (RemainingDistance(newPath.corners) < trigerDistance)
            state = State.FLEE;
        else
            state = State.STOP;
    }

    public virtual void Die()
    {
        Vector3 shift = new Vector3(1 - 2 * Random.value, 0, 1 - 2 * Random.value);
        Instantiate(bonusPrefab, transform.position + shift, Quaternion.identity);
        Teleport();
    }
}
