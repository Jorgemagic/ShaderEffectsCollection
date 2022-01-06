# Shader Effects Collection
This is a collection of visual effects created in [Evergine](https://evergine.com/). I have created these visual effects only for fun and you can use it to create your own effects.

## [Distortion](Distortion/)
[![Distortion Video](Images/DistortionVideo.png)](https://www.youtube.com/watch?v=_LkSGxI8xbE)

Distortion is an effect of built-in Evergine. You only need to apply the _DistortionMat_ included in the **Evergine.Core** package and added a postprocessing volume to your scene and enabled it in Tonemapping section.

## [Stencil buffer trick](StencilBuffer/)
[![Stencil buffer trick Video](Images/StencilTrickVideo.png)](https://www.youtube.com/watch?v=unQRQKMhPYo)

This demo was created without wrote any line of code. The idea is that you can draw a geometry (plane, circle, anything) and draw only in the stencil buffer with a specify value. Now the next drawcall draw another geometry only whether the stencil buffer has this specify value. In the following picture I explain the trick:

![Stencil buffer Trick](Images/StencilBufferTrick.png)

## [Height map](HeightMap/)
[![Height Map Video](Images/HeightmapVideo.png)](https://www.youtube.com/watch?v=XSc4HVF5V8g)

In this demo, I created a MultipleMeshesPlane component that allows creating a plane with multiple horizontal and vertical segments. That is important to increase the resolution of your terrain. I am using a ushort (16 bit) indexbuffer so the maximum number of indices used in a drawcall are ushort.MaxValue (65535 indices). That is the reason why a split the plane geometry in multiple meshes. Each mesh has 100x100 segments 10000 quads so 60000 indices.

I applied the height map texture in the vertex shader to deform the geometry, you only need to remember that you must use shader model 5.0 to sample a texture in vertex shader and the code is:

```cpp
float height = HeightTexture.SampleLevel(Sampler, input.TexCoord,0).r;
float3 position = input.Position + input.Normal * (height * HeightAmount);
output.Position = mul(position, WorldViewProj);		
```
Where the HeightAmount is the maximum height amount of the terrain.

Finally, the most important part is computing the normals based on the heightmap. For this proposal, I sampled the heightmap 4 times in a cross to calculate the slope in the pixel shader stage.

```cpp
float3 getNormal(float2 uv, float texelSize) 
{
    float u = HeightTexture.SampleLevel(Sampler,  uv + texelSize * float2(0.0, -1.0), 0).r;
    float r = HeightTexture.SampleLevel(Sampler,  uv + texelSize * float2(-1.0, 0.0),0).r;
    float l = HeightTexture.SampleLevel(Sampler,  uv + texelSize * float2(1.0, 0.0),0).r;
    float d = HeightTexture.SampleLevel(Sampler,  uv + texelSize * float2(0.0, 1.0),0).r;

    float3 n;
    n.z = u - d;
    n.x = r - l;
    n.y = 1.0 / 256;
    return normalize(n);
}
```
I found a good reference for creation terrain based on heightmap in the paper **Terrain Rendering in Frostbite** from SIGGRAPH 2007 that you can download [here](http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.161.8979&rep=rep1&type=pdf)

## [Grass shader](Grass/)
[![Grass shader Video](Images/GrassVideo.png)](https://www.youtube.com/watch?v=yJBb9ijypsI)

For this demo, I created a SingleMeshPlane that creates a plane with a maximum of 100 horizontal and vertical segments with the following pattern.

<img src="Images/grassMeshPattern.png" alt="Grass mesh pattern" width="300"/>

Using a geometry shader I created a blade of grass per each mesh vertex. The blade grass is a shape with several segments to allow create a curve blade.

<img src="https://roystan.net/media/tutorials/grass/grass-construction.gif" alt="Blade mesh" width="300"/>

This demo is based on this great [post](https://roystan.net/articles/grass-shader.html) by Roystan.