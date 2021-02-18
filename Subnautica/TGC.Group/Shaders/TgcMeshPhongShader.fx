/*
* Shader generico para TgcMesh con iluminacion dinamica por pixel (Phong Shading)
* Hay 3 Techniques, una para cada MeshRenderType:
*	- VERTEX_COLOR
*	- DIFFUSE_MAP
*	- DIFFUSE_MAP_AND_LIGHTMAP
*/

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

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
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Lightmap
texture texLightMap;
sampler2D lightMap = sampler_state
{
	Texture = (texLightMap);
};

//Parametros de la Luz
float3 ambientColor; //Color RGB para Ambient de la luz
float3 diffuseColor; //Color RGB para Ambient de la luz
float3 specularColor; //Color RGB para Ambient de la luz
float specularExp; //Exponente de specular
float4 lightPosition; //Posicion de la luz
float4 eyePosition; //Posicion de la camara

float3 shipAmbientColor;
float shipKSpecular;

float3 goldAmbientColor;
float goldKSpecular;

float3 silverAmbientColor;
float silverKSpecular;

float3 ironAmbientColor;
float ironKSpecular;

/**************************************************************************************/
/* VERTEX_COLOR */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_VERTEX_COLOR
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
};

//Output del Vertex Shader
struct VS_OUTPUT_VERTEX_COLOR
{
	float4 Position : POSITION0;
	float4 Color : COLOR;
	float3 WorldNormal : TEXCOORD0;
	float3 LightVec	: TEXCOORD1;
	float3 HalfAngleVec	: TEXCOORD2;
};

//Vertex Shader
VS_OUTPUT_VERTEX_COLOR vs_VertexColor(VS_INPUT_VERTEX_COLOR input)
{
	VS_OUTPUT_VERTEX_COLOR output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Enviar color directamente
	output.Color = input.Color;

	/* Pasar normal a World-Space
	Solo queremos rotarla, no trasladarla ni escalarla.
	Por eso usamos matInverseTransposeWorld en vez de matWorld */
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
	float3 worldPosition = mul(input.Position, matWorld);
	output.LightVec = lightPosition.xyz - worldPosition;

	//ViewVec (V): vector que va desde el vertice hacia la camara.
	float3 viewVector = eyePosition.xyz - worldPosition;

	//HalfAngleVec (H): vector de reflexion simplificado de Phong-Blinn (H = |V + L|). Usado en Specular
	output.HalfAngleVec = viewVector + output.LightVec;

	return output;
}

//Input del Pixel Shader
struct PS_INPUT_VERTEX_COLOR
{
	float4 Color : COLOR0;
	float3 WorldNormal : TEXCOORD0;
	float3 LightVec	: TEXCOORD1;
	float3 HalfAngleVec	: TEXCOORD2;
};

//Pixel Shader
float4 ps_VertexColor(PS_INPUT_VERTEX_COLOR input) : COLOR0
{
    input.WorldNormal = normalize(input.WorldNormal);

    float3 lightDirection = normalize(input.LightVec);
    float3 viewDirection = normalize(input.WorldNormal);
    float3 halfVector = normalize(input.HalfAngleVec);
	
	//Componente Diffuse: N dot L
    float3 NdotL = dot(input.WorldNormal, lightDirection);
    float3 diffuseLight = 0.75 * diffuseColor * max(0.0, NdotL);

	//Componente Specular: (N dot H)^shininess
    float3 NdotH = dot(input.WorldNormal, halfVector);
    float3 specularLight = NdotL <= 0.0 ? float3(0.0, 0.0, 0.0) : 0.5 * specularColor * pow(max(0.0, NdotH), 10);
	
    float4 finalColor = float4(saturate(ambientColor + diffuseLight) * input.Color.rgb + specularLight, input.Color.a);
    return finalColor;
}
/*
* Technique VERTEX_COLOR
*/
technique VERTEX_COLOR
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_VertexColor();
		PixelShader = compile ps_3_0 ps_VertexColor();
	}
}

/**************************************************************************************/
/* DIFFUSE_MAP */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DIFFUSE_MAP
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DIFFUSE_MAP
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 LightVec	: TEXCOORD2;
	float3 HalfAngleVec	: TEXCOORD3;
};

//Vertex Shader
VS_OUTPUT_DIFFUSE_MAP vs_DiffuseMap(VS_INPUT_DIFFUSE_MAP input)
{
	VS_OUTPUT_DIFFUSE_MAP output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Enviar Texcoord directamente
	output.Texcoord = input.Texcoord;

	/* Pasar normal a World-Space
	Solo queremos rotarla, no trasladarla ni escalarla.
	Por eso usamos matInverseTransposeWorld en vez de matWorld */
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
	float3 worldPosition = mul(input.Position, matWorld);
	output.LightVec = lightPosition.xyz - worldPosition;

	//ViewVec (V): vector que va desde el vertice hacia la camara.
	float3 viewVector = eyePosition.xyz - worldPosition;

	//HalfAngleVec (H): vector de reflexion simplificado de Phong-Blinn (H = |V + L|). Usado en Specular
	output.HalfAngleVec = viewVector + output.LightVec;

	return output;
}

//Input del Pixel Shader
struct PS_DIFFUSE_MAP
{
	float2 Texcoord : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 LightVec	: TEXCOORD2;
	float3 HalfAngleVec	: TEXCOORD3;
};

//Pixel Shader
float4 ps_ShipLight(PS_DIFFUSE_MAP input) : COLOR0
{
    input.WorldNormal = normalize(input.WorldNormal);

    float3 lightDirection = normalize(input.LightVec);
    float3 viewDirection = normalize(input.WorldNormal);
    float3 halfVector = normalize(input.HalfAngleVec);

	// Obtener texel de la textura
    float4 texelColor = tex2D(diffuseMap, input.Texcoord);
	
	//Componente Diffuse: N dot L
    float3 NdotL = dot(input.WorldNormal, lightDirection);
    float3 diffuseLight = 0.4 * diffuseColor * max(0.0, NdotL);

	//Componente Specular: (N dot H)^shininess
    float3 NdotH = dot(input.WorldNormal, halfVector);
    float3 specularLight = ((NdotL <= 0.0) ? 0.0 : 0.65) * specularColor * pow(max(0.0, NdotH), specularExp);

    float4 finalColor = float4(saturate(ambientColor * 0.5 + diffuseLight) * texelColor + specularLight, texelColor.a);
    return finalColor;
}

/*
* Technique Ship_Light
*/
technique Ship_Light
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_DiffuseMap();
        PixelShader = compile ps_3_0 ps_ShipLight();
    }
}