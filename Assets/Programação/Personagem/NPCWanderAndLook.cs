using UnityEngine;
using UnityEngine.AI;

public class NPCWanderAndLook : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public NavMeshAgent agent;
    public sistema_Animacao animacao;          // Arraste o componente aqui

    [Header("Wander")]
    public float wanderRadius = 25f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 6f;
    public float waitAfterArrival = 1.5f;

    [Header("Detecção do Jogador")]
    public float detectionRadius = 8f;
    public float lookSpeed = 5f;

    private enum State { Wandering, Idle, LookingAtPlayer }
    private State currentState = State.Wandering;

    private float idleTimer;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animacao == null)
            animacao = GetComponent<sistema_Animacao>();

        SetNewRandomDestination();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // === DETECÇÃO DO JOGADOR ===
        if (distanceToPlayer <= detectionRadius)
        {
            if (currentState != State.LookingAtPlayer)
            {
                agent.isStopped = true;
                currentState = State.LookingAtPlayer;
            }

            // Para de andar
            if (animacao != null)
                animacao.anda = false;

            LookAtPlayer();
            return;
        }
        else
        {
            if (currentState == State.LookingAtPlayer)
            {
                agent.isStopped = false;
                currentState = State.Wandering;
                SetNewRandomDestination();
            }
        }

        // === LÓGICA DE WANDER ===
        switch (currentState)
        {
            case State.Wandering:
                // Controla apenas o parâmetro "anda"
                if (animacao != null)
                    animacao.anda = agent.velocity.magnitude > 0.1f;

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    currentState = State.Idle;
                    idleTimer = Random.Range(minIdleTime, maxIdleTime) + waitAfterArrival;
                    agent.isStopped = true;

                    if (animacao != null)
                        animacao.anda = false;
                }
                break;

            case State.Idle:
                if (animacao != null)
                    animacao.anda = false;

                idleTimer -= Time.deltaTime;

                if (idleTimer <= 0f)
                {
                    agent.isStopped = false;
                    currentState = State.Wandering;
                    SetNewRandomDestination();
                }
                break;
        }
    }

    void SetNewRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}