using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
    [HideInInspector] public Camera Camera;
    
    [HideInInspector] public int Width = 256;
    [HideInInspector] public int Height = 256;

    private void Awake()
    {
        if (!Camera)
        {
            Camera = gameObject.GetComponentInChildren<Camera>(true);
        }

        if (Camera)
        {
            Camera.enabled = false;
        }
    }

    public void Init(int width, int height)
    {
        if (!Camera)
        {
            Camera = gameObject.GetComponentInChildren<Camera>(true);
        }

        if (Camera)
        {
            Width = width;
            Height = height;
            Camera.targetTexture = new RenderTexture(width, height, 16);
        }
    }
    
    public Texture2D GetScreenshot(int width, int height)
    {
        if (!Camera)
        {
            Camera = gameObject.GetComponentInChildren<Camera>(true);
        }

        if (!Camera)
        {
            return null;
        }

        bool previousCameraState = Camera.enabled;
        Camera.enabled = true;
        
        RenderTexture displayRenderTexture = Camera.activeTexture;
        
        RenderTexture screenshotRenderTexture = new RenderTexture(width, height, 24);
        
        Camera.targetTexture = screenshotRenderTexture;
        Camera.Render();
        
        RenderTexture.active = screenshotRenderTexture;
        
        Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        
        Camera.targetTexture = displayRenderTexture;
        
        RenderTexture.active = null;  
        DestroyImmediate(screenshotRenderTexture);

        Camera.enabled = previousCameraState;
        
        return screenshotTexture;
    }

    public byte[] GetTextureJpgBytes(Vector2Int size)
    {
        return GetTextureJpgBytes(size.x, size.y);
    }

    public byte[] GetTextureJpgBytes(int width, int height)
    {
        var texture = GetScreenshot(width, height);
        return texture.EncodeToJPG();
    }

    public byte[] GetTexturePngBytes(Vector2Int size)
    {
        return GetTexturePngBytes(size.x, size.y);
    }

    public byte[] GetTexturePngBytes(int width, int height)
    {
        var texture = GetScreenshot(width, height);
        return texture.EncodeToPNG();
    }
}