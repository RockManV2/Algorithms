using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    private static AudioSource _musicSource;
    private static AudioSource _sfx;
    private static readonly Dictionary<string, AudioClip> _audios = new();

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this);
        else
            _instance = this;

        _musicSource = _instance.gameObject.AddComponent<AudioSource>();
        _sfx = _instance.gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        foreach (var sound in Resources.LoadAll<AudioClip>("Sounds"))
            _audios[sound.name] = sound;
    }

    public static void PlaySound(string soundName) =>
        _sfx.PlayOneShot(_audios[soundName]);

    public void PlayMusic(string soundName) => _musicSource.PlayOneShot(_audios[soundName]);
}