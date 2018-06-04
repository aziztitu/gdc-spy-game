﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Azee.Interfaces;
using UnityEngine;

namespace GuardStates
{
    public class Chase : StateMachine<Guard>.State
    {
        public class StateData
        {
            public Transform TargetTransform;
            public Vector3 LastKnownPosition;
        }


        const float LostDistance = 3f;

        private static Chase _instance;

        public static Chase Instance
        {
            get { return _instance ?? (_instance = new Chase()); }
        }

        private Chase()
        {
        }

        public void Enter(Guard owner, params object[] args)
        {
            Transform transform = (Transform) args[0];

            StateData stateData = owner.ChaseStateData;
            stateData.TargetTransform = transform;
            stateData.LastKnownPosition = transform.position;

            owner.GetColorChanger().turnRed();
            owner.GetColorChanger().spotlightRed();
        }

        public void Update(Guard owner)
        {
            StateData stateData = owner.ChaseStateData;

            bool targetOnSight = false, targetAudible = false;

            if (owner.IsObjectInSight(stateData.TargetTransform.gameObject))
            {
                targetOnSight = true;
                stateData.LastKnownPosition = stateData.TargetTransform.position;
            }
            else
            {
                AudibleObject targetAudibleObject = stateData.TargetTransform.GetComponent<AudibleObject>();
                if (targetAudibleObject != null)
                {
                    Vector3? targetLocation = owner.LocateObjectFromNoise(targetAudibleObject);

                    if (targetLocation != null)
                    {
                        targetAudible = true;
                        stateData.LastKnownPosition = targetLocation.Value;
                    }
                }
            }

            if (Vector3.Distance(owner.transform.position, stateData.LastKnownPosition) > LostDistance)
            {
                owner.MoveTowards(stateData.LastKnownPosition);
            }
            else
            {
                if (!targetOnSight && !targetAudible)
                {
                    // Lost target
                    StateMachine<Guard> stateMachine = owner.GetStateMachine();
                    stateMachine.SwitchState(stateMachine.GetPreviousState() ?? GuardStates.Idle.Instance);
                }
                else
                {
                    // Too Close to target
                    owner.StopMoving();

                    Vector3 lookAtPos = stateData.TargetTransform.position;
                    lookAtPos.y = owner.transform.position.y;

                    owner.transform.LookAt(lookAtPos);

                    //TODO: Switch state or do something
                }
            }
        }

        public void Exit(Guard owner)
        {
            StateData stateData = owner.ChaseStateData;
            stateData.TargetTransform = null;
            stateData.LastKnownPosition = Vector3.zero;

            owner.GetColorChanger().turnDefault();
            owner.GetColorChanger().spotlightDefault();
        }
    }
}
