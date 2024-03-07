using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AKLD_RTPCMultiBox : CustomEditorBase
{
    public Transform objectToCheck;

    [System.Serializable]
    public class AreaData
    {
        [Header("Box")]
        public Vector3 relativeCenter = Vector3.zero;
        public Vector3 size = new Vector3(1f, 1f, 1f);
        public Color gizmoColor = Color.yellow;

        [Header("Wwise On Enter")]
        public AK.Wwise.RTPC enterRTPC = null;
        public float enterValue = 0f;
        public bool DebugOn = false;
        public string message = "Inside the area";
        public bool global = false;
        public bool otherGameObject = false;
        public GameObject otherObject = null;

        [Header("On Exit")]
        public bool OnExit = false;
        public AK.Wwise.RTPC RTPCOnExit = null;
        public float exitValue = 0f;
        public bool globalExit = false;
        public bool otherGameObjectExit = false;
        public GameObject otherObjectExit = null;

        [HideInInspector]
        public bool areaActivated = false;
        [HideInInspector]
        public bool exitEventTriggered = false;
        [HideInInspector]
        public bool enteredOnce = false;

        // M�todo para inicializar datos del �rea
        public void Initialize()
        {
            size = new Vector3(1f, 1f, 1f);
            exitEventTriggered = false;
            enteredOnce = false;  // Reiniciar la bandera al inicializar
        }
    }

    public List<AreaData> areas = new List<AreaData>() { new AreaData() };

    private void Update()
    {
        if (objectToCheck == null)
        {
            return;
        }

        foreach (var area in areas)
        {
            bool isInside = IsInsideArea(objectToCheck.position, area);

            if (isInside && !area.areaActivated)
            {
                if (area.DebugOn)
                    Debug.Log(area.message);

                if (area.enterRTPC != null)
                    UpdateRTPC(area.enterRTPC, area.enterValue, area.global, area.otherGameObject, area.otherObject);

                area.areaActivated = true;
                area.exitEventTriggered = false;
                area.enteredOnce = true;
            }
            else if (!isInside)
            {
                area.areaActivated = false;

                if (area.OnExit && !area.exitEventTriggered && area.enteredOnce)
                {
                    RTPCOnExit(area.RTPCOnExit, area.exitValue, area.globalExit, area.otherGameObjectExit, area.otherObjectExit);
                    area.exitEventTriggered = true;
                }
            }
        }
    }

    private bool IsInsideArea(Vector3 position, AreaData area)
    {
        Vector3 areaGlobalCenter = transform.position + area.relativeCenter;
        Vector3 halfSize = area.size * 0.5f;
        Vector3 minCorner = areaGlobalCenter - halfSize;
        Vector3 maxCorner = areaGlobalCenter + halfSize;

        return (position.x > minCorner.x && position.x < maxCorner.x &&
                position.y > minCorner.y && position.y < maxCorner.y &&
                position.z > minCorner.z && position.z < maxCorner.z);
    }

    private void UpdateRTPC(AK.Wwise.RTPC myRTPC, float valor, bool global, bool other, GameObject otherGO)
    {
        if (global)
            myRTPC.SetGlobalValue(valor);
        else if (other)
            myRTPC.SetValue(otherGO, valor);
        else
            myRTPC.SetValue(gameObject, valor);
    }

    private void RTPCOnExit(AK.Wwise.RTPC myRTPC, float valor, bool global, bool other, GameObject otherGO)
    {
        if (global)
            myRTPC.SetGlobalValue(valor);
        else if (other)
            myRTPC.SetValue(otherGO, valor);
        else
            myRTPC.SetValue(gameObject, valor);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AKLD_RTPCMultiBox))]
    public class AKLD_RTPCMultiBoxEditor : Editor
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
                EditorGUILayout.HelpBox("No se pudo cargar la imagen. Aseg�rate de que est� en la carpeta Resources.", MessageType.Warning);
            }

            // Mostrar los campos predeterminados del script excepto el campo m_Script
            DrawPropertiesExcluding(serializedObject, "m_Script");

            // Aplicar los cambios al objeto serializado
            serializedObject.ApplyModifiedProperties();

            // Mostrar el bot�n de inicializaci�n de tama�os
            AKLD_RTPCMultiBox manager = target as AKLD_RTPCMultiBox;
            if (GUILayout.Button("Initialize Sizes") && manager != null)
            {
                InitializeSizes(manager);
            }
        }

        private void InitializeSizes(AKLD_RTPCMultiBox manager)
        {
            if (manager != null)
            {
                foreach (var area in manager.areas)
                {
                    area.Initialize();
                }
            }
        }

        private void OnSceneGUI()
        {
            AKLD_RTPCMultiBox manager = target as AKLD_RTPCMultiBox;

            if (manager != null)
            {
                foreach (var area in manager.areas)
                {
                    DrawAreaGizmo(manager, area);
                }
            }
        }

        private void DrawAreaGizmo(AKLD_RTPCMultiBox manager, AKLD_RTPCMultiBox.AreaData area)
        {
            Vector3 areaGlobalCenter = manager.transform.position + area.relativeCenter;

            // Guardar la matriz de transformaci�n actual
            Matrix4x4 currentMatrix = Handles.matrix;

            // Calcular la matriz de transformaci�n para dibujar el wire cube en las coordenadas locales del �rea
            Matrix4x4 areaTransformMatrix = Matrix4x4.TRS(areaGlobalCenter, manager.transform.rotation, area.size);

            // Establecer la matriz de transformaci�n para dibujar el wire cube
            Handles.matrix = areaTransformMatrix;

            Handles.color = area.gizmoColor;
            Handles.DrawWireCube(Vector3.zero, Vector3.one);

            // Restaurar la matriz de transformaci�n original
            Handles.matrix = currentMatrix;
        }
    }
#endif
}
