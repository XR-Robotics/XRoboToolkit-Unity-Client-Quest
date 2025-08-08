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
            float4 _UvTopLeftRight;
            float4 _UvBottomLeftRight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float2 uvTop = lerp(_UvTopLeftRight.xy, _UvTopLeftRight.zw, v.uv.x);
                float2 uvBottom = lerp(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, v.uv.x);
                o.uv = lerp(uvTop, uvBottom, v.uv.y);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Apply proper YUV to RGB conversion for MediaDecoder output
                // MediaDecoder typically outputs in YUV420 format
                float y = col.r;
                float u = col.g - 0.5;
                float v = col.b - 0.5;
                
                // ITU-R BT.601 conversion matrix
                fixed3 rgb;
                rgb.r = saturate(y + 1.402 * v);
                rgb.g = saturate(y - 0.344 * u - 0.714 * v);
                rgb.b = saturate(y + 1.772 * u);
                
                return fixed4(rgb, col.a);
            }
            ENDCG
        }
    }
}