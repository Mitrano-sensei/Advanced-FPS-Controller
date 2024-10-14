using FiniteStateMachine;
using UnityEngine;

namespace FPSController
{
    public interface IFPSState
    {
        float GetMovementSpeedRatio();

        bool IsLimitedSpeed() => true; // Returns false if the state shouldn't be limited in speed
    }
}
