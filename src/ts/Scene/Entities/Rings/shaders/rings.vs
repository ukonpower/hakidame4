#include <common>
#include <vert_h>
#include <rotate>

layout (location = 3) in vec2 computeUV;
layout (location = 4) in vec3 id;
layout (location = 5) in vec3 offsetPosition;

uniform sampler2D gpuSampler0;
uniform sampler2D gpuSampler1;

uniform float uTime;

void main( void ) {

	#include <vert_in>

	// outPos.xz *= (id.x * 0.3 + 0.7) + (sin( - uTime * 0.5 ) * 0.5 + 0.5) * ( id.x * 0.8 + 0.2);

	outPos += offsetPosition;
	
	#include <vert_out>
	
}