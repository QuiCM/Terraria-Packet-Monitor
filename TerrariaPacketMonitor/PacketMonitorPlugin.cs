using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using TerrariaApi.Server;

namespace TerrariaPacketMonitor
{
    [ApiVersion(2, 1)]
    public class PacketMonitorPlugin : TerrariaPlugin
    {
        public override string Author => "Pryaxis";
        public override string Description => "Dumps packet data to the console";
        public override string Name => "Packet Monitor";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public PacketMonitorPlugin(Main game) : base(game)
        {
            Order = -1;
        }

        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData, -1);
            ServerApi.Hooks.NetSendData.Register(this, OnSendData, -1);
        }

        void OnGetData(GetDataEventArgs args)
        {
            PacketTypes type = args.MsgID;

            List<PacketTypes> spamPackets = new List<PacketTypes> { PacketTypes.PlayerUpdate, PacketTypes.PlayerHp, PacketTypes.NpcTalk, PacketTypes.ProjectileDestroy, PacketTypes.ProjectileNew, PacketTypes.Zones };
            if (!spamPackets.Contains(type))
            {
                Console.WriteLine($"{DateTime.Now}: [Recv] {(byte)type} ({type}) from: {args.Msg.whoAmI} ({Main.player[args.Msg.whoAmI].name})");
            }

            if (type == PacketTypes.PlaceTileEntity)
            {
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
                {
                    short x = data.ReadInt16();
                    short y = data.ReadInt16();
                    byte teType = data.ReadInt8();

                    Console.WriteLine($"{DateTime.Now}: \t\t[Recv] TEpl @ ({x}, {y}), type: {teType}");
                }
            }

            if (type == PacketTypes.PlaceObject)
            {
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
                {
                    short x = data.ReadInt16();
                    short y = data.ReadInt16();
                    short objType = data.ReadInt16();
                    short style = data.ReadInt16(); 

                    Console.WriteLine($"{DateTime.Now}: \t\t[Recv] OBJpl @ ({x}, {y}), type: {objType}, style: {style}");
                }
            }

            if (type == PacketTypes.Tile)
            {
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
                {
                    byte action = data.ReadInt8();
                    short x = data.ReadInt16();
                    short y = data.ReadInt16();
                    short var1 = data.ReadInt16();
                    byte var2 = data.ReadInt8();

                    bool fail = var1 == 1;

                    Console.WriteLine($"{DateTime.Now}: \t\t [Recv] Tile Edit @ ({x}, {y}), action: {action}, var1: {var1}, var2: {var2}, fail: {fail}");
                }
            }
        }

        void OnSendData(SendDataEventArgs args)
        {
            PacketTypes type = args.MsgId;

            List<PacketTypes> spamPackets = new List<PacketTypes> { PacketTypes.PlayerUpdate, PacketTypes.PlayerHp, PacketTypes.NpcTalk, PacketTypes.ProjectileDestroy, PacketTypes.ProjectileNew, PacketTypes.NpcUpdate, PacketTypes.Zones, PacketTypes.ItemDrop };
            if (!spamPackets.Contains(type))
            {
                Console.WriteLine($"{DateTime.Now}: [Send] {(byte)type} ({type}) ign: {args.ignoreClient} ({(args.ignoreClient == -1 ? "-" : Main.player[args.ignoreClient].name)}) | rem: {args.remoteClient} ({(args.remoteClient == -1 ? "-" : Main.player[args.remoteClient].name)})");
            }

            if (type == PacketTypes.UpdateTileEntity)
            {
                int id = args.number;
                bool exists = TileEntity.ByID.ContainsKey(id);
                StringBuilder sb = new StringBuilder();

                sb.Append($"{DateTime.Now}: \t\t[Send] TEupd. Entity ID: {id}. Remove entity: {!exists}.");

                if (exists)
                {
                    TEItemFrame iframe = (TEItemFrame)TileEntity.ByID[id];
                    sb.Append($"\n\t\t  Type: {iframe.type}. Position: ({iframe.Position.X}, {iframe.Position.Y})");
                    sb.Append($"\n\t\t  Item details: Type: {iframe.item?.type}. Stack: {iframe.item?.stack}");
                }

                Console.WriteLine(sb.ToString());
            }

            if (type == PacketTypes.Tile)
            {
                byte action = (byte)args.number;
                short x = (short)args.number2;
                short y = (short)args.number3;
                short var1 = (short)args.number4;
                byte var2 = (byte)args.number5;

                Console.WriteLine($"{DateTime.Now}: \t\t [Send] Tile Edit @ ({x}, {y}), action: {action}, var1: {var1}, var2: {var2}, fail: {var1 == 1}");
            }
        }
    }
}
