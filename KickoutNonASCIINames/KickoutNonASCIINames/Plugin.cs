using System.Reflection;
using Terraria;
using Terraria.Chat.Commands;
using Terraria.ID;
using Terraria.WorldBuilding;
using TerrariaApi.Server;
using TShockAPI;
using Conditions = Terraria.GameContent.ItemDropRules.Conditions;

namespace KickoutNonASCIINames;

[ApiVersion(2, 1)]
public class Plugin : TerrariaPlugin
{
    public override string Name => "KickoutNonASCIINames";
    public override string Author => "ichiris";
    public override string Description => "自动踢出id含有ASCII码以外字符的玩家";
    public override Version Version => new Version(1,0,0);

    public Plugin(Main game) : base(game)
    {
    }

    private bool EnableAutoKick = true;
    private string KickMsg = "由于昵称含有非ASCII字符，你已被踢出。";
    
    public override void Initialize()
    {
        Commands.ChatCommands.Add(
            new Command("KickoutNonASCIINames", command_run_function, "KickoutNonASCIINames", "koin")
                {
                HelpText = "自动踢出id含有ASCII码以外字符的玩家"
                }
        );
        ServerApi.Hooks.ServerJoin.Register(this, hook_run_function);
    }

    private void command_run_function(CommandArgs args)
    {
        if (args.Parameters.Count != 1)
        {
            args.Player.SendErrorMessage(
                "无效的koin子命令。\n"+
                "/koin run -- 手动执行一次踢出\n" +
                "/koin enable -- 启用自动踢出\n"+
                "/koin disable -- 禁用自动踢出\n");
            return;
        }

        switch (args.Parameters[0])
        {
            case "run":
                kick_out_function();
                args.Player.SendSuccessMessage("已执行一次踢出");
                break;
            case "enable":
                this.EnableAutoKick = true;
                args.Player.SendSuccessMessage("自动踢出已启用");
                kick_out_function();
                break;
            case "disable":
                this.EnableAutoKick = false;
                args.Player.SendSuccessMessage("自动踢出已禁用");
                break;
            default:
                args.Player.SendErrorMessage(
                    "无效的koin子命令。\n" +
                    "/koin run -- 执行一次踢出\n" +
                    "/koin enable -- 启用自动踢出\n"+
                    "/koin disable -- 禁用自动踢出\n");
                break;       
        }
    }

    private void hook_run_function(JoinEventArgs args)
    {   
        if(!this.EnableAutoKick)
            return;
        kick_out_function();
    }
    
    private void kick_out_function()
    {
        try
        {
            var plrs = TShock.Players;
            if (plrs==null)
                return;
            var plrsLength = plrs.Length;
            string plr;
            for(int i = 0;i<plrsLength;i++){
                if (string.IsNullOrEmpty(plrs[i].Name))
                    continue;
                plr=plrs[i].Name;
                if (HasNonASCIIChars(plr))
                    plrs[i].Kick(KickMsg,true,true,plr,false);
            }
        }
        catch (Exception e)
        {
        }
        
    }
    
    private bool HasNonASCIIChars(string str)
    {
        return (System.Text.Encoding.UTF8.GetByteCount(str) != str.Length);
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var asm = Assembly.GetExecutingAssembly();
            Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == asm);
            ServerApi.Hooks.ServerJoin.Deregister(this, hook_run_function);
        }
        base.Dispose(disposing);
    }
}
