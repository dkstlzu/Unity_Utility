using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Object = UnityEngine.Object;

#pragma warning disable CS0414 // 필드가 대입되었으나 값이 사용되지 않습니다 => 개발시 값 디버깅용 값들이기 때문에 warning 끕니다.

namespace dkstlzu.Utility
{
    /// <summary>
    /// 왼쪽 위 귀퉁이에서 배치가 시작된다고 가정되어있습니다.
    /// </summary>
    [Serializable]
    public class FastScrollGrid
    {
        [Serializable]
        public class Item
        {
            public int Index;
            public GameObject gameObject;
            public bool IsObjectAllocated => gameObject;

            public Item()
            {
            }

            public virtual void OnAllocateObject(GameObject obj)
            {
                gameObject = obj;

#if UNITY_EDITOR
                if (obj.name.EndsWith("(Clone)"))
                {
                    obj.name = obj.name.Replace("(Clone)", $" ({Index})");
                }

                int indexStart = obj.name.IndexOf("(", StringComparison.Ordinal);
                int indexEnd = obj.name.IndexOf(")", StringComparison.Ordinal);
                if (indexStart >= 0 && indexEnd >= 0)
                {
                    string indexString = obj.name.Substring(indexStart + 1, indexEnd - indexStart - 1);
                    obj.name = obj.name.Replace(indexString, Index.ToString());
                }
#endif
            }

            public virtual GameObject DeallocateObject()
            {
                GameObject beforeObj = gameObject;
                gameObject = null;
                return beforeObj;
            }

            public virtual void RefreshByTime()
            {
            }
        }

        [SerializeField] [ReadOnly] protected ScrollRect _scrollRect;
        protected RectTransform _scrollRectTrans;
        [SerializeField] [ReadOnly] protected RectTransform _content;

        protected GameObject _baseObj;

        [SerializeField] [ReadOnly] protected int _fixedRowColumn;
        [SerializeField] [ReadOnly] protected RectOffset _padding;
        [SerializeField] [ReadOnly] protected Vector2 _spacing;
        [SerializeField] [ReadOnly] protected Vector2 _elementSize;
        [SerializeField] [ReadOnly] protected int _row;
        [SerializeField] [ReadOnly] protected int _column;

        protected List<GameObject> _objPool = new List<GameObject>();

        [SerializeReference] public List<Item> ItemList = new List<Item>();

        protected Action<Vector2> _onScrollValueChanged;

        protected int _reqMoreTotalCount;
        protected Action<Action> _onRefreshToServer;
        protected Action<Action> _onRequestMoreToServer;
        protected bool _refreshPending;
        protected bool _reqPending;

        public FastScrollGrid(ScrollRect scrollRect, RectTransform content, RectOffset padding = default, Vector2 spacing = default, int fixedRowColumn = -1)
        {
            _scrollRect = scrollRect;
            _content = content;
            _scrollRectTrans = scrollRect.GetComponent<RectTransform>();

            _padding = padding;
            _spacing = spacing;
            _fixedRowColumn = fixedRowColumn;

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.transform as RectTransform);
            RecalculateContentSize();

            scrollRect.onValueChanged.AddListener(OnValueChanged);
        }

        public void SetElement(GameObject baseObj, int preInstantiateCount, Vector2 forceSize = default)
        {
            _baseObj = baseObj;
            RectTransform rt = (RectTransform)baseObj.transform;
            _elementSize = rt.rect.size;

            _objPool = new List<GameObject>(preInstantiateCount);

            if (forceSize != default)
            {
                _elementSize = forceSize;
            }

            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = Vector2.zero;

            baseObj.SetActive(false);

            for (int i = 0; i < preInstantiateCount; i++)
            {
                GameObject newObj = Object.Instantiate(baseObj, _content);
                ((RectTransform)newObj.transform).sizeDelta = _elementSize;
                _objPool.Add(newObj);
            }
        }

        public int GetCount()
        {
            return ItemList.Count;
        }

        public void SetOnScrollValueChanged(Action<Vector2> onScrollValueChanged)
        {
            _onScrollValueChanged = onScrollValueChanged;
        }

