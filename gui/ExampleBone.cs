using Godot;
using System;
using System.Collections.Generic;
using Numerics = System.Numerics;

using FreedomOfFormFoundation.AnatomyEngine;
using Anatomy = FreedomOfFormFoundation.AnatomyEngine.Anatomy;
using FreedomOfFormFoundation.AnatomyEngine.Anatomy.Bones;
using FreedomOfFormFoundation.AnatomyEngine.Geometry;
using FreedomOfFormFoundation.AnatomyEngine.Calculus;
using FreedomOfFormFoundation.AnatomyEngine.Renderable;

namespace FreedomOfFormFoundation.AnatomyRenderer
{
	public class ExampleBone : MeshInstance
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			// Example method that creates a character and adds a single joint and bone:
			Character character = new Character();
			
			CreateExampleBone(character);
			CreateExampleJoint(character);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			// Visually spin the bone around, just for show:
			RotateY(0.5f*delta);
		}
		
		public void CreateExampleBone(Character character)
		{
			// Generate a simple cubic spline that will act as the radius of a long bone:
			SortedList<float, float> radiusPoints = new SortedList<float, float>();
			radiusPoints.Add(0.0f, 0.7f*0.92f);
			radiusPoints.Add(0.02f, 0.7f*0.92f);
			radiusPoints.Add(0.15f, 0.7f*0.8f);
			radiusPoints.Add(0.5f, 0.7f*0.7f);
			radiusPoints.Add(0.8f, 0.7f*0.76f);
			radiusPoints.Add(0.98f, 0.7f*0.8f);
			radiusPoints.Add(1.0f, 0.7f*0.8f);
			
			CubicSpline1D boneRadius = new CubicSpline1D(radiusPoints);

			// Define the center curve of the long bone:
			SortedList<float, Numerics.Vector3> centerPoints = new SortedList<float, Numerics.Vector3>();
			centerPoints.Add(0.0f, new Numerics.Vector3(0.0f, 0.0f, 2.7f));
			centerPoints.Add(0.25f, new Numerics.Vector3(-0.3f, -0.5f, 1.0f));
			centerPoints.Add(0.5f, new Numerics.Vector3(0.3f, 1.0f, 0.0f));
			centerPoints.Add(0.75f, new Numerics.Vector3(0.8f, 1.0f, -1.0f));
			centerPoints.Add(1.0f, new Numerics.Vector3(0.6f, -0.5f, -0.9f));
			
			SpatialCubicSpline boneCenter = new SpatialCubicSpline(centerPoints);
			
			//Line centerLine = new Line(new Numerics.Vector3(0.0f, 0.0f, 1.7f),
			//						   new Numerics.Vector3(0f, 0.0f, -1.7f));
			
			// Add a long bone to the character:
			character.bones.Add(new Anatomy.Bones.LongBone(boneCenter, boneRadius));
			
			// Generate the geometry vertices of the first bone with resolution U=32 and resolution V=32:
			UVMesh mesh = character.bones[0].GetGeometry().GenerateMesh(64, 1024);
			
			// Finally upload the mesh to Godot:
			MeshInstance newMesh = new MeshInstance();
			newMesh.Mesh = new GodotMeshConverter(mesh);
			newMesh.SetSurfaceMaterial(0, (Material)GD.Load("res://gui/BoneMaterial.tres"));
			
			AddChild(newMesh);
		}
		
		public void CreateExampleJoint(Character character)
		{
			// Generate a simple cubic spline that will act as the radius of a long bone:
			SortedList<float, float> splinePoints = new SortedList<float, float>();
			splinePoints.Add(0.0f, 0.5f*1.1f);
			splinePoints.Add(0.02f, 0.5f*1.1f);
			splinePoints.Add(0.15f, 0.5f*0.95f);
			splinePoints.Add(0.3f, 0.5f*0.9f);
			splinePoints.Add(0.5f, 0.5f*1.2f);
			splinePoints.Add(0.7f, 0.5f*0.9f);
			splinePoints.Add(0.8f, 0.5f*0.95f);
			splinePoints.Add(0.98f, 0.5f*1.1f);
			splinePoints.Add(1.0f, 0.5f*1.1f);
			
			CubicSpline1D jointSpline = new CubicSpline1D(splinePoints);

			// Define the center curve of the long bone:
			Line centerLine = new Line(new Numerics.Vector3(-0.6f, 0.0f, 3.0f),
									   new Numerics.Vector3(0.6f, 0.0f, 3.0f));
			
			// Add a long bone to the character:
			character.joints.Add(new Anatomy.Joints.HingeJoint(centerLine, jointSpline));
			
			// Generate the geometry vertices of the first bone with resolution U=32 and resolution V=32:
			UVMesh mesh = character.joints[0].GetArticularSurface().GenerateMesh(64, 64);
			
			// Finally upload the mesh to Godot:
			MeshInstance newMesh = new MeshInstance();
			newMesh.Mesh = new GodotMeshConverter(mesh);
			
			AddChild(newMesh);
		}
	}
}
