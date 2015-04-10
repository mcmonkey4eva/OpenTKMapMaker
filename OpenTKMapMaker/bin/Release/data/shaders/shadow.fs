#version 430 core

layout (location = 0) in vec4 f_pos;

out vec4 color;

void main()
{
	color = vec4(f_pos.z / f_pos.w);
}
