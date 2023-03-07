using System;
using System.Threading.Tasks;
using DolbyIO.Comms;
using DolbyIO.Comms.Unity;
using UnityEngine;

public class AudioChatManager : MonoBehaviour
{
    public string ConferenceId => _conferenceId;

    public Conference OwnedConference { get; private set; }
    public Conference JoinedConference { get; private set; }

    public bool IsSessionOpen => _dolbySdk.Session.IsOpen;

    // This is required before we can create/join conferences. It works like a "log user in"
    public async Task<UserInfo> OpenSessionAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException($"Empty {nameof(username)}");
        }

        _username = username;

        _localUserInfo = await _dolbySdk.Session.OpenAsync(new UserInfo
        {
            Name = username,
            //ExternalId = null,
            //AvatarURL = null
        });

        Debug.Log($"Session created for a user with id: {_localUserInfo.Id}, name: {_localUserInfo.Name}");
        return _localUserInfo;
    }

    public async Task CloseSessionAsync()
    {
        if (JoinedConference != null || OwnedConference != null)
        {
            await LeaveConferenceAsync();
        }

        await _dolbySdk.Session.CloseAsync();
    }

    public async Task<Conference> StartConferenceAsync()
    {
        Debug.Log("Start new session");

        OwnedConference = await _dolbySdk.Conference.CreateAsync(new ConferenceOptions
        {
            Params = new ConferenceParams
            {
                DolbyVoice = false,
                Stats = false,
                SpatialAudioStyle = SpatialAudioStyle.None
            },
            Alias = _username
        });

        // According to docs creating a conference doesn't automatically join it
        await JoinConferenceAsync(OwnedConference);

        return OwnedConference;
    }

    public Task<Conference> JoinConferenceAsync(string conferenceId)
    {
        var conference = new Conference
        {
            Id = conferenceId,
            Alias = _username,
            Permissions = null
        };

        return JoinConferenceAsync(conference);
    }

    public async Task<Conference> JoinConferenceAsync(Conference conference)
    {
        var joinOptions = new JoinOptions
        {
            Connection = new ConnectionOptions
            {
                //ConferenceAccessToken = _authToken,
                SpatialAudio = false
            },
            Constraints = new MediaConstraints
            {
                Audio = true,
                Video = true
            }
        };

        JoinedConference = await _dolbySdk.Conference.JoinAsync(conference, joinOptions);
        return JoinedConference;
    }

    public async Task LeaveConferenceAsync()
    {
        await _dolbySdk.Conference.LeaveAsync();
        JoinedConference = null;
        OwnedConference = null;
    }

    protected void Start()
    {
        InitAsync().LogIfFailed();
    }

    // In a production we'd have to setup an endpoint that will return token for a logged in user
    [SerializeField]
    private string _authToken;

    // Currently there's no way to list available conference with the Dolby SDK so in a production we'd have to setup an endpoint for this
    [SerializeField]
    private string _conferenceId;

    private readonly DolbyIOSDK _dolbySdk = DolbyIOManager.Sdk;

    private UserInfo _localUserInfo;
    private string _username;

    private async Task InitAsync()
    {
        await _dolbySdk.InitAsync(_authToken, () =>
        {
            Debug.LogWarning("Token requires a refresh!");
            return _authToken;
        });

        _dolbySdk.Conference.InvitationReceived = InvitationReceived;
        _dolbySdk.Conference.StatusUpdated = StatusUpdated;
        _dolbySdk.Conference.ParticipantAdded = ParticipantAdded;
        _dolbySdk.Conference.ParticipantUpdated = ParticipantUpdated;

        Debug.Log("Dolby SDK initialized");
    }

    private void ParticipantUpdated(Participant participant)
    {
        Debug.Log($"ParticipantUpdated {participant.Print()}");
    }

    private void ParticipantAdded(Participant participant)
    {
        Debug.Log($"ParticipantAdded Id: {participant.Print()}");
    }

    private void StatusUpdated(ConferenceStatus status, string conferenceId)
    {
        Debug.Log($"StatusUpdated Id: {conferenceId}, status: {status}");
    }

    private void InvitationReceived(string conferenceId, string conferenceAlias, ParticipantInfo info)
    {
        Debug.Log($"InvitationReceived Id: {conferenceId}");
    }
}