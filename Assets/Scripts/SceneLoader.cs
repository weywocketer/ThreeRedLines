using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Dropdown scenarioDropdown;

    public void LoadScenario()
    {
        SceneManager.LoadScene(scenarioDropdown.value + 1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
