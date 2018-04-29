Shader "Lines / Colored Blended" {
	SubShader{
		Tags{ "RenderType" = "Opaque" 
			  "Queue" = "Background" }
		Pass{
			ZWrite On
			ZTest LEqual
			Cull Off
			Fog{ Mode Off }
			BindChannels{
				Bind "vertex", vertex Bind "color", color
			}
		}
	}
}