/*
void main()
{
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = ftransform();
}
*/
#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoord;
layout (location = 3) in vec4 color;

layout (location = 1) uniform mat4 projection;
layout (location = 2) uniform mat4 model_matrix;
layout (location = 3) uniform vec3 v_color;

layout (location = 0) out vec4 f_color;
layout (location = 1) out vec3 f_texcoord;

void main()
{
	f_color = vec4(color[0] * v_color[0], color[1] * v_color[1], color[2] * v_color[2], color[3]);
	f_texcoord = texcoord;
	gl_Position = projection * model_matrix * vec4(position, 1.0);
}
