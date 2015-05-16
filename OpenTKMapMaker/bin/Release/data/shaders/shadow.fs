#version 430 core

layout (binding = 0) uniform sampler2D s;

layout (location = 0) in vec4 f_pos;
layout (location = 1) in vec2 f_texcoord;

out vec4 color;

void main()
{
	vec4 col = texture(s, vec2(f_texcoord[0], f_texcoord[1]));
	if (col.w < 0.9)
	{
		discard;
	}
	color = vec4(f_pos.z / f_pos.w);
}
