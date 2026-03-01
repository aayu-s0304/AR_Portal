Shader "Unlit/Mask"
{
    Properties
    {
    
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Zwrite Off
        }
    }
}
