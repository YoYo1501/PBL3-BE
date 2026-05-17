namespace BackendAPI.Helpers;

public sealed record RegistrationRoomHold(string HoldToken, int RoomId, DateTime ExpiresAtUtc);

public static class RegistrationRoomHoldStore
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);
    private static readonly object SyncRoot = new();
    private static readonly Dictionary<string, RegistrationRoomHold> HoldsByToken = new();

    public static int CountActiveHolds(int roomId, string? excludingHoldToken = null)
    {
        var nowUtc = DateTime.UtcNow;
        lock (SyncRoot)
        {
            RemoveExpired(nowUtc);
            return HoldsByToken.Values.Count(hold =>
                hold.RoomId == roomId &&
                !string.Equals(hold.HoldToken, excludingHoldToken, StringComparison.Ordinal));
        }
    }

    public static bool HasActiveHold(string holdToken, int roomId)
    {
        if (string.IsNullOrWhiteSpace(holdToken))
            return false;

        var nowUtc = DateTime.UtcNow;
        lock (SyncRoot)
        {
            RemoveExpired(nowUtc);
            return HoldsByToken.TryGetValue(holdToken, out var hold) &&
                hold.RoomId == roomId &&
                hold.ExpiresAtUtc > nowUtc;
        }
    }

    public static void Release(string holdToken)
    {
        if (string.IsNullOrWhiteSpace(holdToken))
            return;

        lock (SyncRoot)
        {
            HoldsByToken.Remove(holdToken);
        }
    }

    public static (bool Success, DateTime? ExpiresAtUtc) TryHold(
        int roomId,
        string holdToken,
        int currentOccupancy,
        int capacity)
    {
        if (string.IsNullOrWhiteSpace(holdToken))
            return (false, null);

        var nowUtc = DateTime.UtcNow;
        lock (SyncRoot)
        {
            RemoveExpired(nowUtc);

            var activeOtherHolds = HoldsByToken.Values.Count(hold =>
                hold.RoomId == roomId &&
                !string.Equals(hold.HoldToken, holdToken, StringComparison.Ordinal));

            if (currentOccupancy + activeOtherHolds >= capacity)
                return (false, null);

            var expiresAtUtc = nowUtc.Add(HoldDuration);
            HoldsByToken[holdToken] = new RegistrationRoomHold(holdToken, roomId, expiresAtUtc);
            return (true, expiresAtUtc);
        }
    }

    private static void RemoveExpired(DateTime nowUtc)
    {
        var expiredTokens = HoldsByToken
            .Where(pair => pair.Value.ExpiresAtUtc <= nowUtc)
            .Select(pair => pair.Key)
            .ToList();

        foreach (var token in expiredTokens)
        {
            HoldsByToken.Remove(token);
        }
    }
}
