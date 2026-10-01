using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // El jugador a seguir (Knight)
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset para mirar hacia la cabeza/centro del jugador
    
    [Header("Camera Settings")]
    public float distance = 5.0f;
    public float xSpeed = 15.0f; // Reducido para ajustarse a Mouse.delta del Input System
    public float ySpeed = 15.0f; 
    
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Ocultar y bloquear el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Obtener inputs del mouse con el nuevo Input System
            if (Mouse.current != null)
            {
                x += Mouse.current.delta.x.ReadValue() * xSpeed * Time.deltaTime;
                y -= Mouse.current.delta.y.ReadValue() * ySpeed * Time.deltaTime;
            }

            // Limitar la rotación vertical
            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            
            // Ajustar la posición basándose en la rotación y la distancia
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + offset;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
