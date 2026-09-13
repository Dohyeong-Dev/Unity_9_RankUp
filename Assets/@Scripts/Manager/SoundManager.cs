using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary> BGM과 SFX의 재생 및 AudioSource 관리를 담당한다. </summary>
public class SoundManager : MonoBehaviour
{
    private const string BgmVolumeParameter = "BgmVolume";
    private const string SfxVolumeParameter = "SfxVolume";

    private AudioMixer _audioMixer;
    private AudioMixerGroup _bgmMixerGroup;
    private AudioMixerGroup _sfxMixerGroup;

    private float _minDistance = 1f;
    private float _maxDistance = 15f;

    private readonly Dictionary<string, Sfx> _sfxMap = new();

    private AudioListener _audioListener;
    private AudioSource _bgmAudioSource;

    private Coroutine _bgmFadeCoroutine;

    private Transform _followTarget;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        Managers.Scene.OnSceneLoaded += HandleSceneLoaded;
    }

    private void Update()
    {
        if (_followTarget == null)
        {
            return;
        }

        _audioListener.transform.position = _followTarget.position;
    }

    private void OnDestroy()
    {
        Managers.Scene.OnSceneLoaded -= HandleSceneLoaded;
    }

    #region ===== 초기화 =====

    /// <summary> SoundManager에서 사용하는 AudioListener, AudioSource 및 AudioMixer를 초기화한다. </summary>
    private void Initialize()
    {
        InitializeAudioMixer();
        InitializeAudioListener();
    }

    /// <summary> SoundMixer를 로드하고 BGM과 SFX에 사용할 Mixer Group을 설정한다. </summary>
    private void InitializeAudioMixer()
    {
        _audioMixer = Managers.Resource.Load<AudioMixer>(ResourceKey.Path.SoundMixer);

        if (_audioMixer == null)
        {
            CPrint.Error($"[SoundManager] AudioMixer를 찾을 수 없습니다. Path: {ResourceKey.Path.SoundMixer}");
            return;
        }

        AudioMixerGroup[] bgmGroups = _audioMixer.FindMatchingGroups("Master/Bgm");
        AudioMixerGroup[] sfxGroups = _audioMixer.FindMatchingGroups("Master/Sfx");

        if (bgmGroups.Length == 0)
        {
            CPrint.Error("[SoundManager] Bgm Mixer Group을 찾을 수 없습니다.");
        }
        else
        {
            _bgmMixerGroup = bgmGroups[0];
        }

        if (sfxGroups.Length == 0)
        {
            CPrint.Error("[SoundManager] Sfx Mixer Group을 찾을 수 없습니다.");
        }
        else
        {
            _sfxMixerGroup = sfxGroups[0];
        }
    }

    /// <summary> SoundManager에서 사용하는 AudioListener와 BGM AudioSource를 초기화한다. </summary>
    private void InitializeAudioListener()
    {
        GameObject listenerObject = new GameObject("AudioListener");
        listenerObject.transform.SetParent(transform);

        _audioListener = listenerObject.AddComponent<AudioListener>();
        _bgmAudioSource = listenerObject.AddComponent<AudioSource>();

        _bgmAudioSource.playOnAwake = false;
        _bgmAudioSource.loop = true;
        _bgmAudioSource.outputAudioMixerGroup = _bgmMixerGroup;
    }

    /// <summary> 씬에서 사용한 SFX를 정리한다. </summary>
    public void Clear()
    {
        foreach (Sfx sfx in _sfxMap.Values)
        {
            sfx.Clear();
        }

        _sfxMap.Clear();
    }

    #endregion ===== 초기화 =====

    #region ===== 이벤트 =====

    /// <summary> 새로운 씬이 로드되었을 때 SoundManager의 씬 종속 참조를 정리한다. </summary>
    private void HandleSceneLoaded()
    {
        CPrint.Log("씬 로드");
        RemoveOtherAudioListeners();
    }

    #endregion ===== 이벤트 =====

    #region ===== 리스너 =====

    /// <summary> 씬에 존재하는 다른 AudioListener를 비활성화한다. </summary>
    private void RemoveOtherAudioListeners()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();

        foreach (AudioListener listener in listeners)
        {
            if (listener != _audioListener)
            {
                listener.enabled = false;
            }
        }
    }

    /// <summary> AudioListener가 따라갈 Transform을 설정한다. </summary>
    public void SetListener(Transform target)
    {
        _followTarget = target;

        if (_followTarget != null)
        {
            _audioListener.transform.position = _followTarget.position;
        }
    }

    #endregion ===== 리스너 =====

    #region ===== SFX =====

    /// <summary> 지정한 이름의 SFX를 재생한다. </summary>
    public void PlaySfx(ResourceKey.Name.SfxType sfxType, float volume = 1f, bool is3D = false,
        Vector3 position = default)
    {
        string sfxName = sfxType.ToString();
        
        if (string.IsNullOrWhiteSpace(sfxName))
        {
            CPrint.Warning("[SoundManager] SFX 이름이 비어 있습니다.");
            return;
        }

        if (!_sfxMap.TryGetValue(sfxName, out Sfx sfx))
        {
            AudioClip clip = Managers.Resource.Load<AudioClip>(ResourceKey.Path.Sfx + sfxName);

            if (clip == null)
            {
                CPrint.Error($"[SoundManager] SFX를 찾을 수 없습니다. Name: {sfxName}");
                return;
            }

            sfx = new Sfx(clip, _minDistance, _maxDistance, _sfxMixerGroup);

            _sfxMap.Add(sfxName, sfx);
        }

        sfx.Play(volume, is3D, position);
    }

    /// <summary> 해당하는 SFX의 플레이를 정지한다. </summary>
    public void StopSfx(ResourceKey.Name.SfxType sfxType)
    {
        string sfxName = sfxType.ToString();
        
        foreach (Sfx sfx in _sfxMap.Values)
        {
            if (sfx.ClipName.Equals(sfxName))
            {
                sfx.Stop();
            }
        }
    }

    /// <summary> 현재 재생 중인 모든 SFX를 정지한다. </summary>
    public void StopAllSfx()
    {
        foreach (Sfx sfx in _sfxMap.Values)
        {
            sfx.Stop();
        }
    }

    #endregion ===== SFX =====

    #region ===== BGM =====

    /// <summary> 현재 재생 중인 BGM을 일시정지한다. </summary>
    public void PauseBgm()
    {
        if (!_bgmAudioSource.isPlaying)
        {
            return;
        }

        _bgmAudioSource.Pause();
    }

    /// <summary> 일시정지된 BGM을 이어서 재생한다. </summary>
    public void ResumeBgm()
    {
        if (_bgmAudioSource.clip == null)
        {
            return;
        }

        _bgmAudioSource.UnPause();
    }

    /// <summary> 지정한 이름의 BGM을 재생한다. </summary>
    public void PlayBgm(ResourceKey.Name.BgmType bgmType, float volume = 1f, float fadeTime = 0f)
    {
        string bgmName = bgmType.ToString();
        
        if (string.IsNullOrWhiteSpace(bgmName))
        {
            CPrint.Warning("[SoundManager] BGM 이름이 비어 있습니다.");
            return;
        }

        AudioClip clip = Managers.Resource.Load<AudioClip>(ResourceKey.Path.Bgm + bgmName);

        if (clip == null)
        {
            CPrint.Error($"[SoundManager] BGM을 찾을 수 없습니다. Name: {bgmName}");
            return;
        }

        if (_bgmAudioSource.clip == clip && _bgmAudioSource.isPlaying)
        {
            return;
        }

        StopBgm();

        _bgmAudioSource.clip = clip;
        _bgmAudioSource.volume = volume;
        _bgmAudioSource.outputAudioMixerGroup = _bgmMixerGroup;

        if (fadeTime > 0f)
        {
            _bgmAudioSource.volume = 0f;
            _bgmAudioSource.Play();

            StartBgmFade(0f, volume, fadeTime);
            return;
        }

        _bgmAudioSource.Play();
    }

    /// <summary> 현재 재생 중인 BGM을 정지한다. </summary>
    public void StopBgm(float fadeTime = 0f, Action onComplete = null)
    {
        if (!_bgmAudioSource.isPlaying)
        {
            onComplete?.Invoke();
            return;
        }

        if (fadeTime <= 0f)
        {
            StopBgmFadeCoroutine();

            _bgmAudioSource.Stop();
            _bgmAudioSource.clip = null;

            onComplete?.Invoke();
            return;
        }

        StopBgmFadeCoroutine();
        _bgmFadeCoroutine = StartCoroutine(FadeOutBgm(fadeTime, onComplete));
    }

    /// <summary> 실행 중인 BGM Fade Coroutine을 정지한다. </summary>
    private void StopBgmFadeCoroutine()
    {
        if (_bgmFadeCoroutine == null)
        {
            return;
        }

        StopCoroutine(_bgmFadeCoroutine);
        _bgmFadeCoroutine = null;
    }

    /// <summary> BGM의 볼륨을 지정한 값까지 부드럽게 변경한다. </summary>
    private void StartBgmFade(float startVolume, float targetVolume, float fadeTime)
    {
        StopBgmFadeCoroutine();
        _bgmFadeCoroutine = StartCoroutine(FadeBgm(startVolume, targetVolume, fadeTime));
    }

    private IEnumerator FadeBgm(float startVolume, float targetVolume, float fadeTime)
    {
        float elapsedTime = 0f;

        _bgmAudioSource.volume = startVolume;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;

            float ratio = Mathf.Clamp01(elapsedTime / fadeTime);
            _bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, ratio);

            yield return null;
        }

        _bgmAudioSource.volume = targetVolume;
        _bgmFadeCoroutine = null;
    }

    private IEnumerator FadeOutBgm(float fadeTime, Action onComplete)
    {
        float startVolume = _bgmAudioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;

            float ratio = Mathf.Clamp01(elapsedTime / fadeTime);
            _bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, ratio);

            yield return null;
        }

        _bgmAudioSource.volume = 0f;
        _bgmAudioSource.Stop();
        _bgmAudioSource.clip = null;

        _bgmFadeCoroutine = null;

        onComplete?.Invoke();
    }

    #endregion ===== BGM =====

    #region ===== Mixer =====

    /// <summary> BGM의 전체 볼륨을 설정한다. </summary>
    public void SetBgmVolume(float volume)
    {
        SetMixerVolume(BgmVolumeParameter, volume);
    }

    /// <summary> SFX의 전체 볼륨을 설정한다. </summary>
    public void SetSfxVolume(float volume)
    {
        SetMixerVolume(SfxVolumeParameter, volume);
    }

    /// <summary> AudioMixer의 볼륨 파라미터를 설정한다. </summary>
    private void SetMixerVolume(string parameterName, float volume)
    {
        if (_audioMixer == null)
        {
            CPrint.Warning("[SoundManager] AudioMixer가 설정되지 않았습니다.");
            return;
        }

        volume = Mathf.Clamp01(volume);

        if (volume <= 0f)
        {
            _audioMixer.SetFloat(parameterName, -80f);
            return;
        }

        float decibel = Mathf.Log10(volume) * 20f;
        _audioMixer.SetFloat(parameterName, decibel);
    }

    #endregion ===== Mixer =====
}

