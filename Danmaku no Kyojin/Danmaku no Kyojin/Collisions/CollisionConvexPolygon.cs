﻿using System;
using Danmaku_no_Kyojin.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Danmaku_no_Kyojin.Utils;
using System.Diagnostics;

namespace Danmaku_no_Kyojin.Collisions
{
    class CollisionConvexPolygon : CollisionElement
    {
        #region Fields

        public List<Vector2> Vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }

        public bool IsFilled { get; set; }

        private List<Vector2> _axes;
        private List<Vector2> _circleAxes;
        private List<Vector2> _vertices;
        private Vector2 _localPosition;
        private Vector2 _center;
        private Vector2 _size;
        private float _healthPoint;

        #endregion

        #region Accessors

        public List<Vector2> GetAxes()
        {
            return _axes;
        }

        #endregion

        public CollisionConvexPolygon(Entity parent, Vector2 relativePosition, List<Vector2> vertices, float healthPoint = 100)
            : base(parent, relativePosition)
        {
            Parent = parent;
            Vertices = vertices;
            _axes = new List<Vector2>();
            _circleAxes = new List<Vector2>();
            _healthPoint = healthPoint;
            _localPosition = Vector2.Zero;
            _center = Vector2.Zero;

            ComputeAxes();
        }

        public override bool Intersects(CollisionElement collisionElement)
        {
            if (collisionElement is CollisionConvexPolygon)
                return Intersects(collisionElement as CollisionConvexPolygon);

            if (collisionElement is CollisionCircle)
                return Intersects(collisionElement as CollisionCircle);

            return collisionElement.Intersects(this);
        }

        private bool Intersects(CollisionConvexPolygon element)
        {
            // loop over the axes of this polygon
            for (var i = 0; i < _axes.Count; i++)
            {
                var axis = _axes[i];
                // project both shapes onto the axis
                var p1 = Project(axis);
                var p2 = element.Project(axis);
                // do the projections overlap?
                if (!Overlap(p1, p2))
                {
                    // then we can guarantee that the shapes do not overlap
                    return false;
                }
            }
            
            // loop over element polygon's axes
            List<Vector2> axes = element.GetAxes();
            for (int i = 0; i < axes.Count; i++)
            {
                Vector2 axis = axes[i];
                // project both shapes onto the axis
                Vector2 p1 = Project(axis);
                Vector2 p2 = element.Project(axis);
                // do the projections overlap?
                if (!Overlap(p1, p2))
                {
                    // then we can guarantee that the shapes do not overlap
                    return false;
                }
            }

            // if we get here then we know that every axis had overlap on it
            // so we can guarantee an intersection
            return true;
        }

        private bool Intersects(CollisionCircle element)
        {
            ComputeCircleAxes(element);

            // loop over the axes of this polygon
            for (int i = 0; i < _axes.Count; i++)
            {
                Vector2 axis = _axes[i];
                // project both shapes onto the axis
                Vector2 p1 = this.Project(axis);
                Vector2 p2 = element.Project(axis);

                // do the projections overlap?
                if (!Overlap(p1, p2))
                {
                    // then we can guarantee that the shapes do not overlap
                    return false;
                }
            }

            // if we get here then we know that every axis had overlap on it
            // so we can guarantee an intersection
            return true;
        }

        public override void Draw(SpriteBatch sp)
        {
            if (Vertices.Count == 0)
                return;

            Vector2 previousPosition = GetWorldPosition(Vertices[0]);

            for (int i = 1; i <= Vertices.Count; i++)
            {
                Vector2 position = GetWorldPosition(i == Vertices.Count ? Vertices[0] : Vertices[i]);

                sp.DrawLine(
                    previousPosition.X,
                    previousPosition.Y,
                    position.X,
                    position.Y, Color.Red);

                Vector2 axis = Vector2.Normalize(position - previousPosition);
                /*
                sp.DrawLine(
                    (previousPosition.X + position.X) / 2f, 
                    (previousPosition.Y + position.Y) / 2f,
                    (previousPosition.X) + axis.Y * 2000, 
                    (previousPosition.Y) - axis.X * 2000,
                    Color.Red);
                */
                /*
                if (_circleAxes.Count > 0)
                {
                    sp.DrawLine(
                        previousPosition.X, previousPosition.Y,
                        (previousPosition.X) + _circleAxes[i - 1].X * 2000, (previousPosition.Y) - _circleAxes[i - 1].Y * -2000,
                        Color.Red);
                }
                */

                previousPosition = position;
            }
        }

        public Vector2 GetSize()
        {
            if (_size == Vector2.Zero)
                ComputeSize();

            return _size;
        }

