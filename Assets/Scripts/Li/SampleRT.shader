Shader "Custom/SampleRT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _visibleRatio ("Visible Ratio", Range(0.0, 2.0)) = 1.0
        _contentRatio ("Content Ratio", Range(0.0, 2.0)) = 0.555
        _heightCompressionFactor ("Height Compression", Range(0.0, 2.0)) = 1.25
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
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
            float4 _MainTex_ST;
            sampler2D _mainRT;
            int _isLE;
            float _contentRatio;
            float _visibleRatio;
            float _heightCompressionFactor;
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {   
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                fixed4 col = fixed4(0.0, 0.0, 0.0, 0.0);
                
                // // Automatically detect which eye we're rendering for
                // bool isLeftEye = (unity_StereoEyeIndex == 0);
                
                // Adjust UV coordinates based on eye
                float2 adjusted_uv = i.uv;
                
                // Apply the x shift to the clipping center
                if(_isLE) // Left eye
                {
                    adjusted_uv.x = i.uv.x + 0.08/2; // Shift left for left eye
                }
                else // Right eye
                {
                    adjusted_uv.x = i.uv.x - 0.08/2; // Shift right for right eye
                }
                
                // Calculate the bounds for visible area based on visibleRatio with 4:3 aspect ratio
                float widthRatio = _visibleRatio;
                float heightRatio = _visibleRatio * (3.0/4.0); // Apply 4:3 aspect ratio
                
                float minBoundX = (1.0 - widthRatio) * 0.5;
                float maxBoundX = 1.0 - minBoundX;
                float minBoundY = (1.0 - heightRatio) * 0.5;
                float maxBoundY = 1.0 - minBoundY;
                
                // Check if the current pixel is within the visible bounds
                // using the adjusted UV for x-coordinate
                if (adjusted_uv.x < minBoundX || adjusted_uv.x > maxBoundX || 
                    i.uv.y < minBoundY || i.uv.y > maxBoundY) {
                    return col; // Return transparent/black pixel
                }
                
                // Use original UV coordinates for texture sampling
                float2 new_uv = i.uv;
                
                // Apply height compression
                new_uv.y = (new_uv.y - 0.5) * _heightCompressionFactor + 0.5;
                
                if(_isLE) // Left eye
                {
                    // Apply content ratio with consistent centering
                    float scaled_x = new_uv.x * _contentRatio + (1.0 - _contentRatio) * 0.5 + 0.08;
                    
                    // Map to left half of stereo texture
                    float final_x = scaled_x * 0.5;
                    float final_y = new_uv.y * _contentRatio + (1.0 - _contentRatio) * 0.5;
                    // Flip vertically by inverting the Y coordinate
                    final_y = 1.0 - final_y;
                    
                    col = tex2D(_mainRT, float2(final_x, final_y));
                }
                else // Right eye
                { 
                    // Apply content ratio with consistent centering
                    float scaled_x = new_uv.x * _contentRatio + (1.0 - _contentRatio) * 0.5 - 0.08;
                    
                    // Map to right half of stereo texture
                    float final_x = scaled_x * 0.5 + 0.5;
                    float final_y = new_uv.y * _contentRatio + (1.0 - _contentRatio) * 0.5;
                    // Flip vertically by inverting the Y coordinate
                    final_y = 1.0 - final_y;
                    
                    col = tex2D(_mainRT, float2(final_x, final_y));
                }
                
                // Apply YUV to RGB conversion for MediaDecoder output
                // MediaDecoder typically outputs in YUV420 format
                if (col.a > 0.0) // Only convert if we have valid pixel data
                {
                    float y = col.r;
                    float u = col.g - 0.5;
                    float v = col.b - 0.5;
                    
                    // ITU-R BT.601 conversion matrix
                    fixed3 rgb;
                    rgb.r = saturate(y + 1.402 * v);
                    rgb.g = saturate(y - 0.344 * u - 0.714 * v);
                    rgb.b = saturate(y + 1.772 * u);
                    
                    col.rgb = rgb;
                }
                
                return col;
            }
            ENDCG
        }
    }
}