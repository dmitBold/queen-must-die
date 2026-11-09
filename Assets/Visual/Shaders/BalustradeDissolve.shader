// Балюстрада, исчезающая от руны: растворение как у глаза из QMD/EyeDissolve, но с обычным освещением,
// чтобы до растворения объект не отличался от соседних балюстрад на S_General.
// Шум считается по мировой позиции: у балюстрады сильный неравномерный масштаб, в пространстве объекта пятна вытянулись бы.
Shader "QMD/BalustradeDissolve"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        [HDR] _BaseColor ("Base Tint", Color) = (1, 1, 1, 1)
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Float) = 1
        _ORMMap ("ORM Map", 2D) = "white" {}
        _ORMScale ("ORM Scale", Vector) = (1, 1, 1, 0)
        [Toggle] _RoughnessInRed ("Map Is RAM (Roughness in R, AO in G)", Float) = 0
        _Desaturate ("Desaturate", Range(0, 1)) = 0.3
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale (per meter)", Float) = 6
        _EdgeWidth ("Edge Width", Range(0.001, 0.3)) = 0.14
        [HDR] _EdgeColor ("Edge Color (at the burn line)", Color) = (1.3, 0.85, 0.12, 1)
        [HDR] _EdgeOuterColor ("Edge Outer Color", Color) = (0.55, 0.06, 1.2, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" }

        HLSLINCLUDE
        // Вне UnityPerMaterial: этот блок объявлен в LitInput.hlsl. Шейдер не попадает в SRP Batcher, для одного объекта это неважно.
        float _Dissolve;
        float _NoiseScale;

        float DissolveHash(float3 p)
        {
            p = frac(p * 0.3183099 + 0.1);
            p *= 17.0;
            return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
        }

        float DissolveValueNoise(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            return lerp(
                lerp(lerp(DissolveHash(i), DissolveHash(i + float3(1, 0, 0)), f.x), lerp(DissolveHash(i + float3(0, 1, 0)), DissolveHash(i + float3(1, 1, 0)), f.x), f.y),
                lerp(lerp(DissolveHash(i + float3(0, 0, 1)), DissolveHash(i + float3(1, 0, 1)), f.x), lerp(DissolveHash(i + float3(0, 1, 1)), DissolveHash(i + float3(1, 1, 1)), f.x), f.y),
                f.z);
        }

        float DissolveNoise(float3 positionWS)
        {
            float3 p = positionWS * _NoiseScale;
            return DissolveValueNoise(p) * 0.6 + DissolveValueNoise(p * 2.3) * 0.3 + DissolveValueNoise(p * 5.1) * 0.1;
        }

        // Запас по краям гарантирует, что при 0 объект целый, а при 1 исчезает полностью.
        float DissolveThreshold()
        {
            return lerp(-0.1, 1.1, _Dissolve);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex LitPassVertex
            #pragma fragment DissolveFragment

            #define _NORMALMAP 1

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"

            TEXTURE2D(_ORMMap); SAMPLER(sampler_ORMMap);
            float _Desaturate;
            float4 _ORMScale;
            float _RoughnessInRed;
            float _EdgeWidth;
            float4 _EdgeColor;
            float4 _EdgeOuterColor;

            // Повторяет поверхность графа S_General: ORM в каналах R, G, B и обесцвечивание базовой текстуры.
            SurfaceData BuildSurface(float2 uv)
            {
                SurfaceData surface = (SurfaceData)0;
                half3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb;
                half luminance = dot(baseColor, half3(0.2126729, 0.7151522, 0.072175));
                half3 orm = SAMPLE_TEXTURE2D(_ORMMap, sampler_ORMMap, uv).rgb;
                if (_RoughnessInRed > 0.5) orm = orm.grb;
                orm *= _ORMScale.xyz;
                surface.albedo = lerp(luminance.xxx, baseColor, 1.0 - _Desaturate) * _BaseColor.rgb;
                surface.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
                surface.occlusion = orm.r;
                surface.smoothness = saturate(1.0 - orm.g);
                surface.metallic = orm.b;
                surface.alpha = 1.0;
                return surface;
            }

            void DissolveFragment(
                Varyings input
                , out half4 outColor : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
            )
            {
                float noise = DissolveNoise(input.positionWS);
                float threshold = DissolveThreshold();
                clip(noise - threshold);

                SurfaceData surface = BuildSurface(input.uv);
                InputData inputData;
                InitializeInputData(input, surface.normalTS, inputData);
            #if defined(_DBUFFER)
                ApplyDecalToSurfaceData(input.positionCS, surface, inputData);
            #endif
                InitializeBakedGIData(input, inputData);

                half4 color = UniversalFragmentPBR(inputData, surface);
                if (_Dissolve > 0.0)
                {
                    float edge = 1.0 - saturate((noise - threshold) / _EdgeWidth);
                    // Квадрат прижимает золото к самой кромке, дальше от неё остаётся фиолетовый.
                    color.rgb += lerp(_EdgeOuterColor.rgb, _EdgeColor.rgb, edge * edge) * edge;
                }
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                outColor = half4(color.rgb, 1.0);

            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DissolveShadowVertex
            #pragma fragment DissolveShadowFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            struct DissolveShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            DissolveShadowVaryings DissolveShadowVertex(Attributes input)
            {
                DissolveShadowVaryings output;
                output.positionCS = GetShadowPositionHClip(input);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 DissolveShadowFragment(DissolveShadowVaryings input) : SV_Target
            {
                clip(DissolveNoise(input.positionWS) - DissolveThreshold());
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DissolveDepthVertex
            #pragma fragment DissolveDepthFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings DissolveDepthVertex(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half DissolveDepthFragment(Varyings input) : SV_Target
            {
                clip(DissolveNoise(input.positionWS) - DissolveThreshold());
                return input.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DissolveNormalsVertex
            #pragma fragment DissolveNormalsFragment
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            Varyings DissolveNormalsVertex(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            void DissolveNormalsFragment(
                Varyings input
                , out half4 outNormalWS : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
            )
            {
                clip(DissolveNoise(input.positionWS) - DissolveThreshold());
                outNormalWS = half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }
    }
}
