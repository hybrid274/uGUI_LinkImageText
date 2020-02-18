 using UnityEditor.UI;
 using UnityEditor;

 namespace LinkText
 {
     [CustomEditor(typeof(LinkText), true)]
     [CanEditMultipleObjects]
     public class LinkTextEditor : TextEditor
     {
         SerializedProperty m_LinkColor;

         protected override void OnEnable()
         {
             base.OnEnable();
             m_LinkColor = serializedObject.FindProperty("LinkColor");
         }
         public override void OnInspectorGUI()
         {
             base.OnInspectorGUI();
             EditorGUILayout.PropertyField(m_LinkColor);
             serializedObject.ApplyModifiedProperties();
         }
     }
 }
