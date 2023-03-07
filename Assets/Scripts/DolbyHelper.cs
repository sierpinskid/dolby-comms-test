using DolbyIO.Comms;

public static class DolbyHelper
{
    public static string Print(this Participant participant)
        => $"Participant {{ Id: {participant.Id}, Type: {participant.Type}, Status: {participant.Status} }}";
}