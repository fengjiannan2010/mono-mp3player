using System;
using Gtk;
using Mp3;
using System.Collections;

public partial class MainWindow: Gtk.Window
{
	//Main player and collection variables
	Collection mp3 = new Collection();
	ListStore songListStore;
	ArrayList songArrayList;
	
	//Queue variables
	Playlist queue = new Playlist();
	ListStore queueListStore;
	ArrayList queueArrayList;
		
	
	//Playlist variables
	ArrayList playlists  = new ArrayList();
	ListStore playListStore;
	ArrayList playListArrayList;
	
	string MusicPath = "/home/julian/music/Oasis";
	double volume = 1;

	public int Minutes=0;
	public int Seconds=0;
	public string endPart="";
	public uint timer;
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		SetupGUI();	
		setupQuelist();
		setupPlaylist();		
	}

	public void setupQuelist(){
		queueListStore = new Gtk.ListStore (typeof (string));
		queueArrayList = new ArrayList();		

		foreach (Song x in queue.SongsList){
			queueArrayList.Add(x);
			queueListStore.AppendValues(x.ToString());
		}
		TreeViewColumn queueColumnSongs = new TreeViewColumn();
		queueColumnSongs.Title = "Songs";	
		
		this.queueTreeView.AppendColumn(queueColumnSongs);
		
		this.queueTreeView.Model= queueListStore;
 
		CellRendererText queueCell = new CellRendererText ();
		queueColumnSongs.PackStart(queueCell, true);

  		queueColumnSongs.AddAttribute (queueCell, "text", 0);
	}
	
	
	public void setupPlaylist(){
		playListStore = new Gtk.ListStore (typeof (string));
		playListArrayList = new ArrayList();		

		foreach (Playlist x in playlists){
			playListArrayList.Add(x);
			playListStore.AppendValues(x.ToString());
		}
		TreeViewColumn playlistColumnSongs = new TreeViewColumn();
		playlistColumnSongs.Title = "Playlists";	

		this.playListTreeView.AppendColumn(playlistColumnSongs);

		this.playListTreeView.Model= playListStore;

		CellRendererText playlistCell = new CellRendererText ();
		playlistColumnSongs.PackStart(playlistCell, true);

		playlistColumnSongs.AddAttribute (playlistCell, "text", 0);	

	}
		
	
	
	
	public void SetupGUI(){			
		// Font Settings for the Song playing
		Pango.FontDescription heading = Pango.FontDescription.FromString ("Sans 10");		
		heading.Weight = Pango.Weight.Ultrabold;
		heading.Stretch = Pango.Stretch.ExtraExpanded;
		npSong.ModifyFont (heading);
		npSong.Layout.FontDescription = heading;
		
				
		// Default folder load music from
		mp3.createLibrary(MusicPath);	
		
		//Setup Searching GUI Elements
		this.searchEntry.Text = "";
		songListStore = new Gtk.ListStore (typeof (string));
		songArrayList = new ArrayList();		

		foreach (Song x in mp3.mp3Library.songList){
			songArrayList.Add(x);
			songListStore.AppendValues(x.ToString());
		}
		TreeViewColumn songs = new TreeViewColumn();
		songs.Title = "Songs";	
		
		this.songListTree.AppendColumn(songs);
		this.songListTree.Model= songListStore;
 
		CellRendererText songCell = new CellRendererText ();
		songs.PackStart(songCell, true);
		
		this.searchEntry.Changed += OnSearchEntryTextChanged;
  		songs.AddAttribute (songCell, "text", 0);

		bottomStatusBar.Push (1, mp3.mp3Library.songList.Count+ " songs found.");
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Gtk.Application.Quit ();
		a.RetVal = true;
	}

	private void OnSearchEntryTextChanged (object o, System.EventArgs args)
	{
		songListStore.Clear();
		songArrayList = new ArrayList();	
		
		foreach (Song x in mp3.FindSong(searchEntry.Text,searchSelectionType.ActiveText)){
			songArrayList.Add(x);
			songListStore.AppendValues(x.ToString());
		}
		
		if (songArrayList.Count == 0)
			bottomStatusBar.Push (1, songArrayList.Count+ " song found.");
		else
			bottomStatusBar.Push (1, songArrayList.Count+ " songs found.");

	}
	public void UpdateNowPlaying(){
		// if we're not playing anything
		Song temp = new Song();
		if (mp3.currentSong.Equals(temp)){
			npSong.Text = "Stopped";
			npByFrom.Text = "";
			return;
		}
		
		// we're playing something, but lets make sure it has a tag
		if (mp3.currentSong.hasTag){
			npSong.Text = mp3.currentSong.songName;
			npByFrom.Text = ("by " + mp3.currentSong.artistName+ " from " + mp3.currentSong.albumName);
		}
		else{
			npSong.Text = mp3.currentSong.fileName;
			npByFrom.Text = mp3.currentSong.fileName;
			
		}
	}

	protected virtual void OnPlayButtonClicked (object sender, System.EventArgs e)
	{
		// search mode
		if (tabbedNotebook.Page==0){
			TreePath[] paths = songListTree.Selection.GetSelectedRows();			
			if (paths.Length == 0)
				return;
			
	
			//start playing it
			mp3.PlaySong((Song)songArrayList.ToArray()[paths[0].Indices[0]]);
			
			// get the song from the Tree
			Song songToPlay =(Song)songArrayList.ToArray()[paths[0].Indices[0]];
			int playAfterTheseSongs = songArrayList.IndexOf(songToPlay);
	
			mp3.NewQueue();
			// queue all the songs from the selected song until the end of the list
			for (int i=playAfterTheseSongs;i< songArrayList.Count; i++){
				mp3.QueueSong((Song)songArrayList.ToArray()[i]);
			}
			UpdateNowPlaying();
			UpdateProgressBar();
		}
		// queue mode
		else if (tabbedNotebook.Page==1){
			TreePath[] paths = queueTreeView.Selection.GetSelectedRows();			
			if (paths.Length == 0)
				return;

			mp3.PlaySong((Song)queueArrayList.ToArray()[paths[0].Indices[0]]);	

			Song songToPlay =((Song)queueArrayList.ToArray()[paths[0].Indices[0]]);	
			int playAfterTheseSongs = queueArrayList.IndexOf(songToPlay);
			mp3.NewQueue();
			for (int i=playAfterTheseSongs;i< queueArrayList.Count; i++){
				Console.WriteLine(((Song)queueArrayList.ToArray()[i]).ToString());
				mp3.QueueSong((Song)queueArrayList.ToArray()[i]);
			}
			UpdateNowPlaying();
			UpdateProgressBar();
		}
		// playlist mode
		else if (tabbedNotebook.Page==2){
			TreePath[] paths = queueTreeView.Selection.GetSelectedRows();			
			if (paths.Length == 0)
				return;
			for (int p = 0; p < paths.Length; p++)
				mp3.PlaySong((Song)queueArrayList.ToArray()[paths[p].Indices[0]]);	
			UpdateNowPlaying();
			UpdateProgressBar();
		}
	}

	protected virtual void OnStopButtonClicked (object sender, System.EventArgs e)
	{
		mp3.StopSong();
		mp3.currentSong = new Song();
		npSong.Text = "Stopped";
		npByFrom.Text = "";
	}
	
	// TODO: Highlight the new song
	protected virtual void OnPreviousButtonClicked (object sender, System.EventArgs e)
	{
		mp3.PreviousSong();
		UpdateNowPlaying();
		UpdateProgressBar();
	}

	protected virtual void OnNextButtonClicked (object sender, System.EventArgs e)
	{
		mp3.NextSong();
		UpdateNowPlaying();
		UpdateProgressBar();
	}

	// TODO Add seeking
	protected virtual void OnRewindClicked (object sender, System.EventArgs e)
	{
		
	}
	// TODO Add seeking
	protected virtual void OnFastfowardClicked (object sender, System.EventArgs e)
	{

	}

	public void RefreshSearchList(){
		songListStore.Clear();
		songArrayList = new ArrayList();	

		foreach (Song x in mp3.FindSong(searchEntry.Text,searchSelectionType.ActiveText)){
			songArrayList.Add(x);
			songListStore.AppendValues(x.ToString());
		}

		if (songArrayList.Count == 0)
			bottomStatusBar.Push (1, songArrayList.Count+ " song found.");
		else
			bottomStatusBar.Push (1, songArrayList.Count+ " songs found.");
	}
	
	protected virtual void OnAddFolderActivated (object sender, System.EventArgs e)
	{
		// Create a new window for picking a file
		Window win = new Window("Add a new file");
		FileChooserDialog dialog = new FileChooserDialog(
			"Select a folder to add",
			win,
			FileChooserAction.SelectFolder,
			Stock.Cancel,
			ResponseType.Cancel,
			Stock.Open,
			ResponseType.Ok,
			null);

		dialog.SelectMultiple = true;
		dialog.DestroyWithParent = true;
		dialog.Modal = true;

		// if they hit OK then add each folder selected to the Library
		if((ResponseType)dialog.Run() == ResponseType.Ok) {
			foreach(string x in dialog.Filenames) {
				mp3.createLibrary(x);
			}
		}
		dialog.Destroy();
		win.Destroy();
		
		RefreshSearchList();
	}

	protected virtual void OnExit1Activated (object sender, System.EventArgs e)
	{
		Gtk.Application.Quit();
	}


	protected virtual void OnAbout3Activated (object sender, System.EventArgs e)
	{
		Window win = new Window("About");
		MessageDialog aboutMessage = new MessageDialog (win, DialogFlags.DestroyWithParent,
		                                                Gtk.MessageType.Info, ButtonsType.None,
		                                                "If you have any problems, please email me at\n jmoyse@umd.edu");
		aboutMessage.Show();
	}
	
	
	// Pop up currently does nothing
	protected virtual void OnNewCollectionActivated (object sender, System.EventArgs e)
	{		
		Window win = new Window("New Collection");
		MessageDialog aboutMessage = new Gtk.MessageDialog (  win, DialogFlags.DestroyWithParent,
		                                  Gtk.MessageType.Question, ButtonsType.OkCancel,
		                                  "Are you sure you want to create a new collection\nAny existing collection will be deleted.");
		aboutMessage.Show();		
	}
	
	// This is broken hardcore		
	protected virtual void OnPreferencesActivated (object sender, System.EventArgs e)
	{
		PreferencesWindow prefwin = new PreferencesWindow();
		prefwin.Show();
	}
			
	protected virtual void OnAddFileActivated (object sender, System.EventArgs e){
		Window win = new Window("Add a new mp3 file");
		FileChooserDialog dialog = new FileChooserDialog(
			"Select a file to add",
			win,
			FileChooserAction.Open,
			Stock.Cancel,
			ResponseType.Cancel,
			Stock.Open,
			ResponseType.Ok,
			null);

		dialog.SelectMultiple = true;
		dialog.DestroyWithParent = true;
		dialog.Modal = true;

		if((ResponseType)dialog.Run() == ResponseType.Ok) {
			foreach(string x in dialog.Filenames) {
				mp3.addSingleSong(x);
				
			}
		}
		dialog.Destroy();
		win.Destroy();
		
		RefreshSearchList();
			
	}

				
	public bool progress_timeout()
	{
		double new_val;

		if (songProgressBar.Fraction>.99)
			songProgressBar.Fraction = 0;
		new_val = songProgressBar.Fraction + 0.01;
		string minutesValue="";
		string secondValue="";
		if (Minutes<10)
			minutesValue = "0"+Minutes;
		else
			minutesValue = Minutes.ToString();
		if (Seconds<10)
			secondValue = "0"+Seconds;
		else
			secondValue = Seconds.ToString();
		songProgressBar.Text = minutesValue+":"+secondValue +" / "+endPart;
		Seconds++;
		if (Seconds ==59){
			Seconds = 0;
			Minutes++;
		}
		songProgressBar.Fraction = new_val;
		
		return true;
	}
 
	public void UpdateProgressBar(){
		string minutesValue="";
		string secondValue="";
		if (mp3.currentMinutes<10)
			minutesValue = "0"+mp3.currentMinutes;
		else
			minutesValue = Minutes.ToString();
		if (mp3.currentSeconds<10)
			secondValue = "0"+mp3.currentSeconds;
		else
			secondValue = Seconds.ToString();
		
		endPart = minutesValue+":"+secondValue;

		this.songProgressBar.Text = "00:00 / "+endPart;
		this.songProgressBar.Fraction = 0.00;
	
		timer = GLib.Timeout.Add(2000, new GLib.TimeoutHandler (progress_timeout) );		
	}
	
	protected virtual void OnMuteButtonClicked (object sender, System.EventArgs e)
	{		
		mp3.VolumeSet(volume);
		volumeScale.Value = volume;		
		volumeButton.Show();
		muteButton.Hide();
	}

	protected virtual void OnVolumeButtonClicked (object sender, System.EventArgs e)
	{
		volume = volumeScale.Value;
		mp3.VolumeSet(0.0);
		volumeScale.Value = 0.0;
		muteButton.Show();
		volumeButton.Hide();

	}

	protected virtual void OnVolumeScaleValueChanged (object sender, System.EventArgs e)
	{
		mp3.VolumeSet(volumeScale.Value);
	}

	protected virtual void OnSearchAddButtonClicked (object sender, System.EventArgs e)
	{		
		TreePath[] paths = songListTree.Selection.GetSelectedRows();			
		if (paths.Length == 0)
			return;
		for (int p = 0; p < paths.Length; p++){
			Song songToAdd = (Song)songArrayList.ToArray()[paths[p].Indices[0]]; 
			queue.AddSong(songToAdd);
			mp3.QueueSong(songToAdd);
			
		}
		
		queueListStore.Clear();
		queueArrayList = new ArrayList();
		
		foreach (Song x in queue.SongsList){
			queueArrayList.Add(x);
			queueListStore.AppendValues(x.ToString());
		}

		if ( queue.Count() == 1)
			bottomStatusBar.Push (1, "1 song currently in the queue.");
		else
			bottomStatusBar.Push (1,  queue.Count()+ " songs currently in the queue.");
		
		
	}

	protected virtual void OnQueueSaveButtonClicked (object sender, System.EventArgs e)
	{
		Playlist tempList = new Playlist();
		if (queueEntryBox.Text.Equals(""))
			tempList.Name = "Unnamed queue";
		else
			tempList.Name = queueEntryBox.Text;
		
		tempList.AddMultiple((ArrayList)queue.SongsList.Clone());
		
		
		playlists.Add(tempList);
		
		playListStore.Clear();
		playListArrayList = new ArrayList();
		
		foreach (Playlist x in playlists){
			playListArrayList.Add(x);
			playListStore.AppendValues(x.ToString());
		}
		
		bottomStatusBar.Push (1,  "added playlist "+tempList.Name+" to the collection.");
	}

	protected virtual void OnQueueNewPlaylistClicked (object sender, System.EventArgs e)
	{
		queueListStore.Clear();
		queueArrayList = new ArrayList();
		queue.ClearList();
		bottomStatusBar.Push (1, "cleared the current list.");
	}

	protected virtual void OnPlayListNewButtonClicked (object sender, System.EventArgs e)
	{
		//TODO: do number checking later
		Playlist tempPlaylist = new Playlist(new ArrayList(), "NewPlayList#");

		playlists.Add(tempPlaylist);
		
		playListStore.Clear();
		playListArrayList = new ArrayList();
		
		foreach (Playlist x in playlists){
			playListArrayList.Add(x);
			playListStore.AppendValues(x.ToString());
		}
			
		bottomStatusBar.Push (1, "created new playlist "+queue.Name+" to the collection.");
	}

	protected virtual void OnPlayListLoadButtonClicked (object sender, System.EventArgs e)
	{
		// get the currently selected Playlist
		TreePath[] paths = playListTreeView.Selection.GetSelectedRows();			
		if (paths.Length == 0)
			return;
		// Find what it points to
		Playlist listToLoad = (Playlist)(playlists.ToArray()[paths[0].Indices[0]]);
		queue = listToLoad;		
		
		queueListStore.Clear();
		queueArrayList = new ArrayList();
		
		foreach (Song x in queue.SongsList){
			queueArrayList.Add(x);
			queueListStore.AppendValues(x.ToString());
		}
		bottomStatusBar.Push (1, "loaded playlist "+listToLoad.Name+".");
	}

	protected virtual void OnPlayListDeleteButtonClicked (object sender, System.EventArgs e)
	{
		TreePath[] paths = playListTreeView.Selection.GetSelectedRows();			
		if (paths.Length == 0)
			return;
		
		Playlist listToDelete = (Playlist)(playlists.ToArray()[paths[0].Indices[0]]);
		
		bottomStatusBar.Push (1, "removed playlist "+listToDelete.Name+" from the collection.");
		playlists.Remove(listToDelete);
		
		playListStore.Clear();
		playListArrayList = new ArrayList();
		
		foreach (Playlist x in playlists){
			playListArrayList.Add(x);
			playListStore.AppendValues(x.ToString());
		}
	}

	protected virtual void OnQueueDeleteButtonClicked (object sender, System.EventArgs e)
	{
		TreePath[] paths = queueTreeView.Selection.GetSelectedRows();			
		if (paths.Length == 0)
			return;	
		
		Song songToRemove = (Song)queueArrayList.ToArray()[paths[0].Indices[0]]; 
		queue.RemoveSong(songToRemove);
		mp3.QueueRemove(songToRemove);
	
		queueListStore.Clear();
		queueArrayList = new ArrayList();
		
		foreach (Song x in queue.SongsList){
			queueArrayList.Add(x);
			queueListStore.AppendValues(x.ToString());
		}
		bottomStatusBar.Push (1,  songToRemove.songName+" removed from the queue.");
	}
}


		