Shader "MyShader/Robot" {

	Properties
	{
		_BaseColor("Base Color", Color) = (1,1,1,1)
	}

	SubShader{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}

		Stencil {
			Ref 1			// リファレンス値
			Comp Always		// 常にステンシルテストをパスさせます。
			Pass Replace    // リファレンス値をバッファに書き込みます。
		}

		CGPROGRAM
		#pragma surface surf Standard addshadow		// 影を追加

		struct Input {
			float2 uv_MainTex;
		};

		fixed4 _BaseColor;
		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = _BaseColor.rgb;
		}

		ENDCG

	}

}