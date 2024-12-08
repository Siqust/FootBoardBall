using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
public struct Row
{
    public List<ActionCardObj> row_modifiers;
    public Dictionary<PlayerCardObj, List<ActionCardObj>> cards;
}

[RequireComponent(typeof(NetworkMatch))]
public class MatchController : NetworkBehaviour
{
    internal readonly SyncDictionary<NetworkIdentity, MatchPlayerData> matchPlayerData = new SyncDictionary<NetworkIdentity, MatchPlayerData>();
    internal readonly Dictionary<CellValue, CellGUI> MatchCells = new Dictionary<CellValue, CellGUI>();

    public GameObject Field;
    public GameObject EndScreen;
    public TextMeshProUGUI EndScore_Me;
    public TextMeshProUGUI EndScore_Opponent;
    public TextMeshProUGUI EndState;
    public Animator animator;
    [Header("move things SERVER")]
    [SerializeField] private float timer;
    [SerializeField] private Slider timer_slider;

    [Header("Field Players")]
    public RowPlayers me_attack_players;
    public RowPlayers me_defence_players;
    public RowPlayers opponent_attack_players;
    public RowPlayers opponent_defence_players;

    [Header("Rows Modifiers Client")]
    public RowModifiers opponent_defence_modifs;
    public RowModifiers opponent_attack_modifs;
    public RowModifiers me_attack_modifs;
    public RowModifiers me_defence_modifs;

    [Header("Row Scores Cient")]
    public TextMeshProUGUI me_attack_score;
    public TextMeshProUGUI me_defence_score;
    public TextMeshProUGUI opponent_attack_score;
    public TextMeshProUGUI opponent_defence_score;


    [Header("Players' Cards")]
    [SerializeField] private List<PlayerCardObj> player1_players; //
    [SerializeField] private List<ActionCardObj> player1_actions; //
    [SerializeField] private List<PlayerCardObj> player2_players; //
    [SerializeField] private List<ActionCardObj> player2_actions; //

    [Header("GameRows SERVER")]
    [SerializeField] private Row[] player1_field_cards = new Row[2]; //
    [SerializeField] private Row[] player2_field_cards = new Row[2]; //

    [Header("Row Modifiers SERVER")]
    [SerializeField] List<ActionCardObj>[] player1_row_modifiers;
    [SerializeField] List<ActionCardObj>[] player2_row_modifiers;

    [Header("Cards")]
    public PlayerCardObj[] players_cards;
    public ActionCardObj[] actions_cards;

    [Header("Cards Holders")]
    [SerializeField] private Transform visual_cards_holder;
    [SerializeField] private Transform player_players_cards_holder;
    [SerializeField] private Transform player_actions_cards_holder;

    [Header("CardSpawns")]
    [SerializeField] private Transform opponent_card_spawn;
    [SerializeField] private Transform stack_card_spawn;
    [SerializeField] private Transform me_card_spawn;

    CellValue boardScore = CellValue.None;
    [SerializeField] bool playAgain = false;
    
    [Header("GUI References")]
    public CanvasGroup canvasGroup;
    public Text gameText;
    public Button exitButton;
    public Button playAgainButton;
    public Text winCountLocal;
    public Text winCountOpponent;

    [Header("Diagnostics - Do Not Modify")]
    public CanvasController canvasController;
    public NetworkIdentity player1;
    public NetworkIdentity player2;
    public NetworkIdentity startingPlayer;

    private bool game_started;
    private bool pre_end;
    [HideInInspector] public bool ended;

