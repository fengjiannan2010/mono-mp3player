using System;
using System.Collections;
using System.IO;
using System.Text;
using Gtk;
using Gnome;


namespace Mp3
{
	class MainClass
	{
		public static void Main (string[] args)
		{			
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
			
		}
	}
}
