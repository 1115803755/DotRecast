/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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
using DotRecast.Detour;
using DotRecast.Recast.Demo.Geom;

namespace DotRecast.Recast.Demo.Builder;

public class SoloNavMeshBuilder : AbstractNavMeshBuilder
{
    public Tuple<IList<RecastBuilderResult>, NavMesh> Build(DemoInputGeomProvider geom, PartitionType partitionType,
        float cellSize, float cellHeight, float agentHeight, float agentRadius, float agentMaxClimb,
        float agentMaxSlope, int regionMinSize, int regionMergeSize, float edgeMaxLen, float edgeMaxError,
        int vertsPerPoly, float detailSampleDist, float detailSampleMaxError, bool filterLowHangingObstacles,
        bool filterLedgeSpans, bool filterWalkableLowHeightSpans)
    {
        RecastBuilderResult rcResult = BuildRecastResult(geom, partitionType, cellSize, cellHeight, agentHeight,
            agentRadius, agentMaxClimb, agentMaxSlope, regionMinSize, regionMergeSize, edgeMaxLen, edgeMaxError,
            vertsPerPoly, detailSampleDist, detailSampleMaxError, filterLowHangingObstacles, filterLedgeSpans,
            filterWalkableLowHeightSpans);
        return Tuple.Create(ImmutableArray.Create(rcResult) as IList<RecastBuilderResult>,
            BuildNavMesh(
                BuildMeshData(geom, cellSize, cellHeight, agentHeight, agentRadius, agentMaxClimb, rcResult),
                vertsPerPoly));
    }

    private NavMesh BuildNavMesh(MeshData meshData, int vertsPerPoly)
    {
        return new NavMesh(meshData, vertsPerPoly, 0);
    }

    private RecastBuilderResult BuildRecastResult(DemoInputGeomProvider geom, PartitionType partitionType, float cellSize,
        float cellHeight, float agentHeight, float agentRadius, float agentMaxClimb, float agentMaxSlope,
        int regionMinSize, int regionMergeSize, float edgeMaxLen, float edgeMaxError, int vertsPerPoly,
        float detailSampleDist, float detailSampleMaxError, bool filterLowHangingObstacles, bool filterLedgeSpans,
        bool filterWalkableLowHeightSpans)
    {
        RecastConfig cfg = new RecastConfig(partitionType, cellSize, cellHeight, agentMaxSlope, filterLowHangingObstacles,
            filterLedgeSpans, filterWalkableLowHeightSpans, agentHeight, agentRadius, agentMaxClimb, regionMinSize,
            regionMergeSize, edgeMaxLen, edgeMaxError, vertsPerPoly, detailSampleDist, detailSampleMaxError,
            SampleAreaModifications.SAMPLE_AREAMOD_WALKABLE, true);
        RecastBuilderConfig bcfg = new RecastBuilderConfig(cfg, geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax());
        RecastBuilder rcBuilder = new RecastBuilder();
        return rcBuilder.Build(geom, bcfg);
    }

    private MeshData BuildMeshData(DemoInputGeomProvider geom, float cellSize, float cellHeight, float agentHeight,
        float agentRadius, float agentMaxClimb, RecastBuilderResult result)
    {
        NavMeshDataCreateParams option = GetNavMeshCreateParams(geom, cellSize, cellHeight, agentHeight, agentRadius,
            agentMaxClimb, result);
        return UpdateAreaAndFlags(NavMeshBuilder.CreateNavMeshData(option));
    }
}