using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Tymski
{
    public class Snapper : OdinEditorWindow
    {
        [MenuItem("Tools/Tymski/Snapper")]
        static void OpenWindow()
        {
            GetWindow<Snapper>().Show();
        }

        [HorizontalGroup("Position"), ToggleLeft, SerializeField] bool snapPosition = true;
        [HorizontalGroup("Position"), HideLabel, EnableIf("snapPosition"), SerializeField] double positionStep = 0.001;

        [HorizontalGroup("Rotation"), ToggleLeft, SerializeField] bool snapRotation = true;
        [HorizontalGroup("Rotation"), HideLabel, EnableIf("snapRotation"), SerializeField] double rotationStep = 0.01;

        [HorizontalGroup("Scale"), ToggleLeft, SerializeField] bool snapScale = true;
        [HorizontalGroup("Scale"), HideLabel, EnableIf("snapScale"), SerializeField] double scaleStep = 0.01;

        [NonSerialized] PropertyInfo gridSize;

        [Button(ButtonSizes.Large)]
        public void Snap()
        {
            // if (Selection.gameObjects.Length > 1) Debug.Log($"Snapping {Selection.gameObjects.Length} objects.");
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

        [Button, ShowIf("@Selection.activeGameObject!=null")]
        public void SetGridSizeToSelected()
        {
            if (Selection.activeGameObject == null) return;
            gridSize.SetValue("size", new Vector3(
                Selection.activeGameObject.GetComponent<MeshRenderer>().bounds.size.x,
                Selection.activeGameObject.GetComponent<MeshRenderer>().bounds.size.y,
                Selection.activeGameObject.GetComponent<MeshRenderer>().bounds.size.z
            ));
            EditorSnapSettings.move = new Vector3(
                Selection.activeGameObject.GetComponent<MeshRenderer>().bounds.size.x,
                Selection.activeGameObject.GetComponent<MeshRenderer>().bounds.size.y,
                Selection.activeGameObject.GetComponent<MeshRenderer>().bounds.size.z
            );
        }

        [Button, ShowIf("@Selection.activeGameObject==null")]
        public void ResetGrid()
        {
            gridSize.SetValue("size", Vector3.one);
            EditorSnapSettings.move = Vector3.one;
        }

        void SnapPosition(Transform t)
        {
            if (!snapPosition) return;

            RectTransform rectTransform = t.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                SnapRectPosition(rectTransform);
            }
            else
            {
                SnapRegularPosition(t);
            }

            void SnapRegularPosition(Transform t2)
            {
                t2.localPosition = new Vector3(
                   (float)SnapTo(t2.localPosition.x, positionStep),
                   (float)SnapTo(t2.localPosition.y, positionStep),
                   (float)SnapTo(t2.localPosition.z, positionStep)
                );
            }

            void SnapRectPosition(RectTransform rt)
            {
                SnapRegularPosition(rt.transform);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)SnapTo(rt.rect.width, positionStep));
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)SnapTo(rt.rect.height, positionStep));
                SnapRegularPosition(rt.transform);
            }
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

        void SetDirty(Transform t)
        {
            t.hasChanged = true;
            EditorUtility.SetDirty(t);
        }

        double SnapTo(double a, double snap)
        {
            return Math.Round(a / snap) * snap;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadSettings();

            Assembly assembly = Assembly.Load("UnityEditor.dll");
            Type gridSettings = assembly.GetType("UnityEditor.GridSettings");
            gridSize = gridSettings.GetProperty("size");
        }

        void OnDisable()
        {
            SaveSettings();
        }

        void SaveSettings()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("Snapper", data);
        }

        void LoadSettings()
        {
            var data = EditorPrefs.GetString("Snapper", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);
        }
    }
}