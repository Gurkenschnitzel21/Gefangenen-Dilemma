// See https://aka.ms/new-console-template for more information
using Spectre.Console;

internal class Program
{
    static readonly int[,] pointTable =
    {
        //none, Coop, Attack
        { 0, 0, 0},
        { 0, 3, 0},
        { 0, 5, 1}
    };
    private static void Main(string[] args)
    {
        var scores = PrisonersDilemma(Misstrauisch, AugeUmAuge, 5);
        AnsiConsole.MarkupLine($"[red]Spieler 1: {scores.Item1}Punkte[/]\n" +
                              $"[blue]Spieler 2: {scores.Item2}Punkte[/]");

        AnsiConsole.MarkupLine("[red]Spieler 1:[/]");
        foreach (var action in scores.Item3) AnsiConsole.MarkupLine($"{action.ToString()}");
        AnsiConsole.MarkupLine("[blue]Spieler 2:[/]");
        foreach (var action in scores.Item4) AnsiConsole.MarkupLine($"{action.ToString()}");
    }

    private static (int, int, List<Actions>, List<Actions>) PrisonersDilemma(Prisoner player1, Prisoner player2, int rounds)
    {
        int player1Score = 0, player2Score = 0;
        List<Actions> player1Actions = new List<Actions>();
        List<Actions> player2Actions = new List<Actions>();
        for (int i = 0; i < rounds; i++)
        {
            Actions player1Action = player1(playerActions: player1Actions.ToArray(), opponentActions: player2Actions.ToArray());
            Actions player2Action = player2(playerActions: player2Actions.ToArray(), opponentActions: player1Actions.ToArray());

            player1Actions.Add(player1Action);
            player2Actions.Add(player2Action);

            player1Score += pointTable[(int)player1Actions[player1Actions.Count - 1], (int)player2Actions[player2Actions.Count - 1]];
            player2Score += pointTable[(int)player2Actions[player2Actions.Count - 1], (int)player1Actions[player1Actions.Count - 1]];
        }
        return (player1Score, player2Score, player1Actions, player2Actions);
    }
    private static (int, int, List<Actions>, List<Actions>) PrisonersDilemma(Prisoner player1, Prisoner player2, int rounds, int milliSecondDelay = 0)
    {
        int player1Score = 0, player2Score = 0;
        List<Actions> player1Actions = new List<Actions>();
        List<Actions> player2Actions = new List<Actions>();
        for (int i = 0; i < rounds; i++)
        {
            player1Actions.Add(player1(playerActions: player1Actions.ToArray(), opponentActions: player2Actions.ToArray()));
            player2Actions.Add(player2(playerActions: player2Actions.ToArray(), opponentActions: player1Actions.ToArray()));

            player1Score += pointTable[(int)player1Actions[player1Actions.Count - 1], (int)player2Actions[player2Actions.Count - 1]];
            player2Score += pointTable[(int)player2Actions[player2Actions.Count - 1], (int)player1Actions[player1Actions.Count - 1]];
        }
        Thread.Sleep(milliSecondDelay);
        return (player1Score, player2Score, player1Actions, player2Actions);
    }

    private delegate Actions Prisoner(Actions[] opponentActions, Actions[] playerActions);
    public enum Actions
    {
        None = 0,
        Cooperate = 1,
        Attack = 2
    }

    public static Actions AugeUmAuge(Actions[] opponentActions, Actions[] actions)
    {
        if (opponentActions.Length == 0) return Actions.Cooperate;
        switch (opponentActions[opponentActions.Length - 1])
        {
            case Actions.Attack: return Actions.Attack;
            case Actions.Cooperate: return Actions.Cooperate;
            default: return Actions.Cooperate;
        }
    }
    public static Actions Feind(Actions[] opponentActions, Actions[] actions)
    {
        return Actions.Attack;
    }
    public static Actions Freund(Actions[] opponentActions, Actions[] actions)
    {
        return Actions.Cooperate;
    }
    public static Actions Nachtragend(Actions[] opponentActions, Actions[] actions)
    {
        if (opponentActions.Length == 0) return Actions.Cooperate;
        if (actions.Length > 0) if (actions[actions.Length - 1] == Actions.Attack) return Actions.Attack;
        switch (opponentActions[opponentActions.Length - 1])
        {
            case Actions.Attack: return Actions.Attack;
            case Actions.Cooperate: return Actions.Cooperate;
            default: return Actions.Cooperate;
        }
    }
    public static Actions Misstrauisch(Actions[] opponentActions, Actions[] actions)
    {
        if (opponentActions.Length == 0) return Actions.Attack;
        if (actions.Length > 0) if (actions[actions.Length - 1] == Actions.Cooperate) return Actions.Cooperate;
        if (opponentActions[opponentActions.Length - 1] == Actions.Cooperate) return Actions.Cooperate;
        return Actions.Attack;
    }
    public static Actions Launisch(Actions[] opponentActions, Actions[] actions)
    {
        if (opponentActions.Length % 2 == 0) return Actions.Cooperate;
        else return Actions.Attack;
    }
}