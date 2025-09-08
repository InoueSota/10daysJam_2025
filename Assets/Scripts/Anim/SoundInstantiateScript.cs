using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundInstantiateScript : MonoBehaviour
{

    [SerializeField] private AudioClip[] sounds;

    public void PlaySound(int soundNumber, float volume,float pitch)
    {
        GameObject sound = new GameObject("SoundPrefab");
        AudioSource audioSource = sound.AddComponent<AudioSource>();
        sound.AddComponent<DestroyScript>();
        audioSource.pitch = pitch;
        audioSource.volume = volume; 
        audioSource.clip = sounds[soundNumber];
        audioSource.Play();
    }
}
