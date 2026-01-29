using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using TMPro;

namespace Network
{
    public class NetworkGameManager : NetworkBehaviour
    {
        #region Public Variables
        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private TextMeshProUGUI _playerCountText;
        [SerializeField] private TextMeshProUGUI _timerCountText;
        #endregion

        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

        private NetworkSessionManager _networkSessionManager;
        
        private const int maxPlayers = 2;
        private const int timerBeforeStart = 3;

        // private void Awake()
        // {
        //     _networkSessionManager = GetComponent<NetworkSessionManager>();
        // }

        public override void Spawned()
        {
            base.Spawned();
            _networkSessionManager.OnPlayerJoinedEvent += OnPlayerJoined;
            _networkSessionManager.OnPlayerLeftEvent += OnPlayerLeft;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            _networkSessionManager.OnPlayerJoinedEvent -= OnPlayerJoined;
            _networkSessionManager.OnPlayerLeftEvent -= OnPlayerLeft;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            
            _playerCountText.text = $"Players: {Object.Runner.ActivePlayers.Count()}/{maxPlayers}";
        }

        private void OnPlayerJoined(PlayerRef player)
        {
            if (!HasStateAuthority) return;
            if (NetworkSessionManager.Instance.JoinedPlayers.Count >= maxPlayers)
            {
                // start game count down and then spawn
                OnGameStarted();
            }
            Debug.Log($"Player {player.PlayerId} Joined");
        }

        private void OnPlayerLeft(PlayerRef player)
        {
            if (!HasStateAuthority) return;
            if (!_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject)) return;
            Object.Runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }

        private void OnGameStarted()
        {
            Debug.Log($"Game Started");
            foreach (var playerSpawn in NetworkSessionManager.Instance.JoinedPlayers)
            {
                var networkObject = Object.Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, playerSpawn);
                _spawnedCharacters.Add(playerSpawn, networkObject);
            }
        }
    }
}
