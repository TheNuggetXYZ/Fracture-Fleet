Shader "Unlit/GasSphere"
{
    Properties
    {
        _SphereRadius ("Sphere Radius", Float) = 300
        _Density ("Density", Float) = 10
        _Fade ("Fade", Float) = 0.1
        _DensityNoiseIntensity ("Density Noise Intensity", Float) = 1
        _DensityNoiseFreq ("Density Noise Freq", Float) = 1
        _ColorNoiseFreq ("Color Noise Freq", Float) = 1
        _Color ("Color", Color) = (0, 0.5, 1)
        _SecondaryColor ("Secondary Color", Color) = (0, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Front
        
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM

            #pragma target 3.0
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "noiseSimplex.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SphereRadius;
            float _Density;
            float _Fade;

            float4 _Color;
            float4 _SecondaryColor;

            float _DensityNoiseFreq;
            float _DensityNoiseIntensity;
            
            float _ColorNoiseFreq;

            bool raySphere(float3 ro, float3 rd, float3 sc, float sr, out float t0, out float t1)
            {
                float3 oc = ro - sc;
                float b = dot(oc, rd);
                float c = dot(oc, oc) - sr * sr;
                float h = b*b - c;
                if (h < 0) return false;
                h = sqrt(h);
                t0 = -b - h; // distance from origin to entry
                t1 = -b + h; // distance from origin to exit
                return true;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.screenPos = ComputeScreenPos(o.vertex);
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = GetNormalizedScreenSpaceUV(i.vertex);

                float rawScreenDepth = SampleSceneDepth(uv);
                float sceneDepth = LinearEyeDepth(rawScreenDepth, _ZBufferParams);
                float3 worldPos = ComputeWorldSpacePosition(uv, rawScreenDepth, UNITY_MATRIX_I_VP);
                
                float3 rayDir = normalize(worldPos - _WorldSpaceCameraPos);
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 sphereCenter = unity_ObjectToWorld._m03_m13_m23;
                float rayLength = length(worldPos - _WorldSpaceCameraPos);

                float entry, exit;
                if (!raySphere(rayOrigin, rayDir, sphereCenter, _SphereRadius, entry, exit))
                    return float4(0,0,0,0);
                
                // clamp entry if camera is inside
                entry = max(entry, 0);

                float midT = entry + max(0, exit - entry) * 0.5;

                exit = min(exit, sceneDepth);
                //return 1-clamp(sceneDepth, 0, 500) / 500;
                
                float gasDepth = max(0, exit - entry);

                float maxDepth = 2.0 * _SphereRadius;
                float depthFixed = saturate(gasDepth / maxDepth);

                float3 samplePos = rayOrigin + rayDir * midT;
                samplePos = (samplePos - sphereCenter) / _SphereRadius;

                float noise = snoise(samplePos * _DensityNoiseFreq) * _DensityNoiseIntensity;
                float alpha = clamp(max(depthFixed - _Fade, 0) * (_Density + noise), 0, 1);
                /*Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;*/
                
                float colorNoise = pow(snoise(samplePos * _ColorNoiseFreq), 1);
                float3 col = _Color.rgb * colorNoise;
                col += _SecondaryColor.rgb * (1 - colorNoise);
                col = col; 
                
                return float4(col.x, col.y, col.z, alpha);
            }
            ENDHLSL
        }
    }
}
