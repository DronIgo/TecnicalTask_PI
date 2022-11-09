using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy Manager is used to organize enemy path recalculation, 
/// making sure that on each fram we only do the calculations for one of the enemies
/// </summary>
public class EnemyManager : MonoBehaviour
{
    List<Enemy> allEnemies;
    List<bool> needUpdate;
    Vector3 currentPlayerPosition;

    public void RemoveEnemy(Enemy enemy)
    {
        for (int i = 0; i < allEnemies.Count; ++i)
        {
            if (enemy == allEnemies[i])
            {
                allEnemies.RemoveAt(i);
                needUpdate.RemoveAt(i);
                break;
            }
        }
    }
    private void Start()
    {
        currentPlayerPosition = GameManager.instance.player.transform.position;
        int ind = 0;
        allEnemies = new List<Enemy>();
        needUpdate = new List<bool>();
        foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
        {
            allEnemies.Add(enemy);
            needUpdate.Add(true);
            enemy.index = ind;
            ++ind;
        }
        StartCoroutine(UpdateEnemyCoroutine());
    }
    public void AllRetreat(float duration)
    {
        foreach (Enemy enemy in allEnemies)
            enemy.Retreat(duration);
    }
    public void UpdatePlayerInformation(Vector3 newPos)
    {
        currentPlayerPosition = newPos;
        for (int i = 0; i < needUpdate.Count; i++)
        {
            needUpdate[i] = true;
        }
    }
    
    //This is the same as Update
    IEnumerator UpdateEnemyCoroutine()
    {
        //let all start function on all Enemies do their work first to avoid errors
        yield return new WaitForEndOfFrame();

        int currentEnemy = 0;
        while (true)
        {
            if (currentEnemy >= needUpdate.Count)
                currentEnemy = 0;
            if (needUpdate[currentEnemy])
            {
                allEnemies[currentEnemy].UpdateDestination(currentPlayerPosition);
            }
            ++currentEnemy;
            yield return new WaitForEndOfFrame();
        }
    }
}
