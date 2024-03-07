using UnityEngine;
using AK.Wwise;

public class AKLD_Steps : MonoBehaviour
{
    public float velocidadMinima = 1f; // Velocidad mínima del objeto
    public float velocidadMaxima = 10f; // Velocidad máxima del objeto
    public float distanciaMinimaPaso = 0.4f; // Distancia mínima entre pasos
    public float distanciaMaximaPaso = 2f; // Distancia máxima entre pasos
    public AK.Wwise.RTPC velocidadRTPC; // RTPC de velocidad en Wwise

    public AK.Wwise.Event eventoPaso; // Evento de sonido de paso en Wwise
    public AK.Wwise.Event stopWalkEvent; // Evento de detener caminar en Wwise
    public AK.Wwise.Event saltoEvent; // Evento de salto en Wwise
    public AK.Wwise.Event fallEvent; // Evento de golpe en el suelo en Wwise

    public float velocidadMinimaDetener = 0.2f; // Velocidad mínima para detener caminar

    public float longitudRaycast = 0.5f; // Longitud del raycast hacia abajo
    public bool isGrounded; // Booleano que indica si el objeto está en el suelo

    private Rigidbody rb;
    private Vector3 posicionAnterior;
    private float distanciaRecorrida;
    private bool sePosteoStopWalk = false; // Control para asegurar que se postee stopWalkEvent una vez
    private bool saltoPosteado = false; // Control para asegurar que se postee saltoEvent una vez
    private bool wasGroundedLastFrame = true; // Control para detectar cambio de estado en isGrounded

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        posicionAnterior = transform.position;
    }

    private void Update()
    {
        // Lanzar un raycast hacia abajo para verificar si estamos en el suelo
        isGrounded = Physics.Raycast(transform.position, Vector3.down, longitudRaycast);

        // Si el salto no ha sido posteado y no estamos en el suelo, postear el evento de salto y salir
        if (!isGrounded && !saltoPosteado)
        {
            saltoEvent?.Post(gameObject);
            saltoPosteado = true;
            return;
        }

        // Si estamos en el suelo y el salto ha sido posteado, resetear el control del salto
        if (isGrounded && saltoPosteado)
        {
            saltoPosteado = false;
        }

        // Postear el evento de golpe en el suelo si hemos aterrizado
        if (!wasGroundedLastFrame && isGrounded)
        {
            fallEvent?.Post(gameObject);
        }

        // Actualizar el estado de wasGroundedLastFrame
        wasGroundedLastFrame = isGrounded;

        // Si no estamos en el suelo, salir sin hacer más cálculos
        if (!isGrounded)
            return;

        // Calcular la velocidad actual del objeto
        float velocidadActual = rb.velocity.magnitude;

        // Si la velocidad es menor que la mínima para detener y stopWalkEvent no se ha posteado aún, postearlo
        if (velocidadActual < velocidadMinimaDetener && !sePosteoStopWalk)
        {
            stopWalkEvent?.Post(gameObject);
            sePosteoStopWalk = true;
        }

        // Si la velocidad está por encima de la mínima para detener y stopWalkEvent se ha posteado, reiniciar la variable de control
        if (velocidadActual > velocidadMinimaDetener && sePosteoStopWalk)
        {
            sePosteoStopWalk = false;
        }

        // Si la velocidad es menor que la mínima, no hacer nada más
        if (velocidadActual < velocidadMinima)
            return;

        // Limitar la velocidad actual entre la mínima y máxima
        velocidadActual = Mathf.Clamp(velocidadActual, velocidadMinima, velocidadMaxima);

        // Actualizar el RTPC de velocidad en Wwise
        if (velocidadRTPC != null)
        {
            float valorRTPC = velocidadActual / velocidadMaxima; // Normalizar la velocidad
            velocidadRTPC.SetValue(gameObject, valorRTPC);
        }

        // Calcular la distancia entre pasos basada en una función exponencial
        float factorExponencial = Mathf.Pow(2f, (velocidadActual - velocidadMinima) / (velocidadMaxima - velocidadMinima));
        float distanciaEntrePasos = Mathf.Lerp(distanciaMinimaPaso, distanciaMaximaPaso, factorExponencial);

        // Calcular la distancia recorrida desde la última vez que se reprodujo el sonido de paso
        float distanciaFrame = Vector3.Distance(transform.position, posicionAnterior);
        distanciaRecorrida += distanciaFrame;

        // Si la distancia recorrida es mayor que la distancia entre pasos, reproducir el sonido de paso
        if (distanciaRecorrida >= distanciaEntrePasos)
        {
            // Lanzar el evento de sonido de paso en Wwise
            eventoPaso?.Post(gameObject);

            // Reiniciar la distancia recorrida
            distanciaRecorrida = 0f;
        }

        // Actualizar la posición anterior para el siguiente frame
        posicionAnterior = transform.position;
    }

    private void OnDrawGizmos()
    {
        // Dibujar el raycast hacia abajo
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * longitudRaycast);
    }
}
