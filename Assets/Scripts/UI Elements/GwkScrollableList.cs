using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

namespace GWK.UI {
    public class ScrollableList : UIElement {
        private ScrollableListEntry _selectedEntry;
        private ScrollableListEntry selectedEntry {
            get => _selectedEntry;
            set {
                _selectedEntry?.SetSelected(false, focused);
                _selectedEntry = value;
                _selectedEntry?.SetSelected(true, focused);
            }
        }
        // double encapsulation coz why tf not :P
        public ScrollableListEntry SelectedEntry => selectedEntry;
        public int SelectedIndex => entries.IndexOf(selectedEntry);

        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject entryPrefab;
        [Min(0)]
        [SerializeField] private float prefabHeight;
        [Space]
        [SerializeField] private string[] trackNames;
        [SerializeField] private Sprite[] trackThumbnails;
        [Space]
        [SerializeField] private TMP_Text emptyText;
        [SerializeField] private Color emptyColorSelected;
        [SerializeField] private Color emptyColorDeselected;
        private Color targetEmptyColor;
        [Space]
        [SerializeField] private UnityEvent<int> OnElementChosen;

        private List<ScrollableListEntry> entries = new();
        public List<ScrollableListEntry> Entries => entries;
        // first items on top
        private int topIdx = 0;
        private int bottomIdx = 5;
        public (string name, Sprite thumbnail) GetAssetsAtIndex(int sceneIdx) => (trackNames[sceneIdx - 1], trackThumbnails[sceneIdx - 1]);
        public override void OnUpDown(InputAction.CallbackContext ctx) {
            float val = ctx.ReadValue<float>();
            if (val > 0) {
                SelectPrevious();
                return;
            }
            if (val < 0) {
                SelectNext();
                return;
            }
        }

        public override void SetFocused(UINavigationInfo info) {
            base.SetFocused(info);
            selectedEntry?.SetSelected(true, true);
            targetEmptyColor = emptyColorSelected;
        }

        public override void SetUnfocused() {
            base.SetUnfocused();
            selectedEntry?.SetSelected(true, false);
            targetEmptyColor = emptyColorDeselected;
        }


        public void AddTrack(Track track) {
            GameObject entry = Instantiate(entryPrefab, content);
            (entry.transform as RectTransform).anchoredPosition = new(0, -prefabHeight * entries.Count);
            entries.Add(entry.GetComponent<ScrollableListEntry>());
            int sceneIdx = track.sceneIdx - 1; 
            entries[^1].SetInfo(trackNames[sceneIdx], trackThumbnails[sceneIdx], track.settings);
            selectedEntry = entries[^1];

            content.sizeDelta = new(0, prefabHeight * entries.Count);
            AdjustViewport(entries.Count - 1);
        }

        public void UpdateAllTracks(Playlist playlist) {
            Assert.IsTrue(entries.Count == playlist.Length);

            int sceneIdx;
            for (int i = 0; i < playlist.Length; i++) {
                sceneIdx = playlist[i].sceneIdx - 1;
                entries[i].SetInfo(trackNames[sceneIdx], trackThumbnails[sceneIdx], playlist[i].settings);
            }
        }

        public void Clear() {
            selectedEntry = null;
            foreach (var e in entries) {
                Destroy(e.gameObject);
            }
            entries = new();
            content.sizeDelta = new(0, 0);
        }

        public void LoadFromPlaylist(Playlist playlist) {
            if (entries.Count > playlist.Length) {
                int diff = entries.Count - playlist.Length;
                for (int i = 0; i < diff; i++) {
                    selectedEntry = entries[^1];
                    Remove();
                }
            }
            else if (entries.Count < playlist.Length) {
                int diff = playlist.Length - entries.Count;
                for (int i = 0; i < diff; i++) {
                    AddTrack(playlist[0]);
                }
            }
            UpdateAllTracks(playlist);
        }

        public void Remove() {
            if (entries.Count == 0) {
                return;
            }
            int idx = entries.IndexOf(selectedEntry);
            entries.Remove(selectedEntry);
            Destroy(selectedEntry.gameObject);
            for (int i = idx; i < entries.Count; i++) {
                (entries[i].transform as RectTransform).anchoredPosition += new Vector2(0, prefabHeight);
            }
            if (entries.Count == 0) {
                selectedEntry = null;
                return;
            }
            if (idx >= entries.Count) {
                selectedEntry = entries[^1];
                return;
            }
            selectedEntry = entries[idx];
        }

        public void SelectPrevious() {
            int idx = entries.IndexOf(selectedEntry);
            if (idx <= 0) {
                return;
            }
            selectedEntry = entries[idx - 1];
            AdjustViewport(idx - 1);
        }

        public void SelectNext() {
            int idx = entries.IndexOf(selectedEntry);
            if (idx == entries.Count - 1) {
                return;
            }
            selectedEntry = entries[idx + 1];
            AdjustViewport(idx + 1);
        }

        private Vector2 targetPosition;
        private void AdjustViewport(int idx) {
            if (idx <= bottomIdx && idx >= topIdx) {
                return;
            }
            int diff = 0;
            if (idx > bottomIdx) {
                diff = idx - bottomIdx;
                bottomIdx = idx;
                topIdx = idx - 5;
            }
            if (idx < topIdx) {
                diff = idx - topIdx;
                topIdx = idx;
                bottomIdx = idx + 5;
            }
            targetPosition += new Vector2(0, prefabHeight) * diff;
        }

        void Update() {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, 10 * Time.unscaledDeltaTime);
            emptyText.enabled = entries.Count == 0;
            emptyText.color = Color.Lerp(emptyText.color, targetEmptyColor, 10 * Time.unscaledDeltaTime);
        }

        public void MoveUp() {
            if (SelectedIndex == 0 || entries.Count == 0) {
                return;
            }
            entries[SelectedIndex].rectTransform.anchoredPosition += new Vector2(0, prefabHeight);
            entries[SelectedIndex - 1].rectTransform.anchoredPosition -= new Vector2(0, prefabHeight);
            (entries[SelectedIndex], entries[SelectedIndex - 1]) = (entries[SelectedIndex - 1], entries[SelectedIndex]);
        }

        public void MoveDown() {
            if (SelectedIndex == entries.Count - 1 || entries.Count == 0) {
                return;
            }
            entries[SelectedIndex].rectTransform.anchoredPosition -= new Vector2(0, prefabHeight);
            entries[SelectedIndex + 1].rectTransform.anchoredPosition += new Vector2(0, prefabHeight);
            (entries[SelectedIndex], entries[SelectedIndex + 1]) = (entries[SelectedIndex + 1], entries[SelectedIndex]);
        }

        public void Choose() {
            if (entries.Count == 0) {
                SoundManager.OnBackUI();
                return;
            }
            SoundManager.OnConfirmUI();
            OnElementChosen.Invoke(SelectedIndex);
        }
    }
}