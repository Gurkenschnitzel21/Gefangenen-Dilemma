// See https://aka.ms/new-console-template for more information
using Spectre.Console;
using System.ComponentModel;

internal class Program
{
    static readonly int[,] pointTable =
    {
        //none, Coop, Attack
        { 0, 0, 0},
        { 0, 2, 0},
        { 0, 3, 0}
    };
    private delegate Actions Prisoner(Actions[] opponentActions, Actions[] playerActions);
    public enum Actions
    {
        None = 0,
        Cooperate = 1,
        Attack = 2
    }


    private static void Main(string[] args)
    {
        Prisoner[] methods = [AugeUmAuge, Feind, Freund, Misstrauisch, Nachtragend, Launisch, Zufällig]; 

        var choices = AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
        .Title("Welche Algorithmen sollen eingesetzt werden?")
        .PageSize(10)
        .MoreChoicesText("[grey](Hoch und Runter bewegen, um mehr Algorithmen einzusehen)[/]")
        .InstructionsText(
            "[grey](Drücke [blue]<space>[/] um eine, " +
            "[green]<enter>[/] um zu bestätigen)[/]")
        .AddChoices(methods.Select(x => x.Method.Name)));

        foreach (var choice in choices) AnsiConsole.WriteLine(choice);

        var results = RunMatches(methods.Where(x => choices.Contains(x.Method.Name)).Select(x => x).ToArray(), 20);
        AnsiConsole.WriteLine("\n");
        var options = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Wähle eine der folgenden Optionen:")
        .PageSize(10)
        .MoreChoicesText("[grey](Hoch und Runter bewegen, um mehr Algorithmen einzusehen)[/]")
        .AddChoices(new[] {
            "Beenden",
            "Wiederholen",
            "Mehr Infos",
        }));

        
        switch (options)
        {
            case "Beenden": break;
            case "Wiederholen": AnsiConsole.Clear(); Main(args); break;
            case "Mehr Infos": ProvideAdditionalInfo(results); Console.ReadLine(); AnsiConsole.Clear(); Main(args); break;
            default: break;
        }
    }

    private static Dictionary<(string, string), (int, int, List<Actions>, List<Actions>)> RunMatches(Prisoner[] methods, int RoundsPerMatch)
    {
        Dictionary<Prisoner, int> prisoners = new Dictionary<Prisoner, int>();
        Dictionary<(string, string), (int, int, List<Actions>, List<Actions>)> results = new Dictionary<(string, string), (int, int, List<Actions>, List<Actions>)>();
        foreach (Prisoner prisoner in methods) prisoners.Add(prisoner, 0);
        foreach (Prisoner prisoner in prisoners.Keys)
        {
            foreach (Prisoner opponent in prisoners.Keys)
            {
                var result = PrisonersDilemma(prisoner, opponent, RoundsPerMatch);
                results.Add((prisoner.Method.Name, opponent.Method.Name), result);
                prisoners[prisoner] += result.Item1;
                prisoners[opponent] += result.Item2;

                PrintMatchTable(prisoner.Method.Name, opponent.Method.Name, result, RoundsPerMatch);
            }
        }
        Random random = new Random();
        AnsiConsole.Write(new BarChart().Width(methods.Length * RoundsPerMatch * 10)
                                        .Label("[underline cyan]Endergebnis[/]")
                                        .CenterLabel()
                                        .AddItems(prisoners, score => new BarChartItem(score.Key.Method.Name, score.Value, new Color((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)))));
    
        return results;
    }
    private static void PrintMatchTable(string prisoner, string opponent, (int, int, List<Actions>, List<Actions>) result, int RoundsPerMatch)
    {
        var resultTable = new Table();
        resultTable.Title = new TableTitle($"[red]{prisoner}[/] vs. [blue]{opponent}[/]");
        resultTable.AddColumn("Algorithmus");
        for (int i = 0; i < RoundsPerMatch; i++)
        {
            resultTable.AddColumn($"{i + 1}");
        }
        resultTable.AddColumn("Punkte");
        string[] prisonerResult = new string[RoundsPerMatch + 2];
        string[] opponentResult = new string[RoundsPerMatch + 2];
        prisonerResult[0] = $"[red]{prisoner}[/]";
        opponentResult[0] = $"[blue]{opponent}[/]";

        for (int i = 1; i < RoundsPerMatch + 1; i++)
        {
            prisonerResult[i] = result.Item3[i - 1] == Actions.Attack ? "[red]o[/]" : "[green]o[/]";
            opponentResult[i] = result.Item4[i - 1] == Actions.Attack ? "[red]o[/]" : "[green]o[/]";
        }
        prisonerResult[prisonerResult.Length - 1] = $"[cyan]{result.Item1}[/]";
        opponentResult[opponentResult.Length - 1] = $"[cyan]{result.Item2}[/]";
        resultTable.AddRow(prisonerResult);
        resultTable.AddRow(opponentResult);

        AnsiConsole.Write(resultTable);
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
    private static void ProvideAdditionalInfo(Dictionary<(string, string), (int, int, List<Actions>, List<Actions>)> matchData)
    {
        //additional info: highest lead, highest sum, most coops, most successful attacks (top 3)
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Höchste Punktedifferenz[/]"));
        var tempData = matchData.Select(x => x.Key).ToList();
        for (int i = 0; i < 3; i++)
        {
            (string, string) bestMatch = ("", "");
            foreach (var kvp in tempData)
            {
                if (bestMatch.Item1 == "" || Math.Abs(matchData[kvp].Item1 - matchData[kvp].Item2) > Math.Abs(matchData[bestMatch].Item1 - matchData[bestMatch].Item2)) bestMatch = kvp;
            }
            tempData.Remove(bestMatch);
            PrintMatchTable(bestMatch.Item1, bestMatch.Item2, matchData[bestMatch], matchData[bestMatch].Item3.Count);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Höchste Gesamtpunktzahl[/]"));
        tempData = matchData.Select(x => x.Key).ToList();
        for (int i = 0; i < 3; i++)
        {
            (string, string) bestMatch = ("", "");
            foreach (var kvp in tempData)
            {
                if (bestMatch.Item1 == "" || matchData[kvp].Item1 + matchData[kvp].Item2 > matchData[bestMatch].Item1 + matchData[bestMatch].Item1) bestMatch = kvp;
            }
            tempData.Remove(bestMatch);
            PrintMatchTable(bestMatch.Item1, bestMatch.Item2, matchData[bestMatch], matchData[bestMatch].Item3.Count);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Meiste Kooperationen[/]"));
        tempData = matchData.Select(x => x.Key).ToList();
        for (int i = 0; i < 3; i++)
        {
            (string, string) bestMatch = ("", "");
            int mostCoops = 0;
            foreach (var kvp in tempData)
            {
                int coops = 0;
                for (int j = 0; j < matchData[kvp].Item3.Count; j++)
                {
                    if (matchData[kvp].Item3[j] == Actions.Cooperate && matchData[kvp].Item4[j] == Actions.Cooperate) coops++;
                }
                if (coops > mostCoops || bestMatch.Item1 == "")
                {
                    mostCoops = coops;
                    bestMatch = kvp;
                }
            }
            tempData.Remove(bestMatch);
            PrintMatchTable(bestMatch.Item1, bestMatch.Item2, matchData[bestMatch], matchData[bestMatch].Item3.Count);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Meiste erfolgreiche Angriffe[/]"));
        tempData = matchData.Select(x => x.Key).ToList();
        for (int i = 0; i < 3; i++)
        {
            (string, string) bestMatch = ("", "");
            int mostAttacks = 0;
            foreach (var kvp in tempData)
            {
                int attacks = 0;
                for (int j = 0; j < matchData[kvp].Item3.Count; j++)
                {
                    if (matchData[kvp].Item3[j] == Actions.Attack && matchData[kvp].Item4[j] == Actions.Cooperate || matchData[kvp].Item3[j] == Actions.Cooperate && matchData[kvp].Item4[j] == Actions.Attack) attacks++;
                }
                if (attacks > mostAttacks || bestMatch.Item1 == "")
                {
                    mostAttacks = attacks;
                    bestMatch = kvp;
                }
            }
            tempData.Remove(bestMatch);
            PrintMatchTable(bestMatch.Item1, bestMatch.Item2, matchData[bestMatch], matchData[bestMatch].Item3.Count);
        }
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
    public static Actions Zufällig(Actions[] opponentActions, Actions[] actions)
    {
        Random random = new Random();
        return random.Next(2) > 0 ? Actions.Attack : Actions.Cooperate;
    }
}