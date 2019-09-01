Shader "Hidden/CameraPaletteFilter"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			
			uniform float _FilterBlendStrength;
            uniform float4 _Colours[32];

			float4 frag (v2f i) : SV_Target
			{
				float4 sourceColour = tex2D(_MainTex, i.uv);
				float3 shadedSource = tex2D(_MainTex, i.uv + float2(0, -0.01)).rgb * float3(2,0,0);
				
				float4 outputColour = tex2D(_MainTex, i.uv);
				
				//float3 shadedSource = sourceColour.rgb * float3(2, 0, 0);
				
				float closestDistance = 1.0;
				for (int i = 0; i < 32; i++) {
				    float dist = distance(shadedSource.rgb, _Colours[i].rgb);
				    if (dist < closestDistance) {
				        closestDistance = dist;
				        outputColour.rgb = _Colours[i].rgb;
				    }
				}
				//outputColour.rgb = shadedSource.rgb;
				
				// placeholder default code; inverting colours
			    //outputColour.rgb = 1 - sourceColour.rgb;
				
				return lerp(sourceColour.rgba, outputColour.rgba, _FilterBlendStrength);;
			}
			ENDCG
		}
	}
}
