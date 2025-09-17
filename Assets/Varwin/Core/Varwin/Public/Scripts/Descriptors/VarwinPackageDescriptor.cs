using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin.Public
{
    [CreateAssetMenu(menuName = "Varwin/Varwin Package", order = 92)]
    public class VarwinPackageDescriptor : ScriptableObject
    {
        public LocalizationDictionary Name;
        
        public LocalizationDictionary Description;
        
        [Obsolete]
        public Texture2D View;
        
        [Obsolete]
        public Texture2D Thumbnail;

        public LocalizedDictionary<Texture2D> ViewImage;
        public LocalizedDictionary<Texture2D> ThumbnailImage;
        
        public string Guid;
        public string RootGuid;
        
        public string AuthorName;
        public string AuthorEmail;
        public string AuthorUrl;

        public string LicenseCode;
        public string LicenseVersion;
        
        public string BuiltAt;
        public bool Locked;

        public GameObject[] Objects;
        public SceneReference[] SceneTemplates;
        public VarwinResource[] Resources;
        
        public bool CurrentVersionWasBuilt;
    }
}
