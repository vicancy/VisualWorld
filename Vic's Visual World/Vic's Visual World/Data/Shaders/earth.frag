// Copyright (c) 2008 the OpenTK Team. See license.txt for legal bla

uniform samplerCube Earth;
varying vec3 Normal;
varying vec4 Position;
void main()
{    
    vec4 cube = textureCube( Earth, Normal.xyz );
    gl_FragColor =cube;
    
}