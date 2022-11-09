using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public static class MetricExtensions
    {
        private const float MetricStep = 1e+03f;
    
        public enum Metric
        {
            Pico   = -4,
            Nano   = -3,
            Micro  = -2,
            Milli  = -1,
            Normal = 0,
            Kilo   = +1,
            Mega   = +2,
            Giga   = +3,
            Tera   = +4
        }
    
        private static readonly Dictionary<Metric, string> PrefixMetricMap = new Dictionary<Metric, string>
        {
            { Metric.Pico,   "p" },
            { Metric.Nano,   "n" },
            { Metric.Micro,  "μ" },
            { Metric.Milli,  "m" },
            { Metric.Normal, ""  },
            { Metric.Kilo,   "K" },
            { Metric.Mega,   "M" },
            { Metric.Giga,   "G" },
            { Metric.Tera,   "T" }
        };
    
        private static readonly int[] AvailableMetrics = Enum.GetValues(typeof(Metric)).Cast<int>().ToArray();
    
        public static float ConvertToNormal(this float value, Metric fromMetric) =>
            ConvertTo(value, fromMetric, Metric.Normal);
    
        public static float ConvertTo(this float value, Metric fromMetric, Metric toMetric)
        {
            int metricDifference = (int) fromMetric - (int) toMetric;
            return value * Mathf.Pow(MetricStep, metricDifference);
        }
    
        public static (float, string) AutoConvertNormalValue(this float value)
        {
            Metric desiredMetric = GetDesiredMetric(value);

            float convertedValue = ConvertTo(value, Metric.Normal, desiredMetric);
            string prefix = PrefixMetricMap[desiredMetric];
        
            return (convertedValue, prefix);
        }
    
        private static Metric GetDesiredMetric(this float value)
        {
            int nearestMetricNumber = Mathf.FloorToInt(Mathf.Log(Mathf.Abs(value), MetricStep));
            return (Metric) Mathf.Clamp(nearestMetricNumber, AvailableMetrics.Min(), AvailableMetrics.Max());
        }
    }
}
