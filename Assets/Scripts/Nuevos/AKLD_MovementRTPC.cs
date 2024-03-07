// Script created by Lautaro Dichio.
// calculates player speed based on rigidbody and creates rtpc to send to wwise

using UnityEditor;
using UnityEngine;

public class AKLD_MovementRTPC : CustomEditorBase
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AK.Wwise.RTPC RTPCVelocity; // Nombre del RTPC en Wwise
    public float velocidad;


    private void Start()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody not assigned. Please assign a Rigidbody to the script.");
        }

        // Initialize the RTPC to a default value if required

        RTPCVelocity.SetValue(this.gameObject, 0.0f);
    }

    private void Update()
    {
        if (rb != null)
        {
            // Get the current Rigidbody speed
            float velocityMagnitude = rb.velocity.magnitude;

            // Updating the RTPC in Wwise with Rigidbody speed
            RTPCVelocity.SetGlobalValue(velocityMagnitude);
            RTPCVelocity.SetValue(this.gameObject,velocityMagnitude);
        }
        velocidad = rb.velocity.magnitude;
        // Debug.Log("VELOCIDAD " +  velocidad);

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AKLD_MovementRTPC))]
    public class AKLD_MovementRTPCEditor : Editor
    {
        private Texture2D image;

        private SerializedProperty m_Script; // Referencia al campo m_Script

        private void OnEnable()
        {
            // Obtener la referencia al campo m_Script
            m_Script = serializedObject.FindProperty("m_Script");

            // Cargar la imagen desde los recursos
            image = Resources.Load<Texture2D>("Titulo script 7");
        }

        public override void OnInspectorGUI()
        {
            // Actualizar el objeto serializado
            serializedObject.Update();

            // Mostrar la imagen en el Inspector (arriba de todo)
            if (image != null)
            {
                GUILayout.Space(10f);
                Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(image.height));
                EditorGUI.DrawTextureTransparent(rect, image, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUILayout.HelpBox("No se pudo cargar la imagen. Asegúrate de que esté en la carpeta Resources.", MessageType.Warning);
            }

            // Mostrar los campos predeterminados del script excepto el campo m_Script
            DrawPropertiesExcluding(serializedObject, "m_Script");

            // Aplicar los cambios al objeto serializado
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
