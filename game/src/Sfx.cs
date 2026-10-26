using System;
using System.Collections.Generic;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Godot;

namespace Glimmerfall.Game;

public enum Sound
{
    Hit,
    Crit,
    Kill,
    HeroHurt,
    HeroDown,
    CastFire,
    CastFrost,
    CastSpark,
    Dash,
    Swap,
    LevelUp,
    Pickup,
    BossSlam,
    BossSpawn,
    EnemyShot,
    UiClick,
    UiHover,
    Victory,
    Defeat,
}

/// <summary>
/// Звуки-заглушки, синтезированные кодом при старте (шум, свипы, аккорды) — ни одного файла, нечего вносить в реестр.
/// Пул голосов на шине «Effects»; частые звуки ограничены по темпу, чтобы орда не превращалась в шум.
/// Настоящий звук — у звукорежиссёра (PRD), этот слой только даёт отклик действиям с первого дня.
/// </summary>
public partial class Sfx : Node
{
    private const int MixRate = 22050;
    private const int Voices = 24;
    private const string Bus = "Effects";

    private static readonly double[] MinGap = new double[Enum.GetValues<Sound>().Length];

    private readonly AudioStreamWav[] _streams = new AudioStreamWav[Enum.GetValues<Sound>().Length];
    private readonly double[] _lastPlayed = new double[Enum.GetValues<Sound>().Length];
    private readonly AudioStreamPlayer[] _voices = new AudioStreamPlayer[Voices];
    private readonly Random _random = new();
    private int _nextVoice;
    private double _now;

    /// <summary>Звуки демо на титульном экране глушатся: меню не должно шуметь боем.</summary>
    public bool Muted { get; set; }

    static Sfx()
    {
        MinGap[(int)Sound.Hit] = 0.035;
        MinGap[(int)Sound.Crit] = 0.06;
        MinGap[(int)Sound.Kill] = 0.03;
        MinGap[(int)Sound.Pickup] = 0.045;
        MinGap[(int)Sound.EnemyShot] = 0.08;
        MinGap[(int)Sound.HeroHurt] = 0.1;
        MinGap[(int)Sound.UiHover] = 0.05;
    }

    public override void _Ready()
    {
        EnsureBus();
        for (int i = 0; i < Voices; i++)
        {
            _voices[i] = new AudioStreamPlayer { Bus = Bus };
            AddChild(_voices[i]);
        }
        foreach (Sound sound in Enum.GetValues<Sound>())
        {
            _streams[(int)sound] = ToStream(Synthesize(sound));
            _lastPlayed[(int)sound] = -1;
        }
    }

    public override void _Process(double delta) => _now += delta;

