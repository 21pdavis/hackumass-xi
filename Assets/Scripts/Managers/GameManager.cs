// Based on code written by Battery Acid Dev

using UnityEngine;
using TMPro;

using Newtonsoft.Json;
using System.Threading.Tasks;
using Aws.GameLift.Realtime.Types;
using System.Net.Sockets;
using System.Net;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);
    private const string GameSessionPlacementEndpoint = "https://kvnk1lfwg0.execute-api.us-east-1.amazonaws.com/rtgl-apigw-stage";

    [SerializeField]
    private TextMeshProUGUI searchingText;
    [SerializeField]
    private GameObject player1;
    [SerializeField]
    private GameObject player2;

    private APIManager apiManager;
    private SQSMessageProcessing sqsMessageProcessing;
    private RealTimeClient realTimeClient;
    private MatchResults matchResults;
    private string playerId;
    private string remotePlayerId;
    private bool gameOver;

    public enum GameState
    {
        Menu,
        Playing
    }

    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CurrentState = GameState.Menu;

        apiManager = FindObjectOfType<APIManager>();
        sqsMessageProcessing = FindObjectOfType<SQSMessageProcessing>();
        matchResults = new MatchResults();

        playerId = System.Guid.NewGuid().ToString();
        remotePlayerId = "";
        gameOver = false;
    }

    public void TransitionState(GameState to)
    {
        Debug.Log($"Transitioning to state '{to}'");

        switch (to)
        {
            //case GameState.Searching:
            //    searchingText.gameObject.SetActive(true);
            //    FindMatch();
            //    break;
            case GameState.Playing:
                SceneManager.LoadScene("Ben");
                //Instantiate(player1, Vector2.zero, Quaternion.identity);
                //Instantiate(player2, new Vector2(1f, 0f), Quaternion.identity);
                break;

        }
    }

    private async void FindMatch()
    {
        Debug.Log("Attempting to find a match");

        // todo: test without tostring
        FindMatch matchMessage = new FindMatch(RealTimeClient.LambdaOpCodes.RequestFindMatch.ToString(), playerId);
        string jsonPostData = JsonUtility.ToJson(matchMessage);

        //! This is where the lambda code gets executed! This goes to API Gateway, which triggers Lambda
        string response = await apiManager.Post(GameSessionPlacementEndpoint, jsonPostData);
        GameSessionPlacementInfo gameSessionPlacementInfo = JsonConvert.DeserializeObject<GameSessionPlacementInfo>(response);

        Debug.Log($"Session placement info:\n{JsonConvert.SerializeObject(gameSessionPlacementInfo, Formatting.Indented)}");

        if (gameSessionPlacementInfo != null)
        {
            if (gameSessionPlacementInfo.PlacementId != null)
            {
                // The response was from a placement request
                Debug.Log("Game session placement request submitted.");

                // Debug.Log(gameSessionPlacementInfo.PlacementId);

                // subscribe to receive the player placement fulfillment notification
                await SubscribeToFulfillmentNotifications(gameSessionPlacementInfo.PlacementId);
            }
            else if (gameSessionPlacementInfo.GameSessionId != null)
            {
                Debug.Log("Existing open game session found!");

                int.TryParse(gameSessionPlacementInfo.Port, out int portAsInt);

                // Once connected, the Realtime service moves the Player session from Reserved to Active, which means we're ready to connect.
                // https://docs.aws.amazon.com/gamelift/latest/apireference/API_CreatePlayerSession.html
                EstablishConnectionToRealtimeServer(gameSessionPlacementInfo.IpAddress, portAsInt, gameSessionPlacementInfo.PlayerSessionId);
            }
            else
            {
                Debug.Log("Game session response not valid...");
            }
        }
        else
        {
            Debug.LogWarning("Error: could not retrieve Game Session Placement Info");
        }
    }

    // TODO: helper method, refactor out of this file
    public static int GetAvailableUdpPort()
    {
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Bind(DefaultLoopbackEndpoint);
            return ((IPEndPoint)socket.LocalEndPoint).Port;
        }
    }

    private void EstablishConnectionToRealtimeServer(string ipAddress, int port, string playerSessionId)
    {
        int localUdpPort = GetAvailableUdpPort();

        RealtimePayload realtimePayload = new RealtimePayload(playerId);
        string payload = JsonUtility.ToJson(realtimePayload);

        realTimeClient = new RealTimeClient(ipAddress, port, localUdpPort, playerSessionId, payload, ConnectionType.RT_OVER_WS_UDP_UNSECURED);
        realTimeClient.RemotePlayerIdEventHandler += OnRemotePlayerIdEvent;
        realTimeClient.GameOverEventHandler += OnGameOverEvent;
    }

    private async Task<bool> SubscribeToFulfillmentNotifications(string placementId)
    {
        PlayerPlacementFulfillmentInfo playerPlacementFulfillmentInfo = await sqsMessageProcessing.SubscribeToFulfillmentNotifications(placementId);

        if (playerPlacementFulfillmentInfo != null)
        {
            Debug.Log("Player placement was fulfilled...");
            // Debug.Log("Placed Player Sessions count: " + playerPlacementFulfillmentInfo.placedPlayerSessions.Count);

            // Once connected, the Realtime service moves the Player session from Reserved to Active, which means we're ready to connect.
            // https://docs.aws.amazon.com/gamelift/latest/apireference/API_CreatePlayerSession.html
            EstablishConnectionToRealtimeServer(playerPlacementFulfillmentInfo.ipAddress, playerPlacementFulfillmentInfo.port,
                playerPlacementFulfillmentInfo.placedPlayerSessions[0].playerSessionId);

            return true;
        }
        else
        {
            Debug.Log("Player placement was null, something went wrong...");
            return false;
        }
    }

    private void OnRemotePlayerIdEvent(object sender, RemotePlayerIdEventArgs remotePlayerIdEventArgs)
    {
        Debug.Log($"Remote player id received: {remotePlayerIdEventArgs.remotePlayerId}.");
        UpdateRemotePlayerId(remotePlayerIdEventArgs);
    }

    private void UpdateRemotePlayerId(RemotePlayerIdEventArgs remotePlayerIdEventArgs)
    {
        remotePlayerId = remotePlayerIdEventArgs.remotePlayerId;
        //updateRemotePlayerId = true;
    }

    private void OnGameOverEvent(object sender, GameOverEventArgs gameOverEventArgs)
    {
        // TODO: trigger UI Event

        Debug.Log($"Game over event received with winner: {gameOverEventArgs.matchResults.winnerId}.");
        matchResults = gameOverEventArgs.matchResults;
        gameOver = true;
    }
}

[System.Serializable]
public class FindMatch
{
    public string opCode;
    public string playerId;
    public FindMatch() { }
    public FindMatch(string opCodeIn, string playerIdIn)
    {
        this.opCode = opCodeIn;
        this.playerId = playerIdIn;
    }
}

[System.Serializable]
public class RealtimePayload
{
    public string playerId;
    // Other fields you wish to pass as payload to the realtime server
    public RealtimePayload() { }
    public RealtimePayload(string playerIdIn)
    {
        this.playerId = playerIdIn;
    }
}

[System.Serializable]
public class StartMatch
{
    public string remotePlayerId;
    public StartMatch() { }
    public StartMatch(string remotePlayerIdIn)
    {
        this.remotePlayerId = remotePlayerIdIn;
    }
}

[System.Serializable]
public class MatchResults
{
    public string playerOneId;
    public string playerTwoId;

    public string playerOneScore;
    public string playerTwoScore;

    public string winnerId;

    public MatchResults() { }
    public MatchResults(string playerOneIdIn, string playerTwoIdIn, string playerOneScoreIn, string playerTwoScoreIn, string winnerIdIn)
    {
        this.playerOneId = playerOneIdIn;
        this.playerTwoId = playerTwoIdIn;
        this.playerOneScore = playerOneScoreIn;
        this.playerTwoScore = playerTwoScoreIn;
        this.winnerId = winnerIdIn;
    }
}
