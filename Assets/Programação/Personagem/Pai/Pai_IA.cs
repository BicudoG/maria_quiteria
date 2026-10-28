using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Pai_IA : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;                    // Arraste o Player aqui
    public Transform[] patrolPoints;            // Pontos de patrulha
    public Derrota fim;
    public GameObject teladofim;

    [Header("Campo de Visão")]
    [Range(1, 180)]
    public float fieldOfView = 45f;             // 45 graus
    public float viewDistance = 15f;            // Distância máxima de visão
    public LayerMask obstacleMask;              // Layer dos obstáculos (paredes, etc)
    public LayerMask playerMask;                // Layer do player

    [Header("Configurações")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6f;
    public float waitTimeAtPoint = 1.5f;        // Tempo parado em cada ponto
    public float loseSightTime = 3f;            // Tempo para perder o player se não o ver

   // Adicione esta variável na classe
    private bool isWalking = false;
    public bool anda;
    public NPC_Animacao scriptAnimacao;
    // Estados internos
    private NavMeshAgent agent;
    private enum State { Idle, Patrol, Chase }
    private State currentState = State.Idle;

    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float timeSinceLastSeen = 0f;
    private bool canSeePlayer = false;

    // Sinais públicos (chame esses métodos de outros scripts)
    public void ActivatePatrol()
    {
        if (currentState == State.Chase) return; // não interrompe perseguição

        currentState = State.Patrol;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        GoToNextPatrolPoint();
        Debug.Log("Patrulha ativada");
    }

    public void ActivateChase()
    {
        currentState = State.Chase;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        timeSinceLastSeen = 0f;
        Debug.Log("Perseguição ativada");
    }

    public void StopAll()
    {
        currentState = State.Idle;
        agent.isStopped = true;
        Debug.Log("IA parada");
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        scriptAnimacao = GetComponent<NPC_Animacao>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Maria_quiteria_Civil")?.transform;
    }

 

    void Update()
    {
        canSeePlayer = CheckFieldOfView();

        if (scriptAnimacao != null)
        {
            // Histerese para evitar o liga/desliga constante
            float speed = agent.velocity.magnitude;

            if (speed > 0.25f)                 // só liga se estiver realmente andando
                isWalking = true;
            else if (speed < 0.08f)             // só desliga quando estiver quase parado
                isWalking = false;

            // Força desligar quando o agent estiver parado de propósito
            if (agent.isStopped)
                isWalking = false;

            scriptAnimacao.anda = isWalking;
        }

        switch (currentState)
        {
            case State.Idle:
                break;
            case State.Patrol:
                HandlePatrol();
                if (canSeePlayer)
                {
                    ActivateChase();
                }
                break;
            case State.Chase:
                HandleChase();
                break;
        }
    }

    // ==================== SISTEMA DE PATRULHA ====================
    void HandlePatrol()
    {
        if (isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAndGoNext());
        }
    }

    IEnumerator WaitAndGoNext()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(waitTimeAtPoint);

        agent.isStopped = false;
        GoToNextPatrolPoint();
        isWaiting = false;
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // ==================== SISTEMA DE PERSEGUIÇÃO ====================
    void HandleChase()
    {
        if (player == null) return;

        agent.SetDestination(player.position);

        if (canSeePlayer)
        {
            timeSinceLastSeen = 0f;
            teladofim.SetActive(true);
            fim.Pause();
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;

            // Se perder o player por muito tempo, volta para patrulha
            if (timeSinceLastSeen >= loseSightTime)
            {
                ActivatePatrol();
            }
        }
    }

    // ==================== CAMPO DE VISÃO 45° ====================
    bool CheckFieldOfView()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > viewDistance) return false;

        // Ângulo entre a frente do inimigo e o player
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > fieldOfView * 0.5f) return false; // 45° → ±22.5°

        // Raycast para ver se tem obstáculo no meio
        if (Physics.Raycast(transform.position + Vector3.up * 1.2f,
                            directionToPlayer.normalized,
                            distance,
                            obstacleMask))
        {
            return false;
        }

        return true;
    }

    // ==================== DEBUG VISUAL ====================
    void OnDrawGizmosSelected()
    {
        // Desenha o cone de visão
        Gizmos.color = canSeePlayer ? Color.red : Color.yellow;

        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);

        // Arco do FOV
#if UNITY_EDITOR
        UnityEditor.Handles.color = canSeePlayer ? Color.red : Color.yellow;
        UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, leftBoundary, fieldOfView, viewDistance);
#endif
    }
}