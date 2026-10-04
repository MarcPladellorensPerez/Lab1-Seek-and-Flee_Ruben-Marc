using UnityEngine;
using UnityEngine.AI;

// Versión de SkeletonAI que delega el movimiento en un NavMeshAgent (diapo 22).
// La decisión (seek / flee / idle según la distancia) es la misma que en SkeletonAI;
// lo que cambia es cómo se mueve: ahora esquiva obstáculos y cruza los NavMeshLink solo.
[RequireComponent(typeof(NavMeshAgent))]
public class SkeletonNavAI : MonoBehaviour
{
    // Objetivo al que vamos a perseguir o del que vamos a huir. En este caso, el jugador
    public GameObject target;

    // Distancia a la que queremos estar del jugador, con un margen de tolerancia para que no tiemble
    public float safeDistance = 8f;
    public float tolerance = 0.5f;

    // Se los pasamos al NavMeshAgent en Awake (así los ajustas aquí y no en dos sitios)
    public float maxSpeed = 4f;
    public float acceleration = 8f;
    // Grados por segundo
    public float angularSpeed = 360f;

    // Cada cuánto recalculamos el destino (diapo 10: reducir la frecuencia de los steerings). 0 = cada frame
    public float repathInterval = 0.2f;
    // A qué distancia por delante buscamos el punto de huida
    public float fleeDistance = 6f;

    // Lo necesitamos para que se reproduzca la animación de andar
    public Animator animator;

    enum Mode { Idle, Seek, Flee }
    Mode mode = Mode.Idle;

    NavMeshAgent agent;
    float nextRepathTime;

    static readonly int SpeedMagnitudeHash = Animator.StringToHash("SpeedMagnitude");

    // Ángulos que probamos al buscar punto de huida (0 = justo en el lado contrario al jugador)
    static readonly float[] fleeAngles = { 0f, 30f, -30f, 60f, -60f, 90f, -90f };

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        agent.speed = maxSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
    }

    void Update()
    {
        // Sin objetivo, o con el agente fuera de la NavMesh, no hay nada que hacer
        if (target == null || !agent.isOnNavMesh) return;

        // Mientras cruza un NavMeshLink no cambiamos de idea: parar o girar a mitad de salto queda raro
        if (agent.isOnOffMeshLink)
        {
            SetAnimation();
            return;
        }

        // Distancia solo en el plano del suelo (x, z), como en la diapo 11
        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        // Aquí es donde decidimos nosotros si hacer seek o flee según la distancia
        Mode newMode;
        if (distance > safeDistance + tolerance) newMode = Mode.Seek;
        else if (distance < safeDistance - tolerance) newMode = Mode.Flee;
        else newMode = Mode.Idle;

        bool modeChanged = newMode != mode;
        mode = newMode;

        if (mode == Mode.Idle)
        {
            // Zona intermedia: nos paramos y miramos al jugador
            if (modeChanged) agent.ResetPath();
            FaceTarget(toTarget);
        }
        else if (modeChanged || Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;

            if (mode == Mode.Seek) Seek();
            else Flee();
        }

        SetAnimation();
    }

    // Seek: el destino es la posición del jugador. El NavMeshAgent calcula el camino (A*, diapo 27),
    // incluidos los NavMeshLink que haga falta cruzar para llegar a otra zona
    void Seek()
    {
        // El jugador puede estar ligeramente fuera de la NavMesh (saltando, encima de un objeto...):
        // buscamos el punto de NavMesh más cercano para que el destino sea válido
        if (NavMesh.SamplePosition(target.transform.position, out NavMeshHit hit, 2f, agent.areaMask))
            agent.SetDestination(hit.position);
    }

    // Flee: no existe un "flee" en el NavMeshAgent, así que elegimos nosotros un destino que nos aleje del jugador.
    // Probamos varias direcciones y nos quedamos con el punto alcanzable más lejos del jugador
    void Flee()
    {
        Vector3 pos = transform.position;
        Vector3 targetPos = target.transform.position;

        Vector3 away = pos - targetPos;
        away.y = 0f;
        away = away.sqrMagnitude > 0.0001f ? away.normalized : transform.forward;

        // Solo nos vale un punto que nos aleje más de lo que ya estamos
        float bestDistance = FlatDistance(pos, targetPos);
        Vector3 bestPoint = pos;
        bool found = false;

        foreach (float angle in fleeAngles)
        {
            Vector3 candidate = pos + Quaternion.Euler(0f, angle, 0f) * away * fleeDistance;

            // NavMesh.Raycast devuelve true si hay un borde de la NavMesh por el camino (pared, hueco...).
            // En ese caso hit.position es hasta dónde podemos llegar en línea recta
            Vector3 reachable = NavMesh.Raycast(pos, candidate, out NavMeshHit hit, agent.areaMask)
                ? hit.position
                : candidate;

            float d = FlatDistance(reachable, targetPos);
            if (d > bestDistance)
            {
                bestDistance = d;
                bestPoint = reachable;
                found = true;
            }
        }

        if (found) agent.SetDestination(bestPoint);
        else agent.ResetPath(); // Acorralado: no hay ningún sitio mejor
    }

    // El agente solo rota mientras se mueve, así que en Idle giramos nosotros hacia el jugador
    void FaceTarget(Vector3 toTarget)
    {
        if (toTarget.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, angularSpeed * Time.deltaTime);
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // Le pasamos la velocidad real del agente al Animator para que ande en vez de deslizarse
    void SetAnimation()
    {
        if (animator != null)
            animator.SetFloat(SpeedMagnitudeHash, agent.velocity.magnitude);
    }

    // Dibuja las zonas al seleccionar el objeto: fuera de la verde hace seek, dentro de la roja hace flee
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance + tolerance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, safeDistance - tolerance);
    }
}
