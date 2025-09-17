using System;
using UnityEngine;
namespace Varwin
{
    public class HighlightPlusManager : MonoBehaviour
    {
        private static HighlightPlusManager Instance { get; set; }
        
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            if (HighlightAdapter.Instance == null)
            {
                HighlightAdapter.Init(new HighlightPlusAdapter());
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
