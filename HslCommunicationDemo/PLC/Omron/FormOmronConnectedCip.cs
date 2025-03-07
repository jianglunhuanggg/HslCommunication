﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using System.Threading;
using HslCommunication.Profinet.Omron;
using HslCommunication;
using HslCommunication.Profinet.AllenBradley;
using System.Xml.Linq;

namespace HslCommunicationDemo
{
	public partial class FormOmronConnectedCip : HslFormContent
	{
		public FormOmronConnectedCip( )
		{
			InitializeComponent( );
			omronCipNet = new OmronConnectedCipNet( "192.168.0.110" );
		}


		private OmronConnectedCipNet omronCipNet = null;


		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;

			Language( Program.Language );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "Omron Read PLC Demo";
				label1.Text = "Ip:";
				label3.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
				label21.Text = "Address:";

				label11.Text = "Address:";
				button25.Text = "Bulk Read";
				label13.Text = "Results:";
				label16.Text = "Message:";
				label14.Text = "Results:";
				button26.Text = "Read";
				button3.Text = "Build";

				groupBox3.Text = "Bulk Read test";
				groupBox4.Text = "CIP reading test, hex string needs to be filled in";
				groupBox5.Text = "Special function test";
				label22.Text = "plc tag name";
				button4.Text = "Write";
				button5.Text = "PLC Model";
			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}

		#region Connect And Close


		private void button1_Click( object sender, EventArgs e )
		{
			if (!int.TryParse( textBox2.Text, out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}

			omronCipNet.IpAddress = textBox1.Text;
			omronCipNet.Port = port;
			omronCipNet.LogNet = LogNet;
			omronCipNet.ConnectTimeOut = 5000;

			omronCipNet.SetPersistentConnection( );

			try
			{
				OperateResult connect = omronCipNet.ConnectServer( );
				if (connect.IsSuccess)
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );
					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;
					userControlReadWriteOp1.SetReadWriteNet( omronCipNet, "A1", true, 1 );
				}
				else
				{
					MessageBox.Show( StringResources.Language.ConnectedFailed + connect.ToMessageShowString( ) );
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
			OperateResult close = omronCipNet.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
			//if (!close.IsSuccess)
			//{
			//	MessageBox.Show( "关闭失败！" + close.Message );
			//}
		}







		#endregion

		#region 批量读取测试

		private void button25_Click( object sender, EventArgs e )
		{
			try
			{
				//    OperateResult write = allenBradleyNet.Write( "Array", new short[] { 101, 102, 103, 104, 105, 106 } );

				// OperateResult<short[]> readResult = allenBradleyNet.ReadInt16( "Array", 300 );

				OperateResult<byte[]> read = null;
				if (!textBox6.Text.Contains( ";" ))
				{
					//MessageBox.Show( HslCommunication.BasicFramework.SoftBasic.ByteToHexString( allenBradleyNet.BuildReadCommand( new string[] { textBox6.Text }, new int[] { int.Parse(textBox9.Text) } ).Content , ' ') );
					read = omronCipNet.Read( textBox6.Text, ushort.Parse( textBox9.Text ) );
				}
				else
				{
					//MessageBox.Show( HslCommunication.BasicFramework.SoftBasic.ByteToHexString( allenBradleyNet.BuildReadCommand( textBox6.Text.Split( ';' ) ).Content, ' ' ) );
					read = omronCipNet.Read( textBox6.Text.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries ),
						textBox9.Text.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).Select( m => ushort.Parse( m ) ).ToArray( ) );
				}

				if (read.IsSuccess)
				{
					textBox10.Text = "Result：" + HslCommunication.BasicFramework.SoftBasic.ByteToHexString( read.Content, ' ' );
				}
				else
				{
					MessageBox.Show( "Read failed：" + read.ToMessageShowString( ) );
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show( "Read failed：" + ex.Message );
			}
		}

		#endregion

		#region 报文读取测试


		private void button26_Click( object sender, EventArgs e )
		{
			//OperateResult<byte[]> read = omronCipNet.ReadCipFromServer( HslCommunication.BasicFramework.SoftBasic.HexStringToBytes( textBox13.Text ) );
			//if (read.IsSuccess)
			//{
			//	textBox11.Text = "Result：" + HslCommunication.BasicFramework.SoftBasic.ByteToHexString( read.Content );
			//}
			//else
			//{
			//	MessageBox.Show( "Read failed：" + read.ToMessageShowString( ) );
			//}
		}


		#endregion

		private void Button3_Click( object sender, EventArgs e )
		{
			//try
			//{
			//	//    OperateResult write = allenBradleyNet.Write( "Array", new short[] { 101, 102, 103, 104, 105, 106 } );

			//	// OperateResult<short[]> readResult = allenBradleyNet.ReadInt16( "Array", 300 );

			//	//MessageBox.Show( HslCommunication.BasicFramework.SoftBasic.ByteToHexString( allenBradleyNet.BuildReadCommand( new string[] { textBox6.Text }, new int[] { int.Parse(textBox9.Text) } ).Content , ' ') );
			//	byte[] read = AllenBradleyHelper.PackRequestReadSegment( textBox6.Text, ushort.Parse( textBox12.Text ), ushort.Parse( textBox9.Text ) );

			//	textBox10.Text = "Result：" + HslCommunication.BasicFramework.SoftBasic.ByteToHexString( read, ' ' );
			//}
			//catch (Exception ex)
			//{
			//	MessageBox.Show( "Build failed：" + ex.Message );
			//}
		}


		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox1.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox1.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}

		private void button4_Click( object sender, EventArgs e )
		{
			try
			{
				OperateResult write = omronCipNet.WriteTag(
					textBox3.Text,
					Convert.ToUInt16( textBox4.Text, 16 ),
					textBox5.Text.ToHexBytes( ),
					int.Parse( textBox7.Text ) );
				DemoUtils.WriteResultRender( write, textBox3.Text );
			}
			catch (Exception ex)
			{
				MessageBox.Show( "write failed：" + ex.Message );
			}
		}

		private void button6_Click( object sender, EventArgs e )
		{
			try
			{
				OperateResult<ushort, byte[]> read = omronCipNet.ReadTag( textBox3.Text, ushort.Parse( textBox7.Text ) );
				if (read.IsSuccess)
				{
					textBox4.Text = read.Content1.ToString( "X2" );
					textBox5.Text = read.Content2.ToHexString( ' ' );
				}
				else
				{
					MessageBox.Show( "read failed：" + read.Message );
				}
			}
			catch( Exception ex)
			{
				MessageBox.Show( "read failed：" + ex.Message );
			}
		}

		private void button5_Click( object sender, EventArgs e )
		{
			MessageBox.Show( omronCipNet.ProductName );
		}

	}
}
