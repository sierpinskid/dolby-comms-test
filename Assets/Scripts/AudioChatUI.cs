using System.Collections.Generic;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioChatUI : MonoBehaviour
{
    protected void Start()
    {
        _startSession.onClick.AddListener(StartSession);
        _closeSession.onClick.AddListener(CloseSession);
        _startConference.onClick.AddListener(StartConference);
        _joinConference.onClick.AddListener(JoinConference);
        _leaveConference.onClick.AddListener(LeaveConference);

        var mainThreadId = Thread.CurrentThread.ManagedThreadId;
        Debug.Log("Main thread ID: " + mainThreadId);
    }

    protected void OnApplicationQuit()
    {
        CloseSession();
    }

    private void StartSession()
    {
        AppendLog($"Clicked StartSession for user: {_joinConferenceUserId.text}");
        _audioChatManager.OpenSessionAsync(_joinConferenceUserId.text).ContinueOrLogError(userInfo =>
        {
            AppendLog($"Session started for user: {userInfo.Id}");
            UpdateStatus();
        });
    }

    private void CloseSession()
    {
        AppendLog($"Clicked CloseSession");
        _audioChatManager.CloseSessionAsync().ContinueOrLogError(() =>
        {
            AppendLog($"Closed session");
            UpdateStatus();
        });
    }

    private void LeaveConference()
    {
        AppendLog($"Clicked LeaveConference");
        _audioChatManager.LeaveConferenceAsync().ContinueOrLogError(() =>
        {
            AppendLog($"Left conference");
            UpdateStatus();
        });
    }

    private void JoinConference()
    {
        AppendLog($"Clicked JoinConference");
        if (string.IsNullOrEmpty(_joinConferenceId.text))
        {
            AppendLog($"Empty join conference ID");
            return;
        }

        _audioChatManager.JoinConferenceAsync(_joinConferenceId.text).ContinueOrLogError(conference =>
        {
            AppendLog($"Joined conference: {conference.Id}");
            UpdateStatus();
        });
    }

    private void StartConference()
    {
        AppendLog($"Clicked StartConference");
        _audioChatManager.StartConferenceAsync().ContinueOrLogError(conference =>
        {
            AppendLog($"Started conference: {conference.Id}");
            UpdateStatus();
        });
    }

    private readonly List<string> _log = new List<string>();
    private readonly StringBuilder _sb = new StringBuilder();

    [SerializeField]
    private AudioChatManager _audioChatManager;

    [SerializeField]
    private Button _startSession;

    [SerializeField]
    private Button _closeSession;

    [SerializeField]
    private Button _startConference;

    [SerializeField]
    private Button _joinConference;

    [SerializeField]
    private Button _leaveConference;

    [SerializeField]
    private TMP_Text _status;

    [SerializeField]
    private TMP_Text _console;

    [SerializeField]
    private TMP_InputField _joinConferenceId;
    
    [SerializeField]
    private TMP_InputField _joinConferenceUserId;

    [SerializeField]
    private int _maxLogEntries = 10;

    private void AppendLog(string message)
    {
        for (int i = _log.Count - 1; i >= _maxLogEntries; i--)
        {
            _log.RemoveAt(i);
        }

        _log.Add(message);

        _sb.Length = 0;
        _sb.AppendLine("Log:");
        foreach (var log in _log)
        {
            _sb.AppendLine(log);
        }

        _console.text = _sb.ToString();
    }

    private void UpdateStatus()
    {
        _sb.Length = 0;
        _sb.AppendLine("Session active: " + _audioChatManager.IsSessionOpen);
        _sb.AppendLine("Owned Conference: " + _audioChatManager.OwnedConference?.Id ?? "none");
        _sb.AppendLine("Joined Conference: " + _audioChatManager.JoinedConference?.Id ?? "none");

        _status.text = _sb.ToString();
    }
}