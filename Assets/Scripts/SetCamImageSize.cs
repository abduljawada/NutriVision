using UnityEngine;

public class SetCamImageSize : MonoBehaviour
{
    private void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
    }
}
