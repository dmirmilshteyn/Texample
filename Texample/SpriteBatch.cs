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
    public class SpriteBatch
    {
        public static readonly int VERTEX_SIZE = 4; // Vertex Size (in Components) ie. (X,Y,U,V)
        public static readonly short VERTICES_PER_SPRITE = 4; // Vertices Per Sprite
        public static readonly int INDICES_PER_SPRITE = 6; // Indices Per Sprite

        //--Members--//
        Vertices vertices;                                 // Vertices Instance Used for Rendering
        float[] vertexBuffer;                              // Vertex Buffer
        int bufferIndex;                                   // Vertex Buffer Start Index
        int maxSprites;                                    // Maximum Sprites Allowed in Buffer
        int numSprites;                                    // Number of Sprites Currently in Buffer

        //--Constructor--//
        // D: prepare the sprite batcher for specified maximum number of sprites
        // A: gl - the gl instance to use for rendering
        //    maxSprites - the maximum allowed sprites per batch
        public SpriteBatch(int maxSprites) {
            this.vertexBuffer = new float[maxSprites * VERTICES_PER_SPRITE * VERTEX_SIZE];  // Create Vertex Buffer
            this.vertices = new Vertices(maxSprites * VERTICES_PER_SPRITE, maxSprites * INDICES_PER_SPRITE, false, true, false);  // Create Rendering Vertices
            this.bufferIndex = 0;                           // Reset Buffer Index
            this.maxSprites = maxSprites;                   // Save Maximum Sprites
            this.numSprites = 0;                            // Clear Sprite Counter

            short[] indices = new short[maxSprites * INDICES_PER_SPRITE];  // Create Temp Index Buffer
            int len = indices.Length;                       // Get Index Buffer Length
            short j = 0;                                    // Counter
            for (int i = 0; i < len; i += INDICES_PER_SPRITE, j += VERTICES_PER_SPRITE) {  // FOR Each Index Set (Per Sprite)
                indices[i + 0] = (short)(j + 0);           // Calculate Index 0
                indices[i + 1] = (short)(j + 1);           // Calculate Index 1
                indices[i + 2] = (short)(j + 2);           // Calculate Index 2
                indices[i + 3] = (short)(j + 2);           // Calculate Index 3
                indices[i + 4] = (short)(j + 3);           // Calculate Index 4
                indices[i + 5] = (short)(j + 0);           // Calculate Index 5
            }
            vertices.SetIndices(indices, 0, len);         // Set Index Buffer for Rendering
        }

        //--Begin Batch--//
        // D: signal the start of a batch. set the texture and clear buffer
        //    NOTE: the overloaded (non-texture) version assumes that the texture is already bound!
        // A: textureId - the ID of the texture to use for the batch
        // R: [none]
        public void BeginBatch(int textureId) {
            GL.BindTexture(All.Texture2D, textureId);  // Bind the Texture
            numSprites = 0;                                 // Empty Sprite Counter
            bufferIndex = 0;                                // Reset Buffer Index (Empty)
        }
        public void BeginBatch() {
            numSprites = 0;                                 // Empty Sprite Counter
            bufferIndex = 0;                                // Reset Buffer Index (Empty)
        }

        //--End Batch--//
        // D: signal the end of a batch. render the batched sprites
        // A: [none]
        // R: [none]
        public void EndBatch() {
            if (numSprites > 0) {                        // IF Any Sprites to Render
                vertices.SetVertices(vertexBuffer, 0, bufferIndex);  // Set Vertices from Buffer
                vertices.Bind();                             // Bind Vertices
                vertices.Draw(All.Triangles, 0, numSprites * INDICES_PER_SPRITE);  // Render Batched Sprites
                vertices.Unbind();                           // Unbind Vertices
            }
        }

        //--Draw Sprite to Batch--//
        // D: batch specified sprite to batch. adds vertices for sprite to vertex buffer
        //    NOTE: MUST be called after beginBatch(), and before endBatch()!
        //    NOTE: if the batch overflows, this will render the current batch, restart it,
        //          and then batch this sprite.
        // A: x, y - the x,y position of the sprite (center)
        //    width, height - the width and height of the sprite
        //    region - the texture region to use for sprite
        // R: [none]
        public void DrawSprite(float x, float y, float width, float height, TextureRegion region) {
            if (numSprites == maxSprites) {              // IF Sprite Buffer is Full
                EndBatch();                                  // End Batch
                                                             // NOTE: leave current texture bound!!
                numSprites = 0;                              // Empty Sprite Counter
                bufferIndex = 0;                             // Reset Buffer Index (Empty)
            }

            float halfWidth = width / 2.0f;                 // Calculate Half Width
            float halfHeight = height / 2.0f;               // Calculate Half Height
            float x1 = x - halfWidth;                       // Calculate Left X
            float y1 = y - halfHeight;                      // Calculate Bottom Y
            float x2 = x + halfWidth;                       // Calculate Right X
            float y2 = y + halfHeight;                      // Calculate Top Y

            vertexBuffer[bufferIndex++] = x1;               // Add X for Vertex 0
            vertexBuffer[bufferIndex++] = y1;               // Add Y for Vertex 0
            vertexBuffer[bufferIndex++] = region.u1;        // Add U for Vertex 0
            vertexBuffer[bufferIndex++] = region.v2;        // Add V for Vertex 0

            vertexBuffer[bufferIndex++] = x2;               // Add X for Vertex 1
            vertexBuffer[bufferIndex++] = y1;               // Add Y for Vertex 1
            vertexBuffer[bufferIndex++] = region.u2;        // Add U for Vertex 1
            vertexBuffer[bufferIndex++] = region.v2;        // Add V for Vertex 1

            vertexBuffer[bufferIndex++] = x2;               // Add X for Vertex 2
            vertexBuffer[bufferIndex++] = y2;               // Add Y for Vertex 2
            vertexBuffer[bufferIndex++] = region.u2;        // Add U for Vertex 2
            vertexBuffer[bufferIndex++] = region.v1;        // Add V for Vertex 2

            vertexBuffer[bufferIndex++] = x1;               // Add X for Vertex 3
            vertexBuffer[bufferIndex++] = y2;               // Add Y for Vertex 3
            vertexBuffer[bufferIndex++] = region.u1;        // Add U for Vertex 3
            vertexBuffer[bufferIndex++] = region.v1;        // Add V for Vertex 3

            numSprites++;                                   // Increment Sprite Count
        }
    }
}