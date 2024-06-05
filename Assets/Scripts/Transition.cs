using UnityEngine;

public static class Transition {
    public static void Move(GameObject oldPanel, GameObject newPanel, Direction direction=Direction.Left, LeanTweenType easingType=LeanTweenType.linear){ // moves an element off the screen and moves another into its place
        RectTransform rectOld = oldPanel.GetComponent<RectTransform>();
        RectTransform rectNew = newPanel.GetComponent<RectTransform>();

        Vector2 oldPos = Vector2.zero; // where the old panel moves to
        Vector2 newPos = Vector2.zero; // where the new panels starts out

        switch (direction){
            case Direction.Up:
                oldPos = new Vector2(rectOld.localPosition.x, Screen.height);
                newPos = oldPos * new Vector2(1, -1);
                break;
            case Direction.Down:
                oldPos = new Vector2(rectOld.localPosition.x, -Screen.height);
                newPos = oldPos * new Vector2(1, -1);
                break;
            case Direction.Left:
                oldPos = new Vector2(-Screen.width, rectOld.localPosition.y);
                newPos = oldPos * new Vector2(-1, 1);
                break;
            case Direction.Right:
                oldPos = new Vector2(Screen.width, rectOld.localPosition.y);
                newPos = oldPos * new Vector2(-1, 1);
                break;
            case Direction.DiagonalPositive:
                oldPos = new Vector2(Screen.width, Screen.height);
                newPos = oldPos * new Vector2(-1,-1);
                break;
            case Direction.DiagonalNegative:
                oldPos = new Vector2(Screen.width, -Screen.height);
                newPos = oldPos * new Vector2(-1,-1);
                break; 
        }

        rectNew.localPosition = newPos;

        LeanTween.moveLocal(newPanel, rectOld.localPosition, 5).setEase(easingType) ;
        LeanTween.moveLocal(oldPanel, oldPos, 5).setEase(easingType);
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