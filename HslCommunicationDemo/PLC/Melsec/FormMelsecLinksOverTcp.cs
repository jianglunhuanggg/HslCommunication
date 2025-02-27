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
using HslCommunication.Profinet.Melsec;
using HslCommunication;
using System.Xml.Linq;

namespace HslCommunicationDemo
{
	public partial class FormMelsecLinksOverTcp : HslFormContent
	{
		public FormMelsecLinksOverTcp( )
		{
			InitializeComponent( );
			melsec = new MelsecFxLinksOverTcp( );
		}


		private MelsecFxLinksOverTcp melsec = null;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;

			comboBox_format.SelectedIndex = 0;
			Language( Program.Language );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "Melsec Read PLC Demo";

				label27.Text = "Ip:";
				label26.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
				button3.Text = "Start Plc";
				button4.Text = "Stop Plc";
				label21.Text = "Station:";

				label11.Text = "Address:";
				label12.Text = "length:";
				button25.Text = "Bulk Read";
				label13.Text = "Results:";
				label16.Text = "Message:";
				label14.Text = "Results:";
				button26.Text = "Read";


				groupBox3.Text = "Bulk Read test";
				groupBox4.Text = "Message reading test, hex string needs to be filled in";
				groupBox5.Text = "Special function test";

				button3.Text = "Start";
				button4.Text = "Stop";
				button5.Text = "PLC Type";
				userControlHead1.ProtocolInfo = "fxlinks over tcp";
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

			melsec?.ConnectClose( );
			melsec = new MelsecFxLinksOverTcp( );
			melsec.IpAddress = textBox1.Text;
			melsec.Port = port;
			melsec.LogNet = LogNet;

			try
			{
				melsec.Station = byte.Parse( textBox15.Text );
				melsec.WaittingTime = byte.Parse( textBox18.Text );
				melsec.SumCheck = checkBox1.Checked;
				melsec.Format = int.Parse( comboBox_format.SelectedItem.ToString( ) );

				OperateResult connect = melsec.ConnectServer( );
				if (connect.IsSuccess)
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );
					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;

