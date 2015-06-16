using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using OpenTK.Graphics.ES11;

namespace Texample
{
    public class Vertices
    {
        //--Constants--//
        static readonly int POSITION_CNT_2D = 2;              // Number of Components in Vertex Position for 2D
        static readonly int POSITION_CNT_3D = 3;              // Number of Components in Vertex Position for 3D
        static readonly int COLOR_CNT = 4;                    // Number of Components in Vertex Color
        static readonly int TEXCOORD_CNT = 2;                 // Number of Components in Vertex Texture Coords
        static readonly int NORMAL_CNT = 3;                   // Number of Components in Vertex Normal

        static readonly int INDEX_SIZE = sizeof(short);      // Index Byte Size

        //--Members--//
        // NOTE: all members are constant, and initialized in constructor!
        readonly bool hasColor;                            // Use Color in Vertices
        readonly bool hasTexCoords;                        // Use Texture Coords in Vertices
        readonly bool hasNormals;                          // Use Normals in Vertices
        public readonly int positionCnt;                      // Number of Position Components (2=2D, 3=3D)
        public readonly int vertexStride;                     // Vertex Stride (Element Size of a Single Vertex)
        public readonly int vertexSize;                       // Bytesize of a Single Vertex
        readonly float[] vertices;                          // Vertex Buffer
        readonly short[] indices;                         // Index Buffer
        public int numVertices;                            // Number of Vertices in Buffer
        public int numIndices;                             // Number of Indices in Buffer
        readonly int[] tmpBuffer;                             // Temp Buffer for Vertex Conversion

        //--Constructor--//
        // D: create the vertices/indices as specified (for 2d/3d)
        // A: gl - the gl instance to use
        //    maxVertices - maximum vertices allowed in buffer
        //    maxIndices - maximum indices allowed in buffer
        //    hasColor - use color values in vertices
        //    hasTexCoords - use texture coordinates in vertices
        //    hasNormals - use normals in vertices
        //    use3D - (false, default) use 2d positions (ie. x/y only)
        //            (true) use 3d positions (ie. x/y/z)
        public Vertices(int maxVertices, int maxIndices, bool hasColor, bool hasTexCoords, bool hasNormals)
            : this(maxVertices, maxIndices, hasColor, hasTexCoords, hasNormals, false) {
        }

        public Vertices(int maxVertices, int maxIndices, bool hasColor, bool hasTexCoords, bool hasNormals, bool use3D) {
            this.hasColor = hasColor;                       // Save Color Flag
            this.hasTexCoords = hasTexCoords;               // Save Texture Coords Flag
            this.hasNormals = hasNormals;                   // Save Normals Flag
            this.positionCnt = use3D ? POSITION_CNT_3D : POSITION_CNT_2D;  // Set Position Component Count
            this.vertexStride = this.positionCnt + (hasColor ? COLOR_CNT : 0) + (hasTexCoords ? TEXCOORD_CNT : 0) + (hasNormals ? NORMAL_CNT : 0);  // Calculate Vertex Stride
            this.vertexSize = this.vertexStride * 4;        // Calculate Vertex Byte Size

            this.vertices = new float[maxVertices * vertexSize];           // Save Vertex Buffer

            if (maxIndices > 0) {                        // IF Indices Required
                this.indices = new short[maxIndices * INDEX_SIZE];       // Save Index Buffer
            } else                                            // ELSE Indices Not Required
                indices = null;                              // No Index Buffer

            numVertices = 0;                                // Zero Vertices in Buffer
            numIndices = 0;                                 // Zero Indices in Buffer

            this.tmpBuffer = new int[maxVertices * vertexSize / 4];  // Create Temp Buffer
        }

        //--Set Vertices--//
        // D: set the specified vertices in the vertex buffer
        //    NOTE: optimized to use integer buffer!
        // A: vertices - array of vertices (floats) to set
        //    offset - offset to first vertex in array
        //    length - number of floats in the vertex array (total)
        //             for easy setting use: vtx_cnt * (this.vertexSize / 4)
        // R: [none]
        public void SetVertices(float[] vertices, int offset, int length) {
            Array.Clear(this.vertices, 0, this.vertices.Length);          // Remove Existing Vertices
            Array.Copy(vertices, offset, this.vertices, 0, length); // Set New Vertices
            this.numVertices = length / this.vertexStride;  // Save Number of Vertices
        }

