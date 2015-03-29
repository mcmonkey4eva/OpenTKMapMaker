#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texcoord;

layout (location = 1) uniform mat4 projection;
layout (location = 2) uniform mat4 model_matrix;

layout (location = 0) out vec4 f_pos;

void main()
{
	vec4 positiony = projection * model_matrix * vec4(position, 1.0);
	f_pos = positiony;
	gl_Position = positiony;
}
