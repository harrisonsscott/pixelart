using UnityEngine;

public static class Transition {
    public static void Move(GameObject from, GameObject to, Direction direction=Direction.Left){ // moves an element off the screen and moves another into its place
        RectTransform fromRect = from.GetComponent<RectTransform>();
        RectTransform toRect = to.GetComponent<RectTransform>();

        Vector2 targetPos = fromRect.localPosition;

        LeanTween.moveLocal(to, targetPos, 1);
    }
}

public enum Direction {
    Up,
    Down,
    Left,
    Right,
    DiagonalPositive,
    DiagonalNegative
}