using System;
using System.Collections;
using System.IO;
using System.Text;
using Gnome;
using System.Text.RegularExpressions;
using GLib;
using Gst;

namespace Mp3
{
	public class Collection
	{		
		public static MainLoop loop;
		public static PlayBin play;
		public static ArrayList queue = new ArrayList();		
		public Library mp3Library = new Library();		
		public Song currentSong = new Song();
		public static int queuePos=0;
		public int currentHours=0;
		public int currentMinutes=0;
		public int currentSeconds=0;

		public Collection(){			
			mp3Library = new Library();
			currentSong = new Song();
			
	        Gst.Application.Init ();
	        loop = new MainLoop ();
	        play = ElementFactory.Make ("playbin", "play") as PlayBin;
	        if (play == null) {
	            Console.WriteLine ("error creating a playbin gstreamer object");
	            return;
	        }
	        play.Bus.AddWatch (new BusFunc (BusCb));
	        play.SetState (State.Ready);		
		}
		public void QueueSong(Song song){
			queue.Add(song);
		}
		public void QueueRemove(Song song){
			queue.Remove(song);
		}
		public void NewQueue(){
			queue.Clear();
			queuePos = 0;
		}
		public void PlaySong(Song song){
			queuePos = 0;
			play.SetState(State.Null);
			currentSong = song;
	        play.Uri = "file:///"+currentSong.path;
	        play.SetState (State.Playing);
			updateTime();
			
		}
		public void updateTime(){		
			long songLength;
			long length= 0;
			play.AudioSink.QueryDuration(Gst.Format.Time,out length);	
			
			songLength = length / (1000 * 1000 * 1000);	

			currentHours=(int)(songLength/3600);
			currentMinutes=(int)(songLength/60);
			currentSeconds=(int)(songLength%60);
		}
		public void StopSong(){			
			currentSong = new Song();
			play.SetState(State.Null);
			loop.Quit();	
			currentHours=0;
			currentMinutes=0;
			currentSeconds=0;
		}
		public void NextSong(){
			queuePos++;
			Song nextSong;
			try{
				nextSong =  (Song)(queue.ToArray()[queuePos]);
			}
			catch(Exception){
				queuePos--;
				return;
			}
			play.SetState (Gst.State.Null);

			play.Uri = "file:///"+nextSong.path;
			play.SetState (State.Playing);
			
			currentSong = nextSong;
			updateTime();
		}
		public void PreviousSong(){
			queuePos--;
			Song nextSong;
			try{
				nextSong =  (Song)(queue.ToArray()[queuePos]);
			}
			catch(Exception){
				queuePos++;
				return;
			}
			play.SetState (Gst.State.Null);
			play.Uri = "file:///"+nextSong.path;				
			play.SetState (State.Playing);

			currentSong = nextSong;
			updateTime();
		}
		
		public double getVolume(){			
			return play.Volume;
		}
		public void VolumeSet(double value){
			play.Volume = value;
		}
		
		public void createLibrary(string directory){
			createLibrary(new DirectoryInfo(directory));
		}

		// TODO Add a dialog to show which mp3s/folders are being processed
		private void createLibrary(DirectoryInfo directory){
			foreach (FileInfo file in directory.GetFiles("*.mp3")){
				Song tempSong = ReadTag(directory+"/"+file.Name);
				mp3Library.AddSong(tempSong);
			}

			DirectoryInfo [] subDirectories = directory.GetDirectories();
			foreach (DirectoryInfo subDirectory in subDirectories)
				createLibrary(subDirectory);
		}

		public void addSingleSong(string fileName){
			Song tempSong = ReadTag(fileName);			
			mp3Library.AddSong(tempSong);
		}
		public ArrayList FindSong(string songToFind, string field){
			return mp3Library.Search(songToFind, field);
		}

		private bool BusCb (Bus bus, Message message)
		{
			switch (message.Type) {
				case Gst.MessageType.Error:
					string err = String.Empty;
					message.ParseError (out err);
					Console.WriteLine ("Gstreamer error: {0}", err);
					loop.Quit ();
				break;
				case Gst.MessageType.Eos:
					if (queuePos>=queue.Count) {
					Console.WriteLine ("Song Queue is empty.  Stopping");
					loop.Quit ();
				} 
				else {				
					Console.WriteLine("Moving to the next song");
					NextSong();
				}
				break;
			}
			return true;
		}
	
		// Found someplace random on the net.  Replace this soon, it barely fucking works.
		private Song ReadTag(string filename){
			string Title;
			string Artist;
			string Album;
			string Year;
			string Comment;
			int GenreID;
			int Track;
			
			FileStream oFileStream;
			byte[] bBuffer;
			
			oFileStream = new FileStream( filename, FileMode.Open);
			bBuffer = new byte[128];		
			try{
				oFileStream.Seek(-128, SeekOrigin.End);
				oFileStream.Read(bBuffer,0, 128);
				
			}
			catch(System.IO.IOException){
				oFileStream.Close();
				Song tempSong = new Song(filename,"ErrorNoSongTitle","", "","","","","");
				return tempSong;
			}
	
			Encoding  instEncoding = new ASCIIEncoding(); 			
			string id3Tag = instEncoding.GetString(bBuffer);

			if (id3Tag .Substring(0,3) == "TAG") 
			{
				Title      = id3Tag.Substring(  3, 30).Trim();
				Artist     = id3Tag.Substring( 33, 30).Trim();
				Album      = id3Tag.Substring( 63, 30).Trim();
				Year     = id3Tag.Substring( 93, 4).Trim();
				Comment    = id3Tag.Substring( 97,28).Trim();

				if (id3Tag[125]==0)
					Track = bBuffer[126];
				else
					Track = 0;
				GenreID = bBuffer[127];
			
				String newTitle="";
				String newArtist="";
				String newAlbum="";
				
				foreach(char x in Title.ToCharArray()){			
					if (x != '\0'){
					newTitle += x;
					}
				}				

				foreach(char x in Artist.ToCharArray()){
					if (x != '\0')
						newArtist += x;
				}	
				foreach(char x in Album.ToCharArray()){
					if (x != '\0')
						newAlbum += x;
				}		
				
				Song tempSong = new Song(filename,newTitle,newArtist, newAlbum, Track.ToString(),GenreID.ToString(),Year, Comment);
				return tempSong;
			}
			else{
			
				Song tempSong = new Song(filename,"ErrorNoSongTitle","", "","","","","");
				return tempSong;
			}
		}			
		

	}
		

}