        private void RecalculateContentSize()
        {
            // 왼쪽위 귀퉁이에서 시작된다고 가정합니다.
            if (_scrollRect.horizontal && _scrollRect.vertical)
            {
                // 미구현
                throw new NotImplementedException();
            }
            else if (_scrollRect.horizontal && !_scrollRect.vertical)
            {
                // 미구현
                throw new NotImplementedException();
            }
            else if (!_scrollRect.horizontal && _scrollRect.vertical)
            {
                _content.anchorMin = new Vector2(0, 1);
                _content.anchorMax = new Vector2(1, 1);
                ((RectTransform)_content.transform).pivot = new Vector2(0, 1);
                _content.localPosition = Vector2.zero;

                if (_padding == null)
                {
                    _content.sizeDelta = new Vector2(0, 0);
                }
                else
                {
                    _content.offsetMin = new Vector2(_padding.left, 0);
                    _content.offsetMax = new Vector2(-_padding.right, 0);
                    _content.sizeDelta = new Vector2(_content.sizeDelta.x, _padding.top + _padding.bottom);
                }

                _column = (int)((_content.rect.width + _spacing.x) / (_elementSize.x + _spacing.x));
                if (_fixedRowColumn > 0)
                {
                    _column = _fixedRowColumn;
                }

                _column = Mathf.Max(_column, 1);

                _row = ItemList.Count / _column + (ItemList.Count % _column > 0 ? 1 : 0);

                _row = Mathf.Max(_row, 1);

                _content.sizeDelta += new Vector2(0, _row * _elementSize.y + _spacing.y * (_row - 1));
            }
            else if (!_scrollRect.horizontal && !_scrollRect.vertical)
            {
                // 미구현
                throw new NotImplementedException();
            }
        }

        private void AddItemInternal(Item addItem)
        {
            addItem.Index = ItemList.Count;

            ItemList.Add(addItem);
        }

