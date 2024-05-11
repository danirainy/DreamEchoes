﻿using UnityEngine;

namespace DreamEchoesCore.Entities.Enemies.Seer.SeerStateMachine;

internal class SeerAnimator : RingLib.Animator
{
    public AudioClip DashSound;
    public AudioClip Slash1Sound;
    public AudioClip Slash2Sound;
    public void PlayDashSound()
    {
        PlaySound(DashSound);
    }
    public void PlaySlash1Sound()
    {
        PlaySound(Slash1Sound);
    }
    public void PlaySlash2Sound()
    {
        PlaySound(Slash2Sound);
    }
}
