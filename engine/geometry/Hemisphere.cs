using System.Collections.Generic;
using System.Numerics;
using System;

namespace FreedomOfFormFoundation.AnatomyEngine.Geometry
{
	public class Hemisphere : Surface
	{
		public Hemisphere(float radius, Vector3 center, Vector3 direction)
		{
			this.Radius = radius;
			this.Center = center;
			this.Direction = direction;
		}
		
		float Radius { get; set; }
		
		Vector3 Center { get; set; }
		
		private Vector3 direction;
		Vector3 Direction
		{
			get { return direction; }
			set { direction = Vector3.Normalize(value); }
		}
		
		public static int CalculateVertexCount(int resolutionU, int resolutionV)
		{
			return resolutionU * resolutionV + 1;
		}
		
		public static int CalculateIndexCount(int resolutionU, int resolutionV)
		{
			return resolutionU * resolutionV * 6 + resolutionU * 3;
		}
		
		public override List<Vertex> GenerateVertexList(int resolutionU, int resolutionV)
		{
			List<Vertex> output = new List<Vertex>(CalculateVertexCount(resolutionU, resolutionV));
			
			Vector3 up = new Vector3(0.0f, 0.0f, 1.0f);
			float sign = 1.0f;
			
			// To prevent division by zero, flip the direction if facing the other way:
			if(Vector3.Dot(up, this.direction) < 0.0f)
			{
				sign = -1.0f;
			}
			
			// Generate a rotation matrix which rotates the 'up' vector onto the
			// 'direction' vector, using Rodrigues' Rotation Formula:
			Vector3 k = Vector3.Cross(sign*this.direction, up);
			
			Matrix4x4 identity = new Matrix4x4(1.0f, 0.0f, 0.0f, 0.0f,
											   0.0f, 1.0f, 0.0f, 0.0f,
											   0.0f, 0.0f, 1.0f, 0.0f,
											   0.0f, 0.0f, 0.0f, 1.0f);
			
			Matrix4x4 K = new Matrix4x4(0.0f, -k.Z,  k.Y, 0.0f,
										 k.Z, 0.0f, -k.X, 0.0f,
										-k.Y,  k.X, 0.0f, 0.0f,
										0.0f, 0.0f, 0.0f, 0.0f);
			
			Matrix4x4 rotationMatrix = identity + K + K*K*(1.0f/(1.0f + Vector3.Dot(up, sign*this.direction)));
			
			Matrix4x4 transformationMatrix = rotationMatrix;
			transformationMatrix.Translation = Center;
			
			float radius = this.Radius;

			// Generate the first point at the pole of the hemisphere:
			output.Add(new Vertex(Vector3.Transform(sign*radius*up, transformationMatrix),
								  Vector3.Transform(sign*up, rotationMatrix)));
			
			// Generate rings of the other points:
			for (int j = 1; j < (resolutionV+1); j++)
			{
				for (int i = 0; i < resolutionU; i++)
				{
					// First find the normalized uv-coordinates, u = [0, 2pi], v = [0, 1/2pi]:
					float u = (float)i/(float)resolutionU * 2.0f * (float)Math.PI;
					float v = (float)j/(float)resolutionV * 0.5f * (float)Math.PI;
					
					// Calculate the position of the rings of vertices:
					float x = (float)Math.Sin(v) * (float)Math.Cos(sign*u);
					float y = (float)Math.Sin(v) * (float)Math.Sin(sign*u);
					float z = (float)Math.Cos(v);
					
					Vector3 position = new Vector3(x, y, z);
					
					// Rotate the vector to orient the hemisphere correctly:
					Vector3 vertexPosition = Vector3.Transform(sign*radius * position, transformationMatrix);
					Vector3 normal = Vector3.Transform(sign*position, rotationMatrix);
					
					output.Add(new Vertex(vertexPosition, normal));
				}
			}
			
			return output;
		}
		
		public override List<int> GenerateIndexList(int resolutionU, int resolutionV, int indexOffset = 0)
		{
			List<int> output = new List<int>(CalculateIndexCount(resolutionU, resolutionV));
			
			// Add a triangle fan on the pole of the hemisphere:
			for (int i = 0; i < resolutionU-1; i++)
			{
				output.Add(indexOffset + 1 + i);
				output.Add(indexOffset + 0);
				output.Add(indexOffset + 2 + i);
			}
			
			// Stitch final triangle on the pole of the hemisphere:
			output.Add(indexOffset + resolutionU);
			output.Add(indexOffset + 0);
			output.Add(indexOffset + 1);
			
			// Add the remaining rings:
			for (int j = 0; j < resolutionV - 1; j++)
			{
				// Add a ring of triangles:
				for (int i = 0; i < resolutionU-1; i++)
				{
					output.Add(indexOffset + 1 + i + resolutionU*(j+1));
					output.Add(indexOffset + 1 + i + resolutionU*j);
					output.Add(indexOffset + 1 + (i+1) + resolutionU*j);

					output.Add(indexOffset + 1 + (i+1) + resolutionU*(j+1));
					output.Add(indexOffset + 1 + i + resolutionU*(j+1));
					output.Add(indexOffset + 1 + (i+1) + resolutionU*j);
				}
				
				// Stitch the end of the ring of triangles:
				output.Add(indexOffset + 1 + resolutionU-1 + resolutionU*(j+1));
				output.Add(indexOffset + 1 + resolutionU-1 + resolutionU*j);
				output.Add(indexOffset + 1 + resolutionU*j);

				output.Add(indexOffset + 1 + resolutionU*(j+1));
				output.Add(indexOffset + 1 + resolutionU-1 + resolutionU*(j+1));
				output.Add(indexOffset + 1 + resolutionU*j);
			}

			return output;
		}
	}
}
