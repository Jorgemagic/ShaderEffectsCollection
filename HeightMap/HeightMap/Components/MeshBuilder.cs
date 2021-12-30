// Copyright © Plain Concepts S.L.U. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using Evergine.Common.Graphics.VertexFormats;
using Evergine.Common.Graphics;
using Evergine.Mathematics;
using Evergine.Framework.Graphics;
using System.Runtime.InteropServices;

namespace HeightMap.Components
{
    /// <summary>
    /// Represents a builder for primitive models.
    /// </summary>
    public class MeshBuilder
    {
        /// <summary>
        /// During the process of constructing a primitive model, vertex data is stored on the CPU in these managed lists.
        /// </summary>
        private List<VertexPositionNormalTangentColorDualTexture> vertices = new List<VertexPositionNormalTangentColorDualTexture>();

        /// <summary>
        /// During the process of constructing a primitive model, index data is stored on the CPU in these managed lists.
        /// </summary>
        private List<ushort> indices = new List<ushort>();

        /// <summary>
        /// Gets the vertices count.
        /// </summary>
        public int VerticesCount
        {
            get { return this.vertices.Count; }
        }

        /// <summary>
        /// Calculate tangent space of the geometry.
        /// </summary>
        public unsafe void CalculateTangentSpace()
        {
            int vertexCount = this.vertices.Count;
            int triangleCount = this.indices.Count / 3;

            Vector3* tan1 = stackalloc Vector3[vertexCount * 2];
            Vector3* tan2 = tan1 + vertexCount;

            VertexPositionNormalTangentColorDualTexture a1, a2, a3;
            Vector3 v1, v2, v3;
            Vector2 w1, w2, w3;

            for (int a = 0; a < triangleCount; a++)
            {
                ushort i1 = this.indices[(a * 3) + 0];
                ushort i2 = this.indices[(a * 3) + 1];
                ushort i3 = this.indices[(a * 3) + 2];

                a1 = this.vertices[i1];
                a2 = this.vertices[i2];
                a3 = this.vertices[i3];

                v1 = a1.Position;
                v2 = a2.Position;
                v3 = a3.Position;

                w1 = a1.TexCoord;
                w2 = a2.TexCoord;
                w3 = a3.TexCoord;

                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;

                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;

                float r = 1.0F / ((s1 * t2) - (s2 * t1));
                Vector3 sdir = new Vector3(((t2 * x1) - (t1 * x2)) * r, ((t2 * y1) - (t1 * y2)) * r, ((t2 * z1) - (t1 * z2)) * r);
                Vector3 tdir = new Vector3(((s1 * x2) - (s2 * x1)) * r, ((s1 * y2) - (s2 * y1)) * r, ((s1 * z2) - (s2 * z1)) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            for (int a = 0; a < vertexCount; a++)
            {
                var vertex = this.vertices[a];

                Vector3 n = vertex.Normal;
                Vector3 t = tan1[a];

                // Gram-Schmidt orthogonalize
                vertex.Tangent = t - (n * Vector3.Dot(n, t));
                vertex.Tangent.Normalize();

                this.vertices[a] = vertex;
            }
        }

        /// <summary>
        /// Gets the Mesh by vertex and index information.
        /// </summary>
        /// <param name="graphicsContext">        
        /// The <see cref="GraphicsContext"/> used for mesh buffers generation.
        /// </param>
        /// <param name="topology">Primitive topology used to create the mesh</param>
        /// <returns>Mesh instance.</returns>
        public Mesh CreateMesh(GraphicsContext graphicsContext, PrimitiveTopology topology)
        {
            if (graphicsContext is null)
            {
                throw new ArgumentNullException(nameof(graphicsContext));
            }

            this.CalculateTangentSpace();

            var vertexBufferDescription = new BufferDescription((uint)(Marshal.SizeOf(typeof(VertexPositionNormalTangentColorDualTexture)) * this.vertices.Count), BufferFlags.ShaderResource | BufferFlags.VertexBuffer, ResourceUsage.Default);
            var vBuffer = graphicsContext.Factory.CreateBuffer(this.vertices.ToArray(), ref vertexBufferDescription);
            var vertexBuffer = new VertexBuffer(vBuffer, VertexPositionNormalTangentColorDualTexture.VertexFormat);

            var indexBufferDescription = new BufferDescription((uint)(sizeof(ushort) * this.indices.Count), BufferFlags.IndexBuffer, ResourceUsage.Default);
            var iBuffer = graphicsContext.Factory.CreateBuffer(this.indices.ToArray(), ref indexBufferDescription);
            var indexBuffer = new IndexBuffer(iBuffer);

            return new Mesh(new VertexBuffer[] { vertexBuffer }, indexBuffer, topology)
            {
                BoundingBox = this.ComputeBoundingBox(),
            };
        }

        /// <summary>
        /// Clears the internal structure.
        /// </summary>
        public void Clear()
        {
            this.indices.Clear();
            this.vertices.Clear();
        }

        /// <summary>
        /// Prepares the internal structure with the specified size for indices and vertices.
        /// </summary>
        /// <param name="indicesCount">The maximum indices count.</param>
        /// <param name="vertexCount">The maximum vertex count.</param>
        public void SetCapacity(int indicesCount, int vertexCount)
        {
            if (indicesCount < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(indicesCount));
            }

            if (vertexCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(vertexCount));
            }

            if (indicesCount > this.indices.Capacity)
            {
                this.indices.Capacity = indicesCount;
            }

            if (vertexCount > this.vertices.Capacity)
            {
                this.vertices.Capacity = vertexCount;
            }
        }

