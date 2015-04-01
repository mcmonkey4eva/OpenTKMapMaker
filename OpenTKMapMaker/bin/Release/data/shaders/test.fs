#version 430 core

layout (binding = 0) uniform sampler2D colortex;
layout (binding = 1) uniform sampler2D positiontex;
layout (binding = 2) uniform sampler2D normaltex;
layout (binding = 3) uniform sampler2D depthtex;
layout (binding = 4) uniform sampler2D shtex;
layout (binding = 5) uniform sampler2D renderhinttex;

layout (location = 0) in vec2 f_texcoord;

layout (location = 3) uniform mat4 shadow_matrix;
layout (location = 4) uniform vec3 light_pos = vec3(5.0, 5.0, 5.0);
layout (location = 5) uniform vec3 ambient = vec3(0.05, 0.05, 0.05);

out vec4 color;

vec4 regularize(vec4 input) // TODO: Is this working the best it can?
{
	if (input.x <= 1.0 && input.y <= 1.0 && input.z <= 1.0)
	{
		return input;
	}
	return vec4(input.xyz / ((input.x >= input.y && input.x >= input.z) ? input.x: ((input.y >= input.z) ? input.y: input.z)), input.w);
}

void main()
{
	vec4 shadow_light_color = texture(shtex, f_texcoord);
	vec4 light_color = regularize(vec4(ambient, 0.0) + shadow_light_color);
	if (light_color.w > 1.0)
	{
		light_color.w = 1.0;
	}
	color = light_color;
}
