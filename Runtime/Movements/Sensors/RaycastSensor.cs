using UnityEngine;

namespace FPSController
{
    public class RaycastSensor
    {

        private float _castLength = 1f;
        private LayerMask _layermask = 255;

        Vector3 origin = Vector3.zero;
        Transform tr;

        public enum CastDirection { Forward, Right, Up, Backward, Left, Down }
        CastDirection castDirection;

        RaycastHit hitInfo;

        public RaycastSensor(Transform playerTransform)
        {
            tr = playerTransform;
        }

        public void Cast()
        {
            Vector3 worldOrigin = tr.TransformPoint(origin);
            Vector3 worldDirection = GetCastDirection();

            Physics.Raycast(worldOrigin, worldDirection, out hitInfo, _castLength, _layermask, QueryTriggerInteraction.Ignore);
        }

        public bool HasDetectedHit() => hitInfo.collider != null;
        public float GetDistance() => hitInfo.distance;
        public Vector3 GetNormal() => hitInfo.normal;
        public Vector3 GetPosition() => hitInfo.point;
        public Collider GetCollider() => hitInfo.collider;
        public Transform GetTransform() => hitInfo.transform;

        public void SetCastDirection(CastDirection direction) => castDirection = direction;
        public void SetCastOrigin(Vector3 pos) => origin = tr.InverseTransformPoint(pos);
        public void SetCastLength(float length) => _castLength = length;

        public void SetLayerMask(LayerMask layermask) => _layermask = layermask;
        public void AddLayer(int layer) => _layermask |= (1 << layer);
        public void RemoveLayer(int layer) => _layermask &= ~(1 << layer);

        public float GetCastLength() => _castLength;
        public Vector3 GetRawCastOriginOffset() => origin;
        public LayerMask GetLayerMasks() => _layermask;

        Vector3 GetCastDirection()
        {
            return castDirection switch
            {
                CastDirection.Forward => tr.forward,
                CastDirection.Right => tr.right,
                CastDirection.Up => tr.up,
                CastDirection.Backward => -tr.forward,
                CastDirection.Left => -tr.right,
                CastDirection.Down => -tr.up,
                _ => Vector3.one
            };
        }

        public void DrawDebug()
        {
            Vector3 worldOrigin = tr.TransformPoint(origin);

            Gizmos.color = HasDetectedHit() ? Color.red : Color.green;
            Gizmos.DrawRay(worldOrigin + Vector3.left * .5f, GetCastDirection() * _castLength);

            //if (!HasDetectedHit()) return;
        }
    }
}
