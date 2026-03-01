using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour {
    [Header("References")]
    private NavMeshAgent navMeshAgent;
    private NoiseSystem noiseSystem;
    private Transform player;

    [Header("Behavior States")]
    public enum AIState {
        Patrol,
        Investigate,
        Chase,
        Attack
    }
    private AIState currentState = AIState.Patrol;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float patrolWaitTime = 3f;
    [SerializeField] private Vector3 patrolCenter = Vector3.zero;
    [SerializeField] private float patrolRadius = 30f;

    [Header("Investigate Settings")]
    [SerializeField] private float investigateSpeed = 5f;
    [SerializeField] private float investigateDistance = 0.5f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float chaseLoseSightTime = 5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackStoppingDistance = 1f;
    [SerializeField] private float attackDamage = 25f;

    [Header("Audio")]
    [SerializeField] private AudioSource enemyAudioSource;
    [SerializeField] private AudioClip growlClip;
    [SerializeField] private AudioClip screamClip;

    private Vector3 investigateTarget;
    private float lastSightTime;
    private float patrolWaitTimer;

    private void Start() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        noiseSystem = FindObjectOfType<NoiseSystem>();
        player = FindObjectOfType<PlayerController>()?.transform;

        if (navMeshAgent == null) {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }
    }

    private void Update() {
        UpdateState();
    }

    private void UpdateState() {
        switch (currentState) {
            case AIState.Patrol:
                PatrolBehavior();
                break;
            case AIState.Investigate:
                InvestigateBehavior();
                break;
            case AIState.Chase:
                ChaseBehavior();
                break;
            case AIState.Attack:
                AttackBehavior();
                break;
        }
    }

    private void PatrolBehavior() {
        navMeshAgent.speed = patrolSpeed;
        patrolWaitTimer -= Time.deltaTime;

        if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f) {
            if (patrolWaitTimer <= 0) {
                SetRandomPatrolPoint();
                patrolWaitTimer = patrolWaitTime;
            }
        }
    }

    private void InvestigateBehavior() {
        navMeshAgent.speed = investigateSpeed;
        navMeshAgent.SetDestination(investigateTarget);

        if (navMeshAgent.remainingDistance < investigateDistance && !navMeshAgent.hasPath) {
            currentState = AIState.Patrol;
        }
    }

    private void ChaseBehavior() {
        if (player == null) return;
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(player.position);

        if (Time.time - lastSightTime > chaseLoseSightTime) {
            currentState = AIState.Patrol;
        }
    }

    private void AttackBehavior() {
        if (player == null) return;
        navMeshAgent.speed = 0;
        navMeshAgent.isStopped = true;
        PlaySound(screamClip);
    }

    public void InvestigateNoise(Vector3 position, float intensity) {
        if (currentState == AIState.Chase || currentState == AIState.Attack) return;
        investigateTarget = position;
        currentState = AIState.Investigate;
        Debug.Log($"[ENEMY] Investigando ruído em: {position}");
    }

    public void ChaseNoise(Vector3 position, float intensity) {
        currentState = AIState.Chase;
        lastSightTime = Time.time;
        PlaySound(growlClip);
        Debug.Log($"[ENEMY] Perseguindo som em: {position}");
    }

    private void SetRandomPatrolPoint() {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += patrolCenter;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas)) {
            navMeshAgent.SetDestination(hit.position);
        }
    }

    private void PlaySound(AudioClip clip) {
        if (enemyAudioSource != null && clip != null) {
            enemyAudioSource.PlayOneShot(clip);
        }
    }

    public AIState CurrentState => currentState;
    public float DistanceToPlayer => player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;
}