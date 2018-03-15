using System;
using System.Collections;

namespace Mp3
{
	
	public class Playlist
	{	
		public string Name= "";
		public ArrayList SongsList = new ArrayList();
		
		public Playlist()
		{
			Name = "Default Playlist";
			SongsList = new ArrayList();
		}
		public Playlist(ArrayList songs)
		{
			Name = "Default Playlist";
			foreach(Song x in songs){
				SongsList.Add(x);	
			}
		}
		
		public Playlist(ArrayList songs, string newName)
		{
			Name = newName;
			foreach(Song x in songs){
				SongsList.Add(x);	
			}
		}
		
		public void ClearList(){
			Name= "";
			SongsList = new ArrayList();
		}
		public int Count(){
			return SongsList.Count;
		}
		
		public void AddSong(Song newSong){
			SongsList.Add(newSong);
		}
				
		public void AddMultiple(ArrayList newSongs){
			foreach (Song x in newSongs){
				AddSong(x);
			}
		}

		public void RemoveSong(Song song){
			SongsList.Remove(song);			
		}
			
		override public string ToString(){
			return this.Name;
		}
	}
}
