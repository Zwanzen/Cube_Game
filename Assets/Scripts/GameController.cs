using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using MoreMountains.Feedbacks;

public class GameController : MonoBehaviour
{
    [Header("Player Stuff")]
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private CameraHandler cameraFollower;
    [SerializeField]
    public int maxHealth = 3;           // Maximum health points
    [SerializeField]
    private Image[] heartImages;         // Array of heart images representing lives
    [SerializeField]
    private TextMeshProUGUI timerText;
    [SerializeField]
    private TextMeshProUGUI crystalCounterText;
    [SerializeField]
    private Image reloadImage;

    [Space(20)]
    [SerializeField]
    private GameObject[] uiToTurnOff;
    [SerializeField]
    private GameObject victoryCamera;
    [SerializeField]
    private TextMeshProUGUI finalTime;

    [Space(20)]
    [SerializeField]
    private MMF_Player crystalFeedback;
    [SerializeField]
    private AudioClip gainCrystal;
    [SerializeField]
    private AudioClip looseCrystal;


    private MMF_TMPColor f_color;
    private AudioSource f_audio;

    [HideInInspector]
    public int currentHealth;

    [HideInInspector]
    public int crystalAmount = 0;
    private float gameTime;

    private float holdTime = 1f;
    private float timer;

    private bool ded;
    public bool victory;

    [HideInInspector]
    public bool finished = false;
    bool hasMoved;
    private Vector2 startPos;

    private void Start()
    {
        // Initialize the variables
        gameTime = 0f;
        currentHealth = maxHealth;
        UpdateHeartImages();
        UpdateCrystalAmount();
        f_color = crystalFeedback.GetFeedbackOfType<MMF_TMPColor>();
        f_audio = crystalFeedback.GetComponent<AudioSource>();
        startPos = new Vector2(player.transform.position.x, player.transform.position.z);
    }

    private void Update()
    {

        if (!ded && hasMoved)
        {
            gameTime += Time.deltaTime / 2.8f;
            UpdateTimeText();
        }

        if(Input.GetKey(KeyCode.R) && gameTime > 1f)
        {
            HoldForRestart();
        }
        else
        {
            timer = 0f;
            reloadImage.fillAmount = 1;
            reloadImage.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        var pos = new Vector2(player.transform.position.x, player.transform.position.z);

        if (pos != startPos)
        {
            hasMoved = true;
        }
    }

    private void HoldForRestart()
    {
        timer += Time.deltaTime;
        reloadImage.gameObject.SetActive(true);

        reloadImage.fillAmount = timer / holdTime;

        if (timer > holdTime * 2.8f)
        {
            RestartGame();
        }
    }

    public void Victory()
    {
        foreach(var x in uiToTurnOff)
        {
            x.SetActive(false);

        }
        victory = true;
        victoryCamera.SetActive(true);
        finalTime.gameObject.SetActive(true);
        finalTime.text = timerText.text;
    }

    private void RestartGame()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // Check if player is still alive
        if (currentHealth <= 0)
        {
            // Player is dead
            // Perform necessary actions, such as game over or respawn
            Debug.Log("Player is dead");
            cameraFollower.dead = true;
            player.SetActive(false);
            ded = true;
        }

        UpdateHeartImages();
    }

    private void UpdateHeartImages()
    {
        // Disable heart images based on current health
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
                heartImages[i].enabled = true;
            else
                heartImages[i].enabled = false;
        }
    }

    public void PurchaseHeart()
    {
        crystalAmount -= 2;
        currentHealth++;
        UpdateHeartImages();
        UpdateCrystalAmount();
        f_audio.clip = looseCrystal;
        f_color.DestinationColor = Color.red;
        crystalFeedback.PlayFeedbacks();
    }

    public void GetCrystal()
    {
        crystalAmount++;
        UpdateCrystalAmount();
        f_color.DestinationColor = Color.green;
        f_audio.clip = gainCrystal;
        crystalFeedback.PlayFeedbacks();

    }

    private void UpdateCrystalAmount()
    {
        crystalCounterText.text = crystalAmount.ToString();
    }

    private void UpdateTimeText()
    {
        // Update the text component with the current game time
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        int milliseconds = Mathf.FloorToInt((gameTime * 100f) % 100f);

        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

}
