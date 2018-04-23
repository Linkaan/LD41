using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioSource player;

    public AudioClip attackSFX;
    public AudioClip gotoSFX;

    public AudioClip towerUpSFX;
    public AudioClip towerDownSFX;
    public AudioClip halfwaySFX;

    public AudioClip selectSFX;
    public AudioClip hoverSFX;

    public AudioClip loseSFX;

    public void PlaySound(AudioClip clip) {
        player.clip = clip;
        player.Play();
    }
}
