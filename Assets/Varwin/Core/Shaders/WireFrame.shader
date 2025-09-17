Shader "Varwin/Effects/Wireframe"
{
    Properties
    {
        _MainColor ("MainColor", Color) = (0,1,0,1)
    }

    SubShader
    {
        Color [_MainColor]
        Pass
        {
        }
    }
}