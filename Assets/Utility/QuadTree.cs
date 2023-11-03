using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Theo 
{
    public class QuadTree
    {
        private readonly List<Collider> elements = new List<Collider>();

        //How many objects are allowed before subdividing
        private readonly int bucketCapacity;
        //The max depth of the tree. How many times we're allowed to subdivide before we prevent splitting.
        private readonly int maxDepth;
        //The bounds for this box
        private readonly Bounds bounds;


        private QuadTree upperLeft;
        private QuadTree upperRight;
        private QuadTree bottomLeft;
        private QuadTree bottomRight;
        private bool IsLeaf
        {
            get 
            {
                return (upperLeft == null) && (upperRight == null) && bottomLeft == null && bottomRight == null;
            }
        }
        int Level;

        public QuadTree(Bounds bounds) : this(bounds, 32, 5) { }

        public QuadTree(Bounds bounds, int bucketCapacity, int maxDepth)
        {
            this.bucketCapacity = bucketCapacity;
            this.maxDepth = maxDepth;
            this.bounds = bounds;
            Level = 0;
        }
        public QuadTree(Bounds bounds, int bucketCapacity, int maxDepth, int PreviousLevel): this(bounds, bucketCapacity, maxDepth)
        {
            Level = PreviousLevel + 1;
        }
        public void Insert(Collider collider)
        {
            if (collider == null) { return; }
            if (!BoundIsContainedInBound(collider.Bounds))
            {
                return;
            }

            if (elements.Count >= bucketCapacity)
            {
                Split();
            }

            QuadTree containingChild = GetContainingChild(collider.Bounds);

            if (containingChild != null) 
            {
                containingChild.Insert(collider);
            }
            else 
            {
                elements.Add(collider);
            }
        }
        public bool Remove(Collider collider) 
        {
            if (!collider) { return false; }

            QuadTree containingChild = GetContainingChild(collider.Bounds);

            bool removed;

            if (containingChild != null)
            {
                removed = containingChild.Remove(collider); 
            }
            else
            {
                removed = elements.Remove(collider);
            }

            int totalCountOfElementsInDecendants = elements.Count;

            CountElements(ref totalCountOfElementsInDecendants);

            if (removed && totalCountOfElementsInDecendants <= bucketCapacity) 
            {
                Merge();
            }
            return removed;
        }

        public void CountElements(ref int total) 
        {
            total += elements.Count;
            if (IsLeaf) 
            {
                return;
            }
            upperLeft.CountElements(ref total);
            upperRight.CountElements(ref total);
            bottomLeft.CountElements(ref total);
            bottomRight.CountElements(ref total);
        }
        private void Split()
        {
            if (!IsLeaf) { return; }
            if(Level + 1 > maxDepth) { return; }
            float HalfXExtents = bounds.extents.x / 2;
            float HalfYExtents = bounds.extents.y / 2;

            upperLeft = CreateChild(bounds.center + new Vector3(-HalfXExtents, HalfYExtents), bounds.extents);
            upperRight = CreateChild(bounds.center + new Vector3(HalfXExtents, HalfYExtents), bounds.extents);
            bottomRight = CreateChild(bounds.center + new Vector3(HalfXExtents, -HalfYExtents), bounds.extents);
            bottomLeft = CreateChild(bounds.center + new Vector3(-HalfXExtents, -HalfYExtents), bounds.extents);

            List<Collider> tempElements = new List<Collider>();
            tempElements.AddRange(elements);

            foreach (Collider element in tempElements) 
            {
                QuadTree containingChild = GetContainingChild(element.Bounds);

                if (containingChild != null) 
                {
                    elements.Remove(element);
                    containingChild.Insert(element);
                }
            }
        }
        private void Merge()
        {
            if (IsLeaf) return;

            elements.AddRange(upperLeft.elements);
            elements.AddRange(upperRight.elements);
            elements.AddRange(bottomLeft.elements);
            elements.AddRange(bottomRight.elements);

            upperLeft = upperRight = bottomLeft = bottomRight = null;
        }
        public void GetAllBounds(ref List<Bounds> boundsList) 
        {
            boundsList.Add(bounds);
            if (IsLeaf) return;

            upperLeft.GetAllBounds(ref boundsList);
            upperRight.GetAllBounds(ref boundsList);
            bottomLeft.GetAllBounds(ref boundsList);
            bottomRight.GetAllBounds(ref boundsList);
        }
        private QuadTree CreateChild(Vector3 center, Vector3 size)
        {
            Bounds _bounds = new Bounds(center, size);
            return new QuadTree(_bounds, bucketCapacity,maxDepth,Level);
        }
        private QuadTree GetContainingChild(Bounds bounds)
        {
            if (IsLeaf) 
            {
                return null;
            }
            if (upperLeft.BoundIsContainedInBound(bounds)) 
            {
                return upperLeft;
            }
            else if (upperRight.BoundIsContainedInBound(bounds))
            {
                return upperRight;
            }
            else if (bottomLeft.BoundIsContainedInBound(bounds))
            {
                return bottomLeft;
            }
            else if (bottomRight.BoundIsContainedInBound(bounds))
            {
                return bottomRight;
            }

            return null;
        }
        public bool BoundIsContainedInBound(Bounds bounds) 
        {
            Vector3[] points = new Vector3[]
            {
                new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y),
                new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y),
                new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y),
                new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y)
            };

            foreach (Vector3 point in points) 
            {
                if (!this.bounds.Contains(point)) 
                {
                    return false;
                }
            }
            return true;
        }
        public IEnumerable<Collider> FindPotentialCollisions(Collider element)
        {
            var nodes = new Queue<QuadTree>();
            var potentialCollisions = new List<Collider>();

            nodes.Enqueue(this);

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();

                if (!element.Bounds.Intersects(node.bounds))
                {
                    continue;
                }

                potentialCollisions.AddRange(node.elements);

                if (!node.IsLeaf)
                {
                    if (element.Bounds.Intersects(node.upperLeft.bounds))
                        nodes.Enqueue(node.upperLeft);

                    if (element.Bounds.Intersects(node.upperRight.bounds))
                        nodes.Enqueue(node.upperRight);

                    if (element.Bounds.Intersects(node.bottomLeft.bounds))
                        nodes.Enqueue(node.bottomLeft);

                    if (element.Bounds.Intersects(node.bottomRight.bounds))
                        nodes.Enqueue(node.bottomRight);

                }
            }
            return potentialCollisions;
        }



        public IEnumerable<Collider> FindCollision(Collider element) 
        {
            var nodes = new Queue<QuadTree>();
            var collisions = new List<Collider>();

            nodes.Enqueue(this);

            while (nodes.Count > 0) 
            {
                var node = nodes.Dequeue();

                if (!element.Bounds.Intersects(node.bounds)) 
                {
                    continue;
                }

                collisions.AddRange(node.elements.Where(e => e.Bounds.Intersects(element.Bounds)));

                if (!node.IsLeaf)
                {
                    if (element.Bounds.Intersects(node.upperLeft.bounds))
                        nodes.Enqueue(node.upperLeft);

                    if (element.Bounds.Intersects(node.upperRight.bounds))
                        nodes.Enqueue(node.upperRight);

                    if (element.Bounds.Intersects(node.bottomLeft.bounds))
                        nodes.Enqueue(node.bottomLeft);

                    if (element.Bounds.Intersects(node.bottomRight.bounds))
                        nodes.Enqueue(node.bottomRight);

                }
            }
            return collisions;
        }

    }
}

