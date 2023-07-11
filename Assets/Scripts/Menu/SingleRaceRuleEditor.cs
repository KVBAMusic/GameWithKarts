using UnityEngine;

public class SingleRaceRuleEditor : MonoBehaviour
{
    [SerializeField] TrackRulesEditor editor;
    private Playlist playlist;

    void OnEnable() {
        playlist = GameRulesManager.instance.GetPlaylist();
        editor.SetDisplayFrom(playlist[0].settings);
    }

    public void UpdateSettings() {
        editor.UpdateRaceSettings(playlist[0].settings);
    }
}
