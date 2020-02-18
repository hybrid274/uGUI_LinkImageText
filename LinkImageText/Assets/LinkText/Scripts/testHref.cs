using UnityEngine;

namespace LinkText
{
    public class testHref : MonoBehaviour
    {
        private ITextHerfEvent textPic;

        private void Awake()
        {
            textPic = GetComponent<ITextHerfEvent>();
        }

        private void OnEnable()
        {
            textPic.OnHrefClick.AddListener(OnHrefClick);
        }

        private void OnDisable()
        {
            textPic.OnHrefClick.RemoveListener(OnHrefClick);
        }

        private void OnHrefClick(string hrefName)
        {
            Debug.Log("OnClick " + hrefName);
        }
    }
}
