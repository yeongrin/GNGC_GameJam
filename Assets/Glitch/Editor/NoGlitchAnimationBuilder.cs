#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam.Editor
{
    public static class NoGlitchAnimationBuilder
    {
        private const string AnimationFolder = "Assets/Glitch/Animations/UI";
        private const float FrameRate = 14f;

        [MenuItem("Tools/GameJam/Rebuild UI Glitch Animations")]
        public static void Build()
        {
            Directory.CreateDirectory(AnimationFolder);
            AssetDatabase.Refresh();

            BuildClipSet("NoGlitch");
            BuildClipSet("WindowGlitch");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildClipSet(string assetName)
        {
            string sourceFolder = $"Assets/Glitch/Resources/Sprites/UI/{assetName}";

            Sprite[] frames = AssetDatabase.FindAssets("t:Sprite", new[] { sourceFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .OrderBy(Path.GetFileNameWithoutExtension, StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(sprite => sprite != null)
                .ToArray();

            if (frames.Length == 0)
            {
                throw new InvalidOperationException($"No sprites found in {sourceFolder}.");
            }

            CreateClip<Image>($"{AnimationFolder}/{assetName}_Image.anim", frames);
            CreateClip<SpriteRenderer>($"{AnimationFolder}/{assetName}_SpriteRenderer.anim", frames);
        }

        private static void CreateClip<TComponent>(string path, Sprite[] frames) where TComponent : UnityEngine.Object
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = FrameRate;
            clip.wrapMode = WrapMode.Loop;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(TComponent),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keys = frames
                .Select((sprite, index) => new ObjectReferenceKeyframe
                {
                    time = index / FrameRate,
                    value = sprite
                })
                .ToArray();

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            EditorUtility.SetDirty(clip);
        }
    }
}
#endif
