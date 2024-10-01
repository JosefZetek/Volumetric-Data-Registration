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

        fixed3 GrayscaleColormap(float x) {
            return fixed3(x, x, x);
        }

        fixed3 ViridisColormap(float x) {

            fixed3 colormap[6] = {
                fixed3(0.267, 0.004, 0.329), // Dark purple
                fixed3(0.282, 0.140, 0.459), // Blue
                fixed3(0.253, 0.265, 0.529), // Transition blue-green
                fixed3(0.163, 0.471, 0.558), // Greenish
                fixed3(0.478, 0.821, 0.318), // Green-yellow
                fixed3(0.993, 0.906, 0.144)  // Yellow
            };

            float idx = x * 5.0;
            int idx1 = int(floor(idx));
            int idx2 = min(idx1 + 1, 5);

            float t = frac(idx);

            return lerp(colormap[idx1], colormap[idx2], t);
        }

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o) {
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            float value = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            fixed3 rgbViridis = GrayscaleColormap(value);

            fixed4 c = fixed4(rgbViridis.x, rgbViridis.y, rgbViridis.z, 1);
            //c *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}