using UnityEngine;

namespace Core
{
    public static class HitUtility
    {
        public static bool HitScan(Vector3 origin, Vector3 direction, float strength, float range, int mask, RaycastHit[] hitBuffer, HitData[] resultBuffer, QueryTriggerInteraction query, out int hits, HitProcessor processor = null)
        {
            int rayHits = Physics.RaycastNonAlloc(origin, direction, hitBuffer, range, mask, query);
            int maxHits = Mathf.Min(rayHits, resultBuffer.Length);
            hits = 0;

            for (int i = 0; i < maxHits; i++)
            {
                RaycastHit hit = hitBuffer[i];
                HitData data = new(hit.collider, hit.point, hit.normal, hit.distance, strength);

                resultBuffer[hits++] = data;
                processor?.Invoke(in data);
            }

            return hits > 0;
        }
        public static bool HitCone(Vector3 origin, Vector3 forward, float strength, float angle, float radius, int mask, Collider[] overlapBuffer, HitData[] resultBuffer, QueryTriggerInteraction query, out int hits, HitProcessor processor = null)
        {
            hits = 0;
            int overlapHits = Physics.OverlapSphereNonAlloc(origin, radius, overlapBuffer, mask, query);
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

            for (int i = 0; i < overlapHits && hits < resultBuffer.Length; i++)
            {
                Collider collider = overlapBuffer[i];

                Vector3 point = collider.ClosestPoint(origin);
                Vector3 direction = point - origin;
                float distance = direction.magnitude;

                if (distance <= Mathf.Epsilon)
                {
                    resultBuffer[hits++] = new(collider, origin, Vector3.up, 0f, strength);
                    continue;
                }

                direction /= distance;

                if (Vector3.Dot(forward, direction) < cos)
                {
                    continue;
                }

                HitData data = new(collider, point, -direction, distance, strength);

                resultBuffer[hits++] = data;
                processor?.Invoke(in data);
            }

            return hits > 0;
        }
        public static bool HitSweep(Vector3 start, Vector3 end, float strength, float radius, int mask, RaycastHit[] hitBuffer, HitData[] resultBuffer, QueryTriggerInteraction query, out int hits, HitProcessor processor = null)
        {
            hits = 0;

            Vector3 direction = end - start;
            float distance = direction.magnitude;

            if (distance <= Mathf.Epsilon)
            {
                return false;
            }

            direction /= distance;

            int count = Physics.SphereCastNonAlloc(start, radius, direction, hitBuffer, distance, mask, query);
            int maxHits = Mathf.Min(count, resultBuffer.Length);

            for (int i = 0; i < maxHits; i++)
            {
                RaycastHit hit = hitBuffer[i];
                HitData data;

                if (hit.distance <= 0f)
                {
                    Vector3 point = hit.collider.ClosestPoint(start);
                    Vector3 normal = (point - start).normalized;

                    if (normal.sqrMagnitude < Mathf.Epsilon)
                    {
                        normal = Vector3.up;
                    }

                    data = new(hit.collider, point, normal, 0f, strength);
                }
                else
                {
                    data = new(hit.collider, hit.point, hit.normal, hit.distance, strength);
                }

                processor?.Invoke(in data);
                resultBuffer[hits++] = data;
            }

            return hits > 0;
        } 
        public static bool HitArea(Vector3 origin, float strength, float radius, int overlapMask, int obstructionMask, Collider[] overlapBuffer, RaycastHit[] obstructionBuffer, HitData[] resultBuffer, QueryTriggerInteraction query, out int hits, HitProcessor processor = null)
        {
            int areaHits = Physics.OverlapSphereNonAlloc(origin, radius, overlapBuffer, overlapMask, query);
            hits = 0;

            for (int i = 0; i < areaHits && hits < resultBuffer.Length; i++)
            {
                Collider collider = overlapBuffer[i];
                Vector3 point = collider.ClosestPoint(origin);
                Vector3 direction = point - origin;
                float distance = direction.magnitude;

                if (distance <= Mathf.Epsilon)
                {
                    resultBuffer[hits++] = new(collider, point, direction.normalized, 0f, strength);
                    continue;
                }

                direction /= distance;

                int obstructionHits = Physics.RaycastNonAlloc(origin, direction, obstructionBuffer, distance, obstructionMask, QueryTriggerInteraction.Ignore);

                bool blocked = false;

                for (int j = 0; j < obstructionHits; j++)
                {
                    RaycastHit hit = obstructionBuffer[j];

                    if (hit.collider == collider || hit.collider.transform.IsChildOf(collider.transform))
                    {
                        continue;
                    }

                    if (hit.distance < distance)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                {
                    continue;
                }

                HitData data = new(collider, point, -direction, distance, strength);

                resultBuffer[hits++] = data;
                processor?.Invoke(in data);
            }

            return hits > 0;
        }
    }
}