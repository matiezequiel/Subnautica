
//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
    ADDRESSU = MIRROR;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture texFogMap;
sampler2D fogMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

float time = 0;
float4 CameraPos;

// variable de fogs
float4 ColorFog;
float StartFogDistance;
float EndFogDistance;

// Parametros de la Luz
float4 globalLightPosition; //Posicion de la luz externa
float4 insideShipLightPosition; //Posicion de la luz indoor
float4 eyePosition; //Posicion de la camara

float3 shipAmbientColor; // Color de ambiente para la nave
float3 shipDiffuseColor; //Color RGB de la luz difusa
float3 shipSpecularColor; //Color RGB de la luz especular

float3 goldAmbientColor; // Color de ambiente para el oro
float3 goldDiffuseColor; //Color RGB de la luz difusa
float3 goldSpecularColor; //Color RGB de la luz especular

float3 silverAmbientColor; // Color de ambiente para la plata
float3 silverDiffuseColor; //Color RGB de la luz difusa
float3 silverSpecularColor; //Color RGB de la luz especular

float3 ironAmbientColor; // Color de ambiente para el hierro
float3 ironDiffuseColor; //Color RGB de la luz difusa
float3 ironSpecularColor; //Color RGB de la luz especular

//Input del Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float4 MeshPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};


//Output del Vertex Shader
struct VS_OUTPUT_VERTEX
{
    float4 Position : POSITION0;
    float2 Texture : TEXCOORD0;
    float4 PosView : COLOR0;
    float4 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

float get_fog_amount(float3 viewDirection, float fogStart, float fogRange)
{
    return saturate((length(viewDirection) - fogStart) / fogRange);
}

float4 calculate_fog(VS_OUTPUT_VERTEX input, float4 texel)
{
    float3 viewDirection = CameraPos.xyz - input.WorldPosition.xyz;
    float FogAmount = get_fog_amount(viewDirection, StartFogDistance, (EndFogDistance - StartFogDistance));

    if (input.PosView.z < StartFogDistance)
        return texel;
    else if (input.PosView.z > EndFogDistance)
        return ColorFog;
    else 
        return lerp(texel, ColorFog, FogAmount);
}

/**************************************************************************************/
                                        /* Mar */
/**************************************************************************************/

//Vertex Shader
VS_OUTPUT_VERTEX vs_main_water(VS_INPUT Input)
{
    VS_OUTPUT_VERTEX Output;
    float dx = Input.Position.x;
    float dy = Input.Position.z;
    float freq = sqrt(dx * dx + dy * dy);
    float amp = 30;
    float angle = -time * 3 + freq / 300;
    Input.Position.y += sin(angle) * amp;
    Output.Position = mul(Input.Position, matWorldViewProj);
    Output.Texture = Input.Texcoord;
    Output.PosView = mul(Input.Position, matWorldView);
    Output.WorldPosition = mul(Input.Position, matWorld);
    Output.WorldNormal = mul(Input.Normal, matInverseTransposeWorld).xyz;
    return Output;
}

//Pixel Shader
float4 ps_main_water(VS_OUTPUT_VERTEX input) : COLOR0
{
    float textureScale = 10;
    float2 waterDirection = float2(.003, .003) * time;
    float4 textureColor = tex2D(diffuseMap, input.Texture * textureScale + waterDirection);
    float distance = distance(input.WorldPosition.xz, CameraPos.xz) / 1500;
    textureColor.a = clamp(1 - 1 / distance, .3, .9);
    return calculate_fog(input, textureColor);
}

technique Waves
{
    pass Pass_0
    {
        AlphaBlendEnable = true;
        VertexShader = compile vs_3_0 vs_main_water();
        PixelShader = compile ps_3_0 ps_main_water();
    }
}

/**************************************************************************************/
                                        /* Niebla */
/**************************************************************************************/

float4 calculate_light(VS_OUTPUT_VERTEX input, float4 lightPosition, float3 ambientColor, float3 diffuseColor, float3 specularColor, 
                        float specularK, float ambientK, float diffuseK, float shininess, float4 texel)
{
    /* Pasar normal a World-Space
	Solo queremos rotarla, no trasladarla ni escalarla.
	Por eso usamos matInverseTransposeWorld en vez de matWorld */
    input.WorldNormal = normalize(input.WorldNormal);
    
    float3 lightDirection = normalize(lightPosition - input.WorldPosition);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition);
    float3 halfVector = normalize(lightDirection + viewDirection);

