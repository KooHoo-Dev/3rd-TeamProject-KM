using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Study.Utilities;
using UnityEngine.InputSystem;

namespace BlueMarble
{
    public class GoldCard : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer iconRenderer;
        
        [Header("Flip VFX")]
        [SerializeField] private ParticleSystem preFlipVfx;         // 뒤집기 시작 시 재생 
        [SerializeField] private ParticleSystem durationFlipVfx;    // 뒤집을 때 재생 
        [SerializeField] private ParticleSystem resultFlipVfx;      // 뒤집기 완료 시 재생 

        [Header("Flip Settings")]
        [SerializeField] private float flipDuration = 0.25f;

        private bool flipped = false;
        private Tween flipTween;

        public GoldCardDef Def { get; private set; }

        private void OnDisable()
        {
            Clear(); // 오브젝트 비활성화 시 트윈/파티클 정리 (씬 전환, 팝업 닫힘 등)
        }

        public void Initialize(GoldCardDef def)
        {
            Clear();

            Def = def;
            // iconImage.sprite = def.Icon;
        }
        
        /// <summary>
        /// 팝업이 닫히거나 카드가 재사용될 때 외부(팝업 컨트롤러)에서 호출.
        /// 파티클 잔상 없이 완전히 초기화한다.
        /// </summary>
        public void ResetVisual()
        {
            PlayVfx(preFlipVfx);
            Clear();
        }

        public UniTask FlipAsync()
        {
            StopVfx(preFlipVfx);
            PlayVfx(durationFlipVfx);
            PlayVfx(resultFlipVfx);
            
            flipped = !flipped;
            flipTween = transform.DORotate(new Vector3(0, flipped ? 180f : 0f, 0), flipDuration);
            
            return flipTween.ToUniTask();
        }

        private void PlayVfx(ParticleSystem vfx)
        {
            if (vfx == null) return;
            vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            vfx.Play(true);
        }

        private void StopVfx(ParticleSystem vfx)
        {
            if (vfx == null) return;
            vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Clear()
        {
            flipTween?.Kill();
            flipped = false;
            transform.rotation = Quaternion.identity;
            
            StopVfx(preFlipVfx);
            StopVfx(resultFlipVfx);
            StopVfx(durationFlipVfx);
        }
    }
}