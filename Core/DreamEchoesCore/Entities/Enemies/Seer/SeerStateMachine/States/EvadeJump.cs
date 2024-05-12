﻿using RingLib.StateMachine;
using UnityEngine;

namespace DreamEchoesCore.Entities.Enemies.Seer.SeerStateMachine.States;

internal class EvadeJump : State<SeerStateMachine>
{
    public override Transition Enter()
    {
        StartCoroutine(Routine());
        return new CurrentState();
    }

    private IEnumerator<Transition> Routine()
    {
        // JumpStart
        var jumpRadiusMin = StateMachine.Config.EvadeJumpRadiusMin;
        var jumpRadiusMax = StateMachine.Config.EvadeJumpRadiusMax;
        var jumpRadius = UnityEngine.Random.Range(jumpRadiusMin, jumpRadiusMax);
        var targetXLeft = StateMachine.Target().Position().x - jumpRadius;
        var targetXRight = StateMachine.Target().Position().x + jumpRadius;
        float targetX;
        if (Mathf.Abs(StateMachine.Position.x - targetXLeft) < Mathf.Abs(StateMachine.Position.x - targetXRight))
        {
            targetX = targetXRight;
        }
        else
        {
            targetX = targetXLeft;
        }
        var velocityX = (targetX - StateMachine.Position.x) * StateMachine.Config.EvadeJumpVelocityXScale;
        if (Mathf.Sign(velocityX) != StateMachine.Direction())
        {
            StateMachine.Turn();
        }
        StateMachine.Animator.PlayAnimation("JumpStart");
        yield return new WaitTill { Condition = () => StateMachine.Animator.Finished };

        // JumpAscend
        StateMachine.Velocity = new Vector2(velocityX, StateMachine.Config.EvadeJumpVelocityY);
        StateMachine.Animator.PlayAnimation("JumpAscend");
        yield return new WaitTill { Condition = () => StateMachine.Velocity.y <= 0 };

        // JumpDescend
        StateMachine.Animator.PlayAnimation("JumpDescend");
        yield return new WaitTill { Condition = () => StateMachine.Landed() };

        // JumpEnd
        StateMachine.Velocity = Vector2.zero;
        StateMachine.Animator.PlayAnimation("JumpEnd");
        yield return new WaitTill { Condition = () => StateMachine.Animator.Finished };
        yield return new ToState { State = typeof(Attack) };
    }
}
