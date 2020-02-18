using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LinkText
{
    public interface ITextHerfEvent
    {
        HrefEvent OnHrefClick { get; set; }
    }

    [Serializable]
    public class HrefEvent : UnityEvent<string> { }

    public class LinkText : Text, IPointerClickHandler, ITextHerfEvent
    {
        private class HrefInfo
        {
            public int startIndex;
            public int endIndex;
            public string name;
            public readonly List<Rect> boxes = new List<Rect>();
        }

        private string m_OutputText;
        private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();
        protected static readonly StringBuilder s_TextBuilder = new StringBuilder();
        private HrefEvent m_OnHrefClick = new HrefEvent();
        private static readonly Regex s_HrefRegex = new Regex(@"<a href=([^>\n\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

        [HideInInspector] public Color LinkColor = Color.blue;

        public HrefEvent OnHrefClick
        {
            get { return m_OnHrefClick; }
            set { m_OnHrefClick = value; }
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
            if (UnityEditor.PrefabUtility.GetPrefabInstanceStatus(this) != UnityEditor.PrefabInstanceStatus.NotAPrefab)
#else
                if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
#endif
            {
                return;
            }
#endif
            m_OutputText = GetOutputText(text);
        }

        public override float preferredWidth
        {
            get
            {
                TextGenerationSettings generationSettings = this.GetGenerationSettings(Vector2.zero);
                return this.cachedTextGeneratorForLayout.GetPreferredWidth(this.m_OutputText, generationSettings) / this.pixelsPerUnit;
            }
        }

        public override float preferredHeight
        {
            get
            {
                TextGenerationSettings generationSettings = this.GetGenerationSettings(new Vector2(base.GetPixelAdjustedRect().size.x, 0f));
                return this.cachedTextGeneratorForLayout.GetPreferredHeight(this.m_OutputText, generationSettings) / this.pixelsPerUnit;
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            var orignText = m_Text;
            m_Text = m_OutputText;
            base.OnPopulateMesh(toFill);
            m_Text = orignText;
            UIVertex vert = new UIVertex();

            foreach (var hrefInfo in m_HrefInfos)
            {
                hrefInfo.boxes.Clear();
                if (hrefInfo.startIndex >= toFill.currentVertCount)
                {
                    continue;
                }

                toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
                var pos = vert.position;
                var bounds = new Bounds(pos, Vector3.zero);
                Vector3 previousPos = Vector3.zero;
                for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
                {
                    if (i >= toFill.currentVertCount)
                    {
                        break;
                    }

                    toFill.PopulateUIVertex(ref vert, i);
                    pos = vert.position;
                    if ((i - hrefInfo.startIndex) % 4 == 1)
                    {
                        previousPos = pos;
                    }
                    if (previousPos != Vector3.zero && (i - hrefInfo.startIndex) % 4 == 0 && pos.x < previousPos.x)
                    {
                        hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                        bounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(pos);
                    }
                }
                hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
            }
        }

        protected virtual string GetOutputText(string outputText)
        {
            s_TextBuilder.Length = 0;
            m_HrefInfos.Clear();
            var indexText = 0;
            foreach (Match match in s_HrefRegex.Matches(outputText))
            {
                s_TextBuilder.Append(outputText.Substring(indexText, match.Index - indexText));
                s_TextBuilder.Append("<color=#" + ColorUtility.ToHtmlStringRGB(LinkColor) + ">");
                var group = match.Groups[1];
                var hrefInfo = new HrefInfo
                {
                    startIndex = s_TextBuilder.Length * 4,
                    endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                    name = group.Value
                };
                m_HrefInfos.Add(hrefInfo);

                s_TextBuilder.Append(match.Groups[2].Value);
                s_TextBuilder.Append("</color>");
                indexText = match.Index + match.Length;
            }
            s_TextBuilder.Append(outputText.Substring(indexText, outputText.Length - indexText));
            return s_TextBuilder.ToString();
        }

        #region IPointerClickHandler Member
        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out lp);

            foreach (var hrefInfo in m_HrefInfos)
            {
                var boxes = hrefInfo.boxes;
                for (var i = 0; i < boxes.Count; ++i)
                {
                    if (boxes[i].Contains(lp))
                    {
                        m_OnHrefClick.Invoke(hrefInfo.name);
                        return;
                    }
                }
            }
        }
        #endregion
    }
}
