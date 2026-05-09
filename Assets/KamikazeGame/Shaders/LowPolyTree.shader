Shader "KamikazeGame/LowPolyTree"
{
    Properties
    {
        _TrunkColor  ("Trunk Color",  Color) = (0.35, 0.20, 0.08, 1)
        _LeafColor   ("Leaf Color",   Color) = (0.15, 0.50, 0.10, 1)
        _TrunkHeight ("Trunk Height (local Y)", Float) = 3.5
        _BlendZone   ("Blend Zone",   Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float  localY      : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _TrunkColor;
                float4 _LeafColor;
                float  _TrunkHeight;
                float  _BlendZone;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.localY      = IN.positionOS.y;   // local space Y = height
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Blend trunk → leaf by local height
                float t = saturate((IN.localY - _TrunkHeight) / max(_BlendZone, 0.01));
                half4 baseColor = lerp(_TrunkColor, _LeafColor, t);

                // Simple Lambert lighting
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
                float3 normal = normalize(IN.normalWS);
                float  NdotL  = saturate(dot(normal, mainLight.direction));
                float3 lit    = baseColor.rgb * (mainLight.color * NdotL * mainLight.shadowAttenuation + 0.50);

                return half4(lit, 1.0);
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attrs { float4 pos : POSITION; float3 normal : NORMAL; };
            struct Vary  { float4 pos : SV_POSITION; };

            Vary ShadowVert(Attrs IN)
            {
                Vary OUT;
                float3 ws = TransformObjectToWorld(IN.pos.xyz);
                float3 wn = TransformObjectToWorldNormal(IN.normal);
                OUT.pos = TransformWorldToHClip(ApplyShadowBias(ws, wn, _MainLightPosition.xyz));
                return OUT;
            }
            half4 ShadowFrag(Vary IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
