using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

public class SoundsManager : MonoBehaviour
{
    enum ClipType
    {
        ClickButton,
        ClickCell
    }

    [SerializeField] private ClipType _clipType;
    [SerializeField] private AudioClip[] _clickButtonClip;
    [SerializeField] private AudioClip[] _clickCellClip;
    private ObjectPool<AudioSource> _audioSourcePool;

    [SerializeField] private float _soundDuration = 0.5f;

    public bool isSoundOn = true;

    private static SoundsManager instance;
    public static SoundsManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        

        CreatePool();

    }

    private void OnDestroy()
    {
        _audioSourcePool.Dispose();
    }

    #region 实现 Unity 对象池
    //对池子进行初始化（创建初始容量个数的物体）
    public void CreatePool()
    {
        _audioSourcePool = new ObjectPool<AudioSource>(
            createFunc: () =>  CreateAudioSource(),
            actionOnGet: (audio) => OnGetAudioSource(audio),
            actionOnRelease: (audio) => OnReleaseAudioSource(audio),
            actionOnDestroy: (audio) => Destroy(audio),
            defaultCapacity: 3,
            maxSize: 8
        );

    }

    private AudioSource CreateAudioSource()
    {
        AudioSource audioSource = transform.AddComponent<AudioSource>();
        return audioSource;
    }

    private void OnGetAudioSource(AudioSource audioSource)
    {
        audioSource.enabled = true;

        switch (_clipType)
        {
            case ClipType.ClickButton:
                audioSource.clip = _clickButtonClip[Random.Range(0, _clickButtonClip.Length)];
                break;
            case ClipType.ClickCell:
                audioSource.clip = _clickCellClip[Random.Range(0, _clickCellClip.Length)];
                break;
        }

        audioSource.Play();
        StartCoroutine(DelayStopAudio(audioSource, _soundDuration));
    }

    private void OnReleaseAudioSource(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.enabled = false;
    }

#endregion

    public void PlayButtonAudio()
    {
        if (!isSoundOn) return;

        _clipType = ClipType.ClickButton;
        _audioSourcePool?.Get();
    }

    public void PlayCellAudio()
    {
        if (!isSoundOn) return;

        _clipType = ClipType.ClickCell;
        _audioSourcePool?.Get();
     }

    IEnumerator DelayStopAudio(AudioSource audio, float delay)
    {
        yield return new WaitForSeconds(delay);
        _audioSourcePool?.Release(audio);
    }

}
