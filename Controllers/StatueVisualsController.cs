using UnityEngine;

namespace GhostInTheMachine.Controllers
{
    public class StatueVisualsController : MonoBehaviour
    {
        public const float EYE_OPEN_ANGLE = 33f;
        public const float EYE_CLOSE_ANGLE = 0f;
        public const float EYE_ANIMATION_DURATION = 1f;
        public const float EYE_GLOW_FADE_DURATION = 1f;
        public const float STATUE_TURN_DURATION = 4f;

        public TransformAnimator[] lowerLidAnimators;
        public TransformAnimator[] upperLidAnimators;
        public OWRenderer eyeRenderer;
        public TransformAnimator turnTransformAnimator;
        public OWAudioSource turnAudioSource;

        bool eyesOpen = true;
        bool eyesGlowing = false;
        float eyeGlowAmount = 1f;
        bool isTurning = false;
        Timer turnTimer;

        protected void Awake()
        {
            enabled = false;
        }

        protected void Update()
        {
            if (!eyesGlowing && eyeGlowAmount > 0f)
            {
                eyeGlowAmount = Mathf.MoveTowards(eyeGlowAmount, 0f, Time.deltaTime / EYE_GLOW_FADE_DURATION);
                SetEyeGlow(eyeGlowAmount);
            }
            if (isTurning)
            {
                turnTimer.Update();
                if (turnTimer.IsFinished())
                {
                    StopTurning();
                }
            }
            if (eyeGlowAmount <= 0f && !isTurning)
            {
                enabled = false;
            }
        }

        public void SetEyesOpen(bool open)
        {
            if (open && !eyesOpen)
            {
                OpenEyes();
            }
            else if (!open && eyesOpen)
            {
                CloseEyes();
            }
        }

        public void SetEyesGlowing(bool glowing)
        {
            if (glowing)
            {
                eyesGlowing = true;
                SetEyeGlow(1f);
            }
            else if (!glowing)
            {
                eyesGlowing = false;
                enabled = true;
            }
        }

        public void OpenEyes()
        {
            eyesOpen = true;
            foreach (var anim in lowerLidAnimators)
            {
                anim.RotateToOriginalLocalRotation(EYE_ANIMATION_DURATION);
            }
            foreach (var anim in upperLidAnimators)
            {
                anim.RotateToOriginalLocalRotation(EYE_ANIMATION_DURATION);
            }
        }

        public void CloseEyes()
        {
            eyesOpen = false;
            foreach (var anim in lowerLidAnimators)
            {
                anim.transform.Rotate(Vector3.up, -EYE_OPEN_ANGLE);
            }
            foreach (var anim in upperLidAnimators)
            {
                anim.transform.Rotate(Vector3.up, EYE_OPEN_ANGLE);
            }
        }

        public void StartTurning(Vector3 targetWorldPos)
        {
            isTurning = true;
            turnTransformAnimator.TurnTowardPosition(targetWorldPos, STATUE_TURN_DURATION, true);
            turnTimer = new Timer(STATUE_TURN_DURATION);
            turnAudioSource.SetLocalVolume(0f);
            turnAudioSource.FadeIn(1f);
        }

        public void StopTurning()
        {
            isTurning = false;
            turnAudioSource.Stop();
            turnAudioSource.PlayOneShot(AudioType.NomaiDoorStopBig);
        }

        void SetEyeGlow(float amount)
        {
            eyeGlowAmount = amount;
            eyeRenderer.SetEmissionColor(Color.Lerp(Color.black, eyeRenderer.GetOriginalEmissionColor(), amount));
        }
    }
}
