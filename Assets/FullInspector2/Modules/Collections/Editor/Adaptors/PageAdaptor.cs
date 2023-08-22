using System;
using FullInspector.Rotorz.ReorderableList;
using UnityEngine;

namespace FullInspector.Internal {
    public class PageAdaptor : IReorderableListAdaptor {
        private IReorderableListAdaptor _backingAdaptor;
        private int _startIndex;
        private int _endIndex;

        public PageAdaptor(IReorderableListAdaptor backingAdaptor, int startIndex, int endIndex) {
            _backingAdaptor = backingAdaptor;
            _startIndex = startIndex;
            _endIndex = endIndex;
        }

        public int Count {
            get {
                return _endIndex - _startIndex;
            }
        }

        private int MapIndex(int index) {
            return _startIndex + index;
        }

        public bool CanDrag(int index) {
            return _backingAdaptor.CanDrag(MapIndex(index));
        }

        public bool CanRemove(int index) {
            return _backingAdaptor.CanRemove(MapIndex(index));
        }

        public void Add() {
            _backingAdaptor.Add();
        }

        public void Insert(int index) {
            _backingAdaptor.Insert(MapIndex(index));
        }

        public void Duplicate(int index) {
            _backingAdaptor.Duplicate(MapIndex(index));
        }

        public void Remove(int index) {
            _backingAdaptor.Remove(MapIndex(index));
        }

        public void Move(int sourceIndex, int destIndex) {
            _backingAdaptor.Move(MapIndex(sourceIndex), MapIndex(destIndex));
        }

        public void Clear() {
            _startIndex = 0;
            _endIndex = 0;
            _backingAdaptor.Clear();
        }

        public void DrawItem(Rect position, int index) {
            _backingAdaptor.DrawItem(position, MapIndex(index));
        }

        public float GetItemHeight(int index) {
            return _backingAdaptor.GetItemHeight(MapIndex(index));
        }
    }
}