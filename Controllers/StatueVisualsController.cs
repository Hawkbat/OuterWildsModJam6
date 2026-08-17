using UnityEngine;

namespace GhostInTheMachine.Controllers
{
    public class StatueVisualsController : MonoBehaviour
    {
        const float EYE_OPEN_ANGLE = 33f;
        const float EYE_ANIMATION_DURATION = 1f;
        const float EYE_GLOW_FADE_DURATION = 1f;
        const float STATUE_TURN_DURATION = 4f;
        static readonly Color DEFAULT_EYE_GLOW_COLOR = new(0.529f, 0.576f, 1.5f, 1f);

        public TransformAnimator[] lowerLidAnimators;
        public TransformAnimator[] upperLidAnimators;
        public OWRenderer eyeRenderer;
        public TransformAnimator turnTransformAnimator;
        public OWAudioSource turnAudioSource;

        bool eyesOpen = true;
        bool eyesGlowing = false;
        float eyeGlowAmount = 1f;
        Color eyeGlowColor;
        bool isTurning = false;
        Timer turnTimer;

        protected void Awake()
        {
            eyeGlowColor = DEFAULT_EYE_GLOW_COLOR;
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
            if (!isTurning && (eyesGlowing || eyeGlowAmount <= 0f))
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

        public void SetEyesGlowing(bool glowing, bool immediate = false)
        {
            if (glowing)
            {
                eyesGlowing = true;
                SetEyeGlow(1f);
            }
            else
            {
                eyesGlowing = false;
                if (immediate)
                {
                    SetEyeGlow(0f);
                }
                else
                {
                    enabled = true;
                }
            }
        }

        public void SetEyeGlowColor(Color color)
        {
            eyeGlowColor = color;
            SetEyeGlow(eyeGlowAmount);
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
            enabled = true;
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
            eyeRenderer.SetEmissionColor(Color.Lerp(Color.black, eyeGlowColor, amount));
        }
    }
}
