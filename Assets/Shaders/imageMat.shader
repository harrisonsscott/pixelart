Shader "Unlit/imageMat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Overlay ("Overlay Texture", 2D) = "white" {}
        _Overlay2 ("Overlay Texture", 2D) = "white" {} 
        _GridSize ("Grid Size", Float) = (1, 1, 1, 1)
        _Thickness ("Grid Thickness", Float) = 0.1
        _Grid ("Render Grid", Float) = 0 // boolean
        
        _NumSelected ("Selected Number", Float) = 1
        _Numbers ("Number Array", 2DArray) = "" {}
        _NumIndex ("Number Index", Float) = 1
        _Spacing ("Number Spacing", Float) = 0.2

        _TextData ("Text Data", 2D) = "white" {}
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _Copy;
            float2 _GridSize;
            float _Grid;
            float _Thickness;
            float4 _MainTex_ST;

            sampler2D _TextData;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvRight = i.uv - float2(0.0, 1.0 / _GridSize.x);
                float2 uvBottom = i.uv - float2(1.0 / _GridSize.y, 0.0);

                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col1 = tex2D(_MainTex, uvRight);
                fixed4 col2 = tex2D(_MainTex, uvBottom);
                
                float gs = (col.r + col.g + col.b) / 3.0;

                gs = clamp(gs, 0.9, 1);

                if (_Grid != 1){
                    return float4(gs, gs, gs, col.a);
                }

                float alpha0 = step(0.1, col.a);
                float alpha1 = step(0.1, col1.a);
                float alpha2 = step(0.1, col2.a);
 
                if (frac(i.uv.y * _GridSize.y) < _Thickness){
                    float alpha = max(alpha0, alpha1);
                    col = fixed4(0, 0, 0, alpha);
                    return col;
                }

                if (frac(i.uv.x * _GridSize.x) < _Thickness){
                    float alpha = max(alpha0, alpha2);
                    col = fixed4(0, 0, 0, alpha);
                    return col;
                }
                

                // return tex2D(_TextData, i.uv);
                return float4(gs, gs, gs, col.a);
            }
            ENDCG
        }

        // text rendering pass
        Pass {
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
            
            UNITY_DECLARE_TEX2DARRAY(_Numbers);
            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _NumSelected;
            int _NumIndex;
            float2 _GridSize;
            float _Thickness;
            float _Spacing;

            sampler2D _TextData;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = frac(i.uv * _GridSize - _Thickness/2.0);

                fixed4 col = float4(0, 0, 0, 1);

                float2 index = float2(i.uv);
                _NumIndex = tex2D(_TextData, index).r * 255.0;

                if (_NumIndex >= 10){
                    fixed4 col0 = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv+float2(_Spacing, 0), int(_NumIndex / 10))); // first digit
                    fixed4 col1 = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv-float2(_Spacing, 0), _NumIndex - int(_NumIndex / 10) * 10 )); // second digit

                    col = col0 + col1;
                } else {
                    col = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv,_NumIndex));
                }
                
                fixed4 col2 = tex2D(_MainTex, i.uv);
                float gs = (col2.r + col2.g + col2.b) / 3.0;

                if (_NumIndex == _NumSelected){
                    return float4(col.rgb, min(col.a, col2.a) * 0.5 + 0.15);
                } else {
                    return float4(col.rgb, min(col.a, col2.a) * 0.5);
                }

                
                // return float4(frac(i.uv * 32 - _Thickness/2.0), 0, 0.5);
            }
            ENDCG
        }

        // overlay the images from the compute shader
        Pass {
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
            sampler2D _Overlay;
            sampler2D _Overlay2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 overlay = tex2D(_Overlay, i.uv);

                return overlay;
            }
            ENDCG
        }
    }
}
