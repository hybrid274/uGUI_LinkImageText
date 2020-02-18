using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LinkText
{
    public static class MenuOptions
    {
        private static MethodInfo m_PlaceUIElementRoot;

        [MenuItem("GameObject/UI/LinkText", false, 2001)]
        public static void AddText(MenuCommand menuCommand)
        {
            GameObject linkImageTextGameObject = new GameObject("LinkText");
            RectTransform rectTransform = linkImageTextGameObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160f, 30f);
            LinkText linkImageTextComponent = linkImageTextGameObject.AddComponent<LinkText>();
            linkImageTextComponent.text = "New LinkText";
            linkImageTextComponent.color = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
#if UNITY_5_3_OR_NEWER
            linkImageTextComponent.alignByGeometry = true;
#endif
            PlaceUIElementRoot(linkImageTextGameObject, menuCommand);
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            if (m_PlaceUIElementRoot == null)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.UI.TextEditor));
                Type type = assembly.GetType("UnityEditor.UI.MenuOptions");
                m_PlaceUIElementRoot = type.GetMethod("PlaceUIElementRoot", BindingFlags.NonPublic | BindingFlags.Static);
            }
            m_PlaceUIElementRoot.Invoke(null, new object[] { element, menuCommand });
        }
    }
}
