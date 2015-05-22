using D.Timekeeper_Universal.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace D.Timekeeper_Universal.Common
{
    public class Timekeeper : IDisposable
    {
        private static Timekeeper instance;
        private Speaker currentSpeaker;
        private TimekeeperReceiver receiver;

        public bool paused  { get; private set; }
        private Stopwatch timeWatch = new Stopwatch();
        private Timer displayUpdateTimer;
        private Timer ringTimer;

        private int visitedSecond;
        private int nextRingPairIndex;

        private Timekeeper()
        {
            paused = true;
        }

        public static Timekeeper getInstance(TimekeeperReceiver receiver, Speaker currentSpeaker)
        {
            if (instance == null)
            {
                instance = new Timekeeper();
            }
            instance.receiver = receiver;
            instance.changeCurrentSpeaker(currentSpeaker);

            return instance;
        }

        public void changeCurrentSpeaker(Speaker newCurrentSpeaker)
        {
            resetTimer();
            currentSpeaker = newCurrentSpeaker;
            currentSpeaker.ringPairs.Sort((a, b) =>
            {
                return a.Key.CompareTo(b.Key);
            });
        }

        // Returns true if timer is paused, false otherwise.
        public bool startStopTimer()
        {
            if(paused)
            {
                paused = false;
                timeWatch.Start();

                if(displayUpdateTimer == null)
                {
                    displayUpdateTimer = new Timer(new TimerCallback(timerTick), null, 0, 100);
                }
                if(ringTimer == null)
                {
                    ringTimer = new Timer(new TimerCallback(ringTick), null, 0, 200);
                }
            }
            else
            {
                paused = true;
                timeWatch.Stop();
            }
            return paused;
        }

        private void ringTick(object state)
        {
            int elapsedSecond = (60 * getElapsedTimeSpan().Minutes) + getElapsedTimeSpan().Seconds;

            if (elapsedSecond == currentSpeaker.ringPairs[nextRingPairIndex % currentSpeaker.ringPairs.Count].Key && elapsedSecond != visitedSecond)
            {
                visitedSecond = elapsedSecond;
                // Quickly change RingPairIndex first in case the ring goes longer than the next ring time.
                nextRingPairIndex += 1;
                receiver.autoRing(currentSpeaker.ringPairs[nextRingPairIndex-1].Value).Wait();
            }
        }

        private void timerTick(object state)
        {
            receiver.updateTimeText(getElapsedTimeSpan()).Wait();
        }

        private TimeSpan getElapsedTimeSpan()
        {
            return timeWatch.Elapsed;
        }

        public void resetTimer()
        {
            paused = true;
            nextRingPairIndex = 0;
            visitedSecond = 0;
            if(timeWatch.IsRunning)
            {
                timeWatch.Stop();
                Dispose();
            }
            timeWatch.Reset();
        }

        public void Dispose()
        {
            if(displayUpdateTimer != null)
            {
                displayUpdateTimer.Dispose();
                displayUpdateTimer = null;
            }
            if(ringTimer != null)
            {
                ringTimer.Dispose();
                ringTimer = null;
            }
        }
    }
}
