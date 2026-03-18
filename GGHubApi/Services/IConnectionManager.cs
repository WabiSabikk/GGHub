using System.Collections.Concurrent;
using System.Linq;

namespace GGHubApi.Services
{
    public interface IConnectionManager
    {
        // Telegram bot connection handling
        void RegisterBotConnection(string connectionId, IEnumerable<long>? userIds = null);
        void AddUserToBotConnection(string connectionId, long userId);
        void RemoveUserFromBotConnection(string connectionId, long userId);

        // Web user connections
        void AddUserConnection(string connectionId, long userId);
        void RemoveConnection(string connectionId);

        // Queries
        List<string> GetConnectionsForUsers(IEnumerable<long> userIds);
        IEnumerable<string> GetBotConnectionIds();
        bool IsUserConnected(long userId);
        bool TryGetUserId(string connectionId, out long userId);
        bool TryGetConnectionId(long userId, out string connectionId);
    }

    public class ConnectionManager : IConnectionManager
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<long>> _connections = new();
        private static readonly ConcurrentDictionary<long, ConcurrentBag<string>> _userConnections = new();
        private static readonly ConcurrentDictionary<string, byte> _botConnections = new();

        public void RegisterBotConnection(string connectionId, IEnumerable<long>? userIds = null)
        {
            var bag = _connections.GetOrAdd(connectionId, _ => new ConcurrentBag<long>());
            if (userIds != null)
            {
                foreach (var id in userIds)
                {
                    bag.Add(id);
                    var cBag = _userConnections.GetOrAdd(id, _ => new ConcurrentBag<string>());
                    cBag.Add(connectionId);
                }
            }
            _botConnections.TryAdd(connectionId, 0);
        }

        public void AddUserToBotConnection(string connectionId, long userId)
        {
            var bag = _connections.GetOrAdd(connectionId, _ => new ConcurrentBag<long>());
            bag.Add(userId);
            var cBag = _userConnections.GetOrAdd(userId, _ => new ConcurrentBag<string>());
            cBag.Add(connectionId);
        }

        public void AddUserConnection(string connectionId, long userId)
        {
            AddUserToBotConnection(connectionId, userId);
        }

        public void RemoveConnection(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var userBag))
            {
                foreach (var userId in userBag)
                {
                    if (_userConnections.TryGetValue(userId, out var bag))
                    {
                        var remaining = new ConcurrentBag<string>(bag.Where(id => id != connectionId));
                        if (remaining.IsEmpty)
                            _userConnections.TryRemove(userId, out _);
                        else
                            _userConnections[userId] = remaining;
                    }
                }
            }
            _botConnections.TryRemove(connectionId, out _);
        }

        public void RemoveUserFromBotConnection(string connectionId, long userId)
        {
            if (_connections.TryGetValue(connectionId, out var bag))
            {
                var remaining = new ConcurrentBag<long>(bag.Where(id => id != userId));
                _connections[connectionId] = remaining;
            }

            if (_userConnections.TryGetValue(userId, out var connBag))
            {
                var remainingConn = new ConcurrentBag<string>(connBag.Where(c => c != connectionId));
                if (remainingConn.IsEmpty)
                    _userConnections.TryRemove(userId, out _);
                else
                    _userConnections[userId] = remainingConn;
            }
        }

        public bool TryGetUserId(string connectionId, out long userId)
        {
            userId = 0;
            if (_connections.TryGetValue(connectionId, out var bag))
            {
                var id = bag.FirstOrDefault();
                if (id != 0)
                {
                    userId = id;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetConnectionId(long userId, out string connectionId)
        {
            connectionId = string.Empty;
            if (_userConnections.TryGetValue(userId, out var bag))
            {
                connectionId = bag.FirstOrDefault();
                return !string.IsNullOrEmpty(connectionId);
            }
            return false;
        }

        public List<string> GetConnectionIds(IEnumerable<long> userIds)
        {
            var ids = new List<string>();
            foreach (var userId in userIds)
            {
                if (_userConnections.TryGetValue(userId, out var bag))
                {
                    ids.AddRange(bag);
                }
            }
            return ids;
        }

        public List<string> GetConnectionsForUsers(IEnumerable<long> userIds) => GetConnectionIds(userIds);

        public IEnumerable<string> GetBotConnectionIds()
        {
            return _botConnections.Keys;
        }

        public bool IsUserConnected(long userId)
        {
            return _userConnections.ContainsKey(userId);
        }
    }
}