/// <summary> 하나의 SFX에 필요한 AudioSource를 관리하고 재사용한다. </summary>
public class Sfx
{
    private readonly Transform _root;

    private readonly List<AudioSource> _audioSourceList = new();

    private readonly AudioClip _clip;
    private readonly AudioMixerGroup _mixerGroup;

    private readonly float _minDistance;
    private readonly float _maxDistance;

    public string ClipName => _clip?.name;

    /// <summary> SFX 재생에 필요한 AudioSource를 초기화한다. </summary>
    public Sfx(AudioClip clip, float minDistance, float maxDistance, AudioMixerGroup mixerGroup)
    {
        _clip = clip;
        _minDistance = minDistance;
        _maxDistance = maxDistance;
        _mixerGroup = mixerGroup;

        GameObject rootObject = new GameObject(clip.name);
        rootObject.transform.SetParent(Managers.Sound.transform);

        _root = rootObject.transform;
    }

    /// <summary> SFX를 재생한다. </summary>
    public void Play(float volume, bool is3D, Vector3 position)
    {
        AudioSource audioSource = GetAvailableAudioSource();

        audioSource.transform.position = position;
        audioSource.volume = volume;
        audioSource.spatialBlend = is3D ? 1f : 0f;

        audioSource.Stop();
        audioSource.Play();
    }

