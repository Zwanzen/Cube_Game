using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{

    public Image[] Categories;
    public Color SelectColor;

    public GameObject[] Panels;

    public void Start()
    {
        // set to the first category
        ChangeCategory(0);
    }


    public void ChangeCategory(int i)
    {
        // set all categories to default color
        foreach (Image category in Categories)
        {
            category.color = Color.white;
        }

        // set the selected category to the select color
        Categories[i].color = SelectColor;

        // set all panels to inactive
        foreach (GameObject panel in Panels)
        {
            panel.SetActive(false);
        }

        // set the selected panel to active
        Panels[i].SetActive(true);
    }

}