        /// <summary>
        /// Adds a new vertex to the primitive model.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
        /// <remarks>
        /// This should only be called during the initialization process,
        /// before InitializePrimitive.
        /// </remarks>
        public void AddVertex(Vector3 position, Vector3 normal)
        {
            this.vertices.Add(new VertexPositionNormalTangentColorDualTexture(position, normal, Vector3.Zero, Color.White, Vector2.Zero, Vector2.Zero));
        }

        /// <summary>
        /// Adds a new vertex to the primitive model.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
        /// <param name="texcoord">The texture coordinate.</param>
        /// <remarks>
        /// This should only be called during the initialization process,
        /// before InitializePrimitive.
        /// </remarks>
        public void AddVertex(Vector3 position, Vector3 normal, Vector2 texcoord)
        {
            this.vertices.Add(new VertexPositionNormalTangentColorDualTexture(position, normal, Vector3.Zero, Color.White, texcoord, texcoord));
        }

        /// <summary>
        /// Adds a new vertex to the primitive model.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
        /// <param name="tangent">The tangent.</param>
        /// <param name="texcoord">The texture coordinate.</param>
        /// <remarks>
        /// This should only be called during the initialization process,
        /// before InitializePrimitive.
        /// </remarks>
        public void AddVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord)
        {
            this.vertices.Add(new VertexPositionNormalTangentColorDualTexture(position, normal, tangent, Color.White, texcoord, texcoord));
        }

        /// <summary>
        /// Adds a new vertex to the primitive model.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
        /// <param name="tangent">The tangent.</param>
        /// <param name="texcoord">The texture coordinate.</param>
        /// <param name="color">The color.</param>
        /// <remarks>
        /// This should only be called during the initialization process,
        /// before InitializePrimitive.
        /// </remarks>
        public void AddVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord, Color color)
        {
            this.vertices.Add(new VertexPositionNormalTangentColorDualTexture(position, normal, tangent, color, texcoord, texcoord));
        }

        /// <summary>
        /// Adds a new index to the primitive model.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <remarks>
        /// This should only be called during the initialization process,
        /// before InitializePrimitive.
        /// </remarks>
        public void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            this.indices.Add((ushort)index);
        }

        /// <summary>
        /// Gets the spherical texture coordinates.
        /// </summary>
        /// <param name="normal">The normal.</param>
        /// <returns>Spherical coordinates.</returns>
        public Vector2 GetSphericalTexCoord(Vector3 normal)
        {
            double tx = (Math.Atan2(normal.X, normal.Z) / (Math.PI * 2)) + 0.25;
            double ty = (Math.Asin(normal.Y) / MathHelper.Pi) + 0.5;

            return new Vector2((float)tx, (float)ty);
        }

        private BoundingBox ComputeBoundingBox()
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            Vector3 currentVertex;
            for (int i = 0; i < this.vertices.Count; i++)
            {
                currentVertex = this.vertices[i].Position;
                Vector3.Max(ref currentVertex, ref max, out max);
                Vector3.Min(ref currentVertex, ref min, out min);
            }

            return new BoundingBox(min, max);
        }
    }
}
