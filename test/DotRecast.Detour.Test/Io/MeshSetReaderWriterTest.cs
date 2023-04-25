/*
recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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
using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using NUnit.Framework;
using static DotRecast.Core.RecastMath;

namespace DotRecast.Detour.Test.Io;

[Parallelizable]
public class MeshSetReaderWriterTest
{
    private readonly MeshSetWriter writer = new MeshSetWriter();
    private readonly MeshSetReader reader = new MeshSetReader();
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
    private const int m_tileSize = 32;
    private const int m_maxTiles = 128;
    private const int m_maxPolysPerTile = 0x8000;

    [Test]
    public void test()
    {
        InputGeomProvider geom = ObjImporter.load(Loader.ToBytes("dungeon.obj"));

        NavMeshSetHeader header = new NavMeshSetHeader();
        header.magic = NavMeshSetHeader.NAVMESHSET_MAGIC;
        header.version = NavMeshSetHeader.NAVMESHSET_VERSION;
        header.option.orig = geom.getMeshBoundsMin();
        header.option.tileWidth = m_tileSize * m_cellSize;
        header.option.tileHeight = m_tileSize * m_cellSize;
        header.option.maxTiles = m_maxTiles;
        header.option.maxPolys = m_maxPolysPerTile;
        header.numTiles = 0;
        NavMesh mesh = new NavMesh(header.option, 6);

        Vector3f bmin = geom.getMeshBoundsMin();
        Vector3f bmax = geom.getMeshBoundsMax();
        int[] twh = DotRecast.Recast.Recast.calcTileCount(bmin, bmax, m_cellSize, m_tileSize, m_tileSize);
        int tw = twh[0];
        int th = twh[1];
        for (int y = 0; y < th; ++y)
        {
            for (int x = 0; x < tw; ++x)
            {
                RecastConfig cfg = new RecastConfig(true, m_tileSize, m_tileSize,
                    RecastConfig.calcBorder(m_agentRadius, m_cellSize), PartitionType.WATERSHED, m_cellSize, m_cellHeight,
                    m_agentMaxSlope, true, true, true, m_agentHeight, m_agentRadius, m_agentMaxClimb, m_regionMinArea,
                    m_regionMergeArea, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly, true, m_detailSampleDist,
                    m_detailSampleMaxError, SampleAreaModifications.SAMPLE_AREAMOD_GROUND);
                RecastBuilderConfig bcfg = new RecastBuilderConfig(cfg, bmin, bmax, x, y);
                TestDetourBuilder db = new TestDetourBuilder();
                MeshData data = db.build(geom, bcfg, m_agentHeight, m_agentRadius, m_agentMaxClimb, x, y, true);
                if (data != null)
                {
                    mesh.removeTile(mesh.getTileRefAt(x, y, 0));
                    mesh.addTile(data, 0, 0);
                }
            }
        }

        using var ms = new MemoryStream();
        using var os = new BinaryWriter(ms);
        writer.write(os, mesh, ByteOrder.LITTLE_ENDIAN, true);
        ms.Seek(0, SeekOrigin.Begin);

        using var @is = new BinaryReader(ms);
        mesh = reader.read(@is, 6);
        Assert.That(mesh.getMaxTiles(), Is.EqualTo(128));
        Assert.That(mesh.getParams().maxPolys, Is.EqualTo(0x8000));
        Assert.That(mesh.getParams().tileWidth, Is.EqualTo(9.6f).Within(0.001f));
        List<MeshTile> tiles = mesh.getTilesAt(6, 9);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(2));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(7 * 3));
        tiles = mesh.getTilesAt(2, 9);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(2));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(9 * 3));
        tiles = mesh.getTilesAt(4, 3);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(3));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(6 * 3));
        tiles = mesh.getTilesAt(2, 8);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(5));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(17 * 3));
    }
}