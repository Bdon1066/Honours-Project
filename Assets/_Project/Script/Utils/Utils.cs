using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// A Utilities class for random useful generic functions useful in a bunch of places
/// like vector math.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Returns the component of a vector in a given direction
    /// </summary>
    /// <param name="vector"> The vector to extract component from</param>
    /// <param name="direction">The direction to extract along</param>
    /// <returns></returns>
    public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return direction * Vector3.Dot(vector, direction);
    }
}
