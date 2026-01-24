using UnityEngine;

public static class Utils
{
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
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }
}
