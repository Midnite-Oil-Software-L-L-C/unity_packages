#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MidniteOilSoftware.Core
{
    [CustomPropertyDrawer(typeof(RangedFloat), true)]
    public class RangedFloatDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            var minProperty = property.FindPropertyRelative("_minValue");
            var maxProperty = property.FindPropertyRelative("_maxValue");

            var minValue = minProperty.floatValue;
            var maxValue = maxProperty.floatValue;
            
            float rangeMin = 0;
            float rangeMax = 1;
            
            var ranges = (MinMaxRangeAttribute[])fieldInfo.GetCustomAttributes(typeof(MinMaxRangeAttribute), true);
            if (ranges.Length > 0)
            {
                rangeMin = ranges[0].Min;
                rangeMax = ranges[0].Max;
            }

            var minLabelRect = new Rect(position)
            {
                width = 40
            };
            GUI.Label(minLabelRect, new GUIContent(minValue.ToString("F2")));
            position.xMin += 40;

            var maxLabelRect = new Rect(position);
            maxLabelRect.xMin = maxLabelRect.xMax - 40;
            GUI.Label(maxLabelRect, new GUIContent(maxValue.ToString("F2")));
            position.xMax -= 40;
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, rangeMin, rangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                minProperty.floatValue = minValue;
                maxProperty.floatValue = maxValue;
            }
            EditorGUI.EndProperty();
        }
    }
}
#endif