        public void AddItem(Item addItem)
        {
            AddItemInternal(addItem);

            RecalculateContentSize();

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        public void AddItems(List<Item> addItems)
        {
            foreach (Item addItem in addItems)
            {
                AddItemInternal(addItem);
            }

            RecalculateContentSize();

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        private void RemoveItemInternal(Item removeItem)
        {
            int index = ItemList.IndexOf(removeItem);
            if (index < 0)
            {
                return;
            }

            ReturnToPool(removeItem.gameObject);
            removeItem.DeallocateObject();
            ItemList.RemoveAt(index);

            for (int i = index; i < ItemList.Count; i++)
            {
                ItemList[i].Index = i;
            }
        }

        public void RemoveItem(Item removeItem)
        {
            RemoveItemInternal(removeItem);

            RecalculateContentSize();

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        public void RemoveItems(List<Item> removeItems)
        {
            foreach (Item removeItem in removeItems.OrderByDescending(i => i.Index))
            {
                RemoveItemInternal(removeItem);
            }

            RecalculateContentSize();

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        public void ChangeItems(List<Item> changeItems)
        {
            float widthDiff = _content.rect.width - _scrollRectTrans.rect.width;
            float heightDiff = _content.rect.height - _scrollRectTrans.rect.height;
            Vector2 scrollableSize = new Vector2(Mathf.Max(widthDiff, 0), Mathf.Max(heightDiff, 0));
            Vector2 scrollPosition = _scrollRect.normalizedPosition;
            scrollPosition.y = 1 - scrollPosition.y;
            scrollPosition *= scrollableSize;

            scrollPosition.x += _padding.left;
            scrollPosition.y += _padding.top;

            Clear();

            AddItems(changeItems);

            ScrollTo(scrollPosition.x, scrollPosition.y);
        }

        public void SortItems(Comparison<Item> comparison)
        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                ItemList[i].DeallocateObject();
            }

            ItemList.Sort(comparison);

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);
        }

        void OnValueChanged(Vector2 value)
        {
            if (ItemList == null || ItemList.Count == 0)
                return;

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();

            if (_nowTopIndex < topIndex)
            {
                for (int i = _nowTopIndex; i < topIndex; i++)
                {
                    if (i >= ItemList.Count || i < 0)
                        continue;
                    if (ItemList[i].IsObjectAllocated == false)
                        continue;
                    GameObject obj = ItemList[i].DeallocateObject();
                    ReturnToPool(obj);
                }
            }

            if (_nowTopIndex + _nowShowCountOnScrollRect > topIndex + showCountOnScrollRect)
            {
                for (int i = topIndex + showCountOnScrollRect; i < _nowTopIndex + _nowShowCountOnScrollRect; i++)
                {
                    if (i >= ItemList.Count || i < 0)
                    {
                        continue;
                    }

                    if (ItemList[i].IsObjectAllocated == false)
                    {
                        continue;
                    }

                    GameObject obj = ItemList[i].DeallocateObject();
                    ReturnToPool(obj);
                }
            }

            ShowItems(topIndex, showCountOnScrollRect);

            _onScrollValueChanged?.Invoke(value);

            if (value.y + _scrollRectTrans.rect.height > GetTotalHeight() && _onRefreshToServer != null && _refreshPending == false)
            {
                _refreshPending = true;
                _onRefreshToServer(() => { _refreshPending = false; });
            }

            if (value.y < 0 && _onRequestMoreToServer != null && _reqMoreTotalCount > ItemList.Count && _reqPending == false)
            {
                _reqPending = true;
                _onRequestMoreToServer(() => { _reqPending = false; });
            }
        }

        [SerializeField] [ReadOnly] protected int _nowTopIndex = 0;
        [SerializeField] [ReadOnly] protected int _nowShowCountOnScrollRect = 0;
        [SerializeField] [ReadOnly] protected float _startX;
        [SerializeField] [ReadOnly] protected float _endX;
        [SerializeField] [ReadOnly] protected float _startY;
        [SerializeField] [ReadOnly] protected float _endY;

        (int startIndex, int showCount) GetTopIndex()
        {
            // scrollRect.normalizedPosition은 왼쪽아래방향이 음수 오른쪽 위방향이 양수
            // 우리 게임 기준 위에서 아래로 내리는 방향이 다음 오브젝트가 등장하는 방향으로 쓰이고 있어서 y를 뒤집어서 사용합니다.
            Vector2 scrollPos = _scrollRect.normalizedPosition;
            scrollPos.y = 1 - scrollPos.y;

            float scrollY = scrollPos.y * (_content.rect.height - _scrollRectTrans.rect.height);
            int findIndex = -1;
            int showCountOnScrollRect = 0;
            _startX = 0;
            _endX = 0;
            _startY = 0;
            _endY = 0;

            // 왼쪽위 귀퉁이에서 시작된다고 가정합니다.
            if (_scrollRect.horizontal && _scrollRect.vertical)
            {
                // 미구현
                throw new NotImplementedException();
            }
            else if (_scrollRect.horizontal && !_scrollRect.vertical)
            {
                // 미구현
                throw new NotImplementedException();
            }
            else if (!_scrollRect.horizontal && _scrollRect.vertical)
            {
                for (int i = 0; i < ItemList.Count; i += _column)
                {
                    if (i == 0)
                    {
                        _startY = _padding.top;
                        _endY = _startY + _elementSize.y;
                    }
                    else
                    {
                        _startY = _endY + _spacing.y;
                        _endY = _startY + _elementSize.y;
                    }

                    if (findIndex < 0)
                    {
                        if (scrollY <= _endY)
                        {
                            showCountOnScrollRect = _column;
                            findIndex = i;
                        }
                    }
                    else if (scrollY + _scrollRectTrans.rect.height <= _startY)
                    {
                        showCountOnScrollRect += _column;
                        break;
                    }
                    else
                    {
                        showCountOnScrollRect += _column;
                    }
                }
            }
            else if (!_scrollRect.horizontal && !_scrollRect.vertical)
            {
                // 미구현
                throw new NotImplementedException();
            }

            showCountOnScrollRect = Mathf.Min(showCountOnScrollRect, ItemList.Count - findIndex);
            return (findIndex, showCountOnScrollRect);
        }

        public void ShowItems(int topIndex, int showCountOnScrollRect)
        {
            _nowTopIndex = topIndex;
            _nowShowCountOnScrollRect = showCountOnScrollRect;

            if (topIndex < 0 || topIndex >= ItemList.Count)
            {
                return;
            }

            float showX;
            float showY;

            int topRow = topIndex / _column;
            int topColumn = topIndex % _column;
            int bottomRow = (topIndex + showCountOnScrollRect - 1) / _column;
            int bottomColumn = (topIndex + showCountOnScrollRect - 1) % _column;

            showX = topColumn * _elementSize.x + Mathf.Max(topColumn - 1, 0) * _spacing.x;
            showY = topRow * _elementSize.y + Mathf.Max(topRow - 1, 0) * _spacing.y + _padding.top;

            for (int i = topRow; i <= bottomRow; i++)
            {
                if (i > 0)
                {
                    showY += _spacing.y;
                }

                for (int j = i == topRow ? topColumn : 0; j < _column; j++)
                {
                    if (i * _column + j >= ItemList.Count)
                    {
                        break;
                    }

                    if (j > 0)
                    {
                        showX += _spacing.x;
                    }
                    else
                    {
                        showX = 0;
                    }

                    Item item = ItemList[i * _column + j];
                    if (item.IsObjectAllocated == false)
                    {
                        var trans = GetFromPool();
                        trans.gameObject.SetActive(true);
                        item.OnAllocateObject(trans.gameObject);

                        Vector3 pos = trans.localPosition;
                        pos.x = showX;
                        pos.y = -showY;
                        trans.localPosition = pos;
                    }

#if UNITY_EDITOR
                    item.gameObject.transform.SetAsLastSibling();
#endif

                    showX += _elementSize.x;
                }

                showY += _elementSize.y;
            }
        }

        RectTransform GetFromPool()
        {
            if (_objPool.Count == 0)
            {
                GameObject newObj = Object.Instantiate(_baseObj, _content);
                return (RectTransform)newObj.transform;
            }
            else
            {
                Transform trans = _objPool[0].transform;
                _objPool.RemoveAt(0);
                return (RectTransform)trans;
            }
        }

        void ReturnToPool(GameObject obj)
        {
            _objPool.Add(obj);
            obj.SetActive(false);
        }

        public void RefreshAll()
        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].IsObjectAllocated == true)
                {
                    ItemList[i].OnAllocateObject(ItemList[i].gameObject);
                }
            }
        }

        public void RefreshByTime()
        {
            foreach (var item in ItemList)
            {
                if (item.gameObject != null)
                    item.RefreshByTime();
            }
        }

        public float GetTotalHeight()
        {
            return _content.rect.height;
        }

        public void SetOnRefreshToServer(Action<Action> onRefreshToServer)
        {
            _onRefreshToServer = _onRequestMoreToServer;
        }

        public void SetRequestMoreTotalCount(int count)
        {
            _reqMoreTotalCount = count;
        }

        public void SetOnRequestMoreToServer(int reqMoreTotalCount, Action<Action> onRequestMoreToServer)
        {
            _reqMoreTotalCount = reqMoreTotalCount;
            _onRequestMoreToServer = onRequestMoreToServer;
        }

        /// <summary>
        /// x, y는 절대 값입니다. 비율이아니라
        /// </summary>
        public void ScrollTo(float x, float y)
        {
            float widthDiff = _content.rect.width - _scrollRectTrans.rect.width;
            float heightDiff = _content.rect.height - _scrollRectTrans.rect.height;
            Vector2 scrollableSize = new Vector2(widthDiff, heightDiff);

            float sx = 0;
            float sy = 1;

            if (scrollableSize.x > 0)
            {
                sx = x / scrollableSize.x;
                sx -= _padding.left / scrollableSize.x;
            }

            if (scrollableSize.y > 0)
            {
                sy = 1 - y / scrollableSize.y;
                sy += _padding.top / scrollableSize.y;
            }

            _scrollRect.normalizedPosition = new Vector2(sx, sy);
        }

        public void Clear()
        {
            foreach (var item in ItemList)
            {
                if (item.IsObjectAllocated == false)
                    continue;
                GameObject obj = item.DeallocateObject();
                ReturnToPool(obj);
            }

            ItemList.Clear();
            RecalculateContentSize();

            (int topIndex, int showCountOnScrollRect) = GetTopIndex();
            ShowItems(topIndex, showCountOnScrollRect);

            ScrollTo(0, 0);
        }
    }
}