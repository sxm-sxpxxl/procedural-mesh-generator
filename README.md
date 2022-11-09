<p align="center">
  <img alt="mesh-generation-preview" src="https://user-images.githubusercontent.com/37039414/200845488-d86fd0e0-5d89-44d2-a8ed-633aec21eec9.gif" />
  <img alt="mesh-modification-preview" src="https://user-images.githubusercontent.com/37039414/200845661-b2faeb53-a567-4cdb-acb1-f29b0d9e5815.gif" />
</p>

[![Unity Version](https://img.shields.io/badge/unity-2021.3%2B-blue)](https://unity3d.com/get-unity/download)

## About

A simple Unity editor tool of procedural mesh generating, modifying and exporting. Created for educational purposes.

## Features

<p align="center">
  <img alt="inspector-preview" src="https://user-images.githubusercontent.com/37039414/200845742-0b66bedc-149f-4a08-a3bc-fbb2bc79a6e5.png" />
</p>

- [x] 🔨 Procedural mesh generation of basic primitive shapes</br>
     - **Plane** (with backace culling option)
     - **Cube** (with roundness option)
     - **Cube Sphere** (a more evenly distributed sphere than a cube with maximum roundness)
- [x] 🔧 Changing mesh vertices with simple modifiers in multi-threaded way with Job System, Burst Compiler and Unity Mathematics library for better calculations</br>
     - **Sine**
     - **Ripple**
- [x] 💾 Export meshes in multiple formats</br>
     - **Unity Asset** (.asset)
     - **Wavefront OBJ** (.obj)
- [x] 📦 Creating and updating according to the mesh of various colliders</br>
     - **Bounds**
     - **Mesh**
- [x] ☕ Custom editor inspector for more convenient work with the procedural generation component</br>
