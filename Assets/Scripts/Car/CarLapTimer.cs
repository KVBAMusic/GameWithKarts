using UnityEngine;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
public class CarLapTimer : CarComponent
{
    private readonly Stopwatch timer = new();
    private List<double> lapTimes;
    private double fastestTime;
    private bool eventsSubscribed = false;
    public List<double> LapTimes => lapTimes;
    public double TotalTimeMS => lapTimes.Sum();
    public double TotalTime => TotalTimeMS / 1000;
    public double ElapsedTime => timer.Elapsed.TotalSeconds;
    public double ElapsedTimeMS => timer.Elapsed.TotalMilliseconds;
    public override void Init() {
        timer.Stop();
        timer.Reset();
        fastestTime = double.MaxValue;
        lapTimes = new();
        if (!eventsSubscribed) {
            eventsSubscribed = true;
            car.Path.OnNextLap += SaveLap;
            car.Path.OnRaceEnd += StopTimer;
        }
    }

    public void ToggleTimer(bool paused) {
        if (paused || !car.Movement.IsControlable){
            timer.Stop();
            return;
        }
        timer.Start();
    }

    private void SaveLap(BaseCar _) {
        timer.Stop();
        double time = Math.Floor(ElapsedTimeMS) - TotalTimeMS;
        lapTimes.Add(time);
        if (time < fastestTime)
            fastestTime = time;
        timer.Start();
    }

    private void StopTimer(BaseCar _) {
        car.Path.OnNextLap -= SaveLap;
        car.Path.OnRaceEnd -= StopTimer;
        eventsSubscribed = false;
        timer.Stop();
        if (!car.playerControlled) {
            Extrapolate();
        }
    }

    public override void StartRace() {
        timer.Start();
    }

    public static string GetFormattedTime(double time, bool asMs = false) {
        if (asMs) {
            time /= 1000;
        }
        if (time == double.MaxValue) return "--:--.---";
        int m = (int)time / 60;
        int s = (int)time % 60;
        int ms = (int)((time - s - m * 60) * 1000);
        string m_str = string.Format("{0:00}", m);
        string s_str = string.Format("{0:00}", s);
        string ms_str = string.Format("{0:000}", ms);
        return $"{m_str}:{s_str}.{ms_str}";
    }

    private void Extrapolate() {
        if (lapTimes.Count == car.Path.numLaps) {
            return;
        }
        if (lapTimes.Count == 0) {
            float elapsed = (float)ElapsedTimeMS;
            float reciprocal = 1 / car.Path.CurrentPathTime;
            for (int i = 0; i < car.Path.numLaps; i++) {
                lapTimes.Add(Mathf.LerpUnclamped(0, elapsed, reciprocal));
            }
            return;
        }
        int lapsRemaining = car.Path.numLaps - car.Path.CurrentLap;
        double averageLapTime;
        if (lapTimes.Count == 1) {
            averageLapTime = Mathf.LerpUnclamped(0, (float)lapTimes[0], .9f / car.Path.CurrentPathTime);
        }
        else {
            averageLapTime = lapTimes.Average();
        }
        if (car.Path.CurrentPathTime == 0) {
            lapTimes.Add(averageLapTime);
        }
        else {
            lapTimes.Add(Mathf.LerpUnclamped(0, (float)(ElapsedTimeMS - TotalTimeMS), .9f / car.Path.CurrentPathTime));
        }
        for (int i = 0; i < lapsRemaining; i++) {
            lapTimes.Add(averageLapTime);
        }
    }
}