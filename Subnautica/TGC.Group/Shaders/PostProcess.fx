/*
* Shaders para efectos de Post Procesadosss
*/

/**************************************************************************************/
                                    /* DEFAULT */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DEFAULT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DEFAULT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
};

//Vertex Shader
VS_OUTPUT_DEFAULT vs_default(VS_INPUT_DEFAULT Input)
{
    VS_OUTPUT_DEFAULT Output;

	//Proyectar posicion
    Output.Position = float4(Input.Position.xy, 0, 1);

	//Las Texcoord quedan igual
    Output.Texcoord = Input.Texcoord;

    return (Output);
}

//Textura del Render target 2D
texture render_target2D;
sampler RenderTarget = sampler_state
{
    Texture = (render_target2D);
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

//Input del Pixel Shader
struct PS_INPUT_DEFAULT
{
    float2 Texcoord : TEXCOORD0;
};

//Pixel Shader
float4 ps_default(PS_INPUT_DEFAULT Input) : COLOR0
{
    float4 color = tex2D(RenderTarget, Input.Texcoord);
    return color;
}

technique DefaultTechnique
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_default();
        PixelShader = compile ps_3_0 ps_default();
    }
}

//Textura casco
texture texture_diving_helmet;
sampler DivingHelmet2D = sampler_state
{
    Texture = (texture_diving_helmet);
};

float time;

/**************************************************************************************/
                                /* CASCO DE BUCEO */
/**************************************************************************************/

//Pixel Shader de Oscurecer
float4 ps_diving_helmet(PS_INPUT_DEFAULT Input) : COLOR0
{
    float4 renderTarget = tex2D(RenderTarget, Input.Texcoord);
    float4 divingHelmet = tex2D(DivingHelmet2D, Input.Texcoord);
    return divingHelmet.a < 0.5 ? renderTarget : divingHelmet;
}

technique DivingHelmet
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_default();
        PixelShader = compile ps_3_0 ps_diving_helmet();
    }
}

/**************************************************************************************/
                                    /* OSCURECER */
/**************************************************************************************/

//Pixel Shader de Oscurecer
float4 ps_darken(PS_INPUT_DEFAULT Input) : COLOR0
{
    float4 renderTarget = tex2D(RenderTarget, Input.Texcoord);
    float4 dark = -abs(sin(0.4 * time));
    float4 divingHelmet = tex2D(DivingHelmet2D, Input.Texcoord);
    
    return divingHelmet.a < 1 ? renderTarget + dark : divingHelmet + dark;
}

technique Darken
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_default();
        PixelShader = compile ps_3_0 ps_darken();
    }
}

/**************************************************************************************/
                                    /* ALARMA */
/**************************************************************************************/

float alarmScaleFactor;

//Textura alarma
texture texture_alarm;
sampler sampler_alarm = sampler_state
{
    Texture = (texture_alarm);
};

//Pixel Shader de Alarma
float4 ps_alarm(PS_INPUT_DEFAULT Input) : COLOR0
{
    float4 renderTarget = tex2D(RenderTarget, Input.Texcoord);
    float4 alarm = tex2D(sampler_alarm, Input.Texcoord) * alarmScaleFactor;
    float4 divingHelmet = tex2D(DivingHelmet2D, Input.Texcoord);
    
    return divingHelmet.a < 0.5 ? renderTarget + alarm : divingHelmet + alarm;
}

technique AlarmTechnique
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_default();
        PixelShader = compile ps_3_0 ps_alarm();
    }
}


/**************************************************************************************/
                                    /* PDA */
/**************************************************************************************/

//Textura casco
texture texture_PDA;
sampler PDA2D = sampler_state
{
    Texture = (texture_PDA);
};

float4 Color;

float4 ps_PDA(PS_INPUT_DEFAULT Input) : COLOR0
{
    float4 renderTarget = tex2D(RenderTarget, Input.Texcoord);
    float4 pda = tex2D(PDA2D, Input.Texcoord);
    
    if ((pda.r + pda.g + pda.b) / 3 >= 1)
    {
        pda.a = 0.7;
        return pda * 0.2 + Color * 0.2 + renderTarget * 0.6;
    }
    else if (pda.a < 0.3)
        return renderTarget;
    else
        return pda;
    
}

technique PDA
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_default();
        PixelShader = compile ps_3_0 ps_PDA();
    }
}
