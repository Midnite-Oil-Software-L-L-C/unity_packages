#if UNITY_EDITOR
using MidniteOilSoftware.Core.Audio;
using UnityEditor;
using UnityEngine;

namespace MidniteOilSoftware.Core
{
    [CustomEditor(typeof(AudioEvent))]
    public class AudioEventEditor : Editor
    {
        AudioSource _previewSource;

        public void OnEnable()
        {
            _previewSource = EditorUtility.CreateGameObjectWithHideFlags(
                    "Audio Preview",
                    HideFlags.HideAndDontSave,
                    typeof(AudioSource))
                .GetComponent<AudioSource>();
        }
        
        public void OnDisable()
        {
            DestroyImmediate(_previewSource.gameObject);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Preview"))
            {
                AudioEvent audioEvent = (AudioEvent)target;
                audioEvent.Play(_previewSource);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif