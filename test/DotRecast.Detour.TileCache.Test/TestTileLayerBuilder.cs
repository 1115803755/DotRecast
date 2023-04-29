/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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

using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using static DotRecast.Core.RecastMath;

namespace DotRecast.Detour.TileCache.Test;

public class TestTileLayerBuilder : AbstractTileLayersBuilder
{
    private const float m_cellSize = 0.3f;
    private const float m_cellHeight = 0.2f;
    private const float m_agentHeight = 2.0f;
    private const float m_agentRadius = 0.6f;
    private const float m_agentMaxClimb = 0.9f;
    private const float m_agentMaxSlope = 45.0f;
    private const int m_regionMinSize = 8;
    private const int m_regionMergeSize = 20;
    private const float m_regionMinArea = m_regionMinSize * m_regionMinSize * m_cellSize * m_cellSize;
    private const float m_regionMergeArea = m_regionMergeSize * m_regionMergeSize * m_cellSize * m_cellSize;
    private const float m_edgeMaxLen = 12.0f;
    private const float m_edgeMaxError = 1.3f;
    private const int m_vertsPerPoly = 6;
    private const float m_detailSampleDist = 6.0f;
    private const float m_detailSampleMaxError = 1.0f;
    private readonly RecastConfig rcConfig;
    private const int m_tileSize = 48;
    protected readonly InputGeomProvider geom;
    private readonly int tw;
    private readonly int th;

    public TestTileLayerBuilder(InputGeomProvider geom)
    {
        this.geom = geom;
        rcConfig = new RecastConfig(true, m_tileSize, m_tileSize, RecastConfig.calcBorder(m_agentRadius, m_cellSize),
            PartitionType.WATERSHED, m_cellSize, m_cellHeight, m_agentMaxSlope, true, true, true, m_agentHeight,
            m_agentRadius, m_agentMaxClimb, m_regionMinArea, m_regionMergeArea, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly,
            true, m_detailSampleDist, m_detailSampleMaxError, SampleAreaModifications.SAMPLE_AREAMOD_GROUND);
        Vector3f bmin = geom.getMeshBoundsMin();
        Vector3f bmax = geom.getMeshBoundsMax();
        int[] twh = Recast.Recast.calcTileCount(bmin, bmax, m_cellSize, m_tileSize, m_tileSize);
        tw = twh[0];
        th = twh[1];
    }

    public List<byte[]> build(ByteOrder order, bool cCompatibility, int threads)
    {
        return build(order, cCompatibility, threads, tw, th);
    }

    public int getTw()
    {
        return tw;
    }

    public int getTh()
    {
        return th;
    }

    protected override List<byte[]> build(int tx, int ty, ByteOrder order, bool cCompatibility)
    {
        HeightfieldLayerSet lset = getHeightfieldSet(tx, ty);
        List<byte[]> result = new();
        if (lset != null)
        {
            TileCacheBuilder builder = new TileCacheBuilder();
            for (int i = 0; i < lset.layers.Length; ++i)
            {
                HeightfieldLayerSet.HeightfieldLayer layer = lset.layers[i];

                // Store header
                TileCacheLayerHeader header = new TileCacheLayerHeader();
                header.magic = TileCacheLayerHeader.DT_TILECACHE_MAGIC;
                header.version = TileCacheLayerHeader.DT_TILECACHE_VERSION;

                // Tile layer location in the navmesh.
                header.tx = tx;
                header.ty = ty;
                header.tlayer = i;
                header.bmin = layer.bmin;
                header.bmax = layer.bmax;

                // Tile info.
                header.width = layer.width;
                header.height = layer.height;
                header.minx = layer.minx;
                header.maxx = layer.maxx;
                header.miny = layer.miny;
                header.maxy = layer.maxy;
                header.hmin = layer.hmin;
                header.hmax = layer.hmax;
                result.Add(builder.compressTileCacheLayer(header, layer.heights, layer.areas, layer.cons, order, cCompatibility));
            }
        }

        return result;
    }

    protected HeightfieldLayerSet getHeightfieldSet(int tx, int ty)
    {
        RecastBuilder rcBuilder = new RecastBuilder();
        Vector3f bmin = geom.getMeshBoundsMin();
        Vector3f bmax = geom.getMeshBoundsMax();
        RecastBuilderConfig cfg = new RecastBuilderConfig(rcConfig, bmin, bmax, tx, ty);
        HeightfieldLayerSet lset = rcBuilder.buildLayers(geom, cfg);
        return lset;
    }
}
