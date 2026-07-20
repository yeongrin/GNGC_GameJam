using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class GlitchSpriteAnimator : MonoBehaviour
{
    [SerializeField] private string resourcesFolder = "Sprites/UI/NoGlitch";
    [SerializeField] private Sprite[] frames = Array.Empty<Sprite>();
    [SerializeField, Min(1f)] private float framesPerSecond = 14f;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool useUnscaledTime = true;

    private Image targetImage;
    private SpriteRenderer targetSpriteRenderer;
    private int frameIndex;
    private float frameTimer;
    private bool isPlaying;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        targetSpriteRenderer = GetComponent<SpriteRenderer>();
        LoadFramesIfNeeded();
    }

    private void OnEnable()
    {
        LoadFramesIfNeeded();

        if (playOnEnable)
        {
            PlayFromStart();
            return;
        }

        ApplyFrame(frameIndex);
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length == 0)
        {
            return;
        }

        frameTimer += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * framesPerSecond;

        while (frameTimer >= 1f)
        {
            frameTimer -= 1f;
            AdvanceFrame();

            if (!isPlaying)
            {
                break;
            }
        }
    }

    public void PlayFromStart()
    {
        frameIndex = 0;
        frameTimer = 0f;
        isPlaying = true;
        ApplyFrame(frameIndex);
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void SetFrame(int index)
    {
        LoadFramesIfNeeded();

        if (frames == null || frames.Length == 0)
        {
            return;
        }

        frameIndex = Mathf.Clamp(index, 0, frames.Length - 1);
        frameTimer = 0f;
        ApplyFrame(frameIndex);
    }

    private void LoadFramesIfNeeded()
    {
        if (frames != null && frames.Length > 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(resourcesFolder))
        {
            return;
        }

        frames = Resources.LoadAll<Sprite>(resourcesFolder)
            .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
            .ToArray();
    }

    private void AdvanceFrame()
    {
        int nextFrame = frameIndex + 1;

        if (nextFrame >= frames.Length)
        {
            if (!loop)
            {
                isPlaying = false;
                nextFrame = frames.Length - 1;
            }
            else
            {
                nextFrame = 0;
            }
        }

        frameIndex = nextFrame;
        ApplyFrame(frameIndex);
    }

    private void ApplyFrame(int index)
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        Sprite frame = frames[Mathf.Clamp(index, 0, frames.Length - 1)];

        if (targetImage != null)
        {
            targetImage.sprite = frame;
        }

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = frame;
        }
    }
}