    /// <summary> 재생 가능한 AudioSource를 찾거나 새로 생성한다. </summary>
    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource audioSource in _audioSourceList)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        return AddAudioSource();
    }

    /// <summary> SFX 재생에 사용할 AudioSource를 생성한다. </summary>
    private AudioSource AddAudioSource()
    {
        GameObject audioObject = new GameObject($"AudioSource_{_audioSourceList.Count}");
        audioObject.transform.SetParent(_root, false);

        AudioSource audioSource = audioObject.AddComponent<AudioSource>();

        audioSource.clip = _clip;
        audioSource.playOnAwake = false;

        audioSource.dopplerLevel = 1f;
        audioSource.spread = 0f;

        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = _minDistance;
        audioSource.maxDistance = _maxDistance;

        audioSource.outputAudioMixerGroup = _mixerGroup;

        _audioSourceList.Add(audioSource);

        return audioSource;
    }

    /// <summary> 현재 재생 중인 모든 AudioSource를 정지한다. </summary>
    public void Stop()
    {
        foreach (AudioSource audioSource in _audioSourceList)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    /// <summary> SFX에 생성된 AudioSource를 정리한다. </summary>
    public void Clear()
    {
        if (_root != null)
        {
            UnityEngine.Object.Destroy(_root.gameObject);
        }
    }
}