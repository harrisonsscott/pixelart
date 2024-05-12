Shader "Unlit/imageMat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridSize ("Grid Size", Vector) = (1, 1, 1, 1)
        _Thickness ("Grid Thickness", Float) = 0.1
        _Grid ("Render Grid", Float) = 0
        
        _Numbers ("Number Array", 2DArray) = "" {}
        _NumIndex ("Number Index", Float) = 1
        _Spacing ("Number Spacing", Float) = 0.2

        _Solved ("Solved Bool Array", Int) = 0
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (_Grid != 1){
                    return tex2D(_MainTex, i.uv);
                }
                float2 uvRight = i.uv - float2(0.0, 1.0 / _GridSize.x);
                float2 uvBottom = i.uv - float2(1.0 / _GridSize.y, 0.0);

                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col1 = tex2D(_MainTex, uvRight);
                fixed4 col2 = tex2D(_MainTex, uvBottom);

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

                float gs = (col.r + col.g + col.b) / 2.5;

                return float4(gs, gs, gs, col.a);
                // return col;
            }
            ENDCG
        }

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
            
            int _NumIndex;
            float2 _GridSize;
            float _Thickness;
            float _Spacing;

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

                if (_NumIndex >= 10){
                    fixed4 col0 = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv+float2(_Spacing, 0), int(_NumIndex / 10)));
                    fixed4 col1 = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv-float2(_Spacing, 0), _NumIndex - int(_NumIndex / 10) * 10 ));

                    col = col0 + col1;
                } else {
                    col = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv,_NumIndex));
                }
                
                fixed4 col2 = tex2D(_MainTex, i.uv);
                float gs = (col2.r + col2.g + col2.b) / 3.0;

                if (gs < 0.3 || gs > 0.7){
                    col.rgb = 1 - gs;
                }
                
                // return float4(frac(i.uv * 32 - _Thickness/2.0), 0, 0.5);
                return float4(col.rgb, min(col.a, col2.a) * 0.5);
            }
            ENDCG
        }
    }
}
