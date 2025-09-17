using System;
using System.Collections.Generic;

[Serializable]
public class BlocklyConfig
{
    public I18n Name { get; set; }
    public I18n Description { get; set; }
    public string Guid { get; set; }  
    public string RootGuid { get; set; }  
    public List<string> Tags { get; set; }
    public bool Locked { get; set; }
    public bool Embedded { get; set; }
    public bool MobileReady { get; set; }
    public bool LinuxReady { get; set; }
    public bool SourcesIncluded { get; set; }
    public bool DisableSceneLogic { get; set; }
    [Obsolete]
    public Config Config { get; set; }
    public JsonAuthor Author { get; set; }
    public JsonLicense License { get; set; }
    public string BuiltAt { get; set; }
    public string SdkVersion { get; set; }
    public string UnityVersion { get; set; }
    public I18n Changelog { get; set; }
}

[Serializable]
public class JsonAuthor
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Url { get; set; }
}

[Serializable]
public class JsonLicense
{
    public string Code { get; set; }
    public string Version { get; set; }
}

[Serializable]
public class Config
{
    public string type { get; set; }
    public List<Block> blocks { get; set; }
}

[Serializable]
public class Item : ILocalizable
{
    public string method { get; set; }
    public I18n i18n { get; set; }
    public I18n variablesFormat { get; set; }
    public string property { get; set; }
    public string name { get; set; }
}

[Serializable]
public class Param : ILocalizable
{
    public string name { get; set; }
    public string valueType { get; set; }
    public I18n i18n { get; set; }
}

[Serializable]
public class Block
{
    public string name { get; set; }
    public string colour { get; set; }
    public string type { get; set; }
    public bool isCoroutine { get; set; }
    public bool isObsolete { get; set; }
    public List<Item> items { get; set; }
    public string valueType { get; set; }
    public List<Param> @params { get; set; }
    public List<Arg> args { get; set; }
    public List<Value> values { get; set; }
    public I18n tooltip { get; set; }
    public I18n group { get; set; }
    public I18n valueArgFormat { get; set; }

    public void AddItem(Item item)
    {
        if (!items.Contains(item))
        {
            items.Add(item);
        }
    }
}

[Serializable]
public class Value : ILocalizable
{
    public string name { get; set; }
    public I18n i18n { get; set; }
}

[Serializable]
public class Arg : ILocalizable
{
    public string values { get; set; }
    public string valueType { get; set; }
    public List<string> valueLists { get; set; }
    public I18n i18n { get; set; }
    public string defaultValueType { get; set; }
    public dynamic defaultValue { get; set; }
}

public interface ILocalizable
{
    I18n i18n { get; set; }
}
