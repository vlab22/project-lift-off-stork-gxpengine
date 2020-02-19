﻿using System;
using System.Collections;
using GXPEngine.GameLocalEvents;

namespace GXPEngine
{
    public class ParticleManager : GameObject
    {
        public static ParticleManager Instance;

        private AnimationSprite _smoke00;
        private AnimationSprite _smallBlackSmoke00;

        public ParticleManager()
        {
            Instance = this;

            _smoke00 = new AnimationSprite("data/Smoke Particle 00.png", 4, 2, 7, true, false);
            _smoke00.SetOrigin(_smoke00.width / 2f, _smoke00.height / 2f);

            _smallBlackSmoke00 = new AnimationSprite("data/Small Black Smokes00.png", 5, 5, 10, true, false);
            _smallBlackSmoke00.SetOrigin(_smallBlackSmoke00.width / 2f, _smallBlackSmoke00.height / 2f);

            LocalEvents.Instance.AddListener<StorkLocalEvent>(StorkLocalEventHandler);
        }

        private void StorkLocalEventHandler(StorkLocalEvent e)
        {
            switch (e.evt)
            {
                case StorkLocalEvent.Event.STORK_HIT_BY_PLANE:
                    //Show smoke
                    CoroutineManager.StartCoroutine(ShowSmokeOnStork00Routine(e.stork), this);

                    break;
                case StorkLocalEvent.Event.STORK_AFTER_HIT_BY_PLANE:
                    break;
                case StorkLocalEvent.Event.STORK_LOSE_PIZZA:
                    break;
                case StorkLocalEvent.Event.STORK_AFTER_HIT_BY_DRONE:
                    break;
                case StorkLocalEvent.Event.STORK_HIT_BY_DRONE:
                    //Show smoke
                    CoroutineManager.StartCoroutine(ShowSmokeOnStork00Routine(e.stork), this);
                    break;
                case StorkLocalEvent.Event.STORK_HIT_BY_HUNTER:
                    //Show smoke
                    CoroutineManager.StartCoroutine(ShowSmokeOnStork00Routine(e.stork), this);
                    break;
                default:
                    break;
            }
        }

        private IEnumerator ShowSmokeOnStork00Routine(Stork stork)
        {
            int time = 0;
            int duration = 1500;

            yield return null;

            _smoke00.visible = true;
            stork.AddChild(_smoke00);
            _smoke00.SetXY(0, 0);
            _smoke00.alpha = 1f;

            SpriteTweener.TweenAlpha(_smoke00, 1, 0, duration - 500, 500);

            while (time < duration)
            {
                float fFrame = Mathf.Map(time, 0, duration, 0, _smoke00.frameCount - 1);
                int frame = Mathf.Round(fFrame) % _smoke00.frameCount;

                _smoke00.SetFrame(frame);

                time += Time.deltaTime;

                yield return null;
            }

            yield return new WaitForMilliSeconds(200);

            stork.RemoveChild(_smoke00);
            _smoke00.visible = false;
        }

        public void PlaySmallSmoke(GameObject parentObj, float px = 0, float py = 0, int duration = 500,
            int depthIndex = -1)
        {
            CoroutineManager.StartCoroutine(PlaySmallSmokeRoutine(parentObj, px, py, duration, depthIndex), this);
        }

        private IEnumerator PlaySmallSmokeRoutine(GameObject parentObj, float px, float py, int duration,
            int depthIndex)
        {
            if (depthIndex < 0)
            {
                parentObj.AddChild(_smallBlackSmoke00);
            }
            else
            {
                parentObj.AddChildAt(_smallBlackSmoke00, depthIndex);
            }

            _smallBlackSmoke00.visible = true;
            _smallBlackSmoke00.SetXY(px, py);

            _smallBlackSmoke00.alpha = 1;

            float time = 0;
            while (time < duration)
            {
                float fFrame = Mathf.Map(time, 0, duration, 0, _smallBlackSmoke00.frameCount - 1);
                int frame = Mathf.Round(fFrame) % _smallBlackSmoke00.frameCount;

                _smallBlackSmoke00.alpha = 1 - Easing.Ease(Easing.Equation.CubicEaseIn, time, 0, 1, duration);

                _smallBlackSmoke00.SetFrame(frame);

                time += Time.deltaTime;
                yield return null;
            }

            _smallBlackSmoke00.visible = false;
            parentObj?.RemoveChild(_smallBlackSmoke00);
        }

        public void Reset()
        {
            _smoke00.parent?.RemoveChild(_smoke00);
            _smallBlackSmoke00.parent?.RemoveChild(_smallBlackSmoke00);
        }
    }
}