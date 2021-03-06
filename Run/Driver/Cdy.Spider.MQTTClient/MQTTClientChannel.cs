﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/8/6 15:03:55.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;

namespace Cdy.Spider.MQTTClient
{
    /// <summary>
    /// 
    /// </summary>
    public class MQTTClientChannel: ChannelBase
    {

        #region ... Variables  ...

        private MQTTChannelData mData;

        private IManagedMqttClient mqttClient;

        private MqttFactory mqttFactory;

        private MqttClientOptions options;

        private string mResTopic;

        private byte[] mResDatas;

        private ManualResetEvent eventreset = new ManualResetEvent(false);

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        public MQTTClientChannel()
        {
            mqttFactory = new MqttFactory();
        }
        #endregion ...Constructor...

        #region ... Properties ...


        /// <summary>
        /// 
        /// </summary>
        public override ChannelData Data { get => mData;}

        /// <summary>
        /// 
        /// </summary>
        public override string TypeName => "MQTTClient";


        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            base.Init();
            var tlsOptions = new MqttClientTlsOptions
            {
                UseTls = false,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                AllowUntrustedCertificates = true
            };

            options = new MqttClientOptions
            {
                ClientId = Guid.NewGuid().ToString(),
                ProtocolVersion = MqttProtocolVersion.V311,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = mData.ServerIp,
                    Port = mData.Port,
                    TlsOptions = tlsOptions
                }
            };

            options.Credentials = new MqttClientCredentials
            {
                Username = mData.UserName,
                Password = Encoding.UTF8.GetBytes(mData.Password)
            };

            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromMilliseconds(mData.Timeout);

            mqttClient = mqttFactory.CreateManagedMqttClient();
            mqttClient.UseApplicationMessageReceivedHandler(HandleReceivedApplicationMessage);
            mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
        }


        /// <summary>
        /// Handles the publisher connected event.
        /// </summary>
        /// <param name="x">The MQTT client connected event args.</param>
        private void OnConnected(MqttClientConnectedEventArgs x)
        {
            ConnectedChanged(true);
        }

        /// <summary>
        /// Handles the publisher disconnected event.
        /// </summary>
        /// <param name="x">The MQTT client disconnected event args.</param>
        private void OnDisconnected(MqttClientDisconnectedEventArgs x)
        {
            ConnectedChanged(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        private void HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs x)
        {
            if (x.ApplicationMessage.Payload != null && x.ApplicationMessage.Payload.Length > 0 && !string.IsNullOrEmpty(x.ApplicationMessage.Topic))
            {
                if (x.ApplicationMessage.Topic.Equals(mResTopic, StringComparison.OrdinalIgnoreCase))
                {
                    mResDatas = x.ApplicationMessage.Payload;
                    eventreset.Set();
                }
                else
                {
                    var vtop = x.ApplicationMessage.Topic;

                    if(vtop.EndsWith(mData.ResponseTopicAppendString))
                    {
                        return;
                    }

                    var vss = string.IsNullOrEmpty(mData.ServerTopicAppendString) ? vtop : vtop.Replace(mData.ServerTopicAppendString, "");

                    vss = string.IsNullOrEmpty(mData.TopicHeadString) ? vss : vss.Replace(mData.TopicHeadString, "");


                    var res = this.OnReceiveCallBack(vss, x.ApplicationMessage.Payload);
                    if (!string.IsNullOrEmpty(x.ApplicationMessage.ResponseTopic) && res != null)
                    {
                        SendToTopicDataWithoutResponse(x.ApplicationMessage.ResponseTopic, res);
                    }
                    else if (res != null)
                    {
                        SendToTopicDataWithoutResponse(vtop + mData.ResponseTopicAppendString, res);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceInfos"></param>
        public override void Prepare(List<string> deviceInfos)
        {
            base.Prepare(deviceInfos);
            foreach (var vv in deviceInfos)
            {
                //将本机认值服务器角色，下位设备认作客户端角色
                //订购服务器端主题
                this.mqttClient.SubscribeAsync(mData.TopicHeadString +  vv + mData.ServerTopicAppendString);

                //订购客户端回复主题
                this.mqttClient.SubscribeAsync(mData.TopicHeadString + vv + mData.ClientTopicAppendString + mData.ResponseTopicAppendString);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool InnerOpen()
        {
            this.mqttClient.StartAsync(new ManagedMqttClientOptions
            {
                ClientOptions = options
            });
            return base.InnerOpen();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool InnerClose()
        {
            this.mqttClient.StopAsync();
            return base.InnerClose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="data"></param>
        private void SendToTopicData(string topic, string responeTopic, byte[] data)
        {
            var msg = new MqttApplicationMessageBuilder().WithTopic(topic).WithResponseTopic(responeTopic).WithPayload(data).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag().Build();
            this.mqttClient.PublishAsync(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="data"></param>
        private void SendToTopicDataWithoutResponse(string topic, byte[] data)
        {
            var msg = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(data).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag().Build();
            this.mqttClient.PublishAsync(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override byte[] SendInner(string key, byte[] data, int timeout, out bool result, params string[] paras)
        {
            string ss = string.IsNullOrEmpty(key) ? this.Data.Name : key;
            string skey = mData.TopicHeadString + ss + mData.ClientTopicAppendString;
            string reskey = paras.Length > 0 ? paras[0] : skey + mData.ResponseTopicAppendString;
            mResTopic = reskey;

            eventreset.Reset();
            SendToTopicData(skey, reskey, data);
            result = eventreset.WaitOne(timeout);

            if (result)
            {
                return mResDatas;
            }
            else
            {
                result = false;
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="result"></param>
        protected override void SendInnerAsync(string key, byte[] data, out bool result, params string[] paras)
        {
            string ss = string.IsNullOrEmpty(key) ? this.Data.Name : key;
            string skey = mData.TopicHeadString + ss + mData.ClientTopicAppendString;
            string reskey = paras.Length > 0 ? paras[0] : ss + mData.ResponseTopicAppendString;
            mResTopic = reskey;
            SendToTopicData(skey, reskey, data);
            base.SendInnerAsync(key, data, out result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void Load(XElement xe)
        {
            mData = new MQTTChannelData();
            mData.LoadFromXML(xe);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ICommChannel NewApi()
        {
            return new MQTTClientChannel();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
