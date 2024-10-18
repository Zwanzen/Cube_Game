using UnityEngine;
using MoreMountains;
public class UIController : MonoBehaviour
{
    public Camera UIcam;
    public LayerMask UILayerMask;

    private UICustomButton currentSelectedButton;

    void Update()
    {
        OnHoverButton();
    }

    private void OnHoverButton()
    {
        // Cast a ray from the mouse position to the UI layer
        Ray ray = UIcam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity,UILayerMask))
        {
            Debug.Log(hit.collider.name);

            // If the ray hits a button, we store it
            UICustomButton button = hit.collider.GetComponent<UICustomButton>();
            if (button != null)
            {
                currentSelectedButton = button;
                PlayHoverFeedbacks(currentSelectedButton.transform as RectTransform);
            }
        }
        else
        {
            // If the ray doesn't hit a button, we reset the current selected button
            if (currentSelectedButton != null)
            {
                currentSelectedButton = null;
            }
        }
    }

    private void PlayHoverFeedbacks(RectTransform t)
    {
        t.localScale = Vector3.one * 1.2f;
    }
}