    [SyncVar(hook = nameof(UpdateGameUI))]
    public NetworkIdentity currentPlayer;
    void Awake()
    {
        pre_end = false;
        ended = false;
        timer = 60f;
        player1_field_cards[1].cards = new Dictionary<PlayerCardObj, List<ActionCardObj>>(4);
        player2_field_cards[1].cards = new Dictionary<PlayerCardObj, List<ActionCardObj>>(4);
        player1_field_cards[0].cards = new Dictionary<PlayerCardObj, List<ActionCardObj>>(4);
        player2_field_cards[0].cards = new Dictionary<PlayerCardObj, List<ActionCardObj>>(4);

        player1_row_modifiers = new List<ActionCardObj>[2];
        player2_row_modifiers = new List<ActionCardObj>[2];
        player1_row_modifiers[0] = new List<ActionCardObj>();
        player1_row_modifiers[1] = new List<ActionCardObj>();
        player2_row_modifiers[0] = new List<ActionCardObj>();
        player2_row_modifiers[1] = new List<ActionCardObj>();

        canvasController = FindObjectOfType<CanvasController>();
    }
    private void Update()
    {
        if (!ended && game_started)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                MoveMade();
            }
            UpdateGameTimer(timer);
        }
    }

    [ClientRpc]
    void UpdateGameTimer(float time)
    {
        timer_slider.value = (60 - time) / 60;
    }
    #region ServerPart
    public override void OnStartServer()
    {
        StartCoroutine(AddPlayersToMatchController());
    }

    // For the SyncDictionary to properly fire the update callback, we must
    // wait a frame before adding the players to the already spawned MatchController
    IEnumerator AddPlayersToMatchController()
    {
        yield return null;

        matchPlayerData.Add(player1, new MatchPlayerData { playerIndex = CanvasController.playerInfos[player1.connectionToClient].playerIndex });
        matchPlayerData.Add(player2, new MatchPlayerData { playerIndex = CanvasController.playerInfos[player2.connectionToClient].playerIndex });

        if (player1.isLocalPlayer)
        {
            player1.gameObject.name = "ME";
            player2.gameObject.name = "OPPONENT";
        }
        else
        {
            player2.gameObject.name = "ME";
            player1.gameObject.name = "OPPONENT";
        }

        for (int i = 0; i < players_cards.Length; i++)
        {
            SpawnPlayerCard(player1, i, -1);
            SpawnPlayerCard(player2, i, -1);
            player1_players.Add(players_cards[i]);
            player2_players.Add(players_cards[i]);
        }
        for (int i = 0; i < actions_cards.Length; i++)
        {
            SpawnActionCard(player1, i, -1, -1, false, ActionCardSpawnType.Init);
            SpawnActionCard(player2, i, -1, -1, false, ActionCardSpawnType.Init);
            player1_actions.Add(actions_cards[i]);
            player2_actions.Add(actions_cards[i]);
        }
        game_started = true;
    }

    public override void OnStartClient()
    {
        matchPlayerData.Callback += UpdateWins;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        exitButton.gameObject.SetActive(false);
        playAgainButton.gameObject.SetActive(false);
    }
    #endregion

    [ClientCallback]
    public void UpdateGameUI(NetworkIdentity _, NetworkIdentity newPlayerTurn)
    {
        if (!newPlayerTurn) return;

        if (newPlayerTurn.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            gameText.text = "Ваш ход";
            gameText.color = Color.blue;
        }
        else
        {
            gameText.text = "Ход другого";
            gameText.color = Color.red;
        }
    }

    [ClientCallback]
    public void UpdateWins(SyncDictionary<NetworkIdentity, MatchPlayerData>.Operation op, NetworkIdentity key, MatchPlayerData matchPlayerData)
    {
        if (key.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
            winCountLocal.text = $"Player {matchPlayerData.playerIndex}\n{matchPlayerData.wins}";
        else
            winCountOpponent.text = $"Player {matchPlayerData.playerIndex}\n{matchPlayerData.wins}";
    }

    bool CheckWin()
    {
        if (player1_field_cards[0].cards.Count == 4 && player1_field_cards[1].cards.Count == 4) return true;
        else if (player2_field_cards[0].cards.Count == 4 && player2_field_cards[1].cards.Count == 4) return true;
        else return false;
    }
    [Command(requiresAuthority = false)]
    public void MakePlay(MoveType movetype, int cardindex, MoveMessage movemessage, NetworkConnectionToClient sender = null)
    {
        int[] scores;
        int player1_attack;
        int player1_defence;
        int player2_attack;
        int player2_defence;
        if (sender.identity != currentPlayer)
            return;
        switch (movetype)
        {
            case MoveType.Player:
                //print((currentPlayer.isLocalPlayer ? "ME" : "OPPONENT", "PLAYED WITH PLAYER CARD", players_cards[cardindex], "ON ROW", movemessage.row_to == 1 ? "ATTACK" : "DEFENCE", "ON PLACE ", movemessage.col_to));
                PlayerCardObj pCard = players_cards[cardindex];
                PlayedWithPlayer(pCard, cardindex, movemessage);
                break;
            case MoveType.ActionOnPlayer:
                //print((currentPlayer.isLocalPlayer ? "ME" : "OPPONENT", "PLAYED WITH ACTION CARD ON PLAYER", players_cards[cardindex] ,"IN", movemessage.row_to == 1 ? "ATTACK" : "DEFENCE", "ON PLACE ", movemessage.col_to, "TO", movemessage.is_opponent ? "OPPONENT": "SELF"));
                if (!(actions_cards[cardindex].cardtype == ActionCardType.ActPlayerLT || actions_cards[cardindex].cardtype == ActionCardType.ActPlayerOT)) { Debug.LogError("not player action card"); return; }
                if (currentPlayer == player1)
                {
                    player1_actions.Remove(actions_cards[cardindex]);
                }
                else
                {
                    player2_actions.Remove(actions_cards[cardindex]);
                }
                PlayedWithActionOnPlayer(cardindex, movemessage);
                break;
            case MoveType.ActionOnRow:
                if (!(actions_cards[cardindex].cardtype == ActionCardType.ActRow)) { Debug.LogError("not row action card"); return; }
                //print((currentPlayer.isLocalPlayer ? "ME" : "OPPONENT", "PLAYED WITH ACTION CARD ON ROW IN", movemessage.row_to == 1 ? "ATTACK" : "DEFENCE"));
                if (currentPlayer == player1)
                {
                    player1_actions.Remove(actions_cards[cardindex]);
                }
                else
                {
                    player2_actions.Remove(actions_cards[cardindex]);
                }
                PlayedWithActionOnRow(cardindex,movemessage);
                break;
        }

        scores = CountScores();
        player1_attack = scores[0];
        player1_defence = scores[1];
        player2_attack = scores[2];
        player2_defence = scores[3];


        UpdateScores(player1,player1_defence, player1_attack, player2_defence, player2_attack);

        if (CheckWin() && pre_end)
        {
            ended = true;
            scores = CountScores();
            player1_attack = scores[0];
            player1_defence = scores[1];
            player2_attack = scores[2];
            player2_defence = scores[3];
            player1_defence -= player2_attack;
            player2_defence -= player1_attack;
            EndGame(player1, player1_defence, player2_defence);
            return;
        }
        else if (CheckWin())
        {
            pre_end = true;
        }
        else
        {
            pre_end = false;
        }

        MoveMade();
        
    }
    void MoveMade()
    {
        currentPlayer = currentPlayer == player1 ? player2 : player1;
        timer = 60f;
    }
    [ClientRpc]
    void EndGame(NetworkIdentity player1, int player1_defence, int player2_defence)
    {
        StartCoroutine(EndgameCoroutine(player1, player1_defence, player2_defence));
    }
    IEnumerator EndgameCoroutine(NetworkIdentity player1, int player1_defence, int player2_defence)
    {
        yield return new WaitForSeconds(2);
        int me_defence;
        int opponent_defence;
        if (player1.isLocalPlayer)
        {
            me_defence = player1_defence;
            opponent_defence = player2_defence;
        }
        else
        {
            me_defence = player2_defence;
            opponent_defence = player1_defence;
        }
        int me_final_score = 0;
        int opponent_final_score = 0;

        if (opponent_defence < 0) me_final_score = opponent_defence / -3 + 1;
        if (me_defence < 0) opponent_final_score = me_defence / -3 + 1;
        
        yield return null;
        animator.SetTrigger("launch");
        yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips[1].length);
        yield return new WaitForSeconds(1);
        if (me_final_score > 0) { opponent_defence_players.gameObject.SetActive(false); }
        else { me_attack_players.gameObject.SetActive(false); }
        opponent_defence_score.text = opponent_defence.ToString();
        animator.SetTrigger("launch");
        yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips[2].length);
        yield return new WaitForSeconds(2);
        if (opponent_final_score > 0) { me_defence_players.gameObject.SetActive(false);  }
        else { opponent_attack_players.gameObject.SetActive(false); }
        me_defence_score.text = me_defence.ToString();
        yield return new WaitForSeconds(3);
        Field.SetActive(false);
        EndScreen.SetActive(true);

        EndScore_Me.text = me_final_score.ToString();
        EndScore_Opponent.text = opponent_final_score.ToString();
        if (me_final_score > opponent_final_score)
        {
            EndState.text = "ПОБЕДА";
            EndState.color = Color.green;
        }
        else if (me_final_score < opponent_final_score)
        {
            EndState.text = "ПОРАЖЕНИЕ";
            EndState.color = Color.red;
        }
        else
        {
            EndState.text = "НИЧЬЯ";
            EndState.color = Color.white;
        }
    }
    int[] CountScores()
    {
        int[][] player1_scores = new int[2][];
        int[][] player2_scores = new int[2][];
        player1_scores[0] = new int[4] { 0, 0, 0, 0 };
        player1_scores[1] = new int[4] { 0, 0, 0, 0 };
        player2_scores[0] = new int[4] { 0, 0, 0, 0 };
        player2_scores[1] = new int[4] { 0, 0, 0, 0 };

        int[][] player1_stat_change = new int[2][];
        int[][] player2_stat_change = new int[2][];
        player1_stat_change[0] = new int[4] { 0, 0, 0, 0 };
        player1_stat_change[1] = new int[4] { 0, 0, 0, 0 };
        player2_stat_change[0] = new int[4] { 0, 0, 0, 0 };
        player2_stat_change[1] = new int[4] { 0, 0, 0, 0 };

        Dictionary<PlayerCardObj, List<ActionCardObj>> list;

        list = player1_field_cards[0].cards;
        for (int i = 0; i < list.Count; i++)
        {
            var player = list.Keys.ToList()[i];
            player1_scores[0][i] = player.defence;
            for (int j = 0; j < list[player].Count; j++)
            {
                var action = list[player][j];
                player1_scores[0][i] += action.modif;
            }
            player1_scores[0][i] = Mathf.Clamp(player1_scores[0][i], 0, 99);
            player1_stat_change[0][i] = player1_scores[0][i] - player.defence;
        }

        list = player1_field_cards[1].cards;
        for (int i = 0; i < list.Count; i++)
        {
            var player = list.Keys.ToList()[i];
            player1_scores[1][i] = player.attack;
            for (int j = 0; j < list[player].Count; j++)
            {
                var action = list[player][j];
                player1_scores[1][i] += action.modif;
            }
            player1_scores[1][i] = Mathf.Clamp(player1_scores[1][i], 0, 99);
            player1_stat_change[1][i] = player1_scores[1][i] - player.attack;
        }
        list = player2_field_cards[0].cards;
        for (int i = 0; i < list.Count; i++)
        {
            var player = list.Keys.ToList()[i];
            player2_scores[0][i] = player.defence;
            for (int j = 0; j < list[player].Count; j++)
            {
                var action = list[player][j];
                player2_scores[0][i] += action.modif;
            }
            player2_scores[0][i] = Mathf.Clamp(player2_scores[0][i], 0, 99);
            player2_stat_change[0][i] = player2_scores[0][i] - player.defence;
        }
        list = player2_field_cards[1].cards;
        for (int i = 0; i < list.Count; i++)
        {
            var player = list.Keys.ToList()[i];
            player2_scores[1][i] = player.attack;
            for (int j = 0; j < list[player].Count; j++)
            {
                var action = list[player][j];
                player2_scores[1][i] += action.modif;
            }
            player2_scores[1][i] = Mathf.Clamp(player2_scores[1][i], 0, 99);
            player2_stat_change[1][i] = player2_scores[1][i] - player.attack;
        }
        int player1_defence = 0;
        for (int i = 0; i < player1_scores[0].Length; i++) { player1_defence += player1_scores[0][i]; }
        int player1_attack = 0;
        for (int i = 0; i < player1_scores[1].Length; i++) { player1_attack += player1_scores[1][i]; }
        int player2_defence = 0;
        for (int i = 0; i < player2_scores[0].Length; i++) { player2_defence += player2_scores[0][i]; }
        int player2_attack = 0;
        for (int i = 0; i < player2_scores[1].Length; i++) { player2_attack += player2_scores[1][i]; }


        int player1_defence_row_modif = 0;
        for (int i = 0; i < player1_row_modifiers[0].Count; i++) { player1_defence_row_modif += player1_row_modifiers[0][i].modif * player1_field_cards[0].cards.Count; }
        int player1_attack_row_modif = 0;
        for (int i = 0; i < player1_row_modifiers[1].Count; i++) { player1_attack_row_modif += player1_row_modifiers[1][i].modif * player1_field_cards[1].cards.Count; }
        int player2_defence_row_modif = 0;
        for (int i = 0; i < player2_row_modifiers[0].Count; i++) { player2_defence_row_modif += player2_row_modifiers[0][i].modif * player2_field_cards[0].cards.Count; }
        int player2_attack_row_modif = 0;
        for (int i = 0; i < player2_row_modifiers[1].Count; i++) { player2_attack_row_modif += player2_row_modifiers[1][i].modif * player2_field_cards[1].cards.Count; }

        UpdateStatsUI(player1, player1_scores[0], player1_scores[1], player2_scores[0], player2_scores[1],player1_defence_row_modif,player1_attack_row_modif, player2_defence_row_modif,player2_attack_row_modif,
            player1_stat_change[0], player1_stat_change[1], player2_stat_change[0], player2_stat_change[1]);

        return new int[4] { player1_attack+player1_attack_row_modif, player1_defence+player1_defence_row_modif, 
            player2_attack+player2_attack_row_modif, player2_defence+player2_defence_row_modif };
    }

    [ClientRpc]
    //void UpdateStatsUI(NetworkIdentity player1, List<int>[] player1_scores, List<int>[] player2_scores, int player1_defence_row_modif, int player1_attack_row_modif, int player2_defence_row_modif, int player2_attack_row_modif,
    //    List<int>[] player1_stat_change, List<int>[] player2_stat_change)
    void UpdateStatsUI(NetworkIdentity player1, int[] player1_scores_defence, int[] player1_scores_attack, int[] player2_scores_defence, int[] player2_scores_attack, 
        int player1_defence_row_modif, int player1_attack_row_modif, int player2_defence_row_modif, int player2_attack_row_modif,
        int[] player1_stat_change_defence, int[] player1_stat_change_attack, int[] player2_stat_change_defence, int[] player2_stat_change_attack)
    {
        int[][] player1_scores = new int[2][];
        int[][] player2_scores = new int[2][];
        player1_scores[0] = player1_scores_defence;
        player1_scores[1] = player1_scores_attack;
        player2_scores[0] = player2_scores_defence;
        player2_scores[1] = player2_scores_attack;

        int[][] player1_stat_change = new int[2][];
        int[][] player2_stat_change = new int[2][];
        player1_stat_change[0] = player1_stat_change_defence;
        player1_stat_change[1] = player1_stat_change_attack;
        player2_stat_change[0] = player2_stat_change_defence;
        player2_stat_change[1] = player2_stat_change_attack;


        if (player1.isLocalPlayer)
        {
            PlayerCard card;
            int child_count;
            for (int row = 0; row < 2; row++)
            {
                child_count = row==0 ? me_defence_players.transform.childCount : me_attack_players.transform.childCount;
                for (int i = 0; i < child_count; i++)
                {
                    if (row == 0)
                    {
                        card = me_defence_players.transform.GetChild(i).GetComponent<PlayerCard>();
                        card.visualcardscript.player_defence_text.text = player1_scores[0][i].ToString();
                        card.visualcardscript.player_attack_text.text = players_cards[card.index].attack.ToString();
                    }
                    else
                    {
                        card = me_attack_players.transform.GetChild(i).GetComponent<PlayerCard>();
                        card.visualcardscript.player_attack_text.text = player1_scores[1][i].ToString();
                        card.visualcardscript.player_defence_text.text = players_cards[card.index].defence.ToString();
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        card.visualcardscript.up_arrows[j].SetActive(false);
                        card.visualcardscript.down_arrows[j].SetActive(false);
                    }
                    if (player1_stat_change[row][i] > 0)
                    {
                        for (int j = 0; j < player1_stat_change[row][i]; j++)
                        {
                            card.visualcardscript.up_arrows[j].SetActive(true);
                        }
                    }
                    else if (player1_stat_change[row][i] < 0)
                    {
                        for (int j = 0; j < -player1_stat_change[row][i]; j++)
                        {
                            card.visualcardscript.down_arrows[j].SetActive(true);
                        }
                    }
                }
            }
            for (int row = 0; row < 2; row++)
            {
                child_count = row == 0 ? opponent_defence_players.transform.childCount : opponent_attack_players.transform.childCount;
                for (int i = 0; i < child_count; i++)
                {
                    if (row == 0)
                    {
                        card = opponent_defence_players.transform.GetChild(i).GetComponent<PlayerCard>();
                        card.visualcardscript.player_defence_text.text = player2_scores[0][i].ToString();
                        card.visualcardscript.player_attack_text.text = players_cards[card.index].attack.ToString();
                    }
                    else
                    {
                        card = opponent_attack_players.transform.GetChild(i).GetComponent<PlayerCard>();
                        card.visualcardscript.player_attack_text.text = player2_scores[1][i].ToString();
                        card.visualcardscript.player_defence_text.text = players_cards[card.index].defence.ToString();
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        card.visualcardscript.up_arrows[j].SetActive(false);
                        card.visualcardscript.down_arrows[j].SetActive(false);
                    }
                    if (player2_stat_change[row][i] > 0)
                    {
                        for (int j = 0; j < player2_stat_change[row][i]; j++)
                        {
                            card.visualcardscript.up_arrows[j].SetActive(true);
                        }
                    }
                    else if (player2_stat_change[row][i] < 0)
                    {
                        for (int j = 0; j < -player2_stat_change[row][i]; j++)
                        {
                            card.visualcardscript.down_arrows[j].SetActive(true);
                        }
                    }
                }
            }
        }

        else
        {
            PlayerCard card;
            int child_count;
            for (int row = 0; row < 2; row++)
            {
                child_count = row == 0 ? opponent_defence_players.transform.childCount : opponent_attack_players.transform.childCount;
                for (int i = 0; i < child_count; i++)
                {
                    if (row == 0)
                    {
                        card = opponent_defence_players.transform.GetChild(i).GetComponent<PlayerCard>();
                        card.visualcardscript.player_defence_text.text = player1_scores[0][i].ToString();
                        card.visualcardscript.player_attack_text.text = players_cards[card.index].attack.ToString();
                    }
                    else
                    {
                        card = opponent_attack_players.transform.GetChild(i).GetComponent<PlayerCard>();
                        card.visualcardscript.player_attack_text.text = player1_scores[1][i].ToString();
                        card.visualcardscript.player_defence_text.text = players_cards[card.index].defence.ToString();
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        card.visualcardscript.up_arrows[j].SetActive(false);
                        card.visualcardscript.down_arrows[j].SetActive(false);
                    }

                    if (player1_stat_change[row][i] > 0)
                    {
                        for (int j = 0; j < player1_stat_change[row][i]; j++)
                        {
                            card.visualcardscript.up_arrows[j].SetActive(true);
                        }
                    }
                    else if (player1_stat_change[row][i] < 0)
                    {
                        for (int j = 0; j < -player1_stat_change[row][i]; j++)
                        {
                            card.visualcardscript.down_arrows[j].SetActive(true);
                        }
                    }

                }
            }
            for (int row = 0; row < 2; row++)
            {
                child_count = row == 0 ? me_defence_players.transform.childCount : me_attack_players.transform.childCount;
                for (int i = 0; i < child_count; i++)
                {
                    if (row == 0)
                    {
                        card = me_defence_players.transform.GetChild(i).GetComponent<PlayerCard>();
                        card.visualcardscript.player_defence_text.text = player2_scores[0][i].ToString();
                        card.visualcardscript.player_attack_text.text = players_cards[card.index].attack.ToString();
                    }
                    else
                    {
                        card = me_attack_players.transform.GetChild(i).GetComponent<PlayerCard>();
                        card.visualcardscript.player_attack_text.text = player2_scores[1][i].ToString();
                        card.visualcardscript.player_defence_text.text = players_cards[card.index].defence.ToString();
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        card.visualcardscript.up_arrows[j].SetActive(false);
                        card.visualcardscript.down_arrows[j].SetActive(false);
                    }
                    if (player2_stat_change[row][i] > 0)
                    {
                        for (int j = 0; j < player2_stat_change[row][i]; j++)
                        {
                            card.visualcardscript.up_arrows[j].SetActive(true);
                        }
                    }
                    else if (player2_stat_change[row][i] < 0)
                    {
                        for (int j = 0; j < -player2_stat_change[row][i]; j++)
                        {
                            card.visualcardscript.down_arrows[j].SetActive(true);
                        }
                    }

                }
            }
        }
    }


    [ClientRpc]
    void UpdateScores(NetworkIdentity player1, int player1_defence, int player1_attack, int player2_defence, int player2_attack)
    {
        if (player1.isLocalPlayer)
        {
            me_attack_score.text = player1_attack.ToString();
            me_defence_score.text = player1_defence.ToString();
            opponent_attack_score.text = player2_attack.ToString();
            opponent_defence_score.text = player2_defence.ToString();
        }
        else
        {
            me_attack_score.text = player2_attack.ToString();
            me_defence_score.text = player2_defence.ToString();
            opponent_attack_score.text = player1_attack.ToString();
            opponent_defence_score.text = player1_defence.ToString();
        }
    }
    void PlayedWithActionOnRow(int cardindex, MoveMessage movemessage)
    {
        if (currentPlayer == player1)
        {
            if (movemessage.is_opponent)
            {
                player2_row_modifiers[movemessage.row_to].Add(actions_cards[cardindex]);
            }
            else
            {
                player1_row_modifiers[movemessage.row_to].Add(actions_cards[cardindex]);
            }
            SpawnActionCard(player2, cardindex, movemessage.row_to, -1, !movemessage.is_opponent, ActionCardSpawnType.Row);
        }
        else
        {
            {
                if (movemessage.is_opponent)
                {
                    player1_row_modifiers[movemessage.row_to].Add(actions_cards[cardindex]);
                }
                else
                {
                    player2_row_modifiers[movemessage.row_to].Add(actions_cards[cardindex]);
                }
            }
            SpawnActionCard(player1, cardindex, movemessage.row_to, -1, !movemessage.is_opponent, ActionCardSpawnType.Row);
        }
    }
    void PlayedWithActionOnPlayer(int cardindex,MoveMessage movemessage)
    {
        ActionCardObj apCard = actions_cards[cardindex];
        switch (apCard.cardtype)
        {
            case ActionCardType.ActPlayerOT:
                switch (apCard.ability)
                {
                    case ActionAbility.RemovePlayer:
                        if (currentPlayer == player1)
                        {
                            if (movemessage.is_opponent)
                            {
                                player2_field_cards[movemessage.row_to].cards.Remove(players_cards[movemessage.card_index]);
                            }
                            else
                            {
                                player1_field_cards[movemessage.row_to].cards.Remove(players_cards[movemessage.card_index]);
                            }
                            DeletePlayerCard(player1, movemessage.col_to, movemessage.row_to, movemessage.is_opponent);
                            DeletePlayerCard(player2, movemessage.col_to, movemessage.row_to, !movemessage.is_opponent);
                        }
                        else
                        {
                            if (movemessage.is_opponent)
                            {
                                player1_field_cards[movemessage.row_to].cards.Remove(players_cards[movemessage.card_index]);
                            }
                            else
                            {
                                player2_field_cards[movemessage.row_to].cards.Remove(players_cards[movemessage.card_index]);
                            }
                            DeletePlayerCard(player2, movemessage.col_to, movemessage.row_to, movemessage.is_opponent);
                            DeletePlayerCard(player1, movemessage.col_to, movemessage.row_to, !movemessage.is_opponent);
                        }
                        break;
                }
                break;
            case ActionCardType.ActPlayerLT:
                if (currentPlayer == player1)
                {
                    if (movemessage.is_opponent)
                    {
                        player2_field_cards[movemessage.row_to].cards[players_cards[movemessage.card_index]].Add(apCard);
                    }
                    else
                    {
                        player1_field_cards[movemessage.row_to].cards[players_cards[movemessage.card_index]].Add(apCard);
                    }
                    SpawnActionCard(player2, cardindex, movemessage.row_to, movemessage.col_to, !movemessage.is_opponent, ActionCardSpawnType.Player);
                }
                else
                {
                    if (movemessage.is_opponent)
                    {
                        player1_field_cards[movemessage.row_to].cards[players_cards[movemessage.card_index]].Add(apCard);
                    }
                    else
                    {
                        player2_field_cards[movemessage.row_to].cards[players_cards[movemessage.card_index]].Add(apCard);
                    }
                    SpawnActionCard(player1, cardindex, movemessage.row_to, movemessage.col_to, !movemessage.is_opponent, ActionCardSpawnType.Player);
                }
                break;
        }
    }
    void PlayedWithPlayer(PlayerCardObj card, int cardindex, MoveMessage movemessage)
    {
        if (currentPlayer == player1)
        {
            if (movemessage.row_from == -1)
            {
                if (player1_players.Contains(card))
                {
                    player1_players.Remove(card);
                    player1_field_cards[movemessage.row_to].cards.Add(card, new List<ActionCardObj>());
                    SpawnPlayerCard(player2, cardindex, movemessage.row_to);
                }
                else Debug.Log(("WTF", "Current player doesn't have that card"), currentPlayer);
            }
            else
            {
                if (player1_field_cards[movemessage.row_from].cards.ContainsKey(card))
                {
                    List<ActionCardObj> cards_modifs = player1_field_cards[movemessage.row_from].cards[card];
                    player1_field_cards[movemessage.row_to].cards.Add(card, cards_modifs);
                    player1_field_cards[movemessage.row_from].cards.Remove(card);
                }
                else Debug.Log(("WTF", "Where did current player pulled that card from?"), currentPlayer);
                MovePlayerCard(player2, movemessage);
            }
        }
        else
        {
            if (movemessage.row_from == -1)
            {
                if (player2_players.Contains(card))
                {
                    player2_players.Remove(card);
                    player2_field_cards[movemessage.row_to].cards.Add(card, new List<ActionCardObj>());
                    SpawnPlayerCard(player1, cardindex, movemessage.row_to);
                }
                else Debug.Log(("WTF", "Current player doesn't have that card"), currentPlayer);
            }
            else
            {
                if (player2_field_cards[movemessage.row_from].cards.ContainsKey(card))
                {
                    List<ActionCardObj> cards_modifs = player2_field_cards[movemessage.row_from].cards[card];
                    player2_field_cards[movemessage.row_to].cards.Add(card, cards_modifs);
                    player2_field_cards[movemessage.row_from].cards.Remove(card);
                }
                else Debug.Log(("WTF", "Where did current player pulled that card from?"), currentPlayer);
                MovePlayerCard(player1, movemessage);
            }
        }
    }

    [ClientRpc]
    public void RpcShowWinner(NetworkIdentity winner)
    {
        foreach (CellGUI cellGUI in MatchCells.Values)
            cellGUI.GetComponent<Button>().interactable = false;

        if (winner == null)
        {
            gameText.text = "Draw!";
            gameText.color = Color.yellow;
        }
        else if (winner.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            gameText.text = "Winner!";
            gameText.color = Color.blue;
        }
        else
        {
            gameText.text = "Loser!";
            gameText.color = Color.red;
        }

        exitButton.gameObject.SetActive(true);
        playAgainButton.gameObject.SetActive(true);
    }

    [Command(requiresAuthority = false)]
    public void CmdMakePlay(CellValue cellValue, NetworkConnectionToClient sender = null)
    {
        if (sender.identity != currentPlayer || MatchCells[cellValue].playerIdentity != null)
            return;

        MatchCells[cellValue].playerIdentity = currentPlayer;
        RpcUpdateCell(cellValue, currentPlayer);

        MatchPlayerData mpd = matchPlayerData[currentPlayer];
        mpd.currentScore = mpd.currentScore | cellValue;
        matchPlayerData[currentPlayer] = mpd;

        boardScore = boardScore | cellValue;

        if (CheckWinner(mpd.currentScore))
        {
            mpd.wins += 1;
            matchPlayerData[currentPlayer] = mpd;
            RpcShowWinner(currentPlayer);
            currentPlayer = null;
        }
        else if (boardScore == CellValue.Full)
        {
            RpcShowWinner(null);
            currentPlayer = null;
        }
        else
        {
            currentPlayer = currentPlayer == player1 ? player2 : player1;
        }

    }

    [ServerCallback]
    bool CheckWinner(CellValue currentScore)
    {
        if ((currentScore & CellValue.TopRow) == CellValue.TopRow)
            return true;
        if ((currentScore & CellValue.MidRow) == CellValue.MidRow)
            return true;
        if ((currentScore & CellValue.BotRow) == CellValue.BotRow)
            return true;
        if ((currentScore & CellValue.LeftCol) == CellValue.LeftCol)
            return true;
        if ((currentScore & CellValue.MidCol) == CellValue.MidCol)
            return true;
        if ((currentScore & CellValue.RightCol) == CellValue.RightCol)
            return true;
        if ((currentScore & CellValue.Diag1) == CellValue.Diag1)
            return true;
        if ((currentScore & CellValue.Diag2) == CellValue.Diag2)
            return true;

        return false;
    }

    [ClientRpc]
    public void RpcUpdateCell(CellValue cellValue, NetworkIdentity player)
    {
        MatchCells[cellValue].SetPlayer(player);
    }

    // Assigned in inspector to ReplayButton::OnClick
    [ClientCallback]
    public void RequestPlayAgain()
    {
        playAgainButton.gameObject.SetActive(false);
        CmdPlayAgain();
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayAgain(NetworkConnectionToClient sender = null)
    {
        if (!playAgain)
            playAgain = true;
        else
        {
            playAgain = false;
            RestartGame();
        }
    }

    [ServerCallback]
    public void RestartGame()
    {
        foreach (CellGUI cellGUI in MatchCells.Values)
            cellGUI.SetPlayer(null);

        boardScore = CellValue.None;

        NetworkIdentity[] keys = new NetworkIdentity[matchPlayerData.Keys.Count];
        matchPlayerData.Keys.CopyTo(keys, 0);

        foreach (NetworkIdentity identity in keys)
        {
            MatchPlayerData mpd = matchPlayerData[identity];
            mpd.currentScore = CellValue.None;
            matchPlayerData[identity] = mpd;
        }

        RpcRestartGame();

        startingPlayer = startingPlayer == player1 ? player2 : player1;
        currentPlayer = startingPlayer;
    }

    [ClientRpc]
    public void RpcRestartGame()
    {
        foreach (CellGUI cellGUI in MatchCells.Values)
            cellGUI.SetPlayer(null);

        exitButton.gameObject.SetActive(false);
        playAgainButton.gameObject.SetActive(false);
    }

    // Assigned in inspector to BackButton::OnClick
    [Client]
    public void RequestExitGame()
    {
        exitButton.gameObject.SetActive(false);
        CmdRequestExitGame();
    }

    [Command(requiresAuthority = false)]
    public void CmdRequestExitGame(NetworkConnectionToClient sender = null)
    {
        StartCoroutine(ServerEndMatch(sender, false));
    }

    [ServerCallback]
    public void OnPlayerDisconnected(NetworkConnectionToClient conn)
    {
        // Check that the disconnecting client is a player in this match
        if (player1 == conn.identity || player2 == conn.identity)
            StartCoroutine(ServerEndMatch(conn, true));
    }

    [ServerCallback]
    public IEnumerator ServerEndMatch(NetworkConnectionToClient conn, bool disconnected)
    {
        RpcExitGame();

        canvasController.OnPlayerDisconnected -= OnPlayerDisconnected;

        // Wait for the ClientRpc to get out ahead of object destruction
        yield return new WaitForSeconds(0.1f);

        // Mirror will clean up the disconnecting client so we only need to clean up the other remaining client.
        // If both players are just returning to the Lobby, we need to remove both connection Players

        if (!disconnected)
        {
            NetworkServer.RemovePlayerForConnection(player1.connectionToClient, true);
            CanvasController.waitingConnections.Add(player1.connectionToClient);

            NetworkServer.RemovePlayerForConnection(player2.connectionToClient, true);
            CanvasController.waitingConnections.Add(player2.connectionToClient);
        }
        else if (conn == player1.connectionToClient)
        {
            // player1 has disconnected - send player2 back to Lobby
            NetworkServer.RemovePlayerForConnection(player2.connectionToClient, true);
            CanvasController.waitingConnections.Add(player2.connectionToClient);
        }
        else if (conn == player2.connectionToClient)
        {
            // player2 has disconnected - send player1 back to Lobby
            NetworkServer.RemovePlayerForConnection(player1.connectionToClient, true);
            CanvasController.waitingConnections.Add(player1.connectionToClient);
        }

        // Skip a frame to allow the Removal(s) to complete
        yield return null;

        // Send latest match list
        canvasController.SendMatchList();

        NetworkServer.Destroy(gameObject);
    }

    public GameObject PlayerCardPrefab;
    public GameObject ActionCardPrefab; 

    [ClientRpc]
    public void MovePlayerCard(NetworkIdentity player, MoveMessage movemessage)
    {
        if (!player.isLocalPlayer) return;
        Transform card = null;
        switch (movemessage.row_from)
        {
            case 1:
                card = opponent_attack_players.transform.GetChild(movemessage.col_from);
                break;
            case 0:
                card = opponent_defence_players.transform.GetChild(movemessage.col_from);
                break;
        }
        card.SetParent(movemessage.row_to == 1 ? opponent_attack_players.transform : opponent_defence_players.transform);
        card.SetAsLastSibling();
    }
    [ClientRpc]
    public void SpawnPlayerCard(NetworkIdentity player, int cardindex, int row)
    {
        if (!player.isLocalPlayer) { return; }
        PlayerCardObj obj = players_cards[cardindex];
        GameObject card = Instantiate(PlayerCardPrefab);
        PlayerCard cardscript = card.GetComponent<PlayerCard>();
        if (row == -1)
        {
            card.transform.SetParent(player_players_cards_holder);
            cardscript.Placed = false;
            cardscript.CanDrag = true;
            ChangeVisualPlayerCardPos(cardscript, -1);
        }
        else if (row == 1)
        {
            card.transform.SetParent(opponent_attack_players.transform);
            cardscript.Placed = true;
            cardscript.CanDrag = false;
            ChangeVisualPlayerCardPos(cardscript, 1);
        }
        else if (row == 0)
        {
            card.transform.SetParent(opponent_defence_players.transform);
            cardscript.Placed = true;
            cardscript.CanDrag = false;
            ChangeVisualPlayerCardPos(cardscript, 1);
        }
        card.transform.localScale = Vector3.one;
        cardscript.index = cardindex;
        cardscript.visualcardparent = visual_cards_holder;
        cardscript.matchcontroller = transform.GetComponent<MatchController>();
        Color[] colors = { Color.cyan, Color.green, Color.red, Color.yellow, Color.blue, Color.magenta };
        card.GetComponent<RawImage>().color = colors[UnityEngine.Random.Range(0, colors.Length)];
        var plrmodifs = card.AddComponent<PlayerModifs>();
        plrmodifs.matchcontroller = transform.GetComponent<MatchController>();
        
    }

    [ClientRpc]
    public void SpawnActionCard(NetworkIdentity player,int cardindex, int row, int col, bool is_opponent, ActionCardSpawnType spawnType)
    {
        
        if (!player.isLocalPlayer) { return; }
        ActionCardObj obj = actions_cards[cardindex];
        GameObject card = Instantiate(ActionCardPrefab);
        ActionCard cardscript = card.GetComponent<ActionCard>();
        switch (spawnType) {
            case ActionCardSpawnType.Init:
                card.transform.SetParent(player_actions_cards_holder);
                cardscript.CanDrag = true;
                ChangeVisualActionCardPos(cardscript, -1);
                break;
            case ActionCardSpawnType.Player:                
                if (is_opponent)
                {
                    switch (row)
                    {
                        case 1:
                            card.transform.SetParent(opponent_attack_players.transform.GetChild(col));
                            break;
                        case 0:
                            card.transform.SetParent(opponent_defence_players.transform.GetChild(col));
                            break;
                    }
                }
                else
                {
                    switch (row)
                    {
                        case 1:
                            card.transform.SetParent(me_attack_players.transform.GetChild(col));
                            break;
                        case 0:
                            card.transform.SetParent(me_defence_players.transform.GetChild(col));
                            break;
                    }
                }
                ChangeVisualActionCardPos(cardscript, 1);
                card.transform.parent.GetComponent<PlayerCard>().offModifsImages();
                card.transform.position = card.transform.parent.position;
                break;

            case ActionCardSpawnType.Row:
                if (is_opponent)
                {
                    switch (row) {
                        case 1:
                            card.transform.SetParent(opponent_attack_modifs.transform);
                            break;
                        case 0:
                            card.transform.SetParent(opponent_defence_modifs.transform);
                            break;
                    }
                }
                else
                {
                    switch (row)
                    {
                        case 1:
                            card.transform.SetParent(me_attack_modifs.transform);
                            break;
                        case 0:
                            card.transform.SetParent(me_defence_modifs.transform);
                            break;
                    }
                }
                ChangeVisualActionCardPos(cardscript, 1);
                card.transform.position = card.transform.parent.position;
                cardscript.CanDrag = false;
                break;
        }
        card.transform.localScale = Vector3.one;
        Color[] colors = { Color.cyan, Color.green, Color.red, Color.yellow, Color.blue, Color.magenta };
        card.GetComponent<RawImage>().color = colors[UnityEngine.Random.Range(0, colors.Length)];
        cardscript.index = cardindex;
        cardscript.matchcontroller = transform.GetComponent<MatchController>();
        cardscript.cardtype = obj.cardtype;
        cardscript.visualcardparent = visual_cards_holder;

    }
    void ChangeVisualActionCardPos(ActionCard cardscript, int is_opponent)
    {
        switch (is_opponent)
        {
            case -1:
                cardscript.visualcard_pos = stack_card_spawn.position;
                break;
            case 1:
                cardscript.visualcard_pos = opponent_card_spawn.position;
                break;
            case 0:
                cardscript.visualcard_pos = me_card_spawn.position;
                break;
        }
    }
    void ChangeVisualPlayerCardPos(PlayerCard cardscript, int is_opponent)
    {
        switch (is_opponent)
        {
            case -1:
                cardscript.visualcard_pos = stack_card_spawn.position;
                break;
            case 1:
                cardscript.visualcard_pos = opponent_card_spawn.position;
                break;
            case 0:
                cardscript.visualcard_pos = me_card_spawn.position;
                break;
        }
    }

    [ClientRpc]
    public void DeletePlayerCard(NetworkIdentity player, int siblingIndex, int row, bool is_opponent)
    {
        if (!player.isLocalPlayer) return;
        else if (is_opponent)
        {
            switch (row)
            {
                case 1:
                    opponent_attack_players.transform.GetChild(siblingIndex).GetComponent<PlayerCard>().delete();
                    break;
                case 0:
                    opponent_defence_players.transform.GetChild(siblingIndex).GetComponent<PlayerCard>().delete();
                    break;
            }
        }
        else
        {
            switch (row)
            {
                case 1:
                    me_attack_players.transform.GetChild(siblingIndex).GetComponent<PlayerCard>().delete();
                    break;
                case 0:
                    me_defence_players.transform.GetChild(siblingIndex).GetComponent<PlayerCard>().delete();
                    break;
            }
        }
    }

    [ClientRpc]
    public void RpcExitGame()
    {
        canvasController.OnMatchEnded();
    }
}
