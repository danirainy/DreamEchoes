﻿using RingLib.StateMachine;
using RingLib.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamEchoesCore.Entities.Enemies.Seer.SeerStateMachine;

internal partial class SeerStateMachine : EntityStateMachine
{
    IEnumerator<Transition> bodyFollow()
    {
        while (true)
        {
            if (!FacingTarget())
            {
                yield return new CoroutineTransition { Routine = Turn() };
            }
            var jumpRadius = Config.LaserBodyDistance;
            var targetXLeft = Target().Position().x - jumpRadius;
            var targetXRight = Target().Position().x + jumpRadius;
            float targetX;
            if (targetXLeft < minX || targetXLeft > maxX)
            {
                targetX = targetXRight;
            }
            else if (targetXRight < minX || targetXRight > maxX)
            {
                targetX = targetXLeft;
            }
            else if (Mathf.Abs(Position.x - targetXLeft) > Mathf.Abs(Position.x - targetXRight))
            {
                targetX = targetXRight;
            }
            else
            {
                targetX = targetXLeft;
            }
            Velocity = new Vector2((targetX - Position.x) * Config.LaserBodyVelocityXScale, 0);
            yield return new NoTransition();
        }
    }

    IEnumerator<Transition> handFollow(GameObject hand, Func<Vector3> targetPos)
    {
        while (true)
        {
            var handPosition = hand.transform.position;
            var heroPosition = targetPos();
            if (gameObject.transform.localScale.x < 0)
            {
                heroPosition.x = 2 * gameObject.transform.position.x - heroPosition.x;
            }
            var direction = (heroPosition - handPosition).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float normalizeAngle(float angle)
            {
                while (angle < -180)
                {
                    angle += 360;
                }
                while (angle > 180)
                {
                    angle -= 360;
                }
                return angle;
            }
            angle = normalizeAngle(angle - 180 - 8);
            var currentAngle = normalizeAngle(hand.transform.localRotation.eulerAngles.z);
            var deltaAngle = angle - currentAngle;
            var newAngle = currentAngle + deltaAngle * Config.LaserHandFollowSpeed * Time.deltaTime;
            hand.transform.localRotation = Quaternion.Euler(0, 0, newAngle);
            yield return new NoTransition();
        }
    }

    IEnumerator<Transition> rayCast(GameObject hand, GameObject hit, Func<Vector3> targetPos)
    {
        while (true)
        {
            var handX = hand.transform.position.x;
            var handY = hand.transform.position.y;
            var heroPosition = targetPos();
            var heroX = heroPosition.x;
            var heroY = heroPosition.y;
            var ray = new Vector2(heroX - handX, heroY - handY);
            RaycastHit2D raycastHit2D4 = Physics2D.Raycast(new Vector2(handX, handY), ray, float.MaxValue, 1 << 8);
            float hitY = -1000;
            float hitX = -1000;
            if (raycastHit2D4.collider != null)
            {
                RingLib.Log.LogInfo("", $"Start: {handX}, {handY}");
                RingLib.Log.LogInfo("", $"Mid: {heroX}, {heroY}");
                RingLib.Log.LogInfo("", $"End: {raycastHit2D4.point.x}, {raycastHit2D4.point.y}");
                hitY = raycastHit2D4.point.y;
                hitX = raycastHit2D4.point.x;
            }
            // animator.UpdateLaserCutoff(hitY + 0.5f);
            var globalPos = hit.transform.position;
            globalPos.y = hitY + Config.HitDY;
            globalPos.x = hitX + Config.HitDX;
            hit.transform.position = globalPos;
            hit.SetActive(true);
            yield return new NoTransition();
        }
    }

