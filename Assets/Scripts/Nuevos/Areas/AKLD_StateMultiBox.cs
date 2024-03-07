using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AKLD_StateMultiBox : MonoBehaviour
{
    public Transform objectToCheck;  // Objeto cuya posición se verifica para determinar la activación del área

    [System.Serializable]
    public class AreaData
    {
        // Configuración de la zona en forma de caja
        [Header("Box")]
        public Vector3 relativeCenter = Vector3.zero;  // Centro relativo de la zona con respecto al objeto principal
        public Vector3 size = new Vector3(1f, 1f, 1f);  // Tamaño de la zona en cada dimensión (x, y, z)
        public Color gizmoColor = Color.yellow;  // Color del gizmo en el editor

        // Configuración del evento Wwise
        [Header("Wwise Al Entrar")]
        public AK.Wwise.State enterState = null;  // Estado de Wwise asociado con la zona
        public bool DebugOn = false;
        public string message = "Dentro del área";  // Mensaje para mostrar cuando se activa la zona

        [Header("Al Salir")]
        public bool sendExitStateOnce = true;  // Indica si enviar el estado de salida una vez
        public AK.Wwise.State stateOnExit = null;
        [HideInInspector]
        public bool hasEntered = false; // Indica si el objeto ha entrado en el área

        // Constructor predeterminado para serialización
        public AreaData() { }

        // Método de inicialización
        public void Initialize()
        {
            size = new Vector3(Mathf.Max(size.x, 0.1f), Mathf.Max(size.y, 0.1f), Mathf.Max(size.z, 0.1f));  // Inicializar el tamaño con valores positivos
        }
    }

    // Lista de datos de áreas
    public List<AreaData> areas = new List<AreaData>();

    private void Start()
    {
        foreach (var area in areas)
        {
            area.Initialize();  // Inicializar cada área
        }
    }

    private void Update()
    {
        if (objectToCheck == null)
        {
            return;
        }
        foreach (var area in areas)
        {
            // Verificar si el objeto está dentro de la zona y la zona no está activada
            if (IsInsideArea(objectToCheck.position, area) && !area.hasEntered)
            {
                if (area.DebugOn)
                    Debug.Log(area.message);  // Mostrar el mensaje de la zona

                // Activar el evento si está configurado
                if (area.enterState != null)
                {
                    UpdateState(area.enterState);
                }

                area.hasEntered = true;  // Marcar la zona como ingresada
            }

            // Si el objeto no está dentro de la zona
            else if (!IsInsideArea(objectToCheck.position, area))
            {
                if (area.sendExitStateOnce && area.hasEntered)
                {
                    if (area.stateOnExit != null)
                    {
                        StateOnExit(area.stateOnExit);
                    }
                    area.hasEntered = false; // Restablecer el estado ingresado
                }
            }
        }
    }

    // Determina si una posición está dentro de una zona
    private bool IsInsideArea(Vector3 position, AreaData area)
    {
        Vector3 areaCenter = transform.position + area.relativeCenter;  // Centro global de la zona
        Vector3 minBound = areaCenter - area.size * 0.5f;
        Vector3 maxBound = areaCenter + area.size * 0.5f;

        return position.x > minBound.x && position.x < maxBound.x &&
               position.y > minBound.y && position.y < maxBound.y &&
               position.z > minBound.z && position.z < maxBound.z;
    }

    private void UpdateState(AK.Wwise.State myState)
    {
        myState.SetValue();
    }

    private void StateOnExit(AK.Wwise.State myState)
    {
        myState.SetValue();
    }
}

#if UNITY_EDITOR
// Editor personalizado para el inspector de Unity
[CustomEditor(typeof(AKLD_StateMultiBox))]
public class AKLD_StateMultiBoxEditor : Editor
{
    private Texture2D image;
    private SerializedProperty m_Script;

    private void OnEnable()
    {
        m_Script = serializedObject.FindProperty("m_Script");
        image = Resources.Load<Texture2D>("Titulo script 7");
    }

    // Dibuja gizmos en la escena del editor
    private void OnSceneGUI()
    {
        AKLD_StateMultiBox manager = target as AKLD_StateMultiBox;

        if (manager != null)
        {
            foreach (var area in manager.areas)
            {
                DrawAreaGizmo(manager, area);
            }
        }
    }

    // Dibuja el inspector personalizado en el Editor de Unity
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Muestra la imagen en el Inspector (arriba)
        if (image != null)
        {
            GUILayout.Space(10f);
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(image.height));
            EditorGUI.DrawTextureTransparent(rect, image, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUILayout.HelpBox("No se pudo cargar la imagen. Asegúrate de que está en la carpeta Resources.", MessageType.Warning);
        }

        // Muestra los campos predeterminados del script excepto el campo m_Script
        DrawPropertiesExcluding(serializedObject, "m_Script");

        serializedObject.ApplyModifiedProperties();
    }

    // Dibuja el gizmo de la zona en la escena del editor
    private void DrawAreaGizmo(AKLD_StateMultiBox manager, AKLD_StateMultiBox.AreaData area)
    {
        Vector3 areaGlobalCenter = manager.transform.position + area.relativeCenter;
        Vector3 minBound = areaGlobalCenter - area.size * 0.5f;
        Vector3 maxBound = areaGlobalCenter + area.size * 0.5f;

        Handles.color = area.gizmoColor;
        Handles.DrawWireCube(areaGlobalCenter, area.size);

        // Dibuja un manipulador de gizmo para mover el área
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(areaGlobalCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(manager, "Mover área");
            area.relativeCenter = newPosition - manager.transform.position;
        }
    }
}
#endif