					userControlReadWriteOp1.SetReadWriteNet( melsec, "D100", true );
				}
				else
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed );
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
			melsec.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}

		

		#endregion

		#region 批量读取测试

		private void button25_Click( object sender, EventArgs e )
		{
			DemoUtils.BulkReadRenderResult( melsec, textBox6, textBox9, textBox10 );
		}



		#endregion

		#region 报文读取测试


		private void button26_Click( object sender, EventArgs e )
		{
			OperateResult<byte[]> read = melsec.ReadFromCoreServer( HslCommunication.BasicFramework.SoftBasic.HexStringToBytes( textBox13.Text ) );
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
		
		#region 测试使用

		private void test1()
		{
			OperateResult<bool[]> read = melsec.ReadBool( "M100", 10 );
			if(read.IsSuccess)
			{
				bool m100 = read.Content[0];
				// and so on
				bool m109 = read.Content[9];
			}
			else
			{
				// failed
			}
		}
		

		private void test3( )
		{
			short d100_short = melsec.ReadInt16( "D100" ).Content;
			ushort d100_ushort = melsec.ReadUInt16( "D100" ).Content;
			int d100_int = melsec.ReadInt32( "D100" ).Content;
			uint d100_uint = melsec.ReadUInt32( "D100" ).Content;
			long d100_long = melsec.ReadInt64( "D100" ).Content;
			ulong d100_ulong = melsec.ReadUInt64( "D100" ).Content;
			float d100_float = melsec.ReadFloat( "D100" ).Content;
			double d100_double = melsec.ReadDouble( "D100" ).Content;
			// need to specify the text length
			string d100_string = melsec.ReadString( "D100", 10 ).Content;
		}
		private void test4( )
		{
			melsec.Write( "D100", (short)5 );
			melsec.Write( "D100", (ushort)5 );
			melsec.Write( "D100", 5 );
			melsec.Write( "D100", (uint)5 );
			melsec.Write( "D100", (long)5 );
			melsec.Write( "D100", (ulong)5 );
			melsec.Write( "D100", 5f );
			melsec.Write( "D100", 5d );
			// length should Multiples of 2 
			melsec.Write( "D100", "12345678" );
		}


		private void test5( )
		{
			OperateResult<byte[]> read = melsec.Read( "D100", 10 );
			if(read.IsSuccess)
			{
				int count = melsec.ByteTransform.TransInt32( read.Content, 0 );
				float temp = melsec.ByteTransform.TransSingle( read.Content, 4 );
				short name1 = melsec.ByteTransform.TransInt16( read.Content, 8 );
				string barcode = Encoding.ASCII.GetString( read.Content, 10, 10 );
			}
		}

		private void test6( )
		{
			OperateResult<UserType> read = melsec.ReadCustomer<UserType>( "D100" );
			if (read.IsSuccess)
			{
				UserType value = read.Content;
			}
			// write value
			melsec.WriteCustomer( "D100", new UserType( ) );

			melsec.LogNet = new HslCommunication.LogNet.LogNetSingle( Application.StartupPath + "\\Logs.txt" );

		}

		// private MelsecMcAsciiNet melsec_ascii_net = null;

		#endregion

		#region 压力测试

		private int thread_status = 0;
		private int failed = 0;
		private DateTime thread_time_start = DateTime.Now;
		// 压力测试，开3个线程，每个线程进行读写操作，看使用时间
		private void button3_Click( object sender, EventArgs e )
		{
			thread_status = 3;
			failed = 0;
			thread_time_start = DateTime.Now;
			new Thread( new ThreadStart( thread_test2 ) ) { IsBackground = true, }.Start( );
			new Thread( new ThreadStart( thread_test2 ) ) { IsBackground = true, }.Start( );
			new Thread( new ThreadStart( thread_test2 ) ) { IsBackground = true, }.Start( );
			button3.Enabled = false;
		}

		private void thread_test2( )
		{
			int count = 500;
			while (count > 0)
			{
				if (!melsec.Write( "D100", (short)1234 ).IsSuccess) failed++;
				if (!melsec.ReadInt16( "D100" ).IsSuccess) failed++;
				count--;
			}
			thread_end( );
		}

		private void thread_end( )
		{
			if (Interlocked.Decrement( ref thread_status ) == 0)
			{
				// 执行完成
				Invoke( new Action( ( ) =>
				{
					button3.Enabled = true;
					MessageBox.Show( "Spend：" + (DateTime.Now - thread_time_start).TotalSeconds + Environment.NewLine + " Failed Count：" + failed );
				} ) );
			}
		}




		#endregion

		private void button3_Click_1( object sender, EventArgs e )
		{
			OperateResult operate = melsec.StartPLC( );
			if(!operate.IsSuccess)
			{
				MessageBox.Show( "Start Failed：" + operate.Message );
			}
			else
			{
				MessageBox.Show( "Start Success" );
			}
		}

		private void button4_Click( object sender, EventArgs e )
		{

			OperateResult operate = melsec.StopPLC( );
			if (!operate.IsSuccess)
			{
				MessageBox.Show( "Stop Failed：" + operate.Message );
			}
			else
			{
				MessageBox.Show( "Stop Success" );
			}
		}

		private void button5_Click( object sender, EventArgs e )
		{
			OperateResult<string> read = melsec.ReadPlcType( );
			if (read.IsSuccess)
			{
				textBox14.Text = read.Content;
			}
			else
			{
				MessageBox.Show( "Read PLC Type failed:" + read.ToMessageShowString( ) );
			}
		}

		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox1.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );
			element.SetAttributeValue( DemoDeviceList.XmlStation, textBox15.Text );
			element.SetAttributeValue( DemoDeviceList.XmlTimeout, textBox18.Text );
			element.SetAttributeValue( DemoDeviceList.XmlSumCheck, checkBox1.Checked );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox1.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
			textBox15.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;
			textBox18.Text = element.Attribute( DemoDeviceList.XmlTimeout ).Value;
			checkBox1.Checked = bool.Parse( element.Attribute( DemoDeviceList.XmlSumCheck ).Value );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}

	}
}
