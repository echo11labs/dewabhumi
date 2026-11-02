Shader "Dewabhumi/Gopala River"
{
    Properties
    {
        _DeepColor("Deep water", Color) = (0.055,0.16,0.15,1)
        _ShallowColor("Shallow water", Color) = (0.22,0.31,0.24,1)
        _FlowSpeed("Surface flow speed", Float) = 0.35
        _RippleScale("Ripple scale", Float) = 0.7
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor, _ShallowColor;
                float _FlowSpeed, _RippleScale;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float2 uv : TEXCOORD1; half fog : TEXCOORD2; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float phase = input.uv.y * 12 - _Time.y * _FlowSpeed * 4;
                float a = sin(phase * _RippleScale + input.uv.x * 11);
                float b = cos(phase * 0.63 - input.uv.x * 17 + 1.2);
                half3 normal = normalize(half3(a * 0.09, 1, b * 0.07));
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                Light light = GetMainLight();
                half shore = pow(saturate(abs(input.uv.x * 2 - 1)), 9);
                half3 baseColor = lerp(_DeepColor.rgb, _ShallowColor.rgb, shore * 0.7 + (a * b + 1) * 0.06);
                half fresnel = pow(1 - saturate(dot(normal, view)), 4);
                half specular = pow(saturate(dot(normal, normalize(light.direction + view))), 100);
                half foam = shore * smoothstep(0.72, 0.95, a * b) * 0.28;
                half3 color = baseColor * (SampleSH(normal) * 0.5 + light.color * (0.3 + saturate(dot(normal, light.direction)) * 0.65));
                color += half3(0.38,0.49,0.5) * fresnel * 0.4 + specular * light.color * 0.55 + foam;
                return half4(MixFog(color, input.fog), 1);
            }
            ENDHLSL
        }
    }
}
