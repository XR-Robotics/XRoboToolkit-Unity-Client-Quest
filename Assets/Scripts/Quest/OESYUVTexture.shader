Shader "NativeOpenGLRender/OESYUVTexture"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _UvTopLeftRight ("UV of top corners", Vector) = (0, 1, 1, 1)
        _UvBottomLeftRight ("UV of bottom corners", Vector) = (0 , 0, 1, 0) 
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile __ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _UvTopLeftRight;
            float4 _UvBottomLeftRight;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float2 uvTop = lerp(_UvTopLeftRight.xy, _UvTopLeftRight.zw, v.uv.x);
                float2 uvBottom = lerp(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, v.uv.x);
                o.uv = lerp(uvTop, uvBottom, v.uv.y);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                fixed4 col = tex2D(_MainTex, i.uv);

                // MediaDecoder typically outputs limited-range YUV
                float y = col.r * 255.0;
                float u = col.g * 255.0 - 128.0;
                float v = col.b * 255.0 - 128.0;

                // Normalize limited-range YUV values
                y = (y - 16.0) / 219.0;
                u = u / 224.0;
                v = v / 224.0;

                // YUV to sRGB conversion
                float3 rgb;
                rgb.r = y + 1.13983 * v;
                rgb.g = y - 0.39465 * u - 0.58060 * v;
                rgb.b = y + 2.03211 * u;

                // Clamp RGB to avoid over/underflow
                rgb = clamp(rgb, 0.0, 1.0);

                // Convert sRGB to Linear space if Unity is using Linear color space
                #ifdef UNITY_COLORSPACE_GAMMA
                // Do nothing, already in gamma
                #else
                    rgb = GammaToLinearSpace(rgb);
                #endif

                return fixed4(rgb, col.a);
            }
            ENDCG
        }
    }
}