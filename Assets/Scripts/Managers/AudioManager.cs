using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [field: SerializeField] public AudioMixer audioMixer {get; private set;}
    [field: SerializeField] public AudioMixerGroup masterAudioMixerGroup {get; private set;}
    [field: SerializeField] public AudioMixerGroup SFXAudioMixerGroup {get; private set;}
    [field: SerializeField] public AudioMixerGroup ambienceAudioMixerGroup {get; private set;}
    [field: SerializeField] public AudioMixerGroup musicAudioMixerGroup {get; private set;}
}
