Shader "Sample/Custom/Bamp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalTex("NormapTex", 2D) = "white" {}
        _AmbientTex("AmbientTex", CUBE) = "white" {}
        _Reflectivity("Reflectivity", Range(0.01, 1)) = 1.0 // 反射率
        _Color("Color", Color) = (1, 1, 1, 1)
        _Specular("Specular", Color) = (1, 1, 1, 1) // ハイライトの色(ライトの色でもOK)
        _Shininess ("Shininess", Range(0.01, 1)) = 0.4 // ハイライトの鋭さ
        _ReflectRatio ("ReflectRatio", Range(0.01, 1)) = 0.5 // 映り込み率
        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactor("SrcFactor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactor("DstFactor", Float) = 10
        [Enum(UnityEngine.Rendering.BlendOp)]
        _BlendOp("BlendOp", Float) = 0
        [Enum(Off, 0, On, 1)]
        _ZWrite("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)]
        _Cull("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_BlendOp]
        ZWrite [_ZWrite]
        Cull [_Cull]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 lightVector : TEXCOORD2;
                float3 reverseViewVector : TECROOD3;
            };

            sampler2D _MainTex;
            sampler2D _NormalTex;
            samplerCUBE _AmbientTex;
            fixed4 _Color;
            fixed4 _Specular;
            float _Reflectivity;
            float _Shininess;
            float _ReflectRatio;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                // 座標をワールド座標系に変換する
                o.vertex = UnityObjectToClipPos(v.vertex);
                // offsetとtilingを反映したUV値を取得する
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // 法線をワールド空間上の物に変換
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                // カメラへのオブジェクト空間ベクトル(光のベクトル)を取得
                o.lightVector = ObjSpaceLightDir(v.vertex);
                // 光の逆ベクトルを取得
                o.reverseViewVector = ObjSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // 全ての法線を正規化する
                float3 normal = normalize(i.worldNormal);
                // テクスチャから法線情報を取得する
                float3 normal2 = UnpackNormal(tex2D(_NormalTex, i.uv));
                // 面法線とテクスチャの法線をブレンドする
                normal = BlendNormals(normal, normal2);

                float3 lightVector = normalize(i.lightVector);
                float3 reflectVector = normalize(reflect(-i.reverseViewVector, normal));
                // 陰の掛かり方を算出
                float shade = saturate(dot(normal, _WorldSpaceLightPos0.xyz));
                shade = shade * 0.5 + 0.5;
                shade = shade * shade;

                // スペキュラを算出
                float reflection = saturate(dot(lightVector, reflectVector));
                // 鏡面反射の強さを算出
                float shininess = pow(500, _Shininess);
                // 写り込む色をキューブテクスチャから取得
                fixed3 mirrorColor = texCUBE(_AmbientTex, reflectVector);
                // スペキュラの色を算出
                fixed3 reflectionColor = _Specular.rgb * pow(reflection, shininess) * _Reflectivity;

                // ライティング
                col.rgb =_LightColor0 * col.rgb * shade;
                // 映り込みを反映
                col.rgb += mirrorColor * _ReflectRatio;
                // スペキュラの色を反映
                col.rgb += reflectionColor;
                return col;
            }
            ENDCG
        }
    }
}