	//Componente Diffuse: N dot L
    float3 NdotL = dot(input.WorldNormal, lightDirection);
    float3 diffuseLight = diffuseK * diffuseColor * max(0.0, NdotL);

	//Componente Specular: (N dot H)^shininess
    float3 NdotH = dot(input.WorldNormal, halfVector);
    float3 specularLight = ((NdotL <= 0.0) ? 0.0 : specularK) * specularColor * pow(max(0.0, NdotH), shininess);

    float4 finalColor = float4(saturate(ambientColor * ambientK + diffuseLight) * texel + specularLight, texel.a);
    return finalColor;
}

VS_OUTPUT_VERTEX vs_main_fog(VS_INPUT input)
{
    VS_OUTPUT_VERTEX output;
    output.Position = mul(input.Position, matWorldViewProj);
    output.Texture = input.Texcoord;
    output.PosView = mul(input.Position, matWorldView);
    output.WorldPosition = mul(input.Position, matWorld);
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;
    return output;
}

VS_OUTPUT_VERTEX vs_main_fog_vegetation(VS_INPUT input)
{
    VS_OUTPUT_VERTEX output;
    
    input.Position.x += sin(time * 0.5) * input.Position.y * 0.1;
    
    output.Position = mul(input.Position, matWorldViewProj);
    output.Texture = input.Texcoord;
    output.PosView = mul(input.Position, matWorldView);
    output.WorldPosition = mul(input.Position, matWorld);
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;
    return output;
}

VS_OUTPUT_VERTEX vs_main_bubble(VS_INPUT input)
{
    VS_OUTPUT_VERTEX output;
    float dx = input.Position.x;
    float dy = input.Position.y;
    float freq = sqrt(dx * dx + dy * dy) / 300;
    float amp = 4;
    float angle = -time * 2 + freq;
    input.Position.z += sin(angle) * amp;
    input.Position.x += amp * cos(angle);
    output.Position = mul(input.Position, matWorldViewProj);
    output.Texture = input.Texcoord;
    output.PosView = mul(input.Position, matWorldView);
    output.WorldPosition = mul(input.Position, matWorld);
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;
    return output;
}

float4 ps_main_fog(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 fvBaseColor = tex2D(fogMap, input.Texture);
    
    if (input.WorldPosition.y > 3550)
        return fvBaseColor;
    else
        return calculate_fog(input, fvBaseColor);
}

//Pixel Shader
float4 ps_main_fog_vegetation(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 texel = tex2D(fogMap, input.Texture);
    
    float3 viewDirection = CameraPos.xyz - input.WorldPosition.xyz;
    float FogAmount = get_fog_amount(viewDirection, StartFogDistance, (EndFogDistance - StartFogDistance));
    
    if (input.PosView.z < StartFogDistance)
        return texel;
    else if (input.PosView.z > EndFogDistance)
        return ColorFog;
    else
    {
        if (texel.a < 0.4)
            discard;
        else
            return lerp(texel, ColorFog, FogAmount);
    }
}

float4 ps_main_fog_bubble(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 texel = tex2D(fogMap, input.Texture);
    float4 fog = calculate_fog(input, texel);
    fog.a = 0.2;
    return fog;
}

technique Fog
{
    pass Pass_0
    {       
        VertexShader = compile vs_3_0 vs_main_fog();
        PixelShader = compile ps_3_0 ps_main_fog();
    }
}

technique FogVegetation
{
    pass Pass_0
    {       
        VertexShader = compile vs_3_0 vs_main_fog_vegetation();
        PixelShader = compile ps_3_0 ps_main_fog_vegetation();
    }
}

technique FogBubble
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_bubble();
        PixelShader = compile ps_3_0 ps_main_fog_bubble();
    }   
}

/**************************************************************************************/
                                    /* Terreno */
/**************************************************************************************/

