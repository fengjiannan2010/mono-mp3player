using System;
using Gtk;
using GtkSharp;


namespace Mp3
{
	public partial class PreferencesWindow : Window
	{	
		public PreferencesWindow() : base ("Preferences")
		{
			this.Build();
		}

		protected virtual void OnButton22Activated (object sender, System.EventArgs e)
		{
			this.Dispose();
		}
	}
}