        private void ComputeSize()
        {
            var min = _vertices[0];
            var max = _vertices[0];

            for (int i = 1; i < _vertices.Count; i++)
            {
                if (_vertices[i].X < min.X ||
                    _vertices[i].X.Equals(min.X) && _vertices[i].Y < min.Y)
                {
                    min = _vertices[i];
                }

                if (_vertices[i].X > max.X ||
                    _vertices[i].X.Equals(max.X) && _vertices[i].Y > max.Y)
                {
                    max = _vertices[i];
                }
            }

            _size = new Vector2(Math.Abs(max.X - min.X), Math.Abs(max.Y - min.Y));
        }

        // Returns the top-left vertex
        public Vector2 GetLocalPosition()
        {
            if (_localPosition == Vector2.Zero)
                ComputeLocalPosition();

            return _localPosition;
        }

        private void ComputeLocalPosition()
        {
            _localPosition = _vertices[0];

            for (int i = 1; i < _vertices.Count; i++)
            {
                if (_vertices[i].X < _localPosition.X ||
                    _vertices[i].X.Equals(_localPosition.X) && _vertices[i].Y < _localPosition.Y)
                {
                    _localPosition = _vertices[i];
                }
            }
        }

        public Vector2 GetWorldPosition()
        {
            return GetWorldPosition(GetLocalPosition());
        }

        private Vector2 GetWorldPosition(Vector2 vertex)
        {
            var cos = (float)Math.Cos(Parent.Rotation);
            var sin = (float)Math.Sin(Parent.Rotation);

            return new Vector2(
                cos * (vertex.X - Parent.Origin.X) - sin * (vertex.Y - Parent.Origin.Y) + Parent.Position.X,
                sin * (vertex.X - Parent.Origin.X) + cos * (vertex.Y - Parent.Origin.Y) + Parent.Position.Y
            );
        }

        public override Vector2 GetCenter()
        {
            if (_center == Vector2.Zero)
                ComputeCenter();

            return _center;
        }

        public Vector2 GetCenterInWorldSpace()
        {
            return GetWorldPosition(GetCenter());
        }

        // Compute the center of the shape (it corresponds to what we call the "centroid")
        private void ComputeCenter()
        {
            var center = Vector2.Zero;
            var previousCenter = Vector2.Zero;
            for (var i = 0; i < Vertices.Count; i++)
            {
                var currentCenter = (Vertices[i] + Vertices[(i + 1) % Vertices.Count]) / 2f;

                if (previousCenter != Vector2.Zero)
                {
                    center = (previousCenter + currentCenter) / 2f;
                }

                previousCenter = currentCenter;
            }

            _center = (center + previousCenter) / 2f;
        }

        private void ComputeAxes()
        {
            if (Vertices.Count == 0)
                return;

            // We start by deleting former axis
            _axes.Clear();

            Vector2 previousPosition = GetWorldPosition(Vertices[0]);

            for (int i = 1; i <= Vertices.Count; i++)
            {
                Vector2 position = GetWorldPosition(i == Vertices.Count ? Vertices[0] : Vertices[i]);

                Vector2 edge = position - previousPosition;
                var normal = new Vector2(edge.Y, -edge.X);

                // We want to avoid to have parallel axes because projection would give us the same result
                if (!_axes.Contains(normal) && !_axes.Contains(-normal))
                    _axes.Add(normal);

                previousPosition = position;
            }
        }

        private void ComputeCircleAxes(CollisionCircle element)
        {
            _circleAxes.Clear();

            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector2 position = GetWorldPosition(Vertices[i]);

                Vector2 edge = element.GetCenter() - position;
                var normal = new Vector2(edge.Y, -edge.X);
                _circleAxes.Add(normal);
            }
        }

        public bool Overlap(Vector2 p1, Vector2 p2)
        {
            // P = (X, Y) with X = min and Y = max
            return (p1.Y > p2.X && p1.X < p2.Y) || (p2.Y > p1.X && p2.Y < p1.X);
        }

        public Vector2 Project(Vector2 axis)
        {
            if (Vertices.Count == 0)
                return Vector2.Zero;

            float min = Vector2.Dot(new Vector2(GetWorldPosition(Vertices[0]).X, GetWorldPosition(Vertices[0]).Y), axis);
            float max = min;
            for (int i = 1; i < Vertices.Count; i++)
            {
                // NOTE: the axis must be normalized to get accurate projections
                float p = Vector2.Dot(new Vector2(GetWorldPosition(Vertices[i]).X, GetWorldPosition(Vertices[i]).Y), axis);
                if (p < min)
                {
                    min = p;
                }
                else if (p > max)
                {
                    max = p;
                }
            }

            return new Vector2(min, max);
        }
    }
}