    public static void SetVolumes(float master, float effects)
    {
        EnsureBus();
        AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(Math.Max(master, 0.0001f)));
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(Bus), Mathf.LinearToDb(Math.Max(effects, 0.0001f)));
    }

    private static void EnsureBus()
    {
        if (AudioServer.GetBusIndex(Bus) >= 0)
        {
            return;
        }
        AudioServer.AddBus();
        int index = AudioServer.BusCount - 1;
        AudioServer.SetBusName(index, Bus);
        AudioServer.SetBusSend(index, "Master");
    }

    public void Play(Sound sound, float volume = 1f, float pitchJitter = 0.06f)
    {
        if (Muted && sound is not (Sound.UiClick or Sound.UiHover))
        {
            return;
        }
        int id = (int)sound;
        if (_lastPlayed[id] >= 0 && _now - _lastPlayed[id] < MinGap[id])
        {
            return;
        }
        _lastPlayed[id] = _now;
        AudioStreamPlayer voice = _voices[_nextVoice];
        _nextVoice = (_nextVoice + 1) % Voices;
        voice.Stream = _streams[id];
        voice.VolumeDb = Mathf.LinearToDb(Math.Max(volume, 0.0001f));
        voice.PitchScale = 1f + (float)(_random.NextDouble() * 2 - 1) * pitchJitter;
        voice.Play();
    }

    /// <summary>События тика → звуки.</summary>
    public void Consume(List<SimEvent> events)
    {
        foreach (SimEvent e in events)
        {
            switch (e.Type)
            {
                case SimEventType.EnemyHit:
                    Play(e.Crit ? Sound.Crit : Sound.Hit, e.Crit ? 0.7f : 0.45f, 0.12f);
                    break;
                case SimEventType.EnemyKilled:
                    Play(Sound.Kill, e.Kind >= EnemyIds.Ravager ? 1f : 0.55f, 0.15f);
                    break;
                case SimEventType.HeroHit:
                    Play(Sound.HeroHurt, 0.9f);
                    break;
                case SimEventType.HeroDown:
                    Play(Sound.HeroDown, 1f, 0f);
                    break;
                case SimEventType.HeroAttack:
                    Play(e.Kind switch { HeroIds.Warden => Sound.CastFrost, HeroIds.Sparkwright => Sound.CastSpark, _ => Sound.CastFire }, 0.5f, 0.08f);
                    break;
                case SimEventType.Dash:
                    Play(Sound.Dash, 0.7f);
                    break;
                case SimEventType.Swap:
                    Play(Sound.Swap, 0.8f, 0f);
                    break;
                case SimEventType.LevelUp:
                    Play(Sound.LevelUp, 0.8f, 0f);
                    break;
                case SimEventType.Slam:
                    Play(Sound.BossSlam, 1f, 0.03f);
                    break;
                case SimEventType.EnemyFired:
                    Play(Sound.EnemyShot, 0.35f, 0.1f);
                    break;
            }
        }
    }

    // ---------- синтез ----------

    private static float[] Synthesize(Sound sound)
    {
        var rng = new Random(1000 + (int)sound);
        return sound switch
        {
            Sound.Hit => Mix(Noise(rng, 0.07, 2800, 0.018, 0.55), Sweep(900, 380, 0.05, 0.012, 0.5)),
            Sound.Crit => Mix(Noise(rng, 0.09, 3600, 0.022, 0.6), Sweep(1500, 900, 0.16, 0.05, 0.45), Sweep(700, 300, 0.08, 0.02, 0.4)),
            Sound.Kill => Mix(Sweep(170, 55, 0.2, 0.06, 0.8), Noise(rng, 0.18, 1300, 0.05, 0.5)),
            Sound.HeroHurt => Drive(Mix(Sweep(240, 100, 0.24, 0.09, 0.8), Noise(rng, 0.2, 900, 0.07, 0.6)), 2.2f),
            Sound.HeroDown => Drive(Mix(Sweep(320, 70, 0.7, 0.3, 0.8), Noise(rng, 0.6, 700, 0.25, 0.5)), 1.6f),
            Sound.CastFire => Swell(Noise(rng, 0.2, 2200, 0.09, 0.7), 0.03),
            Sound.CastFrost => Mix(Sweep(1900, 2300, 0.22, 0.08, 0.35), Sweep(2850, 3300, 0.22, 0.06, 0.2), Noise(rng, 0.05, 6000, 0.01, 0.15)),
            Sound.CastSpark => Mix(Crackle(rng, 0.16, 0.5), Sweep(2600, 1800, 0.08, 0.02, 0.2)),
            Sound.Dash => Swell(NoiseSweep(rng, 0.16, 500, 3500, 0.6), 0.02),
            Sound.Swap => Mix(Sweep(440, 440, 0.35, 0.14, 0.3), Sweep(660, 660, 0.35, 0.12, 0.25), Sweep(880, 990, 0.35, 0.1, 0.2)),
            Sound.LevelUp => Arpeggio(new[] { 523.0, 659.0, 784.0, 1047.0 }, 0.075, 0.22, 0.35),
            Sound.Pickup => Sweep(1250, 1650, 0.05, 0.02, 0.3),
            Sound.BossSlam => Drive(Mix(Sweep(80, 32, 0.8, 0.3, 1f), Noise(rng, 0.7, 420, 0.25, 0.8)), 1.8f),
            Sound.BossSpawn => Drive(Mix(Saw(55, 1.3, 0.7), Swell(Noise(rng, 1.3, 300, 0.6, 0.5), 0.5)), 1.5f),
            Sound.EnemyShot => Mix(Sweep(620, 300, 0.09, 0.03, 0.4), Noise(rng, 0.06, 1800, 0.015, 0.25)),
            Sound.UiClick => Sweep(950, 700, 0.045, 0.012, 0.35),
            Sound.UiHover => Sweep(1400, 1400, 0.03, 0.007, 0.12),
            Sound.Victory => Arpeggio(new[] { 392.0, 523.0, 659.0, 784.0, 1047.0 }, 0.12, 0.5, 0.35),
            Sound.Defeat => Arpeggio(new[] { 392.0, 311.0, 262.0, 196.0 }, 0.18, 0.6, 0.35),
            _ => new float[1],
        };
    }

    private static int Samples(double seconds) => Math.Max(1, (int)(seconds * MixRate));

    /// <summary>Шум через однополюсный фильтр нижних частот с экспоненциальным затуханием.</summary>
    private static float[] Noise(Random rng, double seconds, double cutoffHz, double decay, double gain)
    {
        float[] out_ = new float[Samples(seconds)];
        double a = 1 - Math.Exp(-2 * Math.PI * cutoffHz / MixRate);
        double y = 0;
        for (int i = 0; i < out_.Length; i++)
        {
            y += a * (rng.NextDouble() * 2 - 1 - y);
            out_[i] = (float)(y * gain * Math.Exp(-i / (decay * MixRate)) * 2.2);
        }
        return out_;
    }

    private static float[] NoiseSweep(Random rng, double seconds, double fromHz, double toHz, double gain)
    {
        float[] out_ = new float[Samples(seconds)];
        double y = 0;
        for (int i = 0; i < out_.Length; i++)
        {
            double t = (double)i / out_.Length;
            double cutoff = fromHz * Math.Pow(toHz / fromHz, t);
            double a = 1 - Math.Exp(-2 * Math.PI * cutoff / MixRate);
            y += a * (rng.NextDouble() * 2 - 1 - y);
            out_[i] = (float)(y * gain * (1 - t) * 2.2);
        }
        return out_;
    }

    /// <summary>Синус со свипом частоты (экспоненциально) и затуханием.</summary>
    private static float[] Sweep(double fromHz, double toHz, double seconds, double decay, double gain)
    {
        float[] out_ = new float[Samples(seconds)];
        double phase = 0;
        for (int i = 0; i < out_.Length; i++)
        {
            double t = (double)i / out_.Length;
            double freq = fromHz * Math.Pow(toHz / fromHz, t);
            phase += 2 * Math.PI * freq / MixRate;
            double attack = Math.Min(1.0, i / (0.003 * MixRate));
            out_[i] = (float)(Math.Sin(phase) * gain * attack * Math.Exp(-i / (decay * MixRate)));
        }
        return out_;
    }

    private static float[] Saw(double hz, double seconds, double gain)
    {
        float[] out_ = new float[Samples(seconds)];
        for (int i = 0; i < out_.Length; i++)
        {
            double t = (double)i / MixRate;
            double vibrato = 1 + 0.03 * Math.Sin(2 * Math.PI * 5 * t);
            double x = t * hz * vibrato % 1.0;
            double envelope = Math.Min(1, t / 0.25) * Math.Max(0, 1 - t / seconds);
            out_[i] = (float)((2 * x - 1) * gain * envelope);
        }
        return out_;
    }

    private static float[] Crackle(Random rng, double seconds, double gain)
    {
        float[] out_ = new float[Samples(seconds)];
        for (int i = 0; i < out_.Length; i++)
        {
            if (rng.NextDouble() < 0.02)
            {
                int length = 8 + rng.Next(30);
                double sign = rng.NextDouble() < 0.5 ? -1 : 1;
                for (int k = 0; k < length && i + k < out_.Length; k++)
                {
                    out_[i + k] += (float)(sign * gain * (1 - (double)k / length) * (1 - (double)i / out_.Length));
                }
            }
        }
        return out_;
    }

    private static float[] Arpeggio(double[] notes, double step, double ring, double gain)
    {
        float[] out_ = new float[Samples(step * notes.Length + ring)];
        for (int n = 0; n < notes.Length; n++)
        {
            float[] tone = Mix(Sweep(notes[n], notes[n], ring, ring / 2.5, gain), Sweep(notes[n] * 2, notes[n] * 2, ring, ring / 4, gain * 0.25));
            int offset = Samples(step * n);
            for (int i = 0; i < tone.Length && offset + i < out_.Length; i++)
            {
                out_[offset + i] += tone[i];
            }
        }
        return out_;
    }

    private static float[] Mix(params float[][] layers)
    {
        int length = 0;
        foreach (float[] layer in layers)
        {
            length = Math.Max(length, layer.Length);
        }
        float[] out_ = new float[length];
        foreach (float[] layer in layers)
        {
            for (int i = 0; i < layer.Length; i++)
            {
                out_[i] += layer[i];
            }
        }
        return out_;
    }

    /// <summary>Мягкая атака: шум «нарастает», а не щёлкает.</summary>
    private static float[] Swell(float[] samples, double attackSeconds)
    {
        int attack = Samples(attackSeconds);
        for (int i = 0; i < Math.Min(attack, samples.Length); i++)
        {
            samples[i] *= (float)i / attack;
        }
        return samples;
    }

    private static float[] Drive(float[] samples, float amount)
    {
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = MathF.Tanh(samples[i] * amount) / MathF.Tanh(amount);
        }
        return samples;
    }

    private static AudioStreamWav ToStream(float[] samples)
    {
        byte[] data = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)(Math.Clamp(samples[i], -1f, 1f) * 30000);
            data[2 * i] = (byte)(value & 0xFF);
            data[2 * i + 1] = (byte)((value >> 8) & 0xFF);
        }
        return new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = MixRate, Stereo = false, Data = data };
    }
}
