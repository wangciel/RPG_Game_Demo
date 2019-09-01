using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenuScript : MonoBehaviour {

    public void play()
    {
        SceneManager.LoadScene(4);
    }

    

    public void quit()
    {
        Application.Quit();
        Debug.Log("IT QUIT");
    }

    public void mainMenu(){
        SceneManager.LoadScene(0);
    }
}
