float4x4 World;
float4x4 View;
float4x4 Projection;
float4 AmbientColor;
float4 LightDirection;
texture Texture;

extern bool FogEnabled;
extern float FogNear;
extern float FogFar;

extern float3 playerPos;

sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
};
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    //float Occlusion : COLOR0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Normal : TEXCOORD2;
    float4 Color : COLOR0;
    float2 TextureCordinate : TEXCOORD0;
    float4 Pos2 : TEXCOORD1;
    //float Occlusion : COLOR1;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Color = float4(0.9, 0.9, 0.9, 0.9);
    output.TextureCordinate = input.TexCoord;
    //output.Occlusion = input.Occlusion;
        
    float4 n = input.Normal;
    n[3] = 0; // this prevents translation.
    output.Normal = mul(n, World); //not input.Normal, because that wouldn't be rotated.
        
    output.Pos2 = worldPosition;
    return output;
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    
    //float uvX = frac(input.TextureCordinate.x * 2);
    //float uvY = frac(input.TextureCordinate.y * 1);
    
    float4 color = tex2D(TextureSampler, input.TextureCordinate);
    float3 fogColor = float3(0.529, 0.808, 0.922);
    float distance = length(float4(playerPos, 0) - input.Pos2);
    
    float d = dot(normalize(float4(0.5, 1, 0.3, 0)), normalize(input.Normal));
    d /= 4;
    d += 0.75;
    
    color *= d;
    
    //color *= (((1 - input.Occlusion) / 2) + 0.5);
    
    if (FogEnabled)
    {
        float f = lerp(0, 1, (distance - FogNear) / (FogFar - FogNear));
        f = clamp(f, 0, 1);
        f = pow(f, 2);
        color = lerp(color, float4(fogColor, 0), f);
    }

    return color;

}
technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}