using UnityEngine;
using MoreMountains;
using MoreMountains.Feedbacks;
public class UIController : MonoBehaviour
{
    public MMF_Player startMenu;
    bool hasStarted = false;

    float timer = 0;

    private void LateUpdate()
    {
        timer += Time.deltaTime;

        if (!hasStarted && timer > 0.5f)
        {
            startMenu.PlayFeedbacks();
            hasStarted = true;
        }
    }
}
