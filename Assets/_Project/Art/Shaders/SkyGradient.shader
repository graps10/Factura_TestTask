// A two-colour sky.
//
// A flat background colour cannot work here: the ground dissolves into fog, and
// wherever the fog colour meets a different background colour there is a hard
// line across the screen. Setting the horizon colour equal to the fog colour
// makes that seam impossible - the ground fades into fog, the fog and the bottom
// of the sky are the same colour, and the horizon stops being an edge.
//
// Unity's Procedural skybox could give a gradient too, but its colours come out
// of an atmospheric scattering model, so matching one exactly to a fog colour
// means fighting the model. Two colours and a curve are easier to aim.
Shader "TurretRush/Sky Gradient"
{
    Properties
    {
        _HorizonColor("Horizon Color", Color) = (0.81, 0.68, 0.55, 1)
        _SkyColor("Sky Color", Color) = (0.16, 0.78, 0.92, 1)

        [Space]
        _Exponent("Blend Exponent", Range(0.2, 6)) = 1.6
        _HorizonOffset("Horizon Offset", Range(-0.5, 0.5)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex SkyVertex
            #pragma fragment SkyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _HorizonColor;
                half4 _SkyColor;
                half _Exponent;
                half _HorizonOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 directionOS : TEXCOORD0;
            };

            Varyings SkyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                // Unity draws the skybox on a mesh whose object space is the view
                // direction, so the vertex position is the ray this pixel looks along.
                output.directionOS = input.positionOS.xyz;

                return output;
            }

            half4 SkyFragment(Varyings input) : SV_Target
            {
                // Height above the horizon, 0 at eye level and 1 straight up. The
                // exponent decides how much of the sky the horizon colour keeps: higher
                // values hold it down near the ground, which is what haze looks like.
                half height = saturate(normalize(input.directionOS).y - _HorizonOffset);
                half blend = pow(height, _Exponent);

                return half4(lerp(_HorizonColor.rgb, _SkyColor.rgb, blend), 1);
            }
            ENDHLSL
        }
    }
}
