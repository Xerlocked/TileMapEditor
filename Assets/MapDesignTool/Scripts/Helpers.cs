using System;
using UnityEditor;
using UnityEngine;

namespace MapDesignTool.Scripts
{
    public static class Helpers
    {
        [Serializable]
        public struct TransformData
        {
            public Vector3 localPosition;
            public Vector3 localRotation;
            public Vector3 localScale;

            public TransformData(Transform transform)
            {
                localPosition = transform.localPosition;
                localRotation = transform.localEulerAngles;
                localScale = transform.localScale;
            }

            public void ApplyTransform(Transform transform)
            {
                transform.localPosition = localPosition;
                transform.localEulerAngles = localRotation;
                transform.localScale = localScale;
            }

            public bool CompareTransform(Transform transform)
            {
                return transform.localPosition == localPosition || transform.localEulerAngles == localRotation ||
                       transform.localScale == localScale;
            }
        }
        public static GameObject Select<T>(Event e) where T : UnityEngine.Component
        {
            Camera cam = Camera.current;

            if (cam != null)
            {
                RaycastHit hit;
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        GameObject gameObj = hit.collider.gameObject;
                        if (gameObj.GetComponent<T>() != null)
                        {
                            //e.Use();
                            //Selection.activeGameObject = gameObj;
                            return gameObj;
                        }
                    }
                }
            }
            return null;
        }
        
        
        public static bool HasChild(Transform transform)
        {
            return (transform.childCount > 0) ? true : false;
        }
    
        public static void SwapToZero(out float dst, ref float src)
        {
            dst = src;
            src = 0;
        }
    
    }
}