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
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour.Crowd
{
    using static RcMath;

    public class DtLocalBoundary
    {
        public const int MAX_LOCAL_SEGS = 8;


        RcVec3f m_center = new RcVec3f();
        List<DtSegment> m_segs = new List<DtSegment>();
        List<long> m_polys = new List<long>();

        public DtLocalBoundary()
        {
            m_center.x = m_center.y = m_center.z = float.MaxValue;
        }

        public void Reset()
        {
            m_center.x = m_center.y = m_center.z = float.MaxValue;
            m_polys.Clear();
            m_segs.Clear();
        }

        protected void AddSegment(float dist, SegmentVert s)
        {
            // Insert neighbour based on the distance.
            DtSegment seg = new DtSegment();
            seg.s[0] = s.vmin;
            seg.s[1] = s.vmax;
            //Array.Copy(s, seg.s, 6);
            seg.d = dist;
            if (0 == m_segs.Count)
            {
                m_segs.Add(seg);
            }
            else if (dist >= m_segs[m_segs.Count - 1].d)
            {
                if (m_segs.Count >= MAX_LOCAL_SEGS)
                {
                    return;
                }

                m_segs.Add(seg);
            }
            else
            {
                // Insert inbetween.
                int i;
                for (i = 0; i < m_segs.Count; ++i)
                {
                    if (dist <= m_segs[i].d)
                    {
                        break;
                    }
                }

                m_segs.Insert(i, seg);
            }

            while (m_segs.Count > MAX_LOCAL_SEGS)
            {
                m_segs.RemoveAt(m_segs.Count - 1);
            }
        }

        public void Update(long refs, RcVec3f pos, float collisionQueryRange, DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            if (refs == 0)
            {
                Reset();
                return;
            }

            m_center = pos;
            // First query non-overlapping polygons.
            Result<FindLocalNeighbourhoodResult> res = navquery.FindLocalNeighbourhood(refs, pos, collisionQueryRange,
                filter);
            if (res.Succeeded())
            {
                m_polys = res.result.GetRefs();
                m_segs.Clear();
                // Secondly, store all polygon edges.
                for (int j = 0; j < m_polys.Count; ++j)
                {
                    Result<GetPolyWallSegmentsResult> result = navquery.GetPolyWallSegments(m_polys[j], false, filter);
                    if (result.Succeeded())
                    {
                        GetPolyWallSegmentsResult gpws = result.result;
                        for (int k = 0; k < gpws.CountSegmentRefs(); ++k)
                        {
                            SegmentVert s = gpws.GetSegmentVert(k);
                            var s0 = RcVec3f.Of(s[0], s[1], s[2]);
                            var s3 = RcVec3f.Of(s[3], s[4], s[5]);

                            // Skip too distant segments.
                            var distSqr = DetourCommon.DistancePtSegSqr2D(pos, s0, s3, out var tseg);
                            if (distSqr > Sqr(collisionQueryRange))
                            {
                                continue;
                            }

                            AddSegment(distSqr, s);
                        }
                    }
                }
            }
        }

        public bool IsValid(DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            if (m_polys.Count == 0)
            {
                return false;
            }

            // Check that all polygons still pass query filter.
            foreach (long refs in m_polys)
            {
                if (!navquery.IsValidPolyRef(refs, filter))
                {
                    return false;
                }
            }

            return true;
        }

        public RcVec3f GetCenter()
        {
            return m_center;
        }

        public RcVec3f[] GetSegment(int j)
        {
            return m_segs[j].s;
        }

        public int GetSegmentCount()
        {
            return m_segs.Count;
        }
    }
}