using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class UndoableAction
    {
        public Action Action{get; private set;}
        public Action UndoAction{get; private set;}

        public UndoableAction(Action action, Action undoAction)
        {
            this.Action = action;
            this.UndoAction = undoAction;
            this.Action += action;
        }

        public void Invoke()
        {
            Action();
        }

        public void Undo()
        {
            this.UndoAction();
        }

        public void ReplaceUndoAction(Action undoAction)
        {
            this.UndoAction = undoAction;
        }

        public static implicit operator Action(UndoableAction action) => action.Action;
    }

    public class UndoableAction<T>
    {
        public Action<T> Action{get; private set;}
        public Action<T> UndoAction{get; private set;}

        public UndoableAction(Action<T> action, Action<T> undoAction)
        {
            this.Action = action;
            this.UndoAction = undoAction;
        }

        public void Invoke(T parm)
        {
            Action(parm);
        }

        public void Undo(T parm)
        {
            this.UndoAction(parm);
        }

        public void ReplaceUndoAction(Action<T> undoAction)
        {
            this.UndoAction = undoAction;
        }

        public static implicit operator Action<T>(UndoableAction<T> action) => action.Action;
    }
}