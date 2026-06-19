Shader "Custom/URP_Glass"
{
    Properties
    {
        _Color ("Tint Color", Color) = (1,1,1,0.2)
        _Metallic ("Metallic", Range(0,1)) = 1
        _Smoothness ("Smoothness", Range(0,1)) = 0.95
        _RefractStrength ("Refraction Strength", Range(0, 0.2)) = 0.05
        [Toggle]_ReceiveShadows ("Receive Shadows", Float) = 1
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200

        // Проход для прозрачного рендера с преломлением
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha   // стандартное альфа-смешение
            ZWrite On                         // сохраняем глубину для корректного порядка
            ZTest LEqual
            Cull Back                         // или Off – если нужна двусторонность

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float2 uv         : TEXCOORD1;
                float4 screenPos  : TEXCOORD2;
                float3 viewDirWS  : TEXCOORD3;
                float fogCoord    : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Metallic;
                float _Smoothness;
                float _RefractStrength;
                float _ReceiveShadows;
            CBUFFER_END

            // Объявляем текстуру камеры (должна быть включена Opaque Texture в URP Asset)
            TEXTURE2D_X_FLOAT(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(TransformObjectToWorld(input.positionOS.xyz));
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Нормаль в пространстве экрана (для преломления)
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // 2. Искажение UV для сэмплирования фоновой текстуры
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 refractOffset = normalWS.xy * _RefractStrength;
                float2 distortedUV = screenUV + refractOffset;
                
                // 3. Цвет фона (то, что за стеклом)
                half3 bgColor = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedUV).rgb;
                
                // 4. Простое освещение (блики + металличность)
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = float3(0,0,0); // не нужен для простого specular
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = viewDirWS;
                lightingInput.shadowCoord = TransformWorldToShadowCoord(float3(0,0,0)); // не используем тени для простоты
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = _Color.rgb;
                surfaceData.alpha = _Color.a;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                
                half3 specularColor = half3(1,1,1);
                half3 diffuseColor = _Color.rgb;
                
                // Главный источник света
                Light mainLight = GetMainLight();
                half3 mainLightColor = mainLight.color;
                half3 mainLightDir = mainLight.direction;
                
                half3 diffuse = saturate(dot(normalWS, mainLightDir)) * mainLightColor;
                half3 specular = 0;
                if (_Metallic > 0)
                {
                    half3 H = normalize(mainLightDir + viewDirWS);
                    specular = pow(saturate(dot(normalWS, H)), _Smoothness * 256) * mainLightColor;
                }
                
                half3 lighting = diffuse + specular;
                lighting = lerp(lighting, half3(1,1,1), _Smoothness); // добавляем блеск
                
                // 5. Финальный цвет = фон + освещение стекла, смешанные через прозрачность
                half3 glassColor = lighting * _Color.rgb;
                half3 finalRGB = lerp(bgColor, glassColor, _Color.a);
                
                // 6. Применяем туман
                finalRGB = MixFog(finalRGB, input.fogCoord);
                
                return half4(finalRGB, _Color.a);
            }
            ENDHLSL
        }

        // Проход для теней (опционально, но чтобы объект отбрасывал тени)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AttributesShadow
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            struct VaryingsShadow
            {
                float4 positionCS : SV_POSITION;
            };
            
            VaryingsShadow vertShadow(AttributesShadow input)
            {
                VaryingsShadow output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            half4 fragShadow(VaryingsShadow input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}