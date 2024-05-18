﻿using System.Collections.Generic;
using UnityEngine;

namespace DreamEchoesCore.Entities.Enemies.Seer.SeerStateMachine;

internal class SeerAnimator : MonoBehaviour
{
    public GameObject JumpEffectPrefab;

    public AudioClip DashSound;

    public AudioClip Slash1Sound;
    public AudioClip Slash2Sound;

    public List<AudioClip> HugRadiantNailSounds;

    public void SpawnJumpEffect() { }

    public void PlayDashSound() { }

    public void PlaySlash1Sound() { }

    public void PlaySlash2Sound() { }

    public void SpawnRadiantNails() { }
}
