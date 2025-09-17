using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.Public
{
    [RequireComponent(typeof(VarwinBot))] 
    public class VarwinBotTextBubble : MonoBehaviour
    {
        public TMP_Text HeaderText;
        public TMP_Text MainText;

        public GameObject Container;

        [HideInInspector]
        public VarwinBot.TextBubbleHideType HideType;
        
        [HideInInspector]
        public bool BotHasTextToSpeech;
        [HideInInspector]
        public bool BotTextToSpeechFinished;
        
        private const float FadeTime = 0.3f;

        private Vector3 defaultScale = Vector3.one;
        private bool _showTextBubble = true;
        
        public event Action SpeechCompleted;
        public event Action<string, string> SpeechStarted;

        public bool ShowTextBubble
        {
            get => _showTextBubble;
            set
            {
                _showTextBubble = value;

                if (!_showTextBubble)
                {
                    HideText();
                }
            }
        }
        
        private Vector3 _startContainerLocalPosition;
        private Quaternion _startContainerLocalRotation;

        private bool _lookAtCamera = true;

        public bool LookAtCamera
        {
            get => _lookAtCamera;
            set
            {
                _lookAtCamera = value;

                if (!_lookAtCamera)
                {
                    Container.transform.localPosition = _startContainerLocalPosition;
                    Container.transform.localRotation = _startContainerLocalRotation;
                }
            }
        }

        private void Start()
        {
            _startContainerLocalPosition = Container.transform.localPosition;
            _startContainerLocalRotation = Container.transform.localRotation;
        }

        private void Update()
        {
            if(InputAdapter.Instance != null && InputAdapter.Instance.PlayerController.Nodes.Head != null && LookAtCamera)
            {
                Container.transform.LookAt(InputAdapter.Instance.PlayerController.Nodes.Head.Transform);
                Container.transform.rotation = Quaternion.Euler(0, Container.transform.rotation.eulerAngles.y, 0);
            }
        }

        public void ResetDefaultScale()
        {
            defaultScale = Container.transform.localScale;
        }

        public void ShowText(string header, string text)
        {
            if (!ShowTextBubble)
            {
                return;
            }

            HeaderText.gameObject.SetActive(!string.IsNullOrEmpty(header));
            HeaderText.text = header;
            MainText.text = text;
            
            BotTextToSpeechFinished = false;
            
            StartCoroutine(ShowTextRoutine(header, text));
        }

        public void HideText()
        {
            Container.transform.localScale = defaultScale;
            Container.SetActive(false);
            
            StopAllCoroutines();
        }

        private IEnumerator ShowTextRoutine(string header, string text)
        {
            SpeechStarted?.Invoke(header, text);
            Container.SetActive(true);
            Container.transform.localScale = Vector3.zero;

            float timer = 0;

            while (timer < FadeTime)
            {
                Container.transform.localScale = defaultScale * timer / FadeTime;

                timer += Time.deltaTime;

                yield return null;
            }

            Container.transform.localScale = defaultScale;

            float showTime = CalculateShowTime(header, text);
            
            if (showTime < 0)
            {
                yield break;
            }

            if (BotHasTextToSpeech)
            {
                while (!BotTextToSpeechFinished)
                {
                    yield return null;
                }

                BotTextToSpeechFinished = false;
            }
            else
            {
                yield return new WaitForSeconds(showTime);
            }

            timer = FadeTime;

            while (timer > 0)
            {
                Container.transform.localScale = defaultScale * timer / FadeTime;

                timer -= Time.deltaTime;

                yield return null;
            }
            
            Container.SetActive(false);
            Container.transform.localScale = defaultScale;
            
            SpeechCompleted?.Invoke();
        }

        private float CalculateShowTime(string header, string text)
        {
            if (HideType == VarwinBot.TextBubbleHideType.Never)
            {
                return -1;
            }
            
            string referenceString = text.Length > header.Length ? text : header;
            
            float referenceTime = referenceString.Length * 0.125f;

            return Mathf.Clamp(referenceTime, 3f, referenceTime);
        }
    }
}