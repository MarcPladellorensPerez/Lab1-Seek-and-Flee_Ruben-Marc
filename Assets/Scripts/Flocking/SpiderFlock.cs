using UnityEngine;

public class SpiderFlock : MonoBehaviour
{
    [HideInInspector]
    public FlockManager myManager;

    [HideInInspector]
    public Vector3 currentDirection;

    private float speed;

    void Start()
    {
        // Asignamos una velocidad inicial aleatoria basada en los límites del manager
        speed = Random.Range(myManager.minSpeed, myManager.maxSpeed);
        currentDirection = transform.forward;
    }

    void Update()
    {
        ApplyFlockingRules();

        // Movemos la araña hacia adelante
        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    void ApplyFlockingRules()
    {
        Vector3 cohesion = Vector3.zero;
        Vector3 align = Vector3.zero;
        Vector3 separation = Vector3.zero;
        int numNeighbours = 0;

        foreach (GameObject go in myManager.allSpiders)
        {
            if (go != this.gameObject)
            {
                float distance = Vector3.Distance(go.transform.position, transform.position);

                if (distance <= myManager.neighbourDistance)
                {
                    cohesion += go.transform.position;
                    align += go.GetComponent<SpiderFlock>().currentDirection;

                    // Empujamos en dirección contraria a la posición del vecino para separarnos
                    // Dividimos por la distancia al cuadrado para que la fuerza sea mayor cuanto más cerca estén
                    separation += (transform.position - go.transform.position) / (distance * distance);

                    numNeighbours++;
                }
            }
        }

        if (numNeighbours > 0)
        {
            // 1. Cohesión: Calculamos el punto medio y apuntamos hacia él
            cohesion = (cohesion / numNeighbours - transform.position).normalized * speed;

            // 2. Alineación: Calculamos la media de las direcciones
            align /= numNeighbours;

            // Ajustamos la velocidad para igualarla a la del grupo
            speed = Mathf.Clamp(align.magnitude, myManager.minSpeed, myManager.maxSpeed);

            // Sumamos las 3 fuerzas (Puedes multiplicar cada vector por un factor si quieres darle más peso a una regla u otra)
            currentDirection = (cohesion + align + separation).normalized * speed;

            // Forzamos a que el vector no apunte ni hacia arriba ni hacia abajo (Arañas de suelo)
            currentDirection.y = 0;

            if (currentDirection != Vector3.zero)
            {
                // Rotamos suavemente hacia la nueva dirección
                Quaternion rot = Quaternion.LookRotation(currentDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, myManager.rotationSpeed * Time.deltaTime);
            }
        }
    }
}
