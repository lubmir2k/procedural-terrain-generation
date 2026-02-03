using UnityEngine;
using System.Collections.Generic;

public static class Utils
{
    // Random instance for Shuffle method
    private static System.Random rng = new System.Random();
    /// <summary>
    /// Fractal Brownian Motion - combines multiple octaves of Perlin noise
    /// </summary>
    /// <param name="x">X position (already scaled)</param>
    /// <param name="y">Y position (already scaled)</param>
    /// <param name="octaves">Number of noise layers to combine</param>
    /// <param name="persistence">How much each octave's amplitude changes (typically 0.5)</param>
    /// <returns>Height value normalized between 0 and 1</returns>
    public static float FBM(float x, float y, int octaves, float persistence)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0; // Used for normalizing result to 0-1

        for (int i = 0; i < octaves; i++)
        {
            // Add Perlin noise at current frequency and amplitude
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

            // Track max possible value for normalization
            maxValue += amplitude;

            // Reduce amplitude for next octave
            amplitude *= persistence;

            // Increase frequency for next octave (more detail)
            frequency *= 2;
        }

        // Normalize to 0-1 range
        return total / maxValue;
    }

    /// <summary>
    /// Ridged Multifractal - creates sharp ridges like eroded mountains.
    /// Based on Ken Musgrave's algorithm from "Texturing and Modeling: A Procedural Approach"
    /// Key difference from FBM: ridges are created at each octave, valleys get less detail.
    /// </summary>
    /// <param name="x">X position (already scaled)</param>
    /// <param name="y">Y position (already scaled)</param>
    /// <param name="octaves">Number of noise layers to combine</param>
    /// <param name="persistence">How much each octave's amplitude changes (typically 0.5)</param>
    /// <param name="offset">Ridge offset, typically 1.0</param>
    /// <returns>Height value (may exceed 1.0, consider normalizing)</returns>
    public static float RidgedFBM(float x, float y, int octaves, float persistence, float offset = 1f)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float weight = 1;  // Weights successive octaves - valleys get less detail

        for (int i = 0; i < octaves; i++)
        {
            // Get base Perlin noise signal
            float signal = Mathf.PerlinNoise(x * frequency, y * frequency);

            // Create ridge: invert absolute value
            signal = offset - Mathf.Abs(signal * 2f - 1f);

            // Sharpen the ridge by squaring
            signal *= signal;

            // Weight by previous octave's signal (valleys = less detail)
            signal *= weight;
            weight = Mathf.Clamp01(signal * 2f);

            total += signal * amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total;
    }

    /// <summary>
    /// Remaps a value from one range to another.
    /// </summary>
    /// <param name="value">The value to remap</param>
    /// <param name="originalMin">Original range minimum</param>
    /// <param name="originalMax">Original range maximum</param>
    /// <param name="targetMin">Target range minimum</param>
    /// <param name="targetMax">Target range maximum</param>
    /// <returns>Value remapped to target range</returns>
    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        if (Mathf.Approximately(originalMax, originalMin))
        {
            return targetMin;
        }
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }

    /// <summary>
    /// Generates a list of valid neighbor positions for erosion algorithms.
    /// Returns 8-connected neighbors that are within bounds.
    /// </summary>
    /// <param name="pos">Current position</param>
    /// <param name="width">Map width</param>
    /// <param name="height">Map height</param>
    /// <returns>List of valid neighbor positions</returns>
    public static List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) continue;
                Vector2 n = new Vector2(pos.x + x, pos.y + y);
                if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height)
                    neighbours.Add(n);
            }
        }
        return neighbours;
    }

    /// <summary>
    /// Fisher-Yates shuffle algorithm for randomizing lists in-place.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
