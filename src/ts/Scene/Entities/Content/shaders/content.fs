#include <common>
#include <packing>
#include <frag_h>

#include <sdf>
#include <noise>
#include <rotate>

uniform vec3 cameraPosition;
uniform mat4 modelMatrixInverse;
uniform float uTime;

// ref: https://www.shadertoy.com/view/stdXWH
// ref: https://www.shadertoy.com/view/dsjGDD

const float GRID_INTERVAL = 0.25;

vec4 traverseGrid3D( vec3 ro, vec3 rd ) {

    vec3 grid = floor( ( ro + rd * 0.001 * GRID_INTERVAL ) / GRID_INTERVAL ) * GRID_INTERVAL + 0.5 * GRID_INTERVAL;
    
    vec3 src = ( ro - grid ) / rd;
    vec3 dst = abs( 0.5 * GRID_INTERVAL / rd );
    vec3 bv = -src + dst;
    float b = min( min( bv.x, bv.y ), bv.z );
    
    return vec4( grid, b );
}

vec2 D( vec3 p ) {

	vec2 d = vec2( 0.0 );

	vec3 s = vec3( GRID_INTERVAL / 2.0 * 0.9 );

	d = vec2( sdBox( p, s ), 0.0 );
	
	return d;

}


vec3 N( vec3 pos, float delta ){

    return normalize( vec3(
		D( pos ).x - D( vec3( pos.x - delta, pos.y, pos.z ) ).x,
		D( pos ).x - D( vec3( pos.x, pos.y - delta, pos.z ) ).x,
		D( pos ).x - D( vec3( pos.x, pos.y, pos.z - delta ) ).x
	) );
	
}

void main( void ) {

	#include <frag_in>

	vec3 rayOrigin = ( modelMatrixInverse * vec4( vPos, 1.0 ) ).xyz;
	vec3 rayDir = normalize( ( modelMatrixInverse * vec4( normalize( vPos - cameraPosition ), 0.0 ) ).xyz );
	vec2 rayDirXZ = normalize( rayDir.xz );
	vec3 rayPos = rayOrigin;
	float rayLength = 0.0;
	
	vec3 gridCenter = vec3( 0.0 );
	float lenNextGrid = 0.0;
	
	vec2 dist = vec2( 0.0 );
	bool hit = false;

	vec3 normal;
	
	for( int i = 0; i < 64; i++ ) { 

		if( lenNextGrid <= rayLength ) {

			rayLength = lenNextGrid;
			rayPos = rayOrigin + rayLength * rayDir;
			vec4 grid = traverseGrid3D( rayPos, rayDir );
			gridCenter.xyz = grid.xyz;

			float lg = length(gridCenter.xyz);
			lenNextGrid += grid.w;

		}

		dist = D( rayPos - gridCenter );
		rayLength += dist.x;
		rayPos = rayOrigin + rayLength * rayDir;

		if( dist.x < 0.001 ) {
			hit = true;
			break;
		}
		
		if( abs( rayPos.x ) > 0.5 ) break;
		if( abs( rayPos.z ) > 0.5 ) break;
		if( abs( rayPos.y ) > 0.5 ) break;
		
	}

	if( hit ) {

		vec3 n = N( rayPos - gridCenter, 0.001 );
		outNormal = normalize(modelMatrix * vec4( n, 0.0 )).xyz;
		
	} else {

		discard;
		
	}

	outRoughness = .1;
	outMetalic = 0.0;
	// outColor.xyz = vec3( 0.0 );

	outPos = ( modelMatrix * vec4( rayPos, 1.0 ) ).xyz;

	#include <frag_out>

}