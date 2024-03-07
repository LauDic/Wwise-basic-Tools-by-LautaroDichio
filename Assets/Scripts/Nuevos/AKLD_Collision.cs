using UnityEngine;

public class AKLD_Collision : CustomEditorBase
{
    // Nombre del evento que se enviar� a Wwise
    public AK.Wwise.Event collisionEvent = null;

    // Nombre del RTPC que se utilizar� para la velocidad del impacto
    public AK.Wwise.RTPC impactSpeedRTPC = null;

    // Valor m�ximo del RTPC para la velocidad del impacto
    public float maxImpactSpeed = 100.0f;

    // Funci�n llamada cuando ocurre una colisi�n
    void OnCollisionEnter(Collision collision)
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

        Debug.Log("COLISI�N DETECTADA CON VELOCIDAD a: " + impactSpeed);
    }
}
