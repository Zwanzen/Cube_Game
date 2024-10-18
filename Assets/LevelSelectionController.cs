using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionController : MonoBehaviour
{
    public GameObject levelParent;
    private RectTransform[] levels;

    // the three images that will be displayed
    public RectTransform[] imageRef = new RectTransform[3];
    private RectTransform leftRef;
    private RectTransform middleRef;
    private RectTransform rightRef;

    private int currentLevelIndex = 0;

    [SerializeField] private Image leftArrow;
    [SerializeField] private Image rightArrow;

    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    private void Start()
    {
        InitializeLevelCards();
        UpdateArrows();
    }

    private void InitializeLevelCards()
    {
        // get all children of the level parent
        levels = new RectTransform[levelParent.transform.childCount];
        for (int i = 0; i < levelParent.transform.childCount; i++)
        {
            levels[i] = levelParent.transform.GetChild(i).GetComponent<RectTransform>();
        }

        leftRef = imageRef[0];
        middleRef = imageRef[1];
        rightRef = imageRef[2];

        currentLevelIndex = 0;

        levels[0].position = middleRef.position;
        levels[0].rotation = middleRef.rotation;
        levels[0].localScale = middleRef.localScale;

        levels[1].position = rightRef.position;
        levels[1].rotation = rightRef.rotation;
        levels[1].localScale = rightRef.localScale;

        // set all cards to be invisible
        for (int i = 2; i < levels.Length; i++)
        {
            levels[i].GetChild(0).gameObject.SetActive(false);
        }
    }

    public void MoveRight()
    {
        if (currentLevelIndex < levels.Length - 1)
        {
            // if there are more cards after moving, make next card appare
            if (currentLevelIndex + 2 < levels.Length)
            {
                var r = levels[currentLevelIndex + 2].GetComponent<LevelCard>();
                r.transform.position = rightRef.position;
                r.transform.rotation = rightRef.rotation;

                r.Add(rightRef);
                r.Move(rightRef);
            }

            // move the cards
            var m = levels[currentLevelIndex + 1].GetComponent<LevelCard>();
            m.transform.position = middleRef.position;
            m.transform.rotation = middleRef.rotation;

            m.Move(middleRef);


            var l = levels[currentLevelIndex].GetComponent<LevelCard>();
            l.transform.position = leftRef.position;
            l.transform.rotation = leftRef.rotation;

            l.Move(leftRef);



            // remove the left card
            if (currentLevelIndex - 1 >= 0)
            {
                var g = levels[currentLevelIndex - 1].GetComponent<LevelCard>();
                g.Move(leftRef);
                g.Remove();
            }

            currentLevelIndex++;
        }

        UpdateArrows();
    }

    public void MoveLeft()
    {
        if (currentLevelIndex > 0)
        {
            // if there are more cards after moving, make next card appare
            if (currentLevelIndex - 2 >= 0)
            {
                var l = levels[currentLevelIndex - 2].GetComponent<LevelCard>();
                l.transform.position = leftRef.position;
                l.transform.rotation = leftRef.rotation;

                l.Add(leftRef);
                l.Move(leftRef);
            }

            // move the cards
            var m = levels[currentLevelIndex - 1].GetComponent<LevelCard>();
            m.transform.position = middleRef.position;
            m.transform.rotation = middleRef.rotation;

            m.Move(middleRef);

            var r = levels[currentLevelIndex].GetComponent<LevelCard>();
            r.transform.position = rightRef.position;
            r.transform.rotation = rightRef.rotation;

            r.Move(rightRef);

            // remove the right card
            if (currentLevelIndex + 1 < levels.Length)
            {
                var g = levels[currentLevelIndex + 1].GetComponent<LevelCard>();
                g.Move(rightRef);
                g.Remove();
            }

            currentLevelIndex--;
        }

        UpdateArrows();
    }

    private void UpdateArrows()
    {
        // if there are no more cards to the left, hide the left arrow
        if (currentLevelIndex < 1)
        {
            leftArrow.color = offColor;
        }
        else
        {
            leftArrow.color = onColor;
        }

        // if there are no more cards to the right, hide the right arrow
        if (currentLevelIndex >= levels.Length - 1)
        {
            rightArrow.color = offColor;
        }
        else
        {
            rightArrow.color = onColor;
        }
    }
}
