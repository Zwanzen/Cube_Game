using UnityEngine;

public class TimeController : MonoBehaviour
{

    public float timescale = 2.8f;

    private void Awake()
    {
        // set the timescale to the value of the timescale variable
        Time.timeScale = timescale;
    }

}
