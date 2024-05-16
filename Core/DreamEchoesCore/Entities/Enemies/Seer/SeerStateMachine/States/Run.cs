﻿using RingLib.StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace DreamEchoesCore.Entities.Enemies.Seer.SeerStateMachine;

internal partial class SeerStateMachine : EntityStateMachine
{
    [State]
    private IEnumerator<Transition> Run()
    {
        if (!FacingTarget())
        {
            yield return new CoroutineTransition { Routine = Turn() };
        }

        var direction = Direction();
        var velocityX = config.RunVelocityX * direction;

        Transition startUpdater(float normalizedTime)
        {
            var currentVelocityX = Mathf.Lerp(0, velocityX, normalizedTime);
            Velocity = new Vector2(currentVelocityX, 0);
            return new NoTransition();
        }
        yield return new CoroutineTransition
        {
            Routine = animator.PlayAnimation("RunStart", startUpdater)
        };

        Velocity = new Vector2(velocityX, 0);
        animator.PlayAnimation("Run");
        yield return new WaitFor { Seconds = config.RunDuration };

        Transition endUpdater(float normalizedTime)
        {
            var currentVelocityX = Mathf.Lerp(velocityX, 0, normalizedTime);
            Velocity = new Vector2(currentVelocityX, 0);
            return new NoTransition();
        }
        yield return new CoroutineTransition
        {
            Routine = animator.PlayAnimation("RunEnd", endUpdater)
        };

        yield return new ToState { State = nameof(Attack) };
    }
}
