using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Shared.Antag;
using Content.Shared.Mind;
using Content.Shared.Players;
using Microsoft.EntityFrameworkCore;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.Antag.Commands;

[AdminCommand(AdminFlags.Admin)]
internal class RemoveAntagRoleCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    public override string Command => "removeAntagRole";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteLine(Loc.GetString($"shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
        }

        if (!_players.TryGetSessionByUsername(args[0], out var session))
        {
            shell.WriteLine(Loc.GetString("cmd-erase-player-not-found"));
            return;
        }

        var playerMind = session.GetMind();
        if (playerMind == null || !_ent.TryGetComponent<MindComponent>(playerMind, out var mindComponent))
        {
            shell.WriteLine("The player has no mind");
            return;
        }

        if (!_prototypeManager.TryIndex<AntagLoadoutPrototype>(args[1], out var loadout))
        {
            shell.WriteLine(LocalizationManager.GetString("No AntagLoadout found"));
            return;
        }

        var deleteComp = true;
        if (args.Length > 2 && !bool.TryParse(args[2], out deleteComp))
        {
            shell.WriteLine(LocalizationManager.GetString("shell-argument-must-be-boolean"));
            return;
        }


        if (!_antag.TryRemoveAntag((playerMind.Value, mindComponent), loadout.ID, deleteComp))
        {
            shell.WriteLine(LocalizationManager.GetString("This entity does not have this AntagDataa"));
        }


    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), LocalizationManager.GetString("Player")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<AntagLoadoutPrototype>(),
                LocalizationManager.GetString("Antag loadout")),
            3 => CompletionResult.FromHintOptions(CompletionHelper.Booleans,
                LocalizationManager.GetString("Delete antag components")),
            4 => CompletionResult.Empty
        };
    }
}
