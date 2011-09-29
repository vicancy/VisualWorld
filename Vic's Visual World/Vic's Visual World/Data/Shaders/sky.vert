// Copyright (c) 2008 the OpenTK Team. See license.txt for legal bla

// MUST be written to for FS
varying vec3 Normal;
varying vec4 Position;
void main()
{ 
  gl_Position = ftransform();
  Normal = /*gl_NormalMatrix * */ gl_Normal ;
  Position = gl_Position;
}