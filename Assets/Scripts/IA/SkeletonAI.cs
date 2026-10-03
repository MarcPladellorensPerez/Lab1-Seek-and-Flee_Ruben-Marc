using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    // Objetivo al que vamos a perseguir o del que vamos a huir. En este caso, el jugador
    public GameObject target;

    // Distancia a la que queremos estar del jugador. Si estamos más lejos, vamos hacia él y si estamos más cerca, huimos
    public float safeDistance = 8f;
    // Margen de tolerancia alrededor de safeDistance. Sin él, justo en el límite cambiaba de seek a flee en cada frame y se quedaba temblando. Con 0 funciona igual pero tiembla
    public float tolerance = 0.5f;

    // Cuánto acelera y la velocidad máxima de movimiento
    public float acceleration = 8f;
    public float maxSpeed = 4f;
    // Lo mismo pero para el giro, para que no se dé la vuelta de golpe
    public float turnAcceleration = 20f;
    public float maxTurnSpeed = 8f;

    // Lo necesitamos para que se reproduzca la animación de andar
    public Animator animator;

    // Dirección a la que queremos ir y rotación que queremos que tenga
    Vector3 movement;
    Quaternion rotation;
    // Velocidades actuales (van subiendo poco a poco hasta el máximo)
    float movSpeed = 0f;
    float turnSpeed = 0f;

    void Awake()
    {
        // Si se nos ha olvidado arrastrar el Animator, lo cogemos del propio objeto
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Sin objetivo no hay nada que hacer
        if (target == null) return;

        float distance = Vector3.Distance(target.transform.position, transform.position);

        // Aquí es donde decidimos nosotros si hacer seek o flee según la distancia
        if (distance > safeDistance + tolerance)
        {
            Seek();
        }
        else if (distance < safeDistance - tolerance)
        {
            Flee();
        }
        else
        {
            // Estamos en la zona intermedia: nos quedamos parados
            movSpeed = 0f;
            SetAnimation();
            return;
        }

        // Subimos la velocidad de giro sin pasarnos del máximo
        turnSpeed += turnAcceleration * Time.deltaTime;
        turnSpeed = Mathf.Min(turnSpeed, maxTurnSpeed);

        // Igual con la velocidad de avance
        movSpeed += acceleration * Time.deltaTime;
        movSpeed = Mathf.Min(movSpeed, maxSpeed);

        // Giramos poco a poco hacia la rotación objetivo (Slerp suaviza el giro)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            Time.deltaTime * turnSpeed
        );

        // Avanzamos hacia donde está mirando el personaje.
        // Time.deltaTime hace que vaya igual de rápido con cualquier FPS
        transform.position += transform.forward.normalized * movSpeed * Time.deltaTime;

        SetAnimation();
    }

    // Seek: calculamos la dirección hacia el jugador
    void Seek()
    {
        Vector3 direction = target.transform.position - transform.position;
        // Ponemos la Y a 0 para movernos solo por el suelo, si no se hundiría o subiría
        direction.y = 0f;

        movement = direction.normalized * acceleration;

        // Pasamos esa dirección a un ángulo en el eje Y para saber hacia dónde girar
        float angle = Mathf.Rad2Deg * Mathf.Atan2(movement.x, movement.z);
        rotation = Quaternion.AngleAxis(angle, Vector3.up);
    }

    // Flee: es lo mismo que Seek pero restando al revés, así la dirección sale opuesta
    void Flee()
    {
        Vector3 direction = transform.position - target.transform.position;
        direction.y = 0f;

        movement = direction.normalized * acceleration;

        float angle = Mathf.Rad2Deg * Mathf.Atan2(movement.x, movement.z);
        rotation = Quaternion.AngleAxis(angle, Vector3.up);
    }

    // Le pasamos la velocidad al Animator (con el parámetro SpeedMagnitude) para que ande en vez de deslizarse
    void SetAnimation()
    {
        if (animator != null)
            animator.SetFloat("SpeedMagnitude", movSpeed);
    }
}