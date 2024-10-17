using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WiggleHitPlayer : MonoBehaviour
{

    private MMF_Player wiggleFeedback;
    MMF_ScaleShake wiggle;
    MMScaleShaker _W;

    private void Awake()
    {
        wiggleFeedback = transform.AddComponent<MMF_Player>();
        _W = transform.AddComponent<MMScaleShaker>();
        wiggle = new MMF_ScaleShake();
        wiggle.TargetShaker = _W;
        wiggle.ShakeRange = -3.5f;
        wiggle.ShakeSpeed = 1f;
        wiggle.RandomizeDirection = false;
        wiggle.AddDirectionalNoise = false;
        wiggleFeedback.RandomizeDuration = false;
        wiggle.RandomizeSeedOnShake = false;
        wiggle.ShakeMainDirection = Vector3.one;
        wiggleFeedback.AddFeedback(wiggle);
    }

    public void Wiggle()
    {
        wiggleFeedback.PlayFeedbacks();
    }

}
