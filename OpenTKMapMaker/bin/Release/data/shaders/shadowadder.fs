#version 430 core

layout (binding = 0) uniform sampler2D pre_lighttex;
layout (binding = 1) uniform sampler2D positiontex;
layout (binding = 2) uniform sampler2D normaltex;
layout (binding = 3) uniform sampler2D depthtex;
layout (binding = 4) uniform sampler2DShadow tex;

layout (location = 0) in vec2 f_texcoord;

layout (location = 3) uniform mat4 shadow_matrix;
layout (location = 4) uniform vec3 light_pos = vec3(5.0, 5.0, 5.0);

out vec4 color;

layout (location = 5) uniform vec3 diffuse_albedo = vec3(0.7, 0.7, 0.7);
layout (location = 6) uniform vec3 specular_albedo = vec3(0.7, 0.7, 0.7);
layout (location = 7) uniform float specular_power = 200.0;
layout (location = 8) uniform vec3 light_color = vec3(1.0, 1.0, 1.0);
layout (location = 9) uniform float light_radius = 30.0;

void main()
{
	vec3 normal = texture(normaltex, f_texcoord).xyz;
	vec3 position = texture(positiontex, f_texcoord).xyz;
	vec4 f_spos = shadow_matrix * vec4(position, 1.0);
	if (position == vec3(0.0))
	{
		f_spos = vec4(999999999.0, 999999999.0, -999999999.0, 1.0);
		position = vec3(999999999.0, 999999999.0, -999999999.0);
	}
	vec4 prelight_color = texture(pre_lighttex, f_texcoord);
	vec3 N = normalize(normal);
	vec3 light_path = light_pos - position;
	float light_length = length(light_path);
	float atten;
	if (light_radius == 0)
	{
		atten = 1.0;
	}
	else
	{
		float d = light_length / light_radius;
		atten = clamp(1.0 - (d * d), 0.0, 1.0);
	}
	vec3 L = light_path / light_length;
	vec3 V = normalize(-position);
	vec3 R = reflect(L, N);
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	vec4 specular = vec4(pow(max(dot(R, V), 0.0), specular_power) * specular_albedo, 1.0);
	vec4 fs = f_spos / f_spos.w / 2.0 + 0.5;
	//fs.x = (fs.x - 0.5) / 0.75 + 0.5;
	fs.z -= 0.001;
	float depth = textureProj(tex, fs + vec4(0.00, -0.005, 0.0, 0.0));
	float depth2 = textureProj(tex, fs + vec4(0.005, 0.0, 0.0, 0.0));
	float depth3 = textureProj(tex, fs + vec4(0.0, 0.005, 0.0, 0.0));
	float depth4 = textureProj(tex, fs + vec4(-0.005, 0.0, 0.0, 0.0));
	depth = (depth + depth2 + depth3 + depth4) / 4;
	fs = f_spos / f_spos.w / 2.0 + vec4(0.5);
	if (fs.x < 0.0 || fs.x > 1.0
		|| fs.y < 0.0 || fs.y > 1.0
		|| fs.z < 0.0 || fs.z > 1.0)
	{
		depth = 0.0;
	}
	color = prelight_color + vec4(depth, depth, depth, 1.0) * atten * mix(vec4(1.0), diffuse + specular, bvec4(1.0)) * vec4(light_color, 1.0);
}
