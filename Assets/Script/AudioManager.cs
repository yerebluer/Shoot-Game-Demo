using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel
    {
        Master,Sfx,Music
    }
    public float masterVolumePercent { get; private set; }
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }

    AudioSource[] musicSources;
    //sound effect 2d source
    AudioSource sfx2DSource;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    Transform audioListener;
    Transform playerT;
    SoundLibrary library;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music source" + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                musicSources[i].loop = true;
                newMusicSource.transform.parent = transform;
            }
            GameObject newSfx2Dsource = new GameObject("2D sfx source");
            sfx2DSource = newSfx2Dsource.AddComponent<AudioSource>();
            newSfx2Dsource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            if(FindObjectOfType<Player>()!=null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }
            
            library = GetComponent<SoundLibrary>();

            masterVolumePercent= PlayerPrefs.GetFloat("master vol", 1);
            sfxVolumePercent= PlayerPrefs.GetFloat("sfx vol", 1);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", 1);
            
        }
    }
    private void Update()
    {
        if (playerT != null)
        {
            audioListener.position = playerT.position;
        }
    }
    public void SetVolume(float volumePercent,AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:masterVolumePercent = volumePercent;break;
            case AudioChannel.Sfx: sfxVolumePercent = volumePercent; break;
            case AudioChannel.Music: musicVolumePercent = volumePercent; break;
            default:Debug.Log("wrong Audio Channel");break;
        }
        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;
        //相当于写入缓存
        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlaySound(string soundName,Vector3 pos)
    {
        PlaySound(library.GetClipFromName(soundName), pos);
    }
    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        if(musicSources[activeMusicSourceIndex].clip != clip)
        {
            activeMusicSourceIndex = 1 - activeMusicSourceIndex;
            musicSources[activeMusicSourceIndex].clip = clip;
            musicSources[activeMusicSourceIndex].Play();

            StartCoroutine(AnimateMusicCrossfade(fadeDuration));
        }
    }

    public void PlaySound(AudioClip clip,Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
        }
    }
    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }
    IEnumerator AnimateMusicCrossfade(float duraton)
    {
        float percent=0;
        float speed = 1 / duraton;
        while (percent < 1)
        {
            percent += speed * Time.deltaTime;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent,percent);
            musicSources[1-activeMusicSourceIndex].volume = Mathf.Lerp( musicVolumePercent * masterVolumePercent,0,percent);
            yield return null;
        }
    }
}
