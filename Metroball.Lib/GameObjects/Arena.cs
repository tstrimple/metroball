using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.GameObjects
{
    public class Arena : GameObject
    {
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Near { get; set; }
        public float Far { get; set; }
        private Color _color;

        public Color Color { get { return _color; }
            set { _color = value;
                CreateMesh();
            }
        }

        public Arena()
        {
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;

            Left = -1.4f;
            Right = 1.4f;
            Top = 1.0f;
            Bottom = -1.0f;
            Near = 1.0f;
            Far = 4.5f;

            Color = Color.DarkSlateGray;
            
            CreateMesh();
        }

        private void CreateMesh()
        {
            var lines = new List<VertexPositionColor[]>
                            {
                                new[]
                                    {
                                        new VertexPositionColor(new Vector3(Right, Top, Near), _color),
                                        new VertexPositionColor(new Vector3(Right, Top, Far), _color)
                                    },
                                new[]
                                    {
                                        new VertexPositionColor(new Vector3(Right, Bottom, Near), _color),
                                        new VertexPositionColor(new Vector3(Right, Bottom, Far), _color)
                                    },
                                new[]
                                    {
                                        new VertexPositionColor(new Vector3(Left, Bottom, Near), _color),
                                        new VertexPositionColor(new Vector3(Left, Bottom, Far), _color)
                                    },
                                new[]
                                    {
                                        new VertexPositionColor(new Vector3(Left, Top, Near), _color),
                                        new VertexPositionColor(new Vector3(Left, Top, Far), _color)
                                    }
                            };

            for (int i = 0; i <= 7; i++)
            {
                var depth = (i / 2.0f) + 1.0f;
                lines.Add(new[]
                               {
                                   new VertexPositionColor(
                                       new Vector3(Right,
                                                   Top,
                                                   depth), _color),
                                   new VertexPositionColor(
                                       new Vector3(Left,
                                                   Top,
                                                   depth),
                                       _color),
                                   new VertexPositionColor(
                                       new Vector3(Left,
                                                   Bottom,
                                                   depth),
                                       _color),
                                   new VertexPositionColor(
                                       new Vector3(Right,
                                                   Bottom,
                                                   depth),
                                       _color),
                                   new VertexPositionColor(
                                       new Vector3(Right,
                                                   Top,
                                                   depth), _color)
                               });
            }

            Meshes = lines.ToArray();
        }

        protected override BoundingBox BuildBoundingBox()
        {
            var min = new Vector3(Left, Bottom, Near) + Position;
            var max = new Vector3(Right, Top, Far) + Position;

            return new BoundingBox(min, max);
        }
    }
}
