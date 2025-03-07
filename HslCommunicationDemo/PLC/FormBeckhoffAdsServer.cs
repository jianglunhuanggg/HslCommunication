﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet.Beckhoff;
using HslCommunication;
using HslCommunication.ModBus;
using System.Threading;
using System.Xml.Linq;

namespace HslCommunicationDemo
{
    public partial class FormBeckhoffAdsServer : HslFormContent
    {
        public FormBeckhoffAdsServer( )
        {
            InitializeComponent( );
        }

        private void FormSiemens_Load( object sender, EventArgs e )
        {
            panel2.Enabled = false;

            if(Program.Language == 2)
            {
                Text = "Beckhoff ADS Virtual Server [data support, bool: M, I, Q]";
                label3.Text = "port:";
                button1.Text = "Start Server";
                button11.Text = "Close Server";
                label11.Text = "This server is not a strict mc protocol and only supports perfect communication with HSL components.";
            }
        }
        
        private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
        {
            adsServer?.ServerClose( );
        }

        #region Server Start

        private BeckhoffAdsServer adsServer;

        private void button1_Click( object sender, EventArgs e )
        {
            if (!int.TryParse( textBox2.Text, out int port ))
            {
                MessageBox.Show( DemoUtils.PortInputWrong );
                return;
            }

            try
            {
                adsServer = new BeckhoffAdsServer( );                       // 实例化对象
                adsServer.ActiveTimeSpan = TimeSpan.FromHours( 1 );         // 如果客户端1个小时不通信，就关闭连接
                adsServer.OnDataReceived += MelsecMcServer_OnDataReceived;
                // 添加几个符号数据
                adsServer.AddTagValue( "MAIN.a", (short)0 );
                adsServer.AddTagValue( "MAIN.cc", 0 );
                adsServer.AddTagValue( "MAIN.dd", 0f );
                adsServer.AddTagValue( "MAIN.ee", new bool[11] );
                adsServer.AddTagValue( "MAIN.ff", new short[3] );

                adsServer.ServerStart( port );
                userControlReadWriteServer1.SetReadWriteServer( adsServer, "M100" );

                button1.Enabled = false;
                panel2.Enabled = true;
                button11.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void button11_Click( object sender, EventArgs e )
        {
            // 停止服务
            adsServer?.ServerClose( );
            button1.Enabled = true;
            button11.Enabled = false;
        }

        private void MelsecMcServer_OnDataReceived( object sender,  object source, byte[] receive )
        {
            // 我们可以捕获到接收到的客户端的modbus报文
            // 如果是TCP接收的
            if (source is HslCommunication.Core.Net.AppSession session)
            {
                // 获取当前客户的IP地址
                string ip = session.IpAddress;
            }

            // 如果是串口接收的
            if (source is System.IO.Ports.SerialPort serialPort)
            {
                // 获取当前的串口的名称
                string portName = serialPort.PortName;
            }
        }

        #endregion


        public override void SaveXmlParameter( XElement element )
        {
            element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );
        }

        public override void LoadXmlParameter( XElement element )
        {
            base.LoadXmlParameter( element );
            textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
        }

        private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
        {
            userControlHead1_SaveConnectEvent( sender, e );
        }
    }
}
