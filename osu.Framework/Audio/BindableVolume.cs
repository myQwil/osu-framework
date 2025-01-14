// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Framework.Audio
{
    public sealed class BindableVolume
    {
        public const double MIN = -60;
        public const double STEP = 0.5;

        private static readonly double k = Math.Log(10) / 20;

        public readonly BindableNumber<double> Linear;
        public readonly BindableNumber<double> Decibel = new BindableNumber<double>(1)
        {
            MinValue = MIN,
            MaxValue = 0,
            Precision = STEP,
        };

        private double decibelToLinear(double x) => x <= MIN ? 0 : Math.Exp(k * x);

        private double linearToDecibel(double x) => x <= 0 ? MIN : Math.Log(x) / k;

        public BindableVolume(BindableNumber<double>? linear = null)
        {
            Linear = linear ?? new BindableNumber<double>(1) { MinValue = 0, MaxValue = 1 };
            Decibel.BindValueChanged(x => Linear.Value = decibelToLinear(x.NewValue));
        }

        public void SetFromLinear(double linear)
        {
            Decibel.Value = linearToDecibel(linear);
        }

        public void Scale()
        {
            Decibel.Value = linearToDecibel(Linear.Value);
            Decibel.Default = linearToDecibel(Linear.Default);
        }
    }
}
