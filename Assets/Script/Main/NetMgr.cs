// Copyright(c) Cragon. All rights reserved.

namespace Casinos
{
    using System;
    using XLua;
    using GameCloud.Unity.Common;

    public class NetMgr
    {
        //---------------------------------------------------------------------
        public RpcSession RpcSession { get; set; }
        public Action<ushort, byte[]> LuaOnRpcMethod { get; set; }
        Action LuaOnSocketClose { get; set; }

        //---------------------------------------------------------------------
        public NetMgr()
        {
            var rpc_session_factory = new RpcSessionFactoryTcpClient();
            RpcSession = rpc_session_factory.createRpcSession();
        }

        //---------------------------------------------------------------------
        public void Update(float tm)
        {
            RpcSession.update(tm);
        }

        //---------------------------------------------------------------------
        public void BlindTable(LuaTable lua_table)
        {
            LuaOnSocketClose = lua_table.Get<Action>("OnSocketClose");

            var lua_rpc = CasinosContext.Instance.LuaMgr.LuaEnv.Global.Get<LuaTable>("RPC");
            LuaOnRpcMethod = lua_rpc.Get<Action<ushort, byte[]>>("OnRpcMethod");
        }

        //---------------------------------------------------------------------
        public void Connect(string ip, int port)
        {
            RpcSession.close();
            RpcSession.OnSocketConnected = _onSocketConnected;
            RpcSession.OnSocketClosed = _onSocketClosed;
            RpcSession.OnSocketError = _onSocketError;
            RpcSession.connect(ip, port);
        }

        //---------------------------------------------------------------------
        public void Disconnect()
        {
            RpcSession.close();
        }

        //---------------------------------------------------------------------
        void _onSocketConnected(object client, EventArgs args)
        {
        }

        //---------------------------------------------------------------------
        void _onSocketClosed(object client, EventArgs args)
        {
            _onSocketClose();
        }

        //---------------------------------------------------------------------
        void _onSocketError(object rec, SocketErrorEventArgs args)
        {
            if (args != null && args.Exception != null)
            {
                BuglyAgent.PrintLog(LogSeverity.Log, args.Exception.Message);
                BuglyAgent.ReportException(args.Exception, args.Exception.Message);
            }

            _onSocketClose();
        }

        //---------------------------------------------------------------------
        void _onSocketClose()
        {
            LuaOnSocketClose();
        }
    }
}