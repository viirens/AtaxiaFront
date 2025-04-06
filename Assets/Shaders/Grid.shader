Shader "Unlit/Grid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_GridColour ("Grid Colour", Color) = (.255,.0,.0,1)
        _GridSize ("Grid Size", Range(0.01, 1.0)) = 0.1
        _GridLineThickness ("Grid Line Thickness", Range(0.00001, 0.010)) = 0.003
        _Alpha ("Grid Transparency", Range(0, 1)) = 0.5
        _Intensity ("Emission Intensity", Range(-5,5)) = 0
        _Width ("Grid Width", Float) = 10.0
        _Height ("Grid Height", Float) = 10.0
        _GridAlpha ("Grid Line Alpha", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GridColour;
            float _GridSize;
            float _GridLineThickness;
            float _Alpha;
            float _Intensity;
            float _Width;
            float _Height;
            float _GridAlpha;
    

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float GridTest(float3 worldPos) {
                float result;

                // Adjust the position by 0.5 to center the grid lines on whole units
                float3 adjustedPos = worldPos + 0.5;
                float2 gridPos = adjustedPos.xy; // Using x and y as ground plane axes

                // Loop through whole steps
                for (float i = 0.0; i <= _Width; i += 1.0) {
                    for (int j = 0; j < 2; j++) {
                        result += 1.0 - smoothstep(0.0, _GridLineThickness, abs(gridPos[j] - i));
                    }
                }

                return result;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate the opacity of the grid lines
                float gridOpacity = GridTest(i.worldPos);
    
                // If the grid opacity is near zero, make the fragment fully transparent
                if (gridOpacity < 0.01)
                {
                    return fixed4(0, 0, 0, 0); // Fully transparent
                }

                // Else, render the grid line color with full opacity
                fixed4 gridColor = _GridColour;
                // gridColor.a = 1; // Fully opaque
                return gridColor;
            }
            ENDCG
        }
    }
}