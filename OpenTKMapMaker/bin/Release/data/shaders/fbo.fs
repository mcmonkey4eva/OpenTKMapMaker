#version 430 core

layout (binding = 0) uniform sampler2D s;

layout (location = 0) in vec4 f_position;
layout (location = 1) in vec3 f_normal;
layout (location = 2) in vec2 f_texcoord;

layout (location = 0) out vec4 color;
layout (location = 1) out vec4 position;
layout (location = 2) out vec4 normal;

void main()
{
	color = texture(s, vec2(f_texcoord[0], f_texcoord[1]));
	position = f_position;
	normal = vec4(f_normal, 1.0);
}
