/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float3 eyePosition;

struct Light
{
    float3 position;
    float3 color;
};

Light lights[2];

float screenWidth, screenHeight, timer = 0.0;

static const int kernelRadius = 5;
static const int kernelSize = 25;
static const float kernel[kernelSize] =
{
    0.003765,   0.015019,	0.023792,	0.015019,	0.003765,
    0.015019,	0.059912,	0.094907,	0.059912,	0.015019,
    0.023792,	0.094907,	0.150342,	0.094907,	0.023792,
    0.015019,	0.059912,	0.094907,	0.059912,	0.015019,
    0.003765,	0.015019,	0.023792,	0.015019,	0.003765, 
};


//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para full screen quad
texture renderTarget;
sampler2D renderTargetSampler = sampler_state
{
    Texture = (renderTarget);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};










//Input del Vertex Shader
struct VS_INPUT_DEFAULT
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DEFAULT
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinates : TEXCOORD0;
    //Agreso meshPosition
    float3 MeshPosition : TEXCOORD1;
    //Agrego worldPosition y worldNormal
    float3 WorldPosition : TEXCOORD2;
    float3 WorldNormal : TEXCOORD3;
    
};

//Vertex Shader
VS_OUTPUT_DEFAULT VSDefault(VS_INPUT_DEFAULT input)
{
    VS_OUTPUT_DEFAULT output;
    
    //Defino meshPosition, worldPosition y worldNormal
    output.MeshPosition = input.Position.xyz;
    output.WorldPosition = mul(input.Position, matWorld).xyz;
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;
    
    // Enviamos la posicion transformada
    output.Position = mul(input.Position, matWorldViewProj);
    
    /**************************************************************************************/
                            /*Parcial Repaso Hacer esfera a tgcito*/
    /**************************************************************************************/
    
    /*
    float3 centro = float3(0, 0, 0);
    
    float dis = distance(centro, input.Position.xyz);
    
    float4 esfera = float4(input.Position.xyz * abs(sin(timer)) * 10.0 / dis, input.Position.w);
    
    float4 interpolado = lerp(input.Position , esfera, abs(sin(timer)));

    output.Position = mul(interpolado, matWorldViewProj);
    */
    
    // Propagar las normales por la matriz normal
    output.Normal = mul(input.Normal, matInverseTransposeWorld);
    
	// Propagar coordenadas de textura
    output.TextureCoordinates = input.TextureCoordinates;

	return output;
}

//Pixel Shader
float4 PSDefault(VS_OUTPUT_DEFAULT input) : COLOR0
{
    /**************************************************************************************/
                                   /*Parcial A y B Cortar mesh */
    /**************************************************************************************/
    // Aplicar a tgcito, en las esferas no se nota
    /*
    float4 tex = tex2D(diffuseMap, input.TextureCoordinates);
    
    float distancia = distance(input.MeshPosition.xy, float2(0, 0));
    
    if (fmod(distancia, 4) < 2)
        discard;
    return tex;// esto esta mas copado -> lerp(tex, float4(0, 0, 1, 0), abs(sin(timer)));
    */
    
    
    /**************************************************************************************/
                       /*Parcial C Iluminar esfera con difusa y ambiente */
    /**************************************************************************************/
    /*
    input.WorldNormal = normalize(input.WorldNormal);
    
    float4 texelColor = tex2D(diffuseMap, input.TextureCoordinates);
    float4 finalColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
    
    for (int i = 0; i < 2; i++)
    {
        float3 lightDirection = normalize(lights[i].position - input.WorldPosition);
        float3 NdotL = dot(input.WorldNormal, lightDirection);
        float3 diffuseLight = 4.0f * lights[i].color * max(0.0, NdotL);
        
        finalColor.rgb += float3(saturate(0.1f + diffuseLight) * texelColor);
    }
    
    return finalColor;
    */
    
    
    /**************************************************************************************/
                              /*Parcial D Luz disfusa con cell shading*/
    /**************************************************************************************/
    /*
    input.WorldNormal = normalize(input.WorldNormal);
    
    float4 texel = tex2D(diffuseMap, input.TextureCoordinates);
    
    float3 direccion_luz = normalize(lights[1].position - input.WorldPosition);

    float3 NdotL = dot(input.WorldNormal, direccion_luz);
    //Componente difusa 1
    float3 diffuseLight = 1.0 * lights[1].color * max(0.0, NdotL);
    
    float4 color = float4(0, 0, 0, 0);
    color.rgb = float3(saturate(diffuseLight) * texel);
    color.rgb = round(color.rgb * 3) / 3;
    
    return color;
    */
    
    
    /**************************************************************************************/
                                  /*Recuperatorio Luz especular*/
    /**************************************************************************************/
    /*
    input.WorldNormal = normalize(input.WorldNormal);

    float3 lightDirection = normalize(lights[0].position - input.WorldPosition);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition);
    float3 halfVector = normalize(lightDirection + viewDirection);

    float4 texelColor = tex2D(diffuseMap, input.TextureCoordinates);
    
    float KSpecular = 1;
    float shininess = 16;
    
	//Componente Specular: 
    float3 NdotL = dot(input.WorldNormal, lightDirection);
    float3 NdotH = dot(input.WorldNormal, halfVector);
    float3 specularLight = ((NdotL <= 0.0) ? 0.0 : KSpecular) * lights[0].color * pow(max(0.0, NdotH), shininess);

    float4 finalColor = float4(texelColor + specularLight, texelColor.a);
    return finalColor;
    */
    
    return tex2D(diffuseMap, input.TextureCoordinates);
    
}

