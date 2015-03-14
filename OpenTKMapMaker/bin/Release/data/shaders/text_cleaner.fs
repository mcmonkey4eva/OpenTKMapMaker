/*
uniform sampler2D tex;

void main()
{
	vec4 color = texture2D(tex,gl_TexCoord[0].st);
	gl_FragColor = vec4(1, 1, 1, (color[0] + color[1] + color[2]) / 3);
}
*/
#version 430 core

layout (binding = 0) uniform sampler2DArray tex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec3 f_texcoord;

out vec4 color;

void main()
{
	vec4 tcolor = texture(tex, f_texcoord);
	color = vec4(f_color[0], f_color[1], f_color[2],
	((tcolor[0] + tcolor[1] + tcolor[2]) / 3) * f_color[3]);
}
