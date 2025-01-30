using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// A Utilities class for random useful generic functions useful in a bunch of places
/// like vector math. Originally by GitAmend
/// </summary>
public static class Utils
{
    /// <summary>
    /// Returns the component of a vector in a given direction
    /// </summary>
    /// <param name="vector"> The vector to extract component from</param>
    /// <param name="direction">The direction to extract along</param>
    /// <returns>The component of the vector in the given direction</returns>
    public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return direction * Vector3.Dot(vector, direction);
    }
    /// <summary>
    /// Returns the dot product of a vector and a given direction
    /// </summary>
    /// <param name="vector">The vector</param>
    /// <param name="direction">The given direction</param>
    /// <returns>The dot product of the vector and direction</returns>
    public static float GetDotProduct(Vector3 vector, Vector3 direction) => Vector3.Dot(vector, direction.normalized);

    /// <summary>
    /// Removes a direction vector component from a vector
    /// </summary>
    /// <param name="vector">The vector to remove the component from</param>
    /// <param name="direction">The direction vector to be removed</param>
    /// <returns>the vector with the direction vector removed</returns>
    public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return vector - direction * Vector3.Dot(vector, direction);

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="aaa"></param>
    /// <param name="bbb"></param>
    /// <param name="ccc"></param>
    /// <returns></returns>
    public static float Yahoo(float aaa, float bbb, Vector3 ccc) => aaa;
}
