using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

namespace Game.Utility
{
    static class UpdateFequency 
    {
        public const int UPDATE_10 = 10;
        public const int UPDATE_50 = 50;
        public const int UPDATE_100 = 100;
    }

    public struct CircularBuffer<T>
    {
        public T[] buffer;
        public int size;

        public CircularBuffer(int _size)
        {
            buffer = new T[_size];
            size = _size;
        }
        public void Append(T value)
        {
            if(size != buffer.Length) size = buffer.Length;
            int finalIndex = size - 1;
            for (int i = 0; i < finalIndex; i++)
            {
                buffer[i] = buffer[i + 1];
            }
            buffer[finalIndex] = value;            
        }       

    }
    public static class Misc 
    {
        public static bool TryGetNearestPoint(Vector3 position, float maxDistance, out Vector3 nearestPoint)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
            {
                nearestPoint = hit.position;
                return true;
            }

            nearestPoint = position;
            return false;
        }

        public static IEnumerator LerpAnimationLayer(Animator animator, int layerIndex, float startWeight, float endWeight, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float weight = Mathf.Lerp(startWeight, endWeight, t);
                animator.SetLayerWeight(layerIndex, weight);
                yield return null;
            }

            animator.SetLayerWeight(layerIndex, endWeight); // Snap to final
        }
        public static List<T> GetAllComponentsInFamily<T>(GameObject root, bool includeInactive = false)
        {
            List<T> results = new List<T>();
            Recurse(root.transform, results, includeInactive);
            return results;
        }

        private static void Recurse<T>(Transform current, List<T> results, bool includeInactive)
        {
            if (!includeInactive && !current.gameObject.activeInHierarchy)
                return;

            // Add all components of type T on this GameObject
            T[] comps = current.GetComponents<T>();
            if (comps != null && comps.Length > 0)
                results.AddRange(comps);

            // Recurse into children
            foreach (Transform child in current)
                Recurse(child, results, includeInactive);
        }
    }
    class GameMath
    {
        public enum Axis {Forward, Backward,  Left, Right, Up, Down}

        public static Vector3 ClosestToPoint(Vector3[] arr, Vector3 point) 
        {
            Vector3 bestVec = new Vector3();
            float bestDist = float.MaxValue;
            foreach (Vector3 element in arr) 
            {
                float dist = Vector3.Distance(element, point);
                if (dist < bestDist) 
                {
                    bestVec = element;
                    bestDist = dist;
                }
            }
            return bestVec;
        }
        public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }
        public static bool IsNumerical(string input)
        {
            foreach (char c in input)
            {
                if (c < '0' || c > '9')
                {
          
                    return false; // Character is not a digit
                }
            }
            return true; // All characters are digits

        }
        public static Vector3 PointAlongLine(Vector3 P, Vector3 target, float distance)
        {
            // Direction vector from P to target
            Vector3 dir = (target - P).normalized;

            // D is P + direction * distance
            return P + dir * distance;
        }
        public static Vector3 RandomUnitVector() 
        {
            float x = UnityEngine.Random.Range(-1f, 1f);
            float y = UnityEngine.Random.Range(-1f, 1f);
            float z = UnityEngine.Random.Range(-1f, 1f);

            return new Vector3(x, y, z);
        }
        public static Vector3 RandomUnitVectorUp()
        {
            float x = UnityEngine.Random.Range(-1f, 1f);
            float y = UnityEngine.Random.Range(0f, 1f);
            float z = UnityEngine.Random.Range(-1f, 1f);

            return new Vector3(x, y, z);
        }

        public static bool WorldPositionObscured(Vector3 worldPosition, List<Collider> filterColliders)
        {            
            Vector3 direction = worldPosition - Camera.main.transform.position;
            Ray ray = new Ray(Camera.main.transform.position, direction);
            //Debug.DrawLine(ray.origin, ray.direction, Color.magenta, 0.05f);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.collider.name == "Player")
                    return false;

                if (filterColliders.Contains((Collider)hit.collider))
                {
                    return false;
                }

                //Debug.Log(hit.collider.name);
                return true;
            }

            return false;
        }

        //Detects if a a ray from main camera through a screenpoint intersects any colliders barring the "filterColliders" 
        public static bool ScreenPositionObscured(Vector2 screenPosition, List<Collider> filterColliders) 
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
            Vector3 direction = worldPos - Camera.main.transform.position;
            Ray ray = new Ray(Camera.main.transform.position, direction);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100)) 
            {
                if (hit.collider.name == "Player")
                    return false;

                if (filterColliders.Contains(hit.collider))
                    return false;

                return true;
            }

            return false;
        }
        public static float GetAxialSpeed(Axis axis, Rigidbody rb, Transform trans)
        {
            switch (axis)
            {
                case Axis.Forward:
                    Vector3 forwardVec = Vector3.Project(rb.linearVelocity, trans.forward);
                    return Vector3.Dot(forwardVec, trans.forward.normalized);

                case Axis.Backward:
                    Vector3 backwardVec = Vector3.Project(rb.linearVelocity, -trans.forward);
                    return Vector3.Dot(backwardVec, -trans.forward.normalized);

                case Axis.Left:
                    Vector3 leftVec = Vector3.Project(rb.linearVelocity, -trans.right);
                    return Vector3.Dot(leftVec, -trans.right.normalized);

                case Axis.Right:
                    Vector3 rightVec = Vector3.Project(rb.linearVelocity, trans.right);
                    return Vector3.Dot(rightVec, trans.right.normalized);

                case Axis.Up:
                    Vector3 upVec = Vector3.Project(rb.linearVelocity, trans.up);
                    return Vector3.Dot(upVec, trans.up.normalized);

                case Axis.Down:
                    Vector3 downVec = Vector3.Project(rb.linearVelocity, -trans.up);
                    return Vector3.Dot(downVec, -trans.up.normalized);

                default:
                    return 0f;
            }
        }

        public static T GetRandomElement<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List cannot be null or empty");

            int index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }

        public static double SecondsSince(DateTime time) 
        {
            return DateTime.Now.Subtract(time).TotalSeconds;
        }
        public static bool HasElapstedSince(float seconds, DateTime time) 
        {
            double d = Convert.ToDouble(seconds);
            return d >= SecondsSince(time);
        }

        public static float ClampEulerAngle(float angle, float min, float max)
        {
            if (angle > 180f)
                angle -= 360f;
            angle = Mathf.Clamp(angle, min, max);
            if (angle < -180f)
                angle += 360f;
            return angle;
        }


        public static T[] Append<T>(T[] array, T value)
        {
            T[] newArray = new T[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }
            newArray[array.Length] = value;
            return newArray;
        }

        public static Vector3 AverageVector(Vector3[] vecs) 
        {
            float x = 0f, y = 0f, z = 0f;
            foreach (Vector3 v in vecs) 
            {
                x += v.x;
                y += v.y;
                z += v.z;
            }
            Vector3 total = new Vector3(x, y, z);
            return total / vecs.Length;  
        }

        public static float AbsoluteDifference(float a, float b) 
        {
            a = Mathf.Abs(a);
            b = Mathf.Abs(b);
            return Mathf.Abs(a - b);
        }
        public static Vector3 AbsVec(Vector3 vec) 
        {
            return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.x), Mathf.Abs(vec.x));
        }

        public static Vector3 VecXZ(Vector3 vec) 
        {
            return new Vector3(vec.x, 0f, vec.z);
        }

        
    }

}
