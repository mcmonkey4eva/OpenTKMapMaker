#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoord;
layout (location = 3) in vec4 color;
layout (location = 4) in vec4 Weights;
layout (location = 5) in vec4 BoneID;

const int MAX_BONES = 70;

layout (location = 1) uniform mat4 projection = mat4(1.0);
layout (location = 2) uniform mat4 model_matrix = mat4(1.0);
layout (location = 3) uniform vec3 v_color = vec3(1.0, 1.0, 1.0);
// ...
layout (location = 6) uniform mat4 simplebone_matrix = mat4(1.0);
layout (location = 7) uniform mat4 boneTrans[MAX_BONES];

layout (location = 0) out vec4 f_color;
layout (location = 1) out vec3 f_texcoord;

void main()
{
	vec4 pos1;
	float rem = 1.0 - (Weights[0] + Weights[1] + Weights[2] + Weights[3]);
	mat4 BT = mat4(1.0);
	if (rem < 0.99)
	{
		BT = boneTrans[int(BoneID[0])] * Weights[0];
		BT += boneTrans[int(BoneID[1])] * Weights[1];
		BT += boneTrans[int(BoneID[2])] * Weights[2];
		BT += boneTrans[int(BoneID[3])] * Weights[3];
		BT += mat4(1.0) * rem;
		pos1 = vec4(position, 1.0) * BT;
	}
	else
	{
		pos1 = vec4(position, 1.0);
	}
	pos1 *= simplebone_matrix;
	f_color = vec4(color[0] * v_color[0], color[1] * v_color[1], color[2] * v_color[2], color[3]);
	f_texcoord = texcoord;
	gl_Position = projection * model_matrix * vec4(pos1.xyz, 1.0);
}
