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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Recast;
using DotRecast.Recast.Geom;

namespace DotRecast.Detour.TileCache
{
    public class DtTileCacheLayerBuilder
    {
        private IDtTileCacheCompressorFactory _compFactory;

        public DtTileCacheLayerBuilder(IDtTileCacheCompressorFactory compFactory)
        {
            _compFactory = compFactory;
        }

        public List<byte[]> Build(IInputGeomProvider geom, RcConfig cfg, RcByteOrder order, bool cCompatibility, int threads, int tw, int th)
        {
            if (threads == 1)
            {
                return BuildSingleThread(geom, cfg, order, cCompatibility, tw, th);
            }

            return BuildMultiThread(geom, cfg, order, cCompatibility, tw, th, threads);
        }

        private List<byte[]> BuildSingleThread(IInputGeomProvider geom, RcConfig cfg, RcByteOrder order, bool cCompatibility, int tw, int th)
        {
            List<byte[]> layers = new List<byte[]>();
            for (int y = 0; y < th; ++y)
            {
                for (int x = 0; x < tw; ++x)
                {
                    var list = BuildTileCacheLayer(geom, cfg, x, y, order, cCompatibility);
                    layers.AddRange(list);
                }
            }

            return layers;
        }


        private List<byte[]> BuildMultiThread(IInputGeomProvider geom, RcConfig cfg, RcByteOrder order, bool cCompatibility, int tw, int th, int threads)
        {
            var results = new List<DtTileCacheBuildResult>();
            for (int y = 0; y < th; ++y)
            {
                for (int x = 0; x < tw; ++x)
                {
                    int tx = x;
                    int ty = y;
                    var task = Task.Run(() => BuildTileCacheLayer(geom, cfg, tx, ty, order, cCompatibility));
                    results.Add(new DtTileCacheBuildResult(tx, ty, task));
                }
            }

            return results
                .SelectMany(x => x.task.Result)
                .ToList();
        }

        protected virtual RcHeightfieldLayerSet BuildHeightfieldLayerSet(IInputGeomProvider geom, RcConfig cfg, int tx, int ty)
        {
            RecastBuilder rcBuilder = new RecastBuilder();
            RcVec3f bmin = geom.GetMeshBoundsMin();
            RcVec3f bmax = geom.GetMeshBoundsMax();
            RecastBuilderConfig builderCfg = new RecastBuilderConfig(cfg, bmin, bmax, tx, ty);
            RcHeightfieldLayerSet lset = rcBuilder.BuildLayers(geom, builderCfg);
            return lset;
        }

        protected virtual List<byte[]> BuildTileCacheLayer(IInputGeomProvider geom, RcConfig cfg, int tx, int ty, RcByteOrder order, bool cCompatibility)
        {
            RcHeightfieldLayerSet lset = BuildHeightfieldLayerSet(geom, cfg, tx, ty);
            List<byte[]> result = new List<byte[]>();
            if (lset != null)
            {
                DtTileCacheBuilder builder = new DtTileCacheBuilder();
                for (int i = 0; i < lset.layers.Length; ++i)
                {
                    RcHeightfieldLayer layer = lset.layers[i];

                    // Store header
                    DtTileCacheLayerHeader header = new DtTileCacheLayerHeader();
                    header.magic = DtTileCacheLayerHeader.DT_TILECACHE_MAGIC;
                    header.version = DtTileCacheLayerHeader.DT_TILECACHE_VERSION;

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

                    var comp = _compFactory.Get(cCompatibility);
                    var bytes = builder.CompressTileCacheLayer(header, layer.heights, layer.areas, layer.cons, order, cCompatibility, comp);
                    result.Add(bytes);
                }
            }

            return result;
        }
    }
}