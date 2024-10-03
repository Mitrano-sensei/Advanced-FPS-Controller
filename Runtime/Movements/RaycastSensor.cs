using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSController
{
    public class RaycastSensor
    {
        public float castLength = 1f;
        public LayerMask layerMask = 255;

        Vector3 origin = Vector3.zero;
        Transform transform;

        public enum CastDirection {             
            Forward,
            Backward,
            Right,
            Left,
            Up,
            Down
        }
        CastDirection castDirection;

        RaycastHit hitInfo;

        public RaycastSensor(Transform player)
        {
            transform = player;
        }

        public void Cast()
        {
            Vector3 worldOrigin = transform.TransformPoint(origin);
            Vector3 worldDirection = GetCastDirection();

            Physics.Raycast(worldOrigin, worldDirection, out hitInfo, castLength, layerMask, QueryTriggerInteraction.Ignore);
        }

        public void SetCastDirection(CastDirection direction) => castDirection = direction;
        public void SetCastOrigin(Vector3 pos) => origin = transform.InverseTransformPoint(pos);

        #region HitInfo
        /**
         * Note that a raycasthit is a value object so it can NOT be null, but its value can be null.
         */
        public bool HasDetectedHit() => hitInfo.collider != null;
        public float GetDistance() => hitInfo.distance;
        public Vector3 GetNormal() => hitInfo.normal;
        public Vector3 GetPosition() => hitInfo.point;
        public Collider GetCollider() => hitInfo.collider;
        public Transform GetTransform() => hitInfo.transform;
        #endregion

        private Vector3 GetCastDirection()
        {
            return castDirection switch
            {
                CastDirection.Forward => transform.forward,
                CastDirection.Backward => -transform.forward,
                CastDirection.Right => transform.right,
                CastDirection.Left => -transform.right,
                CastDirection.Up => transform.up,
                CastDirection.Down => -transform.up,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
