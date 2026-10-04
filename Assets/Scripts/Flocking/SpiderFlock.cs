using UnityEngine;

public class SpiderFlock : MonoBehaviour
{
    [HideInInspector]
    public FlockManager myManager;

    // Velocidad actual. Es pública porque los vecinos la leen para igualar su velocidad a la del grupo
    [HideInInspector]
    public float speed;

    // Dirección (normalizada, en el plano XZ) hacia la que queremos girar. La recalculan las reglas
    private Vector3 currentDirection;
    private float nextRulesTime;

    void Start()
    {
        // Si la araña está suelta en la escena sin manager, no podemos hacer nada
        if (myManager == null)
        {
            Debug.LogWarning(name + ": no tiene FlockManager asignado, se desactiva.", this);
            enabled = false;
            return;
        }

        // Asignamos una velocidad inicial aleatoria basada en los límites del manager
        speed = Random.Range(myManager.minSpeed, myManager.maxSpeed);

        currentDirection = transform.forward;
        currentDirection.y = 0f;
        currentDirection.Normalize();

        // Desfasamos el primer cálculo de cada araña para que no calculen todas en el mismo frame
        nextRulesTime = Time.time + Random.Range(0f, myManager.rulesInterval);
    }

    void Update()
    {
        // Las reglas (lo caro) no hace falta calcularlas cada frame (diapo 49)
        if (Time.time >= nextRulesTime)
        {
            nextRulesTime = Time.time + myManager.rulesInterval;
            ApplyFlockingRules();
        }

        // El giro y el movimiento sí se hacen cada frame, así se ve suave aunque las reglas se calculen menos veces
        if (currentDirection != Vector3.zero)
        {
            // Rotamos suavemente hacia la dirección que han calculado las reglas
            Quaternion rot = Quaternion.LookRotation(currentDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, myManager.rotationSpeed * Time.deltaTime);
        }

        // Movemos la araña hacia adelante
        transform.Translate(0f, 0f, speed * Time.deltaTime);
    }

    void ApplyFlockingRules()
    {
        FlockManager m = myManager;

        Vector3 cohesion = Vector3.zero;
        Vector3 align = Vector3.zero;
        Vector3 separation = Vector3.zero;
        float neighbourSpeed = 0f;
        int numNeighbours = 0;

        // Comparamos distancias al cuadrado para no hacer una raíz cuadrada por cada vecino
        float neighbourSqr = m.neighbourDistance * m.neighbourDistance;
        float separationSqr = m.separationDistance * m.separationDistance;

        foreach (SpiderFlock other in m.allSpiders)
        {
            // other == null por si alguna araña ha sido destruida (por ejemplo, si la matan)
            if (other == null || other == this) continue;

            // Vector hacia el vecino, solo en el plano del suelo
            Vector3 offset = other.transform.position - transform.position;
            offset.y = 0f;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance > neighbourSqr) continue;

            // Cohesión: sumamos los vectores hacia cada vecino. Su media apunta al centro del grupo
            cohesion += offset;

            // Alineación: sumamos hacia dónde mira realmente cada vecino
            align += other.transform.forward;
            neighbourSpeed += other.speed;

            numNeighbours++;

            // Separación: empujamos en dirección contraria al vecino (por eso el -=, porque offset va hacia él).
            // Cuanto más cerca, más fuerza. El Max evita dividir entre 0 si dos arañas coinciden
            if (sqrDistance < separationSqr)
                separation -= offset / Mathf.Max(sqrDistance, 0.01f);
        }

        // Partimos de la dirección que ya llevábamos (inercia), así las arañas sin vecinos no giran sin sentido
        Vector3 direction = currentDirection;

        if (numNeighbours > 0)
        {
            cohesion /= numNeighbours;
            align /= numNeighbours;

            // Igualamos la velocidad a la media de los vecinos (la dirección y la velocidad van por separado)
            speed = Mathf.Clamp(neighbourSpeed / numNeighbours, m.minSpeed, m.maxSpeed);

            // Normalizamos cada regla para que todas valgan "1" y los pesos del manager tengan sentido.
            // La separación solo se limita a 1: así es más fuerte cuanto más cerca, pero sin dispararse
            direction += cohesion.normalized * m.cohesionWeight
                       + align.normalized * m.alignWeight
                       + Vector3.ClampMagnitude(separation, 1f) * m.separationWeight;
        }

        // Opcional: un líder o punto al que el grupo tiende a ir
        if (m.goal != null)
        {
            Vector3 toGoal = m.goal.position - transform.position;
            toGoal.y = 0f;
            direction += toGoal.normalized * m.goalWeight;
        }

        // Un poco de azar enriquece el comportamiento (diapo 49)
        if (m.randomness > 0f)
            direction += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * m.randomness;

        // Si nos salimos de la zona, volver al centro tiene prioridad sobre las demás reglas (arbitración, diapo 39)
        if (m.bounded)
        {
            Vector3 fromCenter = transform.position - m.transform.position;
            fromCenter.y = 0f;

            if (Mathf.Abs(fromCenter.x) > m.roamLimits.x || Mathf.Abs(fromCenter.z) > m.roamLimits.z)
                direction = -fromCenter;
        }

        // Forzamos a que el vector no apunte ni hacia arriba ni hacia abajo (arañas de suelo)
        direction.y = 0f;

        // Si por algún motivo todo se ha cancelado, nos quedamos con la dirección anterior
        if (direction.sqrMagnitude > 0.0001f)
            currentDirection = direction.normalized;
    }
}