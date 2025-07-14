using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    [SerializeField] private AudioSource musicSource;

    public static MusicManager Instance => instance;
    public bool isMusicOn = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SiwtchMusicState()
    {
        isMusicOn = !isMusicOn;

        if (isMusicOn)
        {
            musicSource?.Play();
        }
        else
        {
            musicSource?.Stop();
        }
    }
}
