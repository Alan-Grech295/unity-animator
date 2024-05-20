using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorManager : MonoBehaviour
{
    private abstract class AnimationData
    {
        public float StartTime;
        public float Duration;

        public float EndTime
        {
            get
            {
                return StartTime + Duration;
            }
        }

        public abstract void Run(float t);
    }

    private class AnimationData<T> : AnimationData
    {
        public Action<T> ValueUpdate;
        public T StartValue;
        public T EndValue;
        public Func<T, T, float, T> AnimationFunction;
        public Func<float, float> EaseFunction;

        public override void Run(float t)
        {
            ValueUpdate?.Invoke(AnimationFunction(StartValue, EndValue, EaseFunction(t)));
        }
    }

    private class ResettableStack<T> : IEnumerable<T>
    {
        private List<T> list;
        private int startIndex;

        public T Top
        {
            get
            {
                return list[startIndex];
            }
            set
            {
                list[startIndex] = value;
            }
        }

        public int Count
        {
            get { return list.Count - startIndex; }
        }

        public T this[int index]
        {
            get
            {
                return list[index + startIndex];
            }

            set
            {
                list[index + startIndex] = value;
            }
        }

        public ResettableStack()
        {
            list = new List<T>();
            startIndex = 0;
        }

        public ResettableStack(IEnumerable<T> enumerable)
        {
            list = new List<T>(enumerable);
            startIndex = 0;
        }

        public void Push(T item)
        {
            list.Add(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index + startIndex, item);
        }

        public T Pop()
        {
            return list[startIndex++];
        }

        public T Peek()
        {
            return list[startIndex];
        }

        public void Sort(Comparison<T> comparison)
        {
            list.Sort(comparison);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = startIndex; i < list.Count; i++)
            {
                yield return list[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Reset()
        {
            startIndex = 0;
        }
    }

    [System.Serializable]
    public class Event
    {
        public float StartTime;
        public UnityEvent EventFunction;

        public void Run()
        {
            EventFunction?.Invoke();
        }
    }

    public bool reset = false;

    public List<Event> Events;

    public static AnimatorManager Instance { get; private set; }

    private ResettableStack<AnimationData> animations = new ResettableStack<AnimationData>();

    private ResettableStack<Event> events;

    public static Func<float, float> LINEAR = (t) => t;
    public static Func<float, float> EASE_IN_OUT_CUBIC = (t) => t > 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2f * t + 2, 3) / 2f;
    public static Func<float, float> EASE_OUT_BOUNCE = (t) =>
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1 / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2 / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        else if (t < 2.5 / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        else
        {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    };

    private float AnimTime
    {
        get
        {
            return Time.time - timeOffset;
        }
    }
    private float timeOffset = 0;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;

        events = new ResettableStack<Event>(Events);
        events.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        timeOffset = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (reset)
        {
            reset = false;
            ResetAnimations();
        }

        while (events.Count > 0 && events.Top.StartTime <= AnimTime)
        {
            events.Pop().Run();
        }

        foreach (var anim in animations)
        {
            if (AnimTime < anim.StartTime) continue;
            anim.Run((AnimTime - anim.StartTime) / anim.Duration);
        }

        while (animations.Count > 0 && AnimTime >= animations.Top.EndTime)
        {
            animations.Pop();
        }
    }

    public void ResetAnimations()
    {
        timeOffset = Time.time;
        events.Reset();
        animations.Reset();
    }

    public static void AnimateValue(Action<float> setter, float start, float end, float duration, float delay, Func<float, float, float, float> function, Func<float, float> easeFunction)
    {
        AnimationData<float> animationData = new AnimationData<float>()
        {
            StartTime = Instance.AnimTime + delay,
            Duration = duration,
            ValueUpdate = setter,
            StartValue = start,
            EndValue = end,
            AnimationFunction = function,
            EaseFunction = easeFunction
        };

        float endTime = animationData.EndTime;
        int insertIndex = 0;
        for (int i = 0; i < Instance.animations.Count; i++)
        {
            if (Instance.animations[i].EndTime > endTime)
            {
                insertIndex = i;
                break;
            }
        }

        Instance.animations.Insert(insertIndex, animationData);
    }
}