        //--Set Indices--//
        // D: set the specified indices in the index buffer
        // A: indices - array of indices (shorts) to set
        //    offset - offset to first index in array
        //    length - number of indices in array (from offset)
        // R: [none]
        public void SetIndices(short[] indices, int offset, int length) {
            Array.Clear(this.indices, 0, this.indices.Length);          // Clear Existing Indices
            Array.Copy(indices, offset, this.indices, 0, length); // Set New Indices
            this.numIndices = length;                       // Save Number of Indices
        }

        //--Bind--//
        // D: perform all required binding/state changes before rendering batches.
        //    USAGE: call once before calling draw() multiple times for this buffer.
        // A: [none]
        // R: [none]
        public void Bind() {
            GL.EnableClientState(All.VertexArray); // Enable Position in Vertices
            GL.VertexPointer(positionCnt, All.Float, vertexSize, vertices);  // Set Vertex Pointer

            if (hasColor) {                              // IF Vertices Have Color
                GL.EnableClientState(All.ColorArray);  // Enable Color in Vertices
                // Unsafe block to avoid creating copies of the array to allow usage of gl function without specifying an offset
                unsafe
                {
                    fixed (float* colors = this.vertices)
                    {
                        IntPtr ptr = (IntPtr)(colors + positionCnt);
                        GL.ColorPointer(COLOR_CNT, All.Float, vertexSize, ptr);  // Set Color Pointer
                    }
                }
            }

            if (hasTexCoords) {                          // IF Vertices Have Texture Coords
                GL.EnableClientState(All.TextureCoordArray);  // Enable Texture Coords in Vertices
                // Unsafe block to avoid creating copies of the array to allow usage of gl function without specifying an offset
                unsafe
                {
                    fixed (float* textureCoords = this.vertices)
                    {
                        IntPtr ptr = (IntPtr)(textureCoords + positionCnt + (hasColor ? COLOR_CNT : 0));
                        GL.TexCoordPointer(TEXCOORD_CNT, All.Float, vertexSize, ptr);  // Set Texture Coords Pointer
                    }
                }
            }

            if (hasNormals) {
                GL.EnableClientState(All.NormalArray);  // Enable Normals in Vertices
                // Unsafe block to avoid creating copies of the array to allow usage of gl function without specifying an offset
                unsafe
                {
                    fixed (float* normals = this.vertices)
                    {
                        IntPtr ptr = (IntPtr)(normals + positionCnt + (hasColor ? COLOR_CNT : 0) + (hasTexCoords ? TEXCOORD_CNT : 0)); // Set Vertex Buffer to Normals (NOTE: position based on whether color/texcoords is also specified)
                        GL.NormalPointer(All.Float, vertexSize, ptr);  // Set Normals Pointer
                    }
                }
            }
        }

        //--Draw--//
        // D: draw the currently bound vertices in the vertex/index buffers
        //    USAGE: can only be called after calling bind() for this buffer.
        // A: primitiveType - the type of primitive to draw
        //    offset - the offset in the vertex/index buffer to start at
        //    numVertices - the number of vertices (indices) to draw
        // R: [none]
        public void Draw(All primitiveType, int offset, int numVertices) {
            if (indices != null) {                       // IF Indices Exist
                // Unsafe block to avoid creating copies of the array to allow usage of gl function without specifying an offset
                unsafe
                {
                    fixed (short* indicesOffset = this.indices)
                    {
                        IntPtr ptr = new IntPtr(indicesOffset + offset); // Set Index Buffer to Specified Offset
                        GL.DrawElements(primitiveType, numVertices, All.UnsignedShort, ptr);  // Draw Indexed
                    }
                }
            } else {                                         // ELSE No Indices Exist
                GL.DrawArrays(primitiveType, offset, numVertices);  // Draw Direct (Array)
            }
        }

