using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Varwin.Public
{
    [RequireComponent(typeof(VarwinObjectDescriptor))]
    [DisallowMultipleComponent]
    [VarwinComponent(English:"AnimationPlayer",Russian:"Анимации",Chinese:"動畫播放器",Kazakh:"Анимациялар",Korean:"애니메이션 플레이어")]
    public class VarwinAnimationPlayer : MonoBehaviour, ISwitchModeSubscriber
    {
        public List<VarwinCustomAnimation> CustomAnimations;
        public Animator Animator;
        
        private int _animationClipId;
        
        private bool _repeatAnimation;
        
        private RuntimeAnimatorController _animatorController;

        private PlayableGraph _playableGraph;
        private AnimationPlayableOutput _animationPlayableOutput;
        private List<AnimationClipPlayable> _clipPlayables;

        [Variable(English:"stop animation at the last frame",Russian:"остановка анимации на последнем кадре",Chinese:"在最後一幀停止動畫",Kazakh:"анимацияны соңғы кадрда тоқтату",Korean:"마지막 프레임에서 애니메이션 중지")]
        public bool IsAnimationStaysOnLastFrame { get; set; }
        
        [ActionGroup("PlayAnimation")]
        [Action(English:"Play custom animation once",Russian:"Запустить анимацию единожды",Chinese:"播放自定義動畫一次",Kazakh:"Анимацияны бір рет іске қосу",Korean:"의 다음 지정 애니메이션 한 번 재생")]
        public void PlayCustomAnimationOnce([UseValueList("VarwinCustomAnimationClips")] int clipId)
        {
            _repeatAnimation = false;
            
            PlayClipWithId(clipId);
        } 
        
        [ActionGroup("PlayAnimation")]
        [Action(English:"Play custom animation repeatedly",Russian:"Запустить анимацию с повторением",Chinese:"重複播放自定義動畫",Kazakh:"Анимацияны қайталаумен іске қосу",Korean:"의 다음 지정 애니메이션 반복 재생")]
        public void PlayCustomAnimationRepeatedly([UseValueList("VarwinCustomAnimationClips")] int clipId)
        {
            _repeatAnimation = true;
            
            PlayClipWithId(clipId);
        }
        
        [Action(English:"Stop custom animation",Russian:"Остановить анимацию",Chinese:"停止自定義動畫",Kazakh:"Анимацияны тоқтату",Korean:"의 지정 애니메이션 중지")]
        public void StopCustomAnimation()
        {
            _playableGraph.Stop();
        }

        private void Awake()
        {
            if (!Animator)
            {
                Animator = GetComponentInChildren<Animator>();
            }

            Animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimatorControllers/VarwinBotController"); 
            Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            
            _playableGraph = PlayableGraph.Create();
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            _animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", Animator);

            _clipPlayables = new List<AnimationClipPlayable>();
            foreach (var customAnimation in CustomAnimations)
            {
                var clipPlayable = AnimationClipPlayable.Create(_playableGraph, customAnimation.Clip);
                _clipPlayables.Add(clipPlayable);
            }
        }

        private void Update()
        {
            PlayClips();
        }
        
        public void OnSwitchMode(GameMode newMode, GameMode oldMode)
        {
            Animator.applyRootMotion = newMode != GameMode.Edit;
        }

        private void PlayClips()
        {
            if (_playableGraph.IsPlaying())
            {
                float timeDiff = Mathf.Abs((float) _clipPlayables[_animationClipId].GetTime() - CustomAnimations[_animationClipId].Clip.length);

                if (timeDiff < Time.deltaTime)
                {
                    if (_repeatAnimation)
                    {
                        _clipPlayables[_animationClipId].SetTime(0);
                    }
                    else if (!IsAnimationStaysOnLastFrame)
                    {
                        _playableGraph.Stop();
                    }
                }
            }
        }

        private void PlayClipWithId(int clipId)
        {
            if (clipId == -1)
            {
                return;
            }
            
            _animationClipId = clipId;
            
            _animationPlayableOutput.SetSourcePlayable(_clipPlayables[_animationClipId]);
            _clipPlayables[_animationClipId].SetTime(0);
            _playableGraph.Play();
        }
        
        #region BACKWARD COMPATIBILITY CODE
        
        [Obsolete]
        public List<VarwinCustomAnimation> GetCustomAnimations()
        {
            return CustomAnimations;
        }
        
        [Obsolete]
        public string GetCustomAnimationsValueListName()
        {
            return "VarwinCustomAnimationClips";
        }
        
        #endregion
    }
}