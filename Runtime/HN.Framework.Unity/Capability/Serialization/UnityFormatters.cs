using MemoryPack;
using UnityEngine;
using UnityEngine.Scripting;

namespace HN.Framework.Unity.Capability.Serialization
{
    /// <summary>
    /// Vector2 格式化器。
    /// </summary>
    [Preserve]
    public sealed class Vector2Formatter : MemoryPackFormatter<Vector2>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Vector2 value)
        {
            writer.WriteUnmanaged(value.x, value.y);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Vector2 value)
        {
            reader.ReadUnmanaged(out float x, out float y);
            value = new Vector2(x, y);
        }
    }

    /// <summary>
    /// Vector3 格式化器。
    /// </summary>
    [Preserve]
    public sealed class Vector3Formatter : MemoryPackFormatter<Vector3>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Vector3 value)
        {
            writer.WriteUnmanaged(value.x, value.y, value.z);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Vector3 value)
        {
            reader.ReadUnmanaged(out float x, out float y, out float z);
            value = new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// Vector4 格式化器。
    /// </summary>
    [Preserve]
    public sealed class Vector4Formatter : MemoryPackFormatter<Vector4>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Vector4 value)
        {
            writer.WriteUnmanaged(value.x, value.y, value.z, value.w);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Vector4 value)
        {
            reader.ReadUnmanaged(out float x, out float y, out float z, out float w);
            value = new Vector4(x, y, z, w);
        }
    }

    /// <summary>
    /// Vector2Int 格式化器。
    /// </summary>
    [Preserve]
    public sealed class Vector2IntFormatter : MemoryPackFormatter<Vector2Int>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Vector2Int value)
        {
            writer.WriteUnmanaged(value.x, value.y);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Vector2Int value)
        {
            reader.ReadUnmanaged(out int x, out int y);
            value = new Vector2Int(x, y);
        }
    }

    /// <summary>
    /// Vector3Int 格式化器。
    /// </summary>
    [Preserve]
    public sealed class Vector3IntFormatter : MemoryPackFormatter<Vector3Int>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Vector3Int value)
        {
            writer.WriteUnmanaged(value.x, value.y, value.z);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Vector3Int value)
        {
            reader.ReadUnmanaged(out int x, out int y, out int z);
            value = new Vector3Int(x, y, z);
        }
    }

    /// <summary>
    /// Quaternion 格式化器。
    /// </summary>
    [Preserve]
    public sealed class QuaternionFormatter : MemoryPackFormatter<Quaternion>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Quaternion value)
        {
            writer.WriteUnmanaged(value.x, value.y, value.z, value.w);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Quaternion value)
        {
            reader.ReadUnmanaged(out float x, out float y, out float z, out float w);
            value = new Quaternion(x, y, z, w);
        }
    }

    /// <summary>
    /// Color 格式化器。
    /// </summary>
    [Preserve]
    public sealed class ColorFormatter : MemoryPackFormatter<Color>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Color value)
        {
            writer.WriteUnmanaged(value.r, value.g, value.b, value.a);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Color value)
        {
            reader.ReadUnmanaged(out float r, out float g, out float b, out float a);
            value = new Color(r, g, b, a);
        }
    }

    /// <summary>
    /// Color32 格式化器。
    /// </summary>
    [Preserve]
    public sealed class Color32Formatter : MemoryPackFormatter<Color32>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Color32 value)
        {
            writer.WriteUnmanaged(value.r, value.g, value.b, value.a);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Color32 value)
        {
            reader.ReadUnmanaged(out byte r, out byte g, out byte b, out byte a);
            value = new Color32(r, g, b, a);
        }
    }

    /// <summary>
    /// Bounds 格式化器。
    /// </summary>
    [Preserve]
    public sealed class BoundsFormatter : MemoryPackFormatter<Bounds>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Bounds value)
        {
            writer.WriteUnmanaged(value.center.x, value.center.y, value.center.z);
            writer.WriteUnmanaged(value.size.x, value.size.y, value.size.z);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Bounds value)
        {
            reader.ReadUnmanaged(out float cx, out float cy, out float cz);
            reader.ReadUnmanaged(out float sx, out float sy, out float sz);
            value = new Bounds(new Vector3(cx, cy, cz), new Vector3(sx, sy, sz));
        }
    }

    /// <summary>
    /// BoundsInt 格式化器。
    /// </summary>
    [Preserve]
    public sealed class BoundsIntFormatter : MemoryPackFormatter<BoundsInt>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref BoundsInt value)
        {
            var pos = value.position;
            var size = value.size;
            writer.WriteUnmanaged(pos.x, pos.y, pos.z, size.x, size.y, size.z);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref BoundsInt value)
        {
            reader.ReadUnmanaged(out int px, out int py, out int pz, out int sx, out int sy, out int sz);
            value = new BoundsInt(new Vector3Int(px, py, pz), new Vector3Int(sx, sy, sz));
        }
    }

    /// <summary>
    /// Rect 格式化器。
    /// </summary>
    [Preserve]
    public sealed class RectFormatter : MemoryPackFormatter<Rect>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Rect value)
        {
            writer.WriteUnmanaged(value.x, value.y, value.width, value.height);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Rect value)
        {
            reader.ReadUnmanaged(out float x, out float y, out float w, out float h);
            value = new Rect(x, y, w, h);
        }
    }

    /// <summary>
    /// RectInt 格式化器。
    /// </summary>
    [Preserve]
    public sealed class RectIntFormatter : MemoryPackFormatter<RectInt>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref RectInt value)
        {
            writer.WriteUnmanaged(value.x, value.y, value.width, value.height);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref RectInt value)
        {
            reader.ReadUnmanaged(out int x, out int y, out int w, out int h);
            value = new RectInt(x, y, w, h);
        }
    }

    /// <summary>
    /// Matrix4x4 格式化器。
    /// </summary>
    [Preserve]
    public sealed class Matrix4x4Formatter : MemoryPackFormatter<Matrix4x4>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Matrix4x4 value)
        {
            writer.WriteUnmanaged(
                value.m00, value.m01, value.m02, value.m03,
                value.m10, value.m11, value.m12, value.m13,
                value.m20, value.m21, value.m22, value.m23,
                value.m30, value.m31, value.m32, value.m33);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Matrix4x4 value)
        {
            reader.ReadUnmanaged(
                out float m00, out float m01, out float m02, out float m03,
                out float m10, out float m11, out float m12, out float m13,
                out float m20, out float m21, out float m22, out float m23,
                out float m30, out float m31, out float m32, out float m33);
            value = new Matrix4x4(
                new Vector4(m00, m01, m02, m03),
                new Vector4(m10, m11, m12, m13),
                new Vector4(m20, m21, m22, m23),
                new Vector4(m30, m31, m32, m33));
        }
    }

    /// <summary>
    /// LayerMask 格式化器。
    /// </summary>
    [Preserve]
    public sealed class LayerMaskFormatter : MemoryPackFormatter<LayerMask>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref LayerMask value)
        {
            writer.WriteUnmanaged(value.value);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref LayerMask value)
        {
            reader.ReadUnmanaged(out int v);
            value = new LayerMask { value = v };
        }
    }

    /// <summary>
    /// AnimationCurve 格式化器。
    /// 序列化所有关键帧数据（time, value, inTangent, outTangent, inWeight, outWeight, weightedMode）
    /// 以及 preWrapMode / postWrapMode。
    /// </summary>
    [Preserve]
    public sealed class AnimationCurveFormatter : MemoryPackFormatter<AnimationCurve>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref AnimationCurve value)
        {
            if (value == null)
            {
                writer.WriteUnmanaged(0);
                return;
            }

            var keys = value.keys;
            writer.WriteUnmanaged(keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                ref var k = ref keys[i];
                writer.WriteUnmanaged(
                    k.time, k.value, k.inTangent, k.outTangent,
                    k.inWeight, k.outWeight, (int)k.weightedMode);
            }

            writer.WriteUnmanaged((int)value.preWrapMode, (int)value.postWrapMode);
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref AnimationCurve value)
        {
            reader.ReadUnmanaged(out int keyCount);
            if (keyCount == 0)
            {
                value = new AnimationCurve();
                return;
            }

            var keys = new Keyframe[keyCount];
            for (int i = 0; i < keyCount; i++)
            {
                reader.ReadUnmanaged(
                    out float time, out float val, out float inTan, out float outTan,
                    out float inW, out float outW, out int wm);
                keys[i] = new Keyframe(time, val, inTan, outTan, inW, outW)
                {
                    weightedMode = (WeightedMode)wm
                };
            }

            reader.ReadUnmanaged(out int preMode, out int postMode);
            value = new AnimationCurve(keys)
            {
                preWrapMode = (WrapMode)preMode,
                postWrapMode = (WrapMode)postMode
            };
        }
    }

    /// <summary>
    /// Gradient 格式化器。
    /// 序列化 colorKeys（color + time）和 alphaKeys（alpha + time）。
    /// </summary>
    [Preserve]
    public sealed class GradientFormatter : MemoryPackFormatter<Gradient>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Gradient value)
        {
            if (value == null)
            {
                writer.WriteUnmanaged(0);
                return;
            }

            var colorKeys = value.colorKeys;
            writer.WriteUnmanaged(colorKeys.Length);
            for (int i = 0; i < colorKeys.Length; i++)
            {
                var c = colorKeys[i].color;
                writer.WriteUnmanaged(c.r, c.g, c.b, c.a, colorKeys[i].time);
            }

            var alphaKeys = value.alphaKeys;
            writer.WriteUnmanaged(alphaKeys.Length);
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                writer.WriteUnmanaged(alphaKeys[i].alpha, alphaKeys[i].time);
            }
        }

        public override void Deserialize(ref MemoryPackReader reader, scoped ref Gradient value)
        {
            reader.ReadUnmanaged(out int colorKeyCount);
            var colorKeys = new GradientColorKey[colorKeyCount];
            for (int i = 0; i < colorKeyCount; i++)
            {
                reader.ReadUnmanaged(out float r, out float g, out float b, out float a, out float time);
                colorKeys[i] = new GradientColorKey(new Color(r, g, b, a), time);
            }

            reader.ReadUnmanaged(out int alphaKeyCount);
            var alphaKeys = new GradientAlphaKey[alphaKeyCount];
            for (int i = 0; i < alphaKeyCount; i++)
            {
                reader.ReadUnmanaged(out float alpha, out float time);
                alphaKeys[i] = new GradientAlphaKey(alpha, time);
            }

            value = new Gradient();
            value.SetKeys(colorKeys, alphaKeys);
        }
    }
}
