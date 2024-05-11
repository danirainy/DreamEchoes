﻿
using RingLib.StateMachine;
using UnityEngine;

namespace DreamEchoesCore.Entities.Enemies.Seer.SeerStateMachine.States;

internal class Idle : State<SeerStateMachine>
{
    public override Transition Enter()
    {
        StartCoroutine(Routine());
        return new CurrentState();
    }
    private IEnumerator<Transition> Routine()
    {
        if (!StateMachine.FacingTarget())
        {
            StateMachine.Turn();
        }
        StateMachine.Velocity = Vector2.zero;
        StateMachine.Animator.PlayAnimation("Idle");
        yield return new WaitFor { Time = StateMachine.Config.IdleDuration };
        yield return new ToState { State = typeof(Run) };
    }
}