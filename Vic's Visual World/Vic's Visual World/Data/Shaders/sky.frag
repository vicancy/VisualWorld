// Copyright (c) 2008 the OpenTK Team. See license.txt for legal bla

uniform samplerCube Sky;
varying vec3 Normal;
varying vec4 Position;
void main()
{    
    vec4 cube = textureCube( Sky, Normal.xyz );
    gl_FragColor =cube;
    
}