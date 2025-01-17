﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.34209
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MsgPack;
using NetMQ;
using NetMQ.Sockets;
using NLog;

namespace Cosmos.Rpc
{
    public class RpcServer : BaseNetMqServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IRpcService _rpcService;

        struct RpcServiceFuncInfo
        {
            public MethodInfo Method;
            public Type ArgType;
            public Type ReturnType;
        }

        private Dictionary<Type, RpcServiceFuncInfo> _serviceFuncs = new Dictionary<Type, RpcServiceFuncInfo>();

        public RpcServer(IRpcService rpcService, string host = "*", int responsePort = -1) : base(responsePort, 0, host)
        {
            _rpcService = rpcService;

            foreach (var methodInfo in rpcService.GetType().GetMethods())
            {
                foreach (var attr in methodInfo.GetCustomAttributes(typeof(ServiceFuncAttribute)))
                {
                    var funcAttr = (ServiceFuncAttribute)attr;
                    var args = methodInfo.GetParameters();
                    var requestType = args[0].ParameterType;
                    var retType = methodInfo.ReturnType;

                    var info = new RpcServiceFuncInfo()
                    {
                        ArgType = requestType,
                        ReturnType = retType,
                        Method = methodInfo,
                    };
                    _serviceFuncs[requestType] = info;
                    Logger.Info("Register Service Func, RequestType: {0}, ResponseType: {1}", requestType, info.ReturnType);

                }
            }

            if (_serviceFuncs.Count <= 0)
            {
                Logger.Error("RpcServcice(Type:{0}), has no funcs mark with ServiceFuncAttribute", rpcService.GetType());
            }
        }

        protected override async Task<byte[]> ProcessRequest(byte[] reqData)
        {
            var t = new TaskCompletionSource<byte[]>();
            var requestMsg = MsgPackTool.GetMsg<RequestMsg>(reqData);
            var requestType = requestMsg.RequestType;
            var requestObj = MsgPackTool.GetMsg(requestType, requestMsg.RequestObjectData);
            var resMsg = new ResponseMsg();
            resMsg.IsError = false;

            RpcServiceFuncInfo funcInfo;
            if (!_serviceFuncs.TryGetValue(requestType, out funcInfo))
            {
                Logger.Error("Not found RequestType func: {0}", requestType);
                return await t.Task;
            }
            var info = _serviceFuncs[requestType];
            var method = info.Method;
            byte[] executeResult = null;

            if (method != null)
            {
                try
                {
                    var result = method.Invoke(_rpcService, new[] { requestObj });
                    if (result != null)
                        executeResult = MsgPackTool.GetBytes(method.ReturnType, result);
                }
                catch (Exception e)
                {
                    resMsg.IsError = true;
                    resMsg.ErrorMessage = string.Format("[ERROR]Method '{0}' Exception: {1}", requestObj, e);
                    Logger.Error(resMsg.ErrorMessage);
                }
            }
            else
            {
                resMsg.IsError = true;
                resMsg.ErrorMessage = string.Format("[ERROR]Not found method: {0}", requestObj);
                Logger.Error(resMsg.ErrorMessage);
            }

            resMsg.Data = executeResult;
            t.SetResult(MsgPackTool.GetBytes(resMsg));

            return await t.Task;
        }

    }
    /// <summary>
    /// Any call RPC Fucntion must in this class
    /// </summary>
    public interface IRpcService
    {
    }

    /// <summary>
    /// 使用ZeroMQ进行RPC
    /// </summary>
    //public class RpcServer : IDisposable
    //{
    //    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    //    public Poller Poller;
    //    private Task _pollerTask;

    //    internal NetMQContext _context;
    //    private ResponseSocket _server;
    //    public int requestPort { get; private set; }
    //    public string Host { get; private set; }

    //    object RpcInstace;

    //    public RpcServer(rpcService rpcInstance, string host = "0.0.0.0")
    //    {
    //        RpcInstace = rpcInstance;
    //        Poller = new Poller();
    //        Host = host;

    //        _context = NetMQContext.Create();
    //        _server = _context.CreateResponseSocket();

    //        Poller.AddSocket(_server);

    //        requestPort = _server.BindRandomPort("tcp://" + host);
    //        _server.ReceiveReady += OnReceiveReady;

    //        _pollerTask = Task.Run(() =>
    //        {
    //            Poller.Start();
    //        });
    //    }

    //    private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
    //    {
    //        var data = _server.Receive();
    //        var req = RpcShare.RequestSerializer.UnpackSingleObject(data);

    //        ProcessRequest(req);
    //    }

    //    async void ProcessRequest(RequestMsg requestMsg)
    //    {
    //        var method = RpcInstace.GetType().GetMethod(requestMsg.FuncName);
    //        object executeResult = null;

    //        if (method != null)
    //        {
    //            var arguments = new object[requestMsg.Arguments.Length];
    //            for (var i = 0; i < arguments.Length; i++) // MsgPack.MessagePackObject arg in requestProto.Arguments)
    //            {
    //                MsgPack.MessagePackObject arg = (MsgPack.MessagePackObject)requestMsg.Arguments[i];
    //                arguments[i] = arg.ToObject();
    //            }
    //            var result = method.Invoke(RpcInstace, arguments);

    //            if (result is Task)
    //            {
    //                executeResult = await (result as Task<object>);
    //            }
    //            else
    //            {
    //                executeResult = result;
    //            }

    //        }
    //        else
    //        {
    //            Logger.Error("[ERROR]Not found method: {0}", requestMsg.FuncName);
    //            Thread.Sleep(1);
    //        }

    //        var data = RpcShare.ResponseSerializer.PackSingleObject(new ResponseMsg {
    //            RequestId = requestMsg.RequestId,
    //            Data = executeResult,
    //        });
    //        _server.Send(data);
    //    }


    //    public void Dispose()
    //    {
    //        Poller.RemoveSocket(_server);
    //        _server.Close();
    //        _context.Dispose();

    //        Poller.Stop();
    //        Poller.Dispose();
    //        _pollerTask.Dispose();
    //    }
    //}
}