//Input del Vertex Shader
struct VS_INPUT_POSTPROCESS
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_POSTPROCESS
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

//Vertex Shader
VS_OUTPUT_POSTPROCESS VSPostProcess(VS_INPUT_POSTPROCESS input)
{
    VS_OUTPUT_POSTPROCESS output;

	// Propagamos la posicion, ya que esta en espacio de pantalla
    output.Position = input.Position;

	// Propagar coordenadas de textura
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

//Pixel Shader
float4 PSPostProcess(VS_OUTPUT_DEFAULT input) : COLOR0
{
    /**************************************************************************************/
                                  /*Parcial Repaso Monitor CRT*/
    /**************************************************************************************/
    /*
    float4 tex = tex2D(renderTargetSampler, input.TextureCoordinates);
    
    float x = round(input.TextureCoordinates.x * screenWidth);
    float y = round(input.TextureCoordinates.y * screenHeight);
   
    float borde = distance(input.TextureCoordinates, float2(0.5, 0.5));
    
    float cuadrillaX = fmod(x, 6);
    float cuadrillaY = fmod(y, 6);
    
    if (cuadrillaX < 2 || cuadrillaY < 2)
        tex = float4(0, 0, 0, 0);
        
    float4 verde = lerp(tex, float4(0, 1, 0, 0), 0.45);
    
    return lerp(verde, float4(0, 0, 0, 0), borde * 1.5);
    */
    
   
    
    /**************************************************************************************/
                             /*Parcial B Chromatic Aberration*/
    /**************************************************************************************/
    /*
    float4 color;
    
    color.r = tex2D(renderTargetSampler, float2(input.TextureCoordinates.x + 5 / screenWidth, input.TextureCoordinates.y)).r;
    color.g = tex2D(renderTargetSampler, input.TextureCoordinates).g;
    color.b = tex2D(renderTargetSampler, float2(input.TextureCoordinates.x - 5 / screenWidth, input.TextureCoordinates.y)).b;
    color.a = 1;
    
    return color;
     */
    
    
    
    /**************************************************************************************/
                                   /*Parcial C Elipse*/
    /**************************************************************************************/
    /*
    float4 texx = tex2D(renderTargetSampler, input.TextureCoordinates);
    float distancia = distance(input.TextureCoordinates, float2(0.5f, 0.5f));
    
    float4 color = tex2D(renderTargetSampler, input.TextureCoordinates.xy);
    float gris = (color.r + color.g + color.b) / 3;
    color.r += (gris - color.r);
    color.g += (gris - color.g);
    color.b += (gris - color.b);
    
    if(distancia > 0.5f)
        return color;
    else
        return texx;
    */
    
    
    
    /**************************************************************************************/
                                  /*Parcial D RGB en tres*/
    /**************************************************************************************/
    /*
    float4 rojo = float4(1, 0, 0, 0);
    float4 verde = float4(0, 1, 0, 0);
    float4 azul = float4(0, 0, 1, 0);
    
    float4 tex = tex2D(renderTargetSampler, input.TextureCoordinates);
    
    //Ya que las coordenadas de textura van de 0 a 1, cada tercio de pantalla debe ir de su respectivo color
    
    if (input.TextureCoordinates.x < 0.333f)
        tex = lerp(tex, rojo, 0.7f);
    
    if (input.TextureCoordinates.x >= 0.333f && input.TextureCoordinates.x <= 0.666f)
        tex = lerp(tex, verde, 0.7f);
    
    if (input.TextureCoordinates.x > 0.666f)
        tex = lerp(tex, azul, 0.7f);
    
    return tex;
    */
    
    
    
    /**************************************************************************************/
                       /*Recuperatorio Pantalla en 4 partes diagonales*/
    /**************************************************************************************/
    /*
    float4 tex = tex2D(renderTargetSampler, input.TextureCoordinates);
    float condicion = (input.TextureCoordinates.x > input.TextureCoordinates.y) && (-input.TextureCoordinates.x + 1 > input.TextureCoordinates.y)
                            || (input.TextureCoordinates.x < input.TextureCoordinates.y) && (-input.TextureCoordinates.x + 1 < input.TextureCoordinates.y);
    
    float3 color = abs(float3(1, 1, 1) * condicion - tex.rgb);
    
    
    return float4(color, tex.a); 
    */

    
    
    //----------------------------------------- PostProcesado pixelado    
    /*
    float4 t = float4(0, 0, 0, 0);
    float radio = 5;
    
    float x = round(input.TextureCoordinates.x * screenWidth);
    float y = round(input.TextureCoordinates.y * screenHeight);
    
    float cuadrillaX = fmod(x, 10);
    float cuadrillaY = fmod(y, 10);
    
    x -= cuadrillaX;
    y -= cuadrillaY;
    
    for (int i = 0; i < radio; i++)
        for (int j = 0; j < radio; j++)
            t += tex2D(renderTargetSampler, float2( (x + i) / screenWidth, (j + y) / screenHeight) );
        
    t /= radio * radio;
    
    return t;
    */
    
    return tex2D(renderTargetSampler, input.TextureCoordinates);
}






technique Default
{

	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSDefault();
		PixelShader = compile ps_3_0 PSDefault();
	}

}



technique PostProcess
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSPostProcess();
        PixelShader = compile ps_3_0 PSPostProcess();
    }
}