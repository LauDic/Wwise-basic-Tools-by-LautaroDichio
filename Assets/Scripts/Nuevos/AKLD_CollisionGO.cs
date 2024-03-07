using UnityEngine;

public class AKLD_CollisionGO : CustomEditorBase
{
    // Nombre del evento que se enviar� a Wwise
    public AK.Wwise.Event collisionEvent = null;

    // Nombre del RTPC que se utilizar� para la velocidad del impacto
    public AK.Wwise.RTPC impactSpeedRTPC = null;

    // Valor m�ximo del RTPC para la velocidad del impacto
    public float maxImpactSpeed = 100.0f;

    // Etiquetas de los GameObjects que activar�n la colisi�n
    public string[] collisionTags;

    // GameObjects espec�ficos que activar�n la colisi�n
    public GameObject[] collisionObjects;

    // Funci�n llamada cuando ocurre una colisi�n
    void OnCollisionEnter(Collision collision)
    {
        // Verifica si el objeto que colision� est� en la lista de objetos espec�ficos
        if (ArrayContainsGameObject(collision.gameObject, collisionObjects) || ArrayContainsTag(collision.gameObject.tag, collisionTags))
        {
            // Calcula la velocidad del impacto
            float impactSpeed = collision.relativeVelocity.magnitude;

            // Aseg�rate de que la velocidad est� dentro del rango apropiado
            float clampedSpeed = Mathf.Clamp(impactSpeed, 0f, maxImpactSpeed);

            // Env�a el valor del RTPC a Wwise
            if (impactSpeedRTPC != null)
            {
                impactSpeedRTPC.SetValue(gameObject, clampedSpeed);
            }
            else
            {
                Debug.LogWarning("El RTPC para la velocidad del impacto no est� asignado.");
            }

            // Env�a el evento a Wwise
            if (collisionEvent != null)
            {
                collisionEvent.Post(gameObject);
            }
            else
            {
                Debug.LogWarning("El evento de colisi�n no est� asignado.");
            }

            Debug.Log("COLISI�N DETECTADA CON VELOCIDAD: " + impactSpeed);
        }
    }

    // Funci�n para verificar si un GameObject est� en la lista de objetos espec�ficos
    bool ArrayContainsGameObject(GameObject obj, GameObject[] array)
    {
        foreach (GameObject item in array)
        {
            if (item == obj)
            {
                return true;
            }
        }
        return false;
    }

    // Funci�n para verificar si una etiqueta est� en la lista de etiquetas
    bool ArrayContainsTag(string tag, string[] array)
    {
        foreach (string item in array)
        {
            if (item == tag)
            {
                return true;
            }
        }
        return false;
    }
}
