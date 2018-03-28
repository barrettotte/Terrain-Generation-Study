# Terrain-Generation-Study
Using Perlin noise to generate a map texture and then generate an LOD enabled terrain mesh from the map.

## About:
This was going to be my attempt at creating a No Man's Sky-esque planet/terrain system. The planet would share the same map as the terrain and when a ship would land, the appropriate terrain chunks would be generated. I however lost steam after implementing the terrain chunk LOD system and never got around to the UV, transform, and spherical coordinate system translating. If I create a similar project in the future I will use this as the basis.

## Features:
* Implements a port of LibNoise for C# for noise generation
* Octahedron sphere procedural generation
* Color mapping based off of noise map values
* Terrain chunk generation based off of noise map
* Circular buffer implementation to allow terrain chunk wrapping
* Terrain chunk LOD system for better performance
* Threading for map generation

## Planned, but Never Implemented:
* Fix seams between terrain chunks
* UV, transform, and spherical coordinate translation
* Landing on planet generates appropriate terrain chunks
* Planet rotation
* Planetary generation from seed
* Longitude/Latitude system

## Screenshots:

Flat Color Map Generation
![Map Generation](/Screenshot_Map.PNG)

Octahedron Sphere Planet Generation
![Planet Generation](/Screenshot_Planet.PNG)

Mesh Generation
![Mesh Generation](/Screenshot_Mesh.PNG)

LOD System
![LOD System](/Screenshot_LOD.PNG)