        //--Unbind--//
        // D: clear binding states when done rendering batches.
        //    USAGE: call once before calling draw() multiple times for this buffer.
        // A: [none]
        // R: [none]
        public void Unbind() {
            GL.DisableClientState(All.VertexArray);  // Clear Vertex Array State

            if (hasColor)                                 // IF Vertices Have Color
                GL.DisableClientState(All.ColorArray);  // Clear Color State

            if (hasTexCoords)                             // IF Vertices Have Texture Coords
                GL.DisableClientState(All.TextureCoordArray);  // Clear Texture Coords State

            if (hasNormals)                               // IF Vertices Have Normals
                GL.DisableClientState(All.NormalArray);  // Clear Normals State
        }

        //--Draw Full--//
        // D: draw the vertices in the vertex/index buffers
        //    NOTE: unoptimized version! use bind()/draw()/unbind() for batches
        // A: primitiveType - the type of primitive to draw
        //    offset - the offset in the vertex/index buffer to start at
        //    numVertices - the number of vertices (indices) to draw
        // R: [none]
        public void DrawFull(All primitiveType, int offset, int numVertices) {
            Bind();
            Draw(primitiveType, offset, numVertices);
            Unbind();
        }

        //--Set Vertex Elements--//
        // D: use these methods to alter the values (position, color, textcoords, normals) for vertices
        //    WARNING: these do NOT validate any values, ensure that the index AND specified
        //             elements EXIST before using!!
        // A: x, y, z - the x,y,z position to set in buffer
        //    r, g, b, a - the r,g,b,a color to set in buffer
        //    u, v - the u,v texture coords to set in buffer
        //    nx, ny, nz - the x,y,z normal to set in buffer
        // R: [none]
        void SetVtxPosition(int vtxIdx, float x, float y) {
            int index = vtxIdx * vertexStride;              // Calculate Actual Index
            vertices[index + 0] = x;                        // Set X
            vertices[index + 1] = y;                        // Set Y
        }
        void SetVtxPosition(int vtxIdx, float x, float y, float z) {
            int index = vtxIdx * vertexStride;              // Calculate Actual Index
            vertices[index + 0] = x;                        // Set X
            vertices[index + 1] = y;                        // Set Y
            vertices[index + 2] = z;                        // Set Z
        }
        void SetVtxColor(int vtxIdx, float r, float g, float b, float a) {
            int index = (vtxIdx * vertexStride) + positionCnt;  // Calculate Actual Index
            vertices[index + 0] = r;  // Set Red
            vertices[index + 1] = g;  // Set Green
            vertices[index + 2] = b;  // Set Blue
            vertices[index + 3] = a;  // Set Alpha
        }
        void SetVtxColor(int vtxIdx, float r, float g, float b) {
            int index = (vtxIdx * vertexStride) + positionCnt;  // Calculate Actual Index
            vertices[index + 0] = r;  // Set Red
            vertices[index + 1] = g;  // Set Green
            vertices[index + 2] = b;  // Set Blue
        }
        void SetVtxColor(int vtxIdx, float a) {
            int index = (vtxIdx * vertexStride) + positionCnt;  // Calculate Actual Index
            vertices[index + 3] = a;  // Set Alpha
        }
        void SetVtxTexCoords(int vtxIdx, float u, float v) {
            int index = (vtxIdx * vertexStride) + positionCnt + (hasColor ? COLOR_CNT : 0);  // Calculate Actual Index
            vertices[index + 0] = u;  // Set U
            vertices[index + 1] = v;  // Set V
        }
        void SetVtxNormal(int vtxIdx, float x, float y, float z) {
            int index = (vtxIdx * vertexStride) + positionCnt + (hasColor ? COLOR_CNT : 0) + (hasTexCoords ? TEXCOORD_CNT : 0);  // Calculate Actual Index
            vertices[index + 0] = x;                        // Set X
            vertices[index + 1] = y;                        // Set Y
            vertices[index + 2] = z;                        // Set Z
        }
    }
}