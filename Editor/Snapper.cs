using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using System.Linq;

namespace Tymski
{
    public class Snapper : OdinEditorWindow
    {
        const string SNAPPER_KEY = "Tools/Tymski/Snapper";

        [HorizontalGroup("Position"), ToggleLeft, SerializeField, LabelText("Position"), LabelWidth(25)] bool snapPosition = true;
        [HorizontalGroup("Position"), HideLabel, EnableIf("snapPosition"), SerializeField] double positionStep = 0.001;

        [HorizontalGroup("Rotation"), ToggleLeft, SerializeField, LabelText("Rotation"), LabelWidth(25)] bool snapRotation = true;
        [HorizontalGroup("Rotation"), HideLabel, EnableIf("snapRotation"), SerializeField] double rotationStep = 0.01;

        [HorizontalGroup("Scale"), ToggleLeft, SerializeField, LabelText("Scale"), LabelWidth(25)] bool snapScale = true;
        [HorizontalGroup("Scale"), HideLabel, EnableIf("snapScale"), SerializeField] double scaleStep = 0.01;

        [MenuItem(SNAPPER_KEY)]
        static void OpenWindow()
        {
            GetWindow<Snapper>().Show();
        }

        [Button("@\"Snap (\" + UnityEditor.Selection.gameObjects.Length + \")\"", ButtonSizes.Large)]
        public void Snap()
        {
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

            Vector3 inspectorRotation = TransformUtils.GetInspectorRotation(t);

            Vector3 newVector = new Vector3(
                (float)SnapTo(inspectorRotation.x, rotationStep),
                (float)SnapTo(inspectorRotation.y, rotationStep),
                (float)SnapTo(inspectorRotation.z, rotationStep)
            );

            t.localEulerAngles = newVector;
            TransformUtils.SetInspectorRotation(t, newVector);
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

        static void SetDirty(Transform t)
        {
            t.hasChanged = true;
            EditorUtility.SetDirty(t);
        }

        static double SnapTo(double a, double snap)
        {
            return Math.Round(a / snap) * snap;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            var data = EditorPrefs.GetString(SNAPPER_KEY, JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);
        }

        void OnDisable()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString(SNAPPER_KEY, data);
        }
    }
}