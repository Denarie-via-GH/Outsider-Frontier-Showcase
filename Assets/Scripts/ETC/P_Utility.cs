using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Personal
{
    namespace Utils
    {
        public class SingleBoolArgs : EventArgs
        {
            public bool value;
        }
        public class SingleStringArgs : EventArgs
        {
            public string value;
        }
        public class BoolAndStringArgs : EventArgs
        {
            public bool bool_value;
            public string string_value;
        }
        public class FloatArgs : EventArgs
        {
            public float value;
        }
        public class DoubleFloatArgs : EventArgs
        {
            public float value_1;
            public float value_2;
        }

        public class Utility : MonoBehaviour
        {
            #region CREATE FUNCTION

            // Create gameobject on world position [placeholder version]
            public static void CreateGameObject(Vector3 Position)
            {
                GameObject TEMP = new GameObject("PlaceholderObject");
                TEMP.transform.position = Position;
            }

            // Create gameobject on world position [prefab version]
            public static void CreateGameObject(Vector3 Position, Quaternion Rotation, GameObject Prefab)
            {
                GameObject TEMP = Instantiate(Prefab, Position, Rotation);
            }

            // Create text on world position [short Version]
            public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 32, Color? color = null, TextAnchor textAnchor = TextAnchor.MiddleCenter, TextAlignment textAlignment = TextAlignment.Center, int sortingOrder = 0)
            {
                if (color == null) color = Color.white;
                return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
            }

            // Create text on world position [full Version]
            public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
            {
                GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
                Transform transform = gameObject.transform;
                transform.SetParent(parent, false);
                transform.localPosition = localPosition;
                
                TextMesh textMesh = gameObject.GetComponent<TextMesh>();
                textMesh.anchor = textAnchor;
                textMesh.alignment = textAlignment;
                textMesh.text = text;
                textMesh.fontSize = fontSize;
                textMesh.color = color;
                textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

                return textMesh;
            }

            #endregion

            #region GET FUNCTION
            public static Vector3 GetMouseWorldPOS()
            {
                return (GetMouseWorldPOS(Input.mousePosition, Camera.main));
            }
            public static Vector3 GetMouseWorldPOS(Vector3 Positioin, Camera worldMainCamera)
            {
                Vector3 POS = worldMainCamera.ScreenToWorldPoint(Positioin);
                POS.z = 0;
                return POS;
            }

            public static Vector3 GetMouseWorldPOS_Z()
            {
                return (GetMouseWorldPOS_Z(Input.mousePosition, Camera.main));
            }
            public static Vector3 GetMouseWorldPOS_Z(Vector3 Position, Camera worldMainCamera)
            {
                Vector3 POS = worldMainCamera.ScreenToWorldPoint(Position);
                return POS;
            }
            #endregion

            public static float GetAngleFromVectorFloat(Vector3 dir)
            {
                dir = dir.normalized;
                float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (n < 0) n += 360;
                return n;
            }

            public static Vector3 ReturnDegreeRotationToVector(Vector3 vec, float angle)
            {
                return Quaternion.Euler(0, 0, angle) * vec;
            }
        }
    }
}