using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DNSController
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}
		private bool connect = false;
		private void btnConnect_Click(object sender, EventArgs e)
		{
			panel1.Enabled = false;
			if (connect)
			{
				// کد قطع کردن اتصال
				Disconnect();
				btnConnect.Text = "اتصال";
				lbl_connection_status.Text = "قطع است.";
				lbl_connection_status.ForeColor = Color.Red;
				connect = false;
			}
			else
			{
				// کد وصل کردن اتصال
				Connect();
				btnConnect.Text = "قطع";
				lbl_connection_status.Text = "وصل است.";
				lbl_connection_status.ForeColor = Color.MediumSeaGreen;
				connect = true;
			}
			panel1.Enabled = true;
		}
		private void Connect()
		{
			string networkInterfaceName = GetNetworkInterfaceName("Wi-Fi");
			if (string.IsNullOrEmpty(networkInterfaceName))
			{
				MessageBox.Show("شبکه Wi-Fi پیدا نشد!", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				SetDNS(networkInterfaceName, "185.51.200.2", "178.22.122.100");
				MessageBox.Show("DNS با موفقیت تنظیم شد!", "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("خطا در تنظیم DNS: " + ex.Message, "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void Disconnect()
		{
			string networkInterfaceName = GetNetworkInterfaceName("Wi-Fi");
			if (string.IsNullOrEmpty(networkInterfaceName))
			{
				MessageBox.Show("شبکه Wi-Fi پیدا نشد!", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				ResetDNS(networkInterfaceName);
				MessageBox.Show("DNS به حالت پیش‌فرض بازگردانده شد!", "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("خطا در بازگرداندن DNS: " + ex.Message, "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		// متد برای یافتن نام کارت شبکه
		private string GetNetworkInterfaceName(string description)
		{
			var Networks = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface ni in Networks)
			{
				if (ni.OperationalStatus == OperationalStatus.Up && (ni.Description.Contains(description) || ni.Name.Contains(description)))
				{
					return ni.Id;
				}
			}
			return null;
		}
		// متد برای تنظیم DNS
		private void SetDNS(string networkInterfaceName, string primaryDns, string secondaryDns)
		{
			ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
			ManagementObjectCollection moc = mc.GetInstances();

			foreach (ManagementObject mo in moc)
			{
				if ((bool)mo["IPEnabled"] && mo["SettingID"].ToString() == networkInterfaceName)
				{
					ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
					newDNS["DNSServerSearchOrder"] = new string[] { primaryDns, secondaryDns };
					mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
				}
			}
		}

		// متد برای بازگرداندن DNS به حالت پیش‌فرض
		private void ResetDNS(string networkInterfaceName)
		{
			ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
			ManagementObjectCollection moc = mc.GetInstances();

			foreach (ManagementObject mo in moc)
			{
				if ((bool)mo["IPEnabled"] && mo["SettingID"].ToString() == networkInterfaceName)
				{
					ManagementBaseObject resetDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
					resetDNS["DNSServerSearchOrder"] = null; // بازگرداندن به DHCP
					mo.InvokeMethod("SetDNSServerSearchOrder", resetDNS, null);
				}
			}
		}
	}
}
