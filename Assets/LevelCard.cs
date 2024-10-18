using UnityEngine;
using MoreMountains;
using MoreMountains.Feedbacks;

public class LevelCard : MonoBehaviour
{

    [SerializeField] private MMF_Player move;
    [SerializeField] private MMF_Player add;
    [SerializeField] private MMF_Player remove;

    public void Move(RectTransform scale)
    {
        move.StopFeedbacks();

        // change the scale of the feedback
        MMF_Scale scaleFeedback = move.GetFeedbackOfType<MMF_Scale>();
        scaleFeedback.RemapCurveOne = scale.localScale.x + (scale.localScale.x * 0.1f);
        scaleFeedback.RemapCurveZero = scale.localScale.x;

        move.PlayFeedbacks();
    }

    public void Add(RectTransform scale)
    {
        add.PlayFeedbacks();
    }

    public void Remove()
    {
        remove.PlayFeedbacks();
    }

}
