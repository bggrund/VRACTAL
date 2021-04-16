// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/InfiniteResShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		//Tags{ "RenderType" = "Opaque" }
		//ZWrite Off
		LOD 100

		Pass
		{
			CGPROGRAM
			//#pragma enable_d3d11_debug_symbols
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
		
			float		ka;
			float		kb;
			float		scale;
			float		centera;
			float		centerb;

			int			maxIter;
			int			mandel;
			int			fadeEdges;
			float		colorOffset;
			float		colorDensity;
			float4		colors[50];
			int			numColors;
			
			float2 fn(float2 z, float2 c) 
			{
				float2 r = float2(0.0, 0.0);

				// First, square z
				r.x = z.x * z.x - z.y * z.y;
				r.y = z.x * z.y * 2.0;

				// Then add c
				r.x += c.x;
				r.y += c.y;

				// Return z^2 + c
				return r;
			}

			float getIterations(float2 c)
			{
				float2 z = c;

				if (mandel == 1)
				{
					if (pow(c.x + 1.0, 2) + pow(c.y, 2) < .0625)
					{
						return 0.0;
					}
					if (pow(c.x + .25, 2) + pow(c.y, 2) < .25)
					{
						return 0.0;
					}
					if (pow(c.x + 1.3, 2) + pow(c.y, 2) < .0025)
					{
						return 0.0;
					}
				}
				else
				{
					c = float2(ka, kb);
				}

				int iter = 0;
				for (int i = 1; i < 50000; i++)
				{
					z = fn(z, c);
					if (z.x * z.x + z.y * z.y >= 4.0)
					{
						iter = i;
						break;
					}
					if (i >= maxIter)
					{
						return 0.0;
					}
				}
				for (int j = 0; j < 3; j++)
				{
					z = fn(z, c);
					iter++;
				}

				return colorOffset + colorDensity * float(numColors) * (float(iter) + 1.0 - log2(log(z.x * z.x + z.y * z.y) / 2.0));// / float(maxIter);
			}

			struct vInput
			{
				float4 vertex : POSITION;
			};

			struct vOutput
			{
				float4 vertex : SV_POSITION;
				float4 fractalPoint : COLOR0;
				float4 rawVertex : COLOR1;
			};

			vOutput vert(vInput i)
			{
				vOutput o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.fractalPoint = float4(centera + scale * i.vertex.x, centerb + scale * i.vertex.y, 0, 0);
				o.rawVertex = i.vertex;

				return o;
			}

			fixed4 frag(vOutput i) : SV_Target
			{
				float iter = getIterations(i.fractalPoint.xy);
			
				// Fade out edges of mesh
				float alpha = 1;
				if (fadeEdges == 1) 
				{
					float distFromSquircle = pow(i.rawVertex.x, 4) + pow(i.rawVertex.y, 4) - pow(0.95, 4);
					if (distFromSquircle > 0)
					{
						alpha = 1 - distFromSquircle * 6;
						if (alpha < 0)
							alpha = 0;
					}
				}

				float r, g, b;

				if (iter == 0.0)
				{
					r = g = b = 0.0;
				}
				else {
					int index1 = round(floor(iter));
					float t2 = iter - float(index1);
					float t1 = 1.0 - t2;
					index1 = round(fmod(float(index1), float(numColors)));
					int index2 = round(fmod(float(index1 + 1), float(numColors)));

					r = (colors[index1].r * t1 + colors[index2].r * t2);
					g = (colors[index1].g * t1 + colors[index2].g * t2);
					b = (colors[index1].b * t1 + colors[index2].b * t2);
				}
				
				return float4(r, g, b, alpha);
			}

			ENDCG
		}
	}
}
