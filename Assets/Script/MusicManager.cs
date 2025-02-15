using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string sceneName;
    // Start is called before the first frame update
    void Start()
    {
        OnLevelWasLoaded(0);

    }

    private void OnLevelWasLoaded(int level)
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        if (newSceneName != sceneName)
        {
            sceneName = newSceneName;
            Invoke("PlayMusic", .2f);
            Debug.Log("newScene " + newSceneName);
        }
    }
    void PlayMusic()
    {
        AudioClip clipToPlay = null;
        Debug.Log("playMusic  "+sceneName+ (sceneName == "Menu").ToString()+(sceneName == "Game").ToString());
        if (sceneName == "Menu")
        {
            clipToPlay = menuTheme;
        }else if (sceneName == "Game")
        {
            clipToPlay = mainTheme;
        }
        if (clipToPlay != null)
        {
            AudioManager.instance.PlayMusic(clipToPlay, 2);
            Invoke("PlayMusic", clipToPlay.length);
        }
    }
}
