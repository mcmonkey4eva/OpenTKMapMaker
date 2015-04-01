#version 430 core

layout (binding = 0) uniform sampler2D s;

layout (location = 3) uniform float specular_strength = 1.0;
layout (location = 4) uniform float specular_power = 200.0 / 1000.0f;

layout (location = 0) in vec4 f_position;
layout (location = 1) in vec3 f_normal;
layout (location = 2) in vec2 f_texcoord;

layout (location = 0) out vec4 color;
layout (location = 1) out vec4 position;
layout (location = 2) out vec4 normal;
layout (location = 3) out vec4 renderhint;

void main()
{
	color = texture(s, vec2(f_texcoord[0], f_texcoord[1]));
	position = f_position;
	normal = vec4(f_normal, 1.0);
	renderhint = vec4(specular_strength, specular_power, 0.0, 1.0); // TODO: 2 Additional renderhints - we have the slots, why not
}
