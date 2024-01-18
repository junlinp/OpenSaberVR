using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameMenuInterface : MonoBehaviour
{

    public TextMeshPro point_gui;
    private int current_point = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (point_gui != null)
        {
            point_gui.text = string.Format("0");
            current_point = 0;
        }
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnComboSuccess()
    {
        current_point += 1;
        point_gui.text = string.Format("{0}", current_point);
    }

    public void OnComboFails()
    {
        current_point = 0;
        point_gui.text = string.Format("{0}", current_point);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
        //SceneManager.LoadScene("PersistentScene", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("PersistentScene");
    }
}
