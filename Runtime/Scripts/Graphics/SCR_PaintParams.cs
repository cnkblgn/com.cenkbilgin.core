namespace Core.Graphics
{
    public readonly struct PaintParams
    {
        public readonly PaintBlend Blend;
        public readonly PaintShape Shape;
        public readonly float Length;
        public readonly float Strength;
        public readonly float Radius;

        public PaintParams(PaintBlend blend, PaintShape shape, float length, float strength, float radius)
        {
            Blend = blend;
            Shape = shape;
            Length = length;
            Strength = strength;
            Radius = radius;
        }
    }
}