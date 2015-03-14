#version 430 core

layout (binding = 0) uniform sampler2D tex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec3 f_texcoord;

out vec4 color;

void main()
{
	vec4 tcolor = texture(tex, vec2(f_texcoord.x, f_texcoord.y));
	color = vec4(tcolor[0] * f_color[0], tcolor[1] * f_color[1],
				 tcolor[2] * f_color[2], tcolor[3] * f_color[3]);
}
