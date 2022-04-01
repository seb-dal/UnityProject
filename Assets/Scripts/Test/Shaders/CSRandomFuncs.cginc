
float randSeed = 1337;
// Pseudo-random value in half-open range [0:1].
float random(float2 seed)
{
    return (frac(sin(dot(seed.xy, float2(12.9898, 78.233))) * 43758.5453 * randSeed)) * 1;
}

float Range(float min, float max, float2 seed)
{
    return min + random(seed) * (max - min);
}
