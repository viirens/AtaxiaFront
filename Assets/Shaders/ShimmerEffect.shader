Shader "Custom/ShimmerSurfaceShaderWithTransparency"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShimmerColor ("Shimmer Color", Color) = (1,1,1,1)
        _ShimmerStrength ("Shimmer Strength", Range(0,1)) = 0.5
        _ShimmerSpeed ("Shimmer Speed", Range(0,10)) = 1
        _EmissionIntensity ("Emission Intensity", Range(0,10)) = 1
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _Offset("Shimmer Offset", Range(0,1)) = 0.8
        _Amplitude("Shimmer Amplitude", Range(0,1)) = 0.2
        _CanvasGroupAlpha("Canvas Group Alpha", Float) = 1.0

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        sampler2D _MainTex;
        fixed4 _ShimmerColor;
        float _ShimmerStrength;
        float _ShimmerSpeed;
        float _EmissionIntensity;
        float _Transparency;
        float _Offset;
        float _Amplitude;
        float _CanvasGroupAlpha;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _ShimmerColor;
            float offset = 0.8;
            float amplitude = 0.2;
            float shimmer = offset + amplitude * sin(_Time.y * _ShimmerSpeed);

            o.Albedo = c.rgb;
            o.Emission = c.rgb * shimmer * _EmissionIntensity;
            o.Alpha = c.a * _Transparency * _CanvasGroupAlpha; // Apply canvas group alpha here
        }

        ENDCG
    }
    FallBack "Transparent"
}
