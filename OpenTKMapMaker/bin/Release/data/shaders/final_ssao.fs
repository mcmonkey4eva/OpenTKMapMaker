#version 430 core

layout (binding = 0) uniform sampler2D colortex;
layout (binding = 1) uniform sampler2D positiontex;
layout (binding = 2) uniform sampler2D normaltex;
layout (binding = 3) uniform sampler2D depthtex;
layout (binding = 4) uniform sampler2D shtex;
layout (binding = 5) uniform sampler2D renderhinttex;

layout (location = 0) in vec2 f_texcoord;

layout (location = 5) uniform vec3 ambient = vec3(0.05, 0.05, 0.05);
// ...
layout (location = 8) uniform vec3 cameraTargetPos = vec3(0.0, 0.0, 0.0);
layout (location = 9) uniform float dof_strength = 0.5;
layout (location = 10) uniform float depth_range = 10000;

out vec4 color;

vec4 regularize(vec4 input_r) // TODO: Is this working the best it can?
{
	if (input_r.x <= 1.0 && input_r.y <= 1.0 && input_r.z <= 1.0)
	{
		return input_r;
	}
	return vec4(input_r.xyz / ((input_r.x >= input_r.y && input_r.x >= input_r.z) ? input_r.x: ((input_r.y >= input_r.z) ? input_r.y: input_r.z)), input_r.w);
}

const int SSAO_COORD_COUNT = 4;

const float bias = 0.001;

const vec2[SSAO_COORD_COUNT] ssaocoords = vec2[](vec2(0.005, 0.005), vec2(-0.005, 0.005), vec2(0.005, -0.005), vec2(-0.005, -0.005));

vec4 read_single_pixel(vec2 tcoord)
{
	vec4 shadow_light_color = texture(shtex, tcoord);
	vec4 colortex_color = texture(colortex, tcoord);
	return regularize(vec4(ambient, 0.0) * colortex_color + shadow_light_color);
}

void main()
{
	float depth = texture2D(depthtex, f_texcoord).r;
	vec3 normal = texture(normaltex, f_texcoord).xyz;
	vec3 position = texture(positiontex, f_texcoord).xyz;
	if (position == vec3(0.0) && normal == vec3(0.0))
	{
		position = vec3(999999999.0, 999999999.0, -999999999.0);
	}
	float ssaocolor = 1.0;
	for (int i = 0; i < SSAO_COORD_COUNT; i++)
	{
		float nd = texture2D(depthtex, f_texcoord + ssaocoords[i]).r;
		if ((nd < (depth)) || (nd > (depth + bias)))
		{
			ssaocolor -= 0.05;
		}
	}
	vec4 SSAOColor = vec4(ssaocolor);
	vec4 light_color = read_single_pixel(f_texcoord) * SSAOColor * 0.2;
	float targetDepth = texture2D(depthtex, vec2(0.5, 0.5)).r;
	float depth_offset = (depth - targetDepth) / dof_strength;
	light_color += read_single_pixel(vec2(f_texcoord.x - depth_offset, f_texcoord.y)) * 0.2;
	light_color += read_single_pixel(vec2(f_texcoord.x + depth_offset, f_texcoord.y)) * 0.2;
	light_color += read_single_pixel(vec2(f_texcoord.x - depth_offset * 0.5, f_texcoord.y)) * 0.2;
	light_color += read_single_pixel(vec2(f_texcoord.x + depth_offset * 0.5, f_texcoord.y)) * 0.2;
	light_color.w = 1.0;
	color = light_color;
	//color = vec4(depth, targetDepth, depth, 1.0);
}
