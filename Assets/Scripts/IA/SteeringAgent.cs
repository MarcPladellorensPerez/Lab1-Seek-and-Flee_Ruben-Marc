using UnityEngine;

public class SteeringAgent : MonoBehaviour
{
    // Variables que podremos modificar desde el Inspector de Unity
    public Transform target;
    public float speed = 5f;
    public float safeDistance = 8f;

    void Update()
    {
        // Si no hemos asignado un objetivo en el Inspector, no hacemos nada
        if (target == null) return;

        // Calculamos la distancia real entre este agente y el jugador
        float distance = Vector3.Distance(transform.position, target.position);

        // Calculamos la dirección hacia el objetivo (normalizada para que solo sea la flecha direccional, sin magnitud)
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // Lógica de Seek y Flee
        if (distance > safeDistance)
        {
            // SEEK (Buscar): Si está más lejos que la distancia de seguridad, nos movemos hacia él
            transform.position += directionToTarget * speed * Time.deltaTime;
        }
        else
        {
            // FLEE (Huir): Si está más cerca, restamos la dirección para movernos en el sentido contrario
            transform.position -= directionToTarget * speed * Time.deltaTime;
        }
    }
}