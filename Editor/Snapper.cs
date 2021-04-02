using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace Tymski
{
    public class Snapper : OdinEditorWindow
    {
        [MenuItem("Tools/Tymski/Snapper")]
        private static void OpenWindow()
        {
            GetWindow<Snapper>().Show();
        }

        [HorizontalGroup("Position"), ToggleLeft, SerializeField] bool snapPosition = true;
        [HorizontalGroup("Position"), HideLabel, EnableIf("snapPosition"), SerializeField] double positionStep = 0.001;

        [HorizontalGroup("Rotation"), ToggleLeft, SerializeField] bool snapRotation = true;
        [HorizontalGroup("Rotation"), HideLabel, EnableIf("snapRotation"), SerializeField] double rotationStep = 0.01;

        [HorizontalGroup("Scale"), ToggleLeft, SerializeField] bool snapScale = true;
        [HorizontalGroup("Scale"), HideLabel, EnableIf("snapScale"), SerializeField] double scaleStep = 0.01;

        [Button(ButtonSizes.Large)]
        public void Snap()
        {
            if (Selection.gameObjects.Length > 1) Debug.Log($"Snapping {Selection.gameObjects.Length} objects.");
            Undo.RecordObjects(Selection.gameObjects.Select(go => go.transform).ToArray(), "Snap");
            UnityEngine.Object[] ary = new UnityEngine.Object[Selection.gameObjects.Length];

            foreach (GameObject go in Selection.gameObjects)
            {
                Transform t = go.transform;
                SnapPosition(t);
                SnapRotation(t);
                SnapScale(t);
                SetDirty(t);
            }
        }

        void SnapPosition(Transform t)
        {
            if (!snapPosition) return;

            t.localPosition = new Vector3(
               (float)SnapTo(t.localPosition.x, positionStep),
               (float)SnapTo(t.localPosition.y, positionStep),
               (float)SnapTo(t.localPosition.z, positionStep)
            );
        }

        void SnapRotation(Transform t)
        {
            if (!snapRotation) return;

            Vector3 newVector = new Vector3(
                (float)SnapTo(t.localEulerAngles.x, rotationStep),
                (float)SnapTo(t.localEulerAngles.y, rotationStep),
                (float)SnapTo(t.localEulerAngles.z, rotationStep)
            );

            t.localEulerAngles = newVector;

            MethodInfo info = typeof(Transform).GetMethod("SetLocalEulerHint", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.SetProperty);
            info.Invoke(t, new object[] { null });
        }

        void SnapScale(Transform t)
        {
            if (!snapScale) return;
            t.localScale = new Vector3(
                (float)SnapTo(t.localScale.x, scaleStep),
                (float)SnapTo(t.localScale.y, scaleStep),
                (float)SnapTo(t.localScale.z, scaleStep)
            );
        }

        void SetDirty(Transform t)
        {
            t.hasChanged = true;
            EditorUtility.SetDirty(t);
        }

        public static double SnapTo(double a, double snap)
        {
            return Math.Round(a / snap) * snap;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            var data = EditorPrefs.GetString("Snapper", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);
        }

        private void OnDisable()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("Snapper", data);
        }
    }
}