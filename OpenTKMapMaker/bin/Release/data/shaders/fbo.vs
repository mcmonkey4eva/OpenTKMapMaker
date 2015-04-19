#version 430 core

layout (location = 0) in vec4 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texcoords;
layout (location = 3) in vec4 color;

layout (location = 0) out vec4 f_position;
layout (location = 1) out vec3 f_normal;
layout (location = 2) out vec2 f_texcoord;

layout (location = 1) uniform mat4 proj_matrix;
layout (location = 2) uniform mat4 mv_matrix;

void main(void)
{
	f_texcoord = texcoords;
	f_position = mv_matrix * position;
	mat4 mv_mat_simple = mv_matrix;
	mv_mat_simple[3][0] = 0.0;
	mv_mat_simple[3][1] = 0.0;
	mv_mat_simple[3][2] = 0.0;
	vec4 nnormal = mv_mat_simple * vec4(normal, 1.0);
	f_normal = nnormal.xyz / nnormal.w;
	gl_Position = proj_matrix * mv_matrix * position;
}
