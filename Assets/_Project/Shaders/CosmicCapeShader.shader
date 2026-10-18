Shader "TheLastStar/CosmicCape"
{
    Properties
    {
        _DeepSpaceColor ("Deep Space Color", Color) = (0.02, 0.03, 0.12, 1.0)
        _NebulaColorA ("Nebula Color Cyan", Color) = (0.1, 0.5, 0.9, 1.0)
        _NebulaColorB ("Nebula Color Magenta", Color) = (0.6, 0.15, 0.7, 1.0)
        _StarEmission ("Star Sparkle Emission", Range(1.0, 10.0)) = 4.5
        _TwinkleSpeed ("Twinkle Speed", Range(0.1, 5.0)) = 1.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _DeepSpaceColor;
                float4 _NebulaColorA;
                float4 _NebulaColorB;
                float _StarEmission;
                float _TwinkleSpeed;
            CBUFFER_END

            // Procedural pseudo-random
            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // World-projected nebula coordinates
                float2 worldUV = input.positionWS.xy * 0.4 + input.positionWS.zz * 0.2;
                
                // Nebula cloud noise
                float n1 = sin(worldUV.x * 2.5 + _Time.y * 0.15) * cos(worldUV.y * 2.5 - _Time.y * 0.1);
                float n2 = cos(worldUV.x * 4.0 - _Time.y * 0.2) * sin(worldUV.y * 3.5 + _Time.y * 0.15);
                float nebulaMix = saturate(n1 * 0.5 + n2 * 0.5 + 0.5);

                half3 baseNebula = lerp(_DeepSpaceColor.rgb, _NebulaColorA.rgb, nebulaMix);
                baseNebula = lerp(baseNebula, _NebulaColorB.rgb, saturate(n2 * 0.8));

                // Starfield grid
                float2 starGrid = input.uv * 45.0;
                float2 gridId = floor(starGrid);
                float starRnd = Hash(gridId);

                half3 starColor = half3(0, 0, 0);
                if (starRnd > 0.88)
                {
                    float dist = length(frac(starGrid) - 0.5);
                    float twinkle = sin(_Time.y * _TwinkleSpeed * 3.0 + starRnd * 20.0) * 0.5 + 0.5;
                    float starIntensity = smoothstep(0.35, 0.05, dist) * twinkle;
                    starColor = half3(0.7, 0.9, 1.0) * starIntensity * _StarEmission;
                }

                half3 finalColor = baseNebula + starColor;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
