using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Components
{
    public enum RelativeTo
    {
        ControlCenter = 0,
        ControlTopLeft = 1,
        ViewPortCenter = 10,
        ViewPortTopLeft = 11
    }

    public struct Transform
    {
        [JsonIgnore]
        [XmlIgnore]
        public Matrix ParentMatrix { get; set; } = Matrix.Identity;

        public Vector3 CenterPoint { get; set; } = Vector3.Zero;
        public RelativeTo CenterPointRelative { get; set; } = RelativeTo.ControlCenter;

        public Vector3 Translation { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        //private Vector3 CenterPointAbsolute {get; set;} = Vector3.Zero;

        public Transform()
        {

        }

        public Matrix GetMatrix()
        {
            var matrix = MultiplyMatrices(
                     Matrix.CreateTranslation(-CenterPoint),
                     Matrix.CreateScale(Scale),
                     Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z),  // TODO: Check this
                     Matrix.CreateTranslation(CenterPoint),
                     Matrix.CreateTranslation(Translation)
                );

            return Matrix.Multiply(ParentMatrix, matrix);
        }

        private Matrix MultiplyMatrices(Matrix m1, Matrix m2, Matrix m3, Matrix m4, Matrix m5)
        {
            Matrix result = Matrix.Multiply(m1, m2);
            result = Matrix.Multiply(result, m3);
            result = Matrix.Multiply(result, m4);
            result = Matrix.Multiply(result, m5);

            return result;
        }

        public void CalcCenterPoint(UIComponent control)
        {
            // TODO: If Center point relative to Control Center then ... else if relative to Control Left-Top then etc..
            CenterPoint = new Vector3((control.FinalRect.Left + control.FinalRect.Right) / 2f, (control.FinalRect.Top + control.FinalRect.Bottom) / 2f, 0f);
        }

        public static Transform Combine(Transform t1, Transform t2, UIComponent childControl)
        {
            var combined = t2 with { ParentMatrix = t1.GetMatrix() };

            // Calculate center point of layout rect
            combined.CalcCenterPoint(childControl);

            return combined;
        }
    }
}
