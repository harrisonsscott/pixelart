Shader "Unlit/imageMat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Copy ("Copy", 2D) = "white" {}
        _GridSize ("Grid Size", Vector) = (1, 1, 1, 1)
        _Thickness ("Grid Thickness", Float) = 0.1
        _Grid ("Render Grid", Float) = 0
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
                return col;
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _Copy;
            float4 _MainTex_ST;
            float2 _GridSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                fixed4 col2 = tex2D(_Copy, frac(i.uv * _GridSize.x)-0.05);

                if ((col2.r + col2.g + col2.b) / 3 > 0.9 || col.a == 0){
                    return col;
                }
                return col2;
            }
            ENDCG
        }
    }
}
