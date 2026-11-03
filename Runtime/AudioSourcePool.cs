using System.Collections.Generic;
using UnityEngine;

public class AudioSourcePool
{
    private readonly Transform parent;
    private readonly Stack<AudioSource> sources = new Stack<AudioSource>();

    public AudioSourcePool(Transform parent)
    {
        this.parent = parent;
    }

    public AudioSource Get(string name)
    {
        while (sources.Count > 0)
        {
            AudioSource source = sources.Pop();
            if (source == null)
                continue;

            source.gameObject.name = name;
            source.gameObject.SetActive(true);
            return source;
        }

        GameObject newAudio = new GameObject(name);
        newAudio.transform.SetParent(parent);
        return newAudio.AddComponent<AudioSource>();
    }

    public void Release(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.clip = null;
        source.loop = false;
        source.gameObject.SetActive(false);
        sources.Push(source);
    }
}
