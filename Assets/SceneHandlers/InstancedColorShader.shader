Shader "Custom/InstancedColorShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags {"RenderType"="Opaque"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert instancing:setup
        #pragma instancing_options procedural:setup
        #pragma target 3.5

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o) {
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            float value = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            fixed4 c = fixed4(value, value, value, 1);
            //c *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}