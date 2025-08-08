using UnityEngine;

public enum MovementDirection
{
    Right,
    Above,
    Left,
    Below
}

public class CharacterRaycaster2D : MonoBehaviour
{
    [Range(1, 10)]
    public int accuracy = 4;
    [Range(0, 0.1f)]
    public float skinWidth = 0.02f;
    public LayerMask collidableElements;
    public LayerMask collidableFromAboveOnly;
    public Transform self;
    public BoxCollider2D selfBox;

    Vector2 GetPointPositionInBox(float x, float y) => GetPointPositionInBox(new Vector2(x, y));
    Vector2 GetPointPositionInBox(Vector2 position)
    {
        Vector2 result = self.position;

        result.x += selfBox.offset.x * self.lossyScale.x;
        result.y += selfBox.offset.y * self.lossyScale.y;

        result.x += position.x * selfBox.size.x * 0.5f * self.lossyScale.x;
        result.y += position.y * selfBox.size.y * 0.5f * self.lossyScale.y;

        return result;
    }

    Vector2 DirectionToVector(MovementDirection dir)
    {
        if (dir == MovementDirection.Right) return Vector2.right;
        if (dir == MovementDirection.Above) return Vector2.up;
        if (dir == MovementDirection.Left) return Vector2.left;
        if (dir == MovementDirection.Below) return Vector2.down;

        return Vector2.zero;
    }

    public bool CalculateCollision(MovementDirection dir, float dist)
    {
        Vector2 direction = DirectionToVector(dir);
        LayerMask usedLayerMask = collidableElements;
        if (dir == MovementDirection.Below) usedLayerMask |= collidableFromAboveOnly;
        
        if (accuracy == 1)
        {
            Vector2 origin = GetPointPositionInBox(direction);
            origin += direction * skinWidth;
            RaycastHit2D hitResult = Physics2D.Raycast(origin, direction, dist, usedLayerMask);
            return hitResult.collider != null;
        }

        Vector2 cornerA = Vector2.zero;
        Vector2 cornerB = Vector2.zero;

        if (dir == MovementDirection.Below)
        {
            cornerA = GetPointPositionInBox(-1, -1);
            cornerB = GetPointPositionInBox(1, -1);
            cornerA.x += skinWidth;
            cornerB.x -= skinWidth;
        }
        if (dir == MovementDirection.Above)
        {
            cornerA = GetPointPositionInBox(-1, 1);
            cornerB = GetPointPositionInBox(1, 1);
            cornerA.x += skinWidth;
            cornerB.x -= skinWidth;
        }
        if (dir == MovementDirection.Left)
        {
            cornerA = GetPointPositionInBox(-1, -1);
            cornerB = GetPointPositionInBox(-1, 1);
            cornerA.y += skinWidth;
            cornerB.y -= skinWidth;
        }
        if (dir == MovementDirection.Right)
        {
            cornerA = GetPointPositionInBox(1, -1);
            cornerB = GetPointPositionInBox(1, 1);
            cornerA.y += skinWidth;
            cornerB.y -= skinWidth;
        }

        for (int i = 0; i < accuracy; i++)
        {
            float ratio = ((float)i) / (float)(accuracy-1);
            Vector2 origin = Vector2.Lerp(cornerA, cornerB, ratio);
            origin += direction * skinWidth;

            RaycastHit2D hitResult = Physics2D.Raycast(origin, direction, dist, usedLayerMask);
            Debug.DrawRay(origin, direction, Color.blue);

            if (hitResult.collider != null) return true;
        }

        return false;
    }
}
