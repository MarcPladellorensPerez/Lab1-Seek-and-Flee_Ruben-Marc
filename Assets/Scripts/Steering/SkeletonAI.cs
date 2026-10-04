using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    // Objetivo al que vamos a perseguir o del que vamos a huir. En este caso, el jugador
    public GameObject target;

    // Distancia a la que queremos estar del jugador. Si estamos más lejos, vamos hacia él y si estamos más cerca, huimos
    public float safeDistance = 8f;
    // Margen de tolerancia alrededor de safeDistance. Sin él, justo en el límite cambiaba de seek a flee en cada frame y se quedaba temblando
    public float tolerance = 0.5f;

    // Cuánto acelera, cuánto frena y la velocidad máxima de movimiento
    public float acceleration = 8f;
    public float deceleration = 12f;
    public float maxSpeed = 4f;
    // Lo mismo pero para el giro, para que no se dé la vuelta de golpe
    public float turnAcceleration = 20f;
    public float maxTurnSpeed = 8f;
    // Si el ángulo entre hacia donde miramos y hacia donde queremos ir es mayor que esto, primero giramos y luego avanzamos
    public float maxAngleToMove = 60f;

    // Lo necesitamos para que se reproduzca la animación de andar
    public Animator animator;

    // Los tres estados posibles según la distancia al objetivo
    enum Mode { Idle, Seek, Flee }
    Mode mode = Mode.Idle;

    // Dirección a la que queremos ir (normalizada, en el plano XZ) y rotación que queremos que tenga
    Vector3 desiredDirection;
    Quaternion rotation;
    // Velocidades actuales (van subiendo poco a poco hasta el máximo)
    float movSpeed = 0f;
    float turnSpeed = 0f;

    // Buscar el parámetro por string en cada frame es lento: guardamos su hash una sola vez
    static readonly int SpeedMagnitudeHash = Animator.StringToHash("SpeedMagnitude");

    void Awake()
    {
        // Si se nos ha olvidado arrastrar el Animator, lo cogemos del propio objeto o de un hijo (el modelo suele ser un hijo)
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Un Quaternion sin inicializar vale (0,0,0,0), que no es una rotación válida
        rotation = transform.rotation;
        desiredDirection = transform.forward;
    }

    void Update()
    {
        // Sin objetivo no hay nada que hacer
        if (target == null) return;

        // Distancia solo en el plano del suelo (x, z), como en la fórmula de la diapo 11.
        // Así no influye la altura del pivote del jugador respecto al del esqueleto
        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        // Aquí es donde decidimos nosotros si hacer seek o flee según la distancia
        Mode newMode;
        if (distance > safeDistance + tolerance) newMode = Mode.Seek;
        else if (distance < safeDistance - tolerance) newMode = Mode.Flee;
        else newMode = Mode.Idle;

        // Al cambiar de estado reiniciamos la velocidad de giro: así turnAcceleration
        // se nota cada vez que cambia de idea y no solo la primera vez
        if (newMode != mode)
        {
            mode = newMode;
            turnSpeed = 0f;
        }

        // Calculamos hacia dónde queremos ir. En Idle también miramos al objetivo, pero sin avanzar
        if (mode == Mode.Flee) Flee();
        else Seek();

        // Subimos la velocidad de giro sin pasarnos del máximo
        turnSpeed = Mathf.Min(turnSpeed + turnAcceleration * Time.deltaTime, maxTurnSpeed);

        // Giramos poco a poco hacia la rotación objetivo (Slerp suaviza el giro)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            Time.deltaTime * turnSpeed
        );

        // Solo avanzamos si ya estamos mirando más o menos hacia donde queremos ir (Vector3.Angle, diapo 11).
        // Sin esto, al pasar de seek a flee seguiría caminando hacia el jugador mientras se da la vuelta
        float angle = Vector3.Angle(transform.forward, desiredDirection);
        float targetSpeed = (mode != Mode.Idle && angle <= maxAngleToMove) ? maxSpeed : 0f;

        // Aceleramos hacia la velocidad objetivo, y si hay que frenar lo hacemos poco a poco (no de golpe)
        float rate = targetSpeed > movSpeed ? acceleration : deceleration;
        movSpeed = Mathf.MoveTowards(movSpeed, targetSpeed, rate * Time.deltaTime);

        // Avanzamos hacia donde está mirando el personaje.
        // Time.deltaTime hace que vaya igual de rápido con cualquier FPS
        transform.position += transform.forward * movSpeed * Time.deltaTime;

        SetAnimation();
    }

    // Seek: la dirección es del esqueleto hacia el jugador
    void Seek()
    {
        Steer(target.transform.position - transform.position);
    }

    // Flee: es lo mismo que Seek pero restando al revés, así la dirección sale opuesta
    void Flee()
    {
        Steer(transform.position - target.transform.position);
    }

    // Parte común de Seek y Flee: de una dirección saca la rotación que queremos tener
    void Steer(Vector3 direction)
    {
        // Ponemos la Y a 0 para movernos solo por el suelo, si no se hundiría o subiría
        direction.y = 0f;

        // Si el objetivo está justo encima, Atan2(0, 0) daría un ángulo sin sentido
        if (direction.sqrMagnitude < 0.0001f) return;

        desiredDirection = direction.normalized;

        // Pasamos esa dirección a un ángulo en el eje Y para saber hacia dónde girar
        float angle = Mathf.Rad2Deg * Mathf.Atan2(desiredDirection.x, desiredDirection.z);
        rotation = Quaternion.AngleAxis(angle, Vector3.up);
    }

    // Le pasamos la velocidad al Animator (con el parámetro SpeedMagnitude) para que ande en vez de deslizarse
    void SetAnimation()
    {
        if (animator != null)
            animator.SetFloat(SpeedMagnitudeHash, movSpeed);
    }

    // Dibuja las zonas en el editor al seleccionar el objeto: fuera de la verde hace seek, dentro de la roja hace flee
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance + tolerance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, safeDistance - tolerance);
    }
}