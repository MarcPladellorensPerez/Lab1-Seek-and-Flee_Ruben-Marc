using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public Transform target; // Arrastra al jugador (Knight) aquí en el inspector, o usa el tag "Player"
    public float detectionDistance = 10f; // Distancia a la que detecta al jugador
    
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Si no hay target asignado, intentar encontrar al jugador automáticamente por su tag
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Calcular distancia al jugador
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            // Si el jugador está dentro del rango, hacer Seek (perseguir)
            if (distanceToTarget <= detectionDistance)
            {
                Seek();
            }
            else
            {
                // Opcional: Detener al agente si el jugador se aleja (comentar si quieres que lo persiga siempre que lo haya detectado una vez)
                agent.ResetPath();
            }
        }
    }

    // Comportamiento de Seek basado en NavMesh
    void Seek()
    {
        agent.destination = target.position;
    }

    // Dibujar el rango de detección en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}
