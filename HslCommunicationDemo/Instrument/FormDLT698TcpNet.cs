﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using HslCommunication;
using HslCommunication.Instrument.DLT;
using System.Threading;
using System.IO.Ports;
using System.Xml.Linq;

namespace HslCommunicationDemo
{
    public partial class FormDLT698TcpNet : HslFormContent
    {
        public FormDLT698TcpNet( )
        {
            InitializeComponent( );
        }

        private DLT698TcpNet dLT698 = null;

        private void FormSiemens_Load( object sender, EventArgs e )
        {
            panel2.Enabled = false;
            Language( Program.Language );
        }


        private void Language( int language )
        {
            if (language == 2)
            {
                Text = "DLT698 Read Demo";

                label1.Text = "Com:";
                label3.Text = "baudRate:";
                label_address.Text = "station";
                button1.Text = "Connect";
                button2.Text = "Disconnect";
                button3.Text = "Active";

                label11.Text = "Address:";
                label12.Text = "length:";
                button25.Text = "Bulk Read";
                textBox_password.Text = "Pwd:";
                textBox_op_code.Text = "Op Code:";
                label13.Text = "Results:";
                label16.Text = "Message:";
                label14.Text = "Results:";
                button26.Text = "Read";

                groupBox3.Text = "Bulk Read test";
                groupBox4.Text = "Message reading test, hex string needs to be filled in";
                groupBox5.Text = "Special function test";
            }
        }

        private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
        {

        }
        

        #region Connect And Close



        private void button1_Click( object sender, EventArgs e )
        {
            if(!int.TryParse(textBox_port.Text,out int port ))
            {
                MessageBox.Show( DemoUtils.PortInputWrong );
                return;
            }

            dLT698?.ConnectClose( );
            dLT698 = new DLT698TcpNet( textBox_ip.Text, port, textBox_station.Text);
            dLT698.LogNet = LogNet;

            try
            {
                OperateResult connect = dLT698.ConnectServer( );
                if (connect.IsSuccess)
                {
                    MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );
                    button2.Enabled = true;
                    button1.Enabled = false;
                    panel2.Enabled = true;

                    userControlReadWriteOp1.SetReadWriteNet( dLT698, "00-00-00-00", true );
                }
                else
                {
                    MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed + connect.Message + Environment.NewLine +
                        "Error: " + connect.ErrorCode );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void button2_Click( object sender, EventArgs e )
        {
            // 断开连接
            dLT698.ConnectClose( );
            button2.Enabled = false;
            button1.Enabled = true;
            panel2.Enabled = false;
        }
        
        #endregion

        #region 批量读取测试

        private void button25_Click( object sender, EventArgs e )
        {
            DemoUtils.BulkReadRenderResult( dLT698, textBox6, textBox9, textBox10 );
        }

        #endregion

        #region 报文读取测试


        private void button26_Click( object sender, EventArgs e )
        {
            OperateResult<byte[]> read = dLT698.ReadByApdu( HslCommunication.BasicFramework.SoftBasic.HexStringToBytes( textBox13.Text ) );
            if (read.IsSuccess)
            {
                textBox11.Text = "Result：" + HslCommunication.BasicFramework.SoftBasic.ByteToHexString( read.Content );
            }
            else
            {
                MessageBox.Show( "Read Failed：" + read.ToMessageShowString( ) );
            }
        }


        #endregion

        private async void button3_Click( object sender, EventArgs e )
        {
            OperateResult active = await dLT698.ActiveDeveiceAsync( );
            if (active.IsSuccess)
            {
                MessageBox.Show( "Send Active Code Success" );
            }
            else
            {
                MessageBox.Show( "Active Code failed:" + active.Message );
            }
        }

        private async void button4_Click( object sender, EventArgs e )
        {
            OperateResult<string> read = await dLT698.ReadAddressAsync( );
            if (read.IsSuccess)
            {
                textBox_station.Text = read.Content;
                textBox12.Text = $"[{DateTime.Now:HH:mm:ss}] Address:{read.Content}";
            }
            else
            {
                MessageBox.Show( "Read failed: " + read.Message );
            }
        }

        private void button6_Click( object sender, EventArgs e )
        {
            // 广播当前时间
           // OperateResult read = await dLT698.BroadcastTimeAsync( DateTime.Now );
            //if (read.IsSuccess)
            //{
            //    textBox12.Text = $"[{DateTime.Now:HH:mm:ss}] BroadcastTime Success";
            //}
            //else
            //{
            //    MessageBox.Show( "Read failed: " + read.Message );
            //}
        }

        private async void button5_Click( object sender, EventArgs e )
        {
            // 写通信地址
            OperateResult read = await dLT698.WriteAddressAsync( textBox1.Text );
            if (read.IsSuccess)
            {
                textBox12.Text = $"[{DateTime.Now:HH:mm:ss}] Write Success";
            }
            else
            {
                MessageBox.Show( "Read failed: " + read.Message );
            }
        }


        public override void SaveXmlParameter( XElement element )
        {
            element.SetAttributeValue( DemoDeviceList.XmlBaudRate, textBox_port.Text );
            element.SetAttributeValue( DemoDeviceList.XmlStation, textBox_station.Text );
        }

        public override void LoadXmlParameter( XElement element )
        {
            base.LoadXmlParameter( element );
            textBox_port.Text = element.Attribute( DemoDeviceList.XmlBaudRate ).Value;
            textBox_station.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;
        }

        private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
        {
            userControlHead1_SaveConnectEvent( sender, e );
        }
    }
}
