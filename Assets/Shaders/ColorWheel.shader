Shader "Unlit/ColorWheel"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define TAU 6.28318530718

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

            float _Thickness;
            float _ColorCount;
            float4 _WheelColors[64];
            float _SegmentAngleSpread;

            float remapAngle(float angle)
            {
                angle = fmod(angle, TAU);
                angle += angle < 0 ? TAU : 0;
                return angle;
            }
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 r_uv = i.uv * 2 - 1;

                float radial = -(distance(r_uv, 0) - 1);
                float innerRadial = radial - _Thickness;
                clip(radial);
                clip(-innerRadial);

                float stepped = step(0, radial);

                float angle = atan2(r_uv.y, r_uv.x);
                angle = remapAngle(angle);
                
                float colorPicker = floor(angle / _SegmentAngleSpread);

                float4 color = _WheelColors[fmod(colorPicker, _ColorCount)];

                return stepped * color;
            }
            ENDCG
        }
    }
}
