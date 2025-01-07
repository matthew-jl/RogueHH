using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private Queue<ITurnAction> turnQueue = new Queue<ITurnAction>();
    [SerializeField] private bool isPlayerTurn = true;

    private bool isProcessingAction = false;

    public float turnDelay = 0f;

    public EnemyManager enemyManager;

    public AudioSource dungeonAudioSource;
    public AudioSource combatAudioSource;
    public bool combatAudioIsPlaying = false;

    void OnEnable()
    {
        FindObjectOfType<PlayerController>().OnPlayerTurn.AddListener(HandlePlayerTurnStateChange);
    }

    void OnDisable()
    {
        if (FindObjectOfType<PlayerController>())
        {
            FindObjectOfType<PlayerController>().OnPlayerTurn.RemoveListener(HandlePlayerTurnStateChange);
        }
    }

    void Update()
    {
        // only process if not already processing an action
        if (!isProcessingAction && turnQueue.Count > 0)
        {
            StartCoroutine(ProcessTurn());
        }
        else if (turnQueue.Count == 0)
        {
            PlayerController playerMovement = FindObjectOfType<PlayerController>();
            if (playerMovement != null)
            {
                AddActionToQueue(playerMovement);
            }
        }
    }

    private IEnumerator ProcessTurn()
    {
        isProcessingAction = true;

        // get the action at the front of the queue
        ITurnAction currentAction = turnQueue.Peek();
        currentAction.ExecuteAction();

        // wait until the action is complete
        while (!currentAction.IsActionComplete())
        {
            yield return null;
        }

        // once action is complete, reset the action
        currentAction.ResetActionComplete();
        Debug.Log("Dequeue: " + currentAction);
        turnQueue.Dequeue(); // Remove the action from the queue

        // add a delay before processing the next action
        yield return new WaitForSeconds(turnDelay); // Wait for the specified delay

        isProcessingAction = false;
    }

    public void AddActionToQueue(ITurnAction action)
    {
        Debug.Log("Enqueue: " + action);
        turnQueue.Enqueue(action);
    }

    public void HandlePlayerTurnStateChange(bool isPlayerTurn)
    {
        this.isPlayerTurn = isPlayerTurn;
        if (!isPlayerTurn)
        {
            bool thereIsAggro = false;
            foreach (EnemyController enemyController in enemyManager.spawnedEnemies)
            {
                if (enemyController.currentState != EnemyState.Idle)
                {
                    AddActionToQueue(enemyController);
                }

                if (enemyController.currentState == EnemyState.Aggro || enemyController.currentState == EnemyState.ReadyToAttack)
                {
                    thereIsAggro = true;
                    if (!combatAudioIsPlaying)
                    {
                        dungeonAudioSource.Stop();
                        combatAudioSource.Play();
                        combatAudioIsPlaying = true;
                    }
                }
            }
            if (!thereIsAggro)
            {
                combatAudioSource.Stop();
                combatAudioIsPlaying = false;
                if (!dungeonAudioSource.isPlaying)
                {
                    dungeonAudioSource.Play();
                }
            }
        }
    }
}