texture texReflex;
sampler2D reflex = sampler_state
{
    Texture = (texReflex);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Pixel Shader
float4 ps_main_terrain(VS_OUTPUT_VERTEX input) : COLOR0
{
    float3 Nn = normalize(input.WorldNormal);
    float3 Ln = normalize(float3(0, -2, 1));
    float n_dot_l = abs(dot(Nn, Ln));
    float textureScale = 200;
    float4 textureColor = tex2D(diffuseMap, input.Texture * textureScale);
    float3 diffuseColor = 0.4 * float3(0.5, 0.4, 0.2) * n_dot_l;
    textureColor += float4(diffuseColor, 1);    
    float movement = 0.001 * sin(time * 2);
    float4 reflexTexture = tex2D(reflex, (input.Texture + float2(1, 1) * movement) * 50);
    float4 fvBaseColor = textureColor * 0.9 + reflexTexture * 0.4;    
    return calculate_fog(input, fvBaseColor);
}

technique DiffuseMap
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_fog();
        PixelShader = compile ps_3_0 ps_main_terrain();
    }
}

/**************************************************************************************/
                                    /* Sun */
/**************************************************************************************/

//Pixel Shader
float4 ps_sun(VS_OUTPUT_VERTEX Input) : COLOR0
{
    float4 texel = tex2D(diffuseMap, Input.Texture);
    float4 color = distance(Input.Texture.xy, .4);
		
    color = 1 / color;
    if (distance(Input.Texture.xy, .5) > 0.8)
        return texel;
    else
        return lerp(texel, color, 0.0070);
}

technique Sun
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_fog();
        PixelShader = compile ps_3_0 ps_sun();
    }
}

/**************************************************************************************/
                                    /* Ship */
/**************************************************************************************/

//Pixel Shader
float4 ps_main_ship(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 texel = tex2D(diffuseMap, input.Texture);
    float4 light_color = calculate_light(input, globalLightPosition, shipAmbientColor, shipDiffuseColor, shipSpecularColor, 
                                            0.5, 0.3, 0.6, 20, texel);
    if (input.WorldPosition.y < 3550)
        return calculate_fog(input, light_color);
    else
        return light_color;
}

//Pixel Shader Inside Ship
float4 ps_inside_ship(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 texel = tex2D(diffuseMap, input.Texture);
    return calculate_light(input, insideShipLightPosition, shipAmbientColor, shipDiffuseColor, shipSpecularColor,
                            0.1, 0.7, 0.6, 20, texel);
}


technique Ship_Light
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_fog();
        PixelShader = compile ps_3_0 ps_main_ship();
    }
}

technique Inside_Ship_Light
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_fog();
        PixelShader = compile ps_3_0 ps_inside_ship();
    }
}

/**************************************************************************************/
                                    /* Gold */
/**************************************************************************************/
//Pixel Shader from gold
float4 ps_gold_light(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 texel = tex2D(diffuseMap, input.Texture);
    float4 light_color = calculate_light(input, globalLightPosition, goldAmbientColor, goldDiffuseColor, 
                                            goldSpecularColor, 0.75, 0.6, 0.55, 10, texel);
    return calculate_fog(input, light_color);
}


technique Gold
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_fog();
        PixelShader = compile ps_3_0 ps_gold_light();
    }
}

/**************************************************************************************/
                                    /* Silver */
/**************************************************************************************/
//Pixel Shader from silver
float4 ps_silver_light(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 texel = tex2D(diffuseMap, input.Texture);
    float4 light_color = calculate_light(input, globalLightPosition, silverAmbientColor, silverDiffuseColor,
                                            silverSpecularColor, 0.2, 0.45, 0.75, 12.5, texel);
    return calculate_fog(input, light_color);
}


technique Silver
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_fog();
        PixelShader = compile ps_3_0 ps_silver_light();
    }
}

/**************************************************************************************/
                                    /* Iron */
/**************************************************************************************/
//Pixel Shader from iron
float4 ps_iron_light(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 texel = tex2D(diffuseMap, input.Texture);
    float4 light_color = calculate_light(input, globalLightPosition, ironAmbientColor, ironDiffuseColor,
                                            ironSpecularColor, 0.1, 0.35, 0.7, 15, texel);
    return calculate_fog(input, light_color);
}


technique Iron
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_fog();
        PixelShader = compile ps_3_0 ps_iron_light();
    }
}
