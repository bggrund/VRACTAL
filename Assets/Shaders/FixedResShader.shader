// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/FixedResShader"
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
			int			depth;
			float		depthScale;
			float		inclination;
			int			invertDepth;
			int			fadeEdges;
			float		colorOffset;
			float		colorDensity;
			float4		colors[50];
			int			numColors;
			
			float2 fn(float2 z, float2 c) 
			{
				float2 x = float2(0.0, 0.0);
				x.x = z.x * z.x - z.y * z.y;
				x.y = z.x * z.y + z.y * z.x;
				
				z = x;

				x.x = z.x + c.x;
				x.y = z.y + c.y;

				return x;
			}

			float getIterations(float2 c)
			{
				float2 z = c;

				if (mandel == 1)
				{
					if (pow(c.x + 1.0, 2.0) + pow(c.y, 2.0) < .0625)
					{
						return 0.0;
					}
					if (pow(c.x + .25, 2.0) + pow(c.y, 2.0) < .25)
					{
						return 0.0;
					}
					if (pow(c.x + 1.3, 2.0) + pow(c.y, 2.0) < .0025)
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

				return float(iter) + 1.0 - log2(log(z.x * z.x + z.y * z.y) / 2.0);// colorOffset + colorDensity * (float(iter) + 1.0 - log2(log(z.x * z.x + z.y * z.y) / 2.0)) / float(maxIter) * float(numColors);
			}

			struct vInput
			{
				float4 vertex : POSITION;
			};

			struct vOutput
			{
				float4 vertex : SV_POSITION;
				fixed4 fColor : COLOR0;
			};

			vOutput vert(vInput i)
			{
				vOutput o;

				// Fade out edges of mesh
				float alpha = 1;
				if (fadeEdges == 1)
				{
					float distFromSquircle = pow(i.vertex.x, 4) + pow(i.vertex.y, 4) - pow(0.95, 4);
					if (distFromSquircle > 0)
					{
						alpha = 1 - distFromSquircle * 6;
						if (alpha < 0)
							alpha = 0;
					}
				}
				
				float iter = getIterations(float2(centera + scale * i.vertex.x, centerb + scale * i.vertex.y));
				float colorIter = colorOffset + colorDensity * float(numColors) * iter;// / float(maxIter);

				float r, g, b;

				if (iter == 0.0)
				{
					r = g = b = 0.0;
				}
				else {
					int index1 = round(floor(colorIter));
					float t2 = colorIter - float(index1);
					float t1 = 1.0 - t2;
					index1 = round(fmod(float(index1), float(numColors)));
					int index2 = round(fmod(float(index1 + 1), float(numColors)));

					r = (colors[index1].r * t1 + colors[index2].r * t2);
					g = (colors[index1].g * t1 + colors[index2].g * t2);
					b = (colors[index1].b * t1 + colors[index2].b * t2);
				}

				o.fColor = fixed4(r, g, b, alpha);
				if (depth == 1)
				{
					float d;

					if (iter == 0.0)
					{
						iter = maxIter;
					}

					if (invertDepth == 0) 
					{
						d = (-depthScale / 4) + depthScale * (pow(iter / maxIter, inclination));
					}
					else
					{
						d = depthScale - depthScale * (pow(iter / maxIter, inclination / 4));
					}

					o.vertex = UnityObjectToClipPos(float4(i.vertex.xy, d, i.vertex.w));
				}
				else
				{
					o.vertex = UnityObjectToClipPos(i.vertex);
				}
				return o;
			}

			fixed4 frag(vOutput i) : SV_Target
			{
				return i.fColor;
			}

			ENDCG
		}
	}
}
