Shader "Unlit/InfiniteResShader_DP"
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
			#pragma optionNV(fastmath off)
			#pragma optionNV(fastprecision off)
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
			#pragma exclude_renderers gles
			//#pragma enable_d3d11_debug_symbols
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
		
			float2		ds_ka;
			float2		ds_kb;
			float2		ds_scale;
			float2		ds_centera;
			float2		ds_centerb;

			int			maxIter;
			int			mandel;
			int			fadeEdges;
			float		colorOffset;
			float		colorDensity;
			float4		colors[50];
			int			numColors;

			// Emulation based on Fortran-90 double-single package. See http://crd.lbl.gov/~dhbailey/mpdist/
			// Add: res = ds_add(a, b) => res = a + b
			float2 ds_add(float2 dsa, float2 dsb)
			{
				float2 dsc;
				float t1, t2, e;

				t1 = dsa.x + dsb.x;
				e = t1 - dsa.x;
				t2 = ((dsb.x - e) + (dsa.x - (t1 - e))) + dsa.y + dsb.y;

				dsc.x = t1 + t2;
				dsc.y = t2 - (dsc.x - t1);
				return dsc;
			}

			// Substract: res = ds_sub(a, b) => res = a - b
			float2 ds_sub(float2 dsa, float2 dsb)
			{
				float2 dsc;
				float e, t1, t2;

				t1 = dsa.x - dsb.x;
				e = t1 - dsa.x;
				t2 = ((-dsb.x - e) + (dsa.x - (t1 - e))) + dsa.y - dsb.y;

				dsc.x = t1 + t2;
				dsc.y = t2 - (dsc.x - t1);
				return dsc;
			}

			// Compare: res = -1 if a < b
			//              = 0 if a == b
			//              = 1 if a > b
			float ds_compare(float2 dsa, float2 dsb)
			{
				if (dsa.x < dsb.x) return -1.;
				else if (dsa.x == dsb.x)
				{
					if (dsa.y < dsb.y) return -1.;
					else if (dsa.y == dsb.y) return 0.;
					else return 1.;
				}
				else return 1.;
			}

			// Multiply: res = ds_mul(a, b) => res = a * b
			float2 ds_mul(float2 dsa, float2 dsb)
			{
				float2 dsc;
				float c11, c21, c2, e, t1, t2;
				float a1, a2, b1, b2, cona, conb, split = 8193.;

				cona = dsa.x * split;
				conb = dsb.x * split;
				a1 = cona - (cona - dsa.x);
				b1 = conb - (conb - dsb.x);
				a2 = dsa.x - a1;
				b2 = dsb.x - b1;

				c11 = dsa.x * dsb.x;
				c21 = a2 * b2 + (a2 * b1 + (a1 * b2 + (a1 * b1 - c11)));

				c2 = dsa.x * dsb.y + dsa.y * dsb.x;

				t1 = c11 + c2;
				e = t1 - c11;
				t2 = dsa.y * dsb.y + ((c2 - e) + (c11 - (t1 - e))) + c21;

				dsc.x = t1 + t2;
				dsc.y = t2 - (dsc.x - t1);

				return dsc;
			}

			// Create double-single number from float
			float2 ds_set(float a)
			{
				float2 z;
				z.x = a;
				z.y = 0;
				return z;
			}

			float getIterations(float2 ds_ca, float2 ds_cb)
			{
				float2 ds_za = ds_ca;
				float2 ds_zb = ds_cb;

				if (mandel != 1)
				{
					ds_ca = ds_ka;
					ds_cb = ds_kb;
				}

				float2 two = ds_set(2.0);
				float2 four = ds_set(4.0);

				int iter = 0;
				for (int i = 1; i < 50000; i++)
				{
					float2 temp = ds_za;
					ds_za = ds_add(ds_sub(ds_mul(ds_za, ds_za), ds_mul(ds_zb, ds_zb)), ds_ca);
					ds_zb = ds_add(ds_mul(ds_mul(ds_zb, temp), two), ds_cb);

					if (ds_compare(ds_add(ds_mul(ds_za, ds_za), ds_mul(ds_zb, ds_zb)), four) > 0)
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
					float2 temp = ds_za;
					ds_za = ds_add(ds_sub(ds_mul(ds_za, ds_za), ds_mul(ds_zb, ds_zb)), ds_ca);
					ds_zb = ds_add(ds_mul(ds_mul(ds_zb, temp), two), ds_cb);

					iter++;
				}

				return colorOffset + colorDensity * float(numColors) * (float(iter) + 1.0 - log2(log(ds_za.x * ds_za.x + ds_zb.x * ds_zb.x) / 2.0));
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

                //this doesn't do anything because 1.1 (hypothetical centera) + 0.00000000000000001 (hypothetical scaled point) is still 1.1 with floating-point precision limits
				float2 ds_ca = ds_add(ds_centera, ds_mul(ds_scale, ds_set(i.vertex.x)));
				float2 ds_cb = ds_add(ds_centerb, ds_mul(ds_scale, ds_set(i.vertex.y)));

				o.fractalPoint = float4(ds_ca, ds_cb);
				o.rawVertex = i.vertex;

				return o;
			}

			fixed4 frag(vOutput i) : SV_Target
			{
				float iter = getIterations(i.fractalPoint.xy, i.fractalPoint.zw);
			
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
