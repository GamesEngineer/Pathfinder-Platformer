using System;

public class Countdown
{
    public Countdown(float interval, float cooldown = 0f, bool autoReset = false)
    {
        if (interval < 0f) throw new ArgumentOutOfRangeException(nameof(interval), $"Must be non-negative.");
        if (cooldown < 0f) throw new ArgumentOutOfRangeException(nameof(cooldown), $"Must be non-negative.");
        Interval = interval > 0f ? interval : 0f;
        Cooldown = cooldown > 0f ? cooldown : 0f;
        AutoReset = autoReset;
        t = interval;
    }

    public event Action OnReset;
    public event Action OnElapsed;
    public event Action OnReady;

    public float Interval { get; private set; }
    public float Cooldown { get; private set; }
    public bool AutoReset { get; set; }
    public float IntervalRemaining => t > 0f ? t : 0f;
    public float CooldownRemaining => t < 0f ? Cooldown + t : Cooldown;

    public enum Phase { Ready, Active, Cooling }
    public Phase State
    {
        get
        {
            if (t > 0f) return Phase.Active;
            if (t <= -Cooldown) return Phase.Ready;
            return Phase.Cooling;
        }
    }

    public void Reset()
    {
        t = Interval;
        OnReset?.Invoke();
    }
    public void Ready()
    {
        t = -Cooldown;
        OnReady?.Invoke();
    }
    public float Update(float decrement)
    {
        if (decrement < 0f) throw new ArgumentOutOfRangeException(nameof(decrement), $"Must be non-negative.");
        if (t <= -Cooldown)
        {
            if (AutoReset) Reset();
            return t;
        }
        float before = t;
        t -= decrement;
        if (before > 0f && t <= 0f)
        {
            OnElapsed?.Invoke();
        }
       
        if (before > -Cooldown && t <= -Cooldown)
        {
            Ready();
            if (AutoReset) Reset(); // TODO - FIXME: account for amount of decrement that passed the cooldown threshold
        }

        return t;
    }

    private float t;
}
