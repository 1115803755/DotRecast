/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotRecast.Core;

namespace DotRecast.Recast.Geom
{
    public class SingleTrimeshInputGeomProvider : InputGeomProvider
    {
        private readonly Vector3f bmin;
        private readonly Vector3f bmax;
        private readonly TriMesh _mesh;

        public SingleTrimeshInputGeomProvider(float[] vertices, int[] faces)
        {
            bmin = Vector3f.Zero;
            bmax = Vector3f.Zero;
            RecastVectors.copy(ref bmin, vertices, 0);
            RecastVectors.copy(ref bmax, vertices, 0);
            for (int i = 1; i < vertices.Length / 3; i++)
            {
                RecastVectors.min(ref bmin, vertices, i * 3);
                RecastVectors.max(ref bmax, vertices, i * 3);
            }

            _mesh = new TriMesh(vertices, faces);
        }

        public Vector3f getMeshBoundsMin()
        {
            return bmin;
        }

        public Vector3f getMeshBoundsMax()
        {
            return bmax;
        }

        public IEnumerable<TriMesh> meshes()
        {
            return ImmutableArray.Create(_mesh);
        }

        public IList<ConvexVolume> convexVolumes()
        {
            return ImmutableArray<ConvexVolume>.Empty;
        }
    }
}