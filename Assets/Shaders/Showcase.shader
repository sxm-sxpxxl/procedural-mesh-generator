Shader "Showcase"
{
	Properties
	{
		_Base("Base", 2D) = "white" {}
		[Toggle]_Invert("Invert", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _Invert;
		uniform sampler2D _Base;
		uniform float4 _Base_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Base = i.uv_texcoord * _Base_ST.xy + _Base_ST.zw;
			float4 tex2DNode2 = tex2D( _Base, uv_Base );
			o.Albedo = (( _Invert )?( ( 1.0 - tex2DNode2 ) ):( tex2DNode2 )).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
