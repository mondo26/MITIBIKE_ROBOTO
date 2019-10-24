Shader "MyShader/Block" {
	Properties
	{
		_BaseColor("Base Color", Color) = (1,1,1,1)
	}


	SubShader
	{
		Tags { "RenderType" = "Transparent"			// レンダリングタイプを透過に		
				"Queue" = "Transparent"				// オブジェクトのレンダリング順を設定
			 }

		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha				// アルファブレンディングするようにする
		ZWrite Off									// デプスバッファの頂点情報を書き込まないようにする

		Stencil {
			Ref 0			// リファレンス値
			Comp Equal
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

		Stencil {
			Ref 1			// リファレンス値
			Comp Equal		// ピクセルのリファレンス値がバッファの値と等しい場合のみレンダリングします。
		}

		CGPROGRAM
		#pragma surface surf Standard addshadow		// 影を追加

		struct Input {
		float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = fixed4(0.392, 0.25, 0.25, 1.0);
		}

		ENDCG
	}
}