// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Framework.Audio
{
    public class Decibel
    {
        public const double MIN = -60;
        public const double STEP = 0.5;

        private const double ln_ten = 2.302585092994045684017991454684364208;
        private const double k = ln_ten / 20;

        public readonly BindableNumber<double> Real;
        public readonly BindableNumber<double> Scale = new BindableNumber<double>(1)
        {
            MinValue = MIN,
            MaxValue = 0,
            Precision = STEP,
        };

        private double scaleToReal(double x) => x <= MIN ? 0 : Math.Exp(k * x);

        private double realToScale(double x) => x <= 0 ? MIN : Math.Log(x) / k;

        public Decibel(BindableNumber<double>? real = null)
        {
            Real = real ?? new BindableNumber<double>(1) { MinValue = 0, MaxValue = 1 };
            Scale.BindValueChanged(x => Real.Value = scaleToReal(x.NewValue));
        }

        public double Value
        {
            get => Real.Value;
            set => Scale.Value = realToScale(value);
        }

        public void UpdateScale()
        {
            Scale.Value = realToScale(Real.Value);
            Scale.Default = realToScale(Real.Default);
        }
    }
}