    [State]
    private IEnumerator<Transition> Laser()
    {
        // JumpStart
        var jumpRadius = Config.LaserBodyDistance;
        var targetXLeft = Target().Position().x - jumpRadius;
        var targetXRight = Target().Position().x + jumpRadius;
        float targetX;
        if (Mathf.Abs(Position.x - targetXLeft) > Mathf.Abs(Position.x - targetXRight))
        {
            targetX = targetXRight;
        }
        else
        {
            targetX = targetXLeft;
        }
        var velocityX = (targetX - Position.x) * Config.EvadeJumpVelocityXScale;
        var velocityY = Config.EvadeJumpVelocityY;
        if (!FacingTarget())
        {
            yield return new CoroutineTransition
            {
                Routine = Turn()
            };
        }
        yield return new CoroutineTransition { Routine = animator.PlayAnimation("JumpStart") };

        // JumpAscend
        Velocity = new Vector2(velocityX, velocityY);
        animator.PlayAnimation("JumpAscend");
        yield return new WaitTill { Condition = () => Velocity.y <= 0 };


        // LaserStart
        speak.PlayOneShot(animator.LaserWord);
        Rigidbody2D.gravityScale = 0;
        Velocity = Vector2.zero;
        yield return new CoroutineTransition { Routine = animator.PlayAnimation("LaserBegin") };

        // LaserFly
        var hand = animator.gameObject.transform.Find("LaserHand").gameObject;
        var aim = hand.transform.Find("LaserAim").gameObject;
        var att = hand.transform.Find("LaserAtt").gameObject;
        var hit = animator.LaserHit;
        aim.SetActive(false);
        att.SetActive(false);
        hit.SetActive(false);
        animator.PlayAnimation("LaserFly");

        // InitialWait
        RingLib.Log.LogInfo("", "Laser Initial Wait");
        yield return new CoroutineTransition
        {
            Routines = [
                bodyFollow(),
                handFollow(hand, () => Target().Position()),
                new WaitFor { Seconds = Config.LaserAttackInitialWait }
            ]
        };

        // LaserAttack
        RandomSelector<int> dirSel = new([
            new(value: -1, weight: 1, maxCount: 2, maxMiss: 5),
            new(value: 1, weight: 1, maxCount: 2, maxMiss: 5),
        ]);
        for (int i = 0; i < Config.LaserAttackCount; i++)
        {
            RingLib.Log.LogInfo("", "Laser Attack Wait");
            yield return new CoroutineTransition
            {
                Routines = [
                    bodyFollow(),
                    handFollow(hand, () => Target().Position()),
                    new WaitFor { Seconds = Config.LaserAttackIdleWait }
                ]
            };
            var direction = dirSel.Get();
            Vector3 positionWithOffset()
            {
                var heroPosition = Target().Position();
                heroPosition.x += direction * Config.LaserAttackAimDeltaX;
                return heroPosition;
            }
            aim.SetActive(true);
            RingLib.Log.LogInfo("", "Laser Attack Aim");
            yield return new CoroutineTransition
            {
                Routines = [
                    bodyFollow(),
                    handFollow(hand, positionWithOffset),
                    new WaitFor { Seconds = Config.LaserAttackAimWait }
               ]
            };
            aim.SetActive(false);
            att.SetActive(true);
            var lockedPosition = positionWithOffset();
            animator.PlayLaserFire();
            Velocity = Vector2.zero;
            Vector3 movingPos()
            {
                var tmp = lockedPosition;
                lockedPosition.x -= direction * Config.LaserAttackMoveSpeed * Time.deltaTime;
                return tmp;
            }
            RingLib.Log.LogInfo("", "Laser Attack Laser");
            yield return new CoroutineTransition
            {
                Routines = [
                    handFollow(hand, movingPos),
                    // rayCast(hand, hit, movingPos),
                    new WaitFor { Seconds = Config.LaserAttackLaserWait }
                ]
            };
            att.SetActive(false);
            hit.SetActive(false);
        }

        // LaserEnd1
        Rigidbody2D.gravityScale = Config.GravityScale;
        Velocity = new Vector2(0, Config.LaserBodyEndVelY);
        animator.PlayAnimation("LaserEnd1");
        yield return new WaitTill { Condition = Landed };
        animator.PlayLandSound();

        // LaserEnd2
        yield return new CoroutineTransition { Routine = animator.PlayAnimation("LaserEnd2") };

        yield return new ToState { State = nameof(Idle) };
    }
}
