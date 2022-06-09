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
using System.Threading;
using System.IO.Ports;
using HslCommunication.Profinet.AllenBradley;
using System.Xml.Linq;

namespace HslCommunicationDemo
{
    public partial class FormAllenBradleyDF1Serial : HslFormContent
    {
        public FormAllenBradleyDF1Serial( )
        {
            InitializeComponent( );
        }

        private AllenBradleyDF1Serial allenBradley = null;

        private void FormSiemens_Load( object sender, EventArgs e )
        {
            panel2.Enabled = false;
            comboBox3.DataSource = SerialPort.GetPortNames( );
            try
            {
                comboBox3.SelectedIndex = 0;
            }
            catch
            {
                comboBox3.Text = "COM3";
            }

            Language( Program.Language );
            comboBox1.SelectedIndex = 0;
        }


        private void Language( int language )
        {
            if (language == 2)
            {
                Text = "Delta DVP Read Demo";

                label1.Text = "Com:";
                label3.Text = "baudRate:";
                label22.Text = "DataBit";
                label23.Text = "StopBit";
                label24.Text = "parity";
                label21.Text = "station";
                button1.Text = "Connect";
                button2.Text = "Disconnect";
                label2.Text = "Target:";
                label4.Text = "Sender:";

                label11.Text = "Address:";
                label12.Text = "length:";
                button25.Text = "Bulk Read";
                label13.Text = "Results:";
                label16.Text = "Message:";
                label14.Text = "Results:";
                button26.Text = "Read";

                groupBox3.Text = "Bulk Read test";
                groupBox4.Text = "Message reading test, hex string needs to be filled in,without crc";
                groupBox5.Text = "Special function test";

                comboBox1.DataSource = new string[] { "None", "Odd", "Even" };
            }
        }

        private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
        {

        }
        

        #region Connect And Close

        private void button1_Click( object sender, EventArgs e )
        {
            if(!int.TryParse(textBox2.Text,out int baudRate ))
            {
                MessageBox.Show( DemoUtils.BaudRateInputWrong );
                return;
            }

            if (!int.TryParse( textBox16.Text, out int dataBits ))
            {
                MessageBox.Show( DemoUtils.DataBitsInputWrong );
                return;
            }

            if (!int.TryParse( textBox17.Text, out int stopBits ))
            {
                MessageBox.Show( DemoUtils.StopBitInputWrong );
                return;
            }


            if (!byte.TryParse(textBox15.Text,out byte station))
            {
                MessageBox.Show( "Station input wrong！" );
                return;
            }

            if (!byte.TryParse( textBox1.Text, out byte dst ))
            {
                MessageBox.Show( "DstNode input wrong！" );
                return;
            }

            if (!byte.TryParse( textBox3.Text, out byte src ))
            {
                MessageBox.Show( "SrcNode input wrong！" );
                return;
            }

            allenBradley?.Close( );
            allenBradley = new AllenBradleyDF1Serial( );
            allenBradley.Station = station;
            allenBradley.DstNode = dst;
            allenBradley.SrcNode = src;
            allenBradley.LogNet = LogNet;

            try
            {
                allenBradley.SerialPortInni( sp =>
                 {
                     sp.PortName = comboBox3.Text;
                     sp.BaudRate = baudRate;
                     sp.DataBits = dataBits;
                     sp.StopBits = stopBits == 0 ? StopBits.None : (stopBits == 1 ? StopBits.One : StopBits.Two);
                     sp.Parity = comboBox1.SelectedIndex == 0 ? Parity.None : (comboBox1.SelectedIndex == 1 ? Parity.Odd : Parity.Even);
                 } );
                allenBradley.RtsEnable = checkBox5.Checked;
                allenBradley.Open( );

                button2.Enabled = true;
                button1.Enabled = false;
                panel2.Enabled = true;

                userControlReadWriteOp1.SetReadWriteNet( allenBradley, "N7:0", false );
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void button2_Click( object sender, EventArgs e )
        {
            // 断开连接
            allenBradley.Close( );
            button2.Enabled = false;
            button1.Enabled = true;
            panel2.Enabled = false;
        }
        
        #endregion

        #region 批量读取测试

        private void button25_Click( object sender, EventArgs e )
        {
            DemoUtils.BulkReadRenderResult( allenBradley, textBox6, textBox9, textBox10 );
        }

        #endregion

        #region 报文读取测试


        private void button26_Click( object sender, EventArgs e )
        {
            OperateResult<byte[]> read = allenBradley.ReadFromCoreServer( HslCommunication.Serial.SoftCRC16.CRC16( HslCommunication.BasicFramework.SoftBasic.HexStringToBytes( textBox13.Text ) ) );
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

        public override void SaveXmlParameter( XElement element )
        {
            element.SetAttributeValue( DemoDeviceList.XmlCom, comboBox3.Text );
            element.SetAttributeValue( DemoDeviceList.XmlBaudRate, textBox2.Text );
            element.SetAttributeValue( DemoDeviceList.XmlDataBits, textBox16.Text );
            element.SetAttributeValue( DemoDeviceList.XmlStopBit, textBox17.Text );
            element.SetAttributeValue( DemoDeviceList.XmlParity, comboBox1.SelectedIndex );
            element.SetAttributeValue( DemoDeviceList.XmlStation, textBox15.Text );
            element.SetAttributeValue( DemoDeviceList.XmlRtsEnable, checkBox5.Checked );
            element.SetAttributeValue( DemoDeviceList.XmlTarget, textBox1.Text );
            element.SetAttributeValue( DemoDeviceList.XmlSender, textBox3.Text );
        }

        public override void LoadXmlParameter( XElement element )
        {
            base.LoadXmlParameter( element );
            comboBox3.Text = element.Attribute( DemoDeviceList.XmlCom ).Value;
            textBox2.Text = element.Attribute( DemoDeviceList.XmlBaudRate ).Value;
            textBox16.Text = element.Attribute( DemoDeviceList.XmlDataBits ).Value;
            textBox17.Text = element.Attribute( DemoDeviceList.XmlStopBit ).Value;
            comboBox1.SelectedIndex = int.Parse( element.Attribute( DemoDeviceList.XmlParity ).Value );
            textBox15.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;
            checkBox5.Checked = bool.Parse( element.Attribute( DemoDeviceList.XmlRtsEnable ).Value );
            textBox1.Text = element.Attribute( DemoDeviceList.XmlTarget ).Value;
            textBox3.Text = element.Attribute( DemoDeviceList.XmlSender ).Value;
        }

        private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
        {
            userControlHead1_SaveConnectEvent( sender, e );
        }
    }
}
