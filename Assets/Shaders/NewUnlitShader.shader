Shader "Custom/SpriteOutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Float) = 0.1
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionIntensity ("Emission Intensity", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineSize;
            float4 _EmissionColor;
            float _EmissionIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_TARGET
            {
                // Sample the texture at its original location
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Calculate emission
                fixed4 emission = _EmissionColor * _EmissionIntensity;

                // Outline effect
                fixed4 outlineColor = _OutlineColor;
                float2 offset = float2(_OutlineSize, _OutlineSize) * 0.001;

                bool isOutlinePixel = tex.a < 0.1 && (tex2D(_MainTex, i.uv + float2(0, offset.y)).a > 0.1 || tex2D(_MainTex, i.uv - float2(0, offset.y)).a > 0.1 || tex2D(_MainTex, i.uv - float2(offset.x, 0)).a > 0.1 || tex2D(_MainTex, i.uv + float2(offset.x, 0)).a > 0.1);

                if (isOutlinePixel)
                {
                    // Add emission to the outline and set alpha
                    outlineColor.rgb += emission.rgb;
                    outlineColor.a = 1.0; // Or adjust for desired outline transparency
                    return outlineColor;
                }
                else
                {
                    // Add emission to the sprite color, preserving its alpha
                    //tex.rgb += emission.rgb;
                    return tex;
                }
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
