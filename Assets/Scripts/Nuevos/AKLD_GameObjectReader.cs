using UnityEngine;
using System.Collections.Generic;

public enum Direction { Adelante, Atr�s, Arriba, Abajo, Izquierda, Derecha }

public class AKLD_GameObjectReader : CustomEditorBase
{
    // Direcci�n seleccionable desde la lista
    public Direction selectedDirection = Direction.Adelante;

    // Distancia m�xima para detectar Game Objects
    public float maxDistance = 10.0f;

    // Lista de chequeadores para Game Objects
    public List<GameObjectChecker> checkers = new List<GameObjectChecker>();

    // Activar o desactivar el debug.log del nombre del Game Object
    public bool debugObjectName = false;

    // Mostrar el raycast solo en el editor
    public bool showRaycast = true;

    // Si la c�mara afecta la direcci�n del rayo para adelante, atr�s, izquierda y derecha
    public bool cameraAffectsDirection = false;

    // Referencia a la c�mara
    public GameObject cameraObject;

    private GameObject GO;
    private Camera cam;

    // Para almacenar el �ltimo objeto detectado
    private GameObject lastHitObject;

    void Start()
    {
        GO = this.gameObject;
        cam = cameraObject != null ? cameraObject.GetComponent<Camera>() : Camera.main;
    }

    void Update()
    {
        // Obtener la posici�n de inicio para la lectura
        Vector3 origin = transform.position;

        // Definir la direcci�n del rayo en funci�n de la direcci�n seleccionada
        Vector3 rayDirection = Vector3.zero;
        switch (selectedDirection)
        {
            case Direction.Adelante:
                rayDirection = GetForwardDirection();
                break;
            case Direction.Atr�s:
                rayDirection = GetBackwardDirection();
                break;
            case Direction.Arriba:
                rayDirection = GetUpDirection();
                break;
            case Direction.Abajo:
                rayDirection = GetDownDirection();
                break;
            case Direction.Izquierda:
                rayDirection = GetLeftDirection();
                break;
            case Direction.Derecha:
                rayDirection = GetRightDirection();
                break;
        }

        // Rayo para detectar Game Objects
        Ray ray = new Ray(origin, rayDirection);

        // Visualizar el raycast solo en el editor
        if (showRaycast)
        {
            Debug.DrawRay(origin, rayDirection * maxDistance, Color.red);
        }

        // Variable para almacenar la informaci�n de la colisi�n
        RaycastHit hit;

        // Realizar la detecci�n de Game Objects
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Verificar si el objeto golpeado es un Game Object
            GameObject hitObject = hit.collider.gameObject;

            // Debug.log del nombre del Game Object si est� activado
            if (debugObjectName)
            {
                Debug.Log("GameObject Name: " + hitObject.name);
            }

            // Si el objeto detectado cambi�, detener el evento anterior y restablecer el evento enviado
            if (lastHitObject != null && lastHitObject != hitObject)
            {
                foreach (var checker in checkers)
                {
                    if (checker.IsValidGameObject(lastHitObject))
                    {
                        checker.StopWwiseEvent(GO, lastHitObject);
                        checker.ResetEventSent();
                        break; // Salir del bucle una vez que se haya detenido el evento
                    }
                }
            }

            // Verificar si el Game Object coincide con alg�n chequeador
            foreach (var checker in checkers)
            {
                if (checker.IsValidGameObject(hitObject) && !checker.eventSent)
                {
                    // Enviar el evento de Wwise asociado al chequeador v�lido
                    checker.SendWwiseEvent(GO, hitObject);
                    checker.eventSent = true;
                    break; // Salir del bucle una vez que se haya enviado el evento
                }
            }

            // Almacenar el objeto detectado para la pr�xima comparaci�n
            lastHitObject = hitObject;
        }
        else
        {
            // Si no hay objeto detectado, detener el evento anterior y restablecer el evento enviado
            if (lastHitObject != null)
            {
                foreach (var checker in checkers)
                {
                    if (checker.IsValidGameObject(lastHitObject))
                    {
                        checker.StopWwiseEvent(GO, lastHitObject);
                        checker.ResetEventSent();
                        break; // Salir del bucle una vez que se haya detenido el evento
                    }
                }

                // Limpiar el �ltimo objeto detectado
                lastHitObject = null;
            }
        }
    }

    // Funci�n para obtener la direcci�n hacia adelante
    private Vector3 GetForwardDirection()
    {
        if (cameraAffectsDirection && cam != null)
        {
            return cam.transform.forward;
        }
        else
        {
            return transform.forward;
        }
    }

    // Funci�n para obtener la direcci�n hacia atr�s
    private Vector3 GetBackwardDirection()
    {
        if (cameraAffectsDirection && cam != null)
        {
            return -cam.transform.forward;
        }
        else
        {
            return -transform.forward;
        }
    }

    // Funci�n para obtener la direcci�n hacia arriba
    private Vector3 GetUpDirection()
    {
        return Vector3.up;
    }

    // Funci�n para obtener la direcci�n hacia abajo
    private Vector3 GetDownDirection()
    {
        return Vector3.down;
    }

    // Funci�n para obtener la direcci�n hacia la izquierda
    private Vector3 GetLeftDirection()
    {
        if (cameraAffectsDirection && cam != null)
        {
            return -cam.transform.right;
        }
        else
        {
            return -transform.right;
        }
    }

    // Funci�n para obtener la direcci�n hacia la derecha
    private Vector3 GetRightDirection()
    {
        if (cameraAffectsDirection && cam != null)
        {
            return cam.transform.right;
        }
        else
        {
            return transform.right;
        }
    }
}

[System.Serializable]
public class GameObjectChecker
{
    // Nombre del Game Object que se espera detectar
    public GameObject expectedGameObject;
    public bool postOnOther;

    // Evento de Wwise a enviar si se encuentra el Game Object correcto
    public AK.Wwise.Event eventToSend;

    // Evento de Wwise para detener si se aleja del Game Object correcto
    public AK.Wwise.Event eventToStop;

    // Para rastrear si el evento ya se envi�
    public bool eventSent = false;

    // Funci�n para verificar si el Game Object coincide con el nombre esperado
    public bool IsValidGameObject(GameObject gameObject)
    {
        return gameObject == expectedGameObject;
    }

    // Funci�n para enviar el evento de Wwise asociado al chequeador v�lido
    public void SendWwiseEvent(GameObject _GO, GameObject other)
    {
        if (postOnOther)
            eventToSend.Post(other);
        else
            eventToSend.Post(_GO);
    }

    // Funci�n para detener el evento de Wwise asociado al chequeador
    public void StopWwiseEvent(GameObject _GO, GameObject other)
    {
        if (postOnOther)
            eventToStop.Post(other);
        else
            eventToStop.Post(_GO);
    }

    // Funci�n para restablecer el estado de evento enviado
    public void ResetEventSent()
    {
        eventSent = false;
    }
}
