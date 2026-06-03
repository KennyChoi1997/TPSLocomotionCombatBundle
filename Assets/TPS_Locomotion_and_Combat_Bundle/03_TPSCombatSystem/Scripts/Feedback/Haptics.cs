using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSCombatSystem.Feedback
{
    /// <summary>
    /// Static haptics utility for gamepad vibration.
    /// 
    /// Responsibilities:
    /// 1) Trigger vibration pulses on the current gamepad.
    /// 2) Safely merge overlapping pulses by extending duration and preserving stronger values.
    /// 3) Stop vibration explicity when required.
    /// 
    /// Notes:
    /// - Uses <see cref="HapticsRunner"/> for coroutine execution.
    /// - Uses unscaled time so vibration duration is unaffected by time scale.
    /// </summary>
    public static class Haptics
    {
        private static Coroutine routine;
        private static float endTime;

        private static float low;
        private static float high;

        /// <summary>
        /// Triggers a vibration pulse on the current gamepad.
        /// If another pulse is already active, the stronger values are preserved
        /// and the duration is extended safely.
        /// </summary>
        public static void Pulse(float lowFrequency,float highFrequency, float duration)
        {
            var pad = Gamepad.current;
            if (pad == null)
            {
                return;
            }

            HapticsRunner runner = HapticsRunner.Instance;
            if (runner == null)
            {
                return;
            }

            low = Mathf.Max(low, lowFrequency);
            high = Mathf.Max(high, highFrequency);
            endTime = Mathf.Max(endTime, Time.unscaledTime + duration);

            if (routine != null)
            {
                runner.StopCoroutine(routine);
            }

            routine = runner.StartCoroutine(PulseRoutine(pad));
        }

        /// <summary>
        /// Immediately stops all active vibration and clears internal pulse state.
        /// </summary>
        public static void Stop()
        {
            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                pad.SetMotorSpeeds(0f, 0f);
            }

            endTime = 0f;
            low = 0f;
            high = 0f;
            routine = null;
        }

        private static IEnumerator PulseRoutine(Gamepad pad)
        {
            while (Time.unscaledTime < endTime)
            {
                pad.SetMotorSpeeds(low, high);
                yield return null;
            }

            pad.SetMotorSpeeds(0f, 0f);
            low = 0f;
            high = 0f;
            routine = null;
        }
    }
}