using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using Gtk;
using Gdk;
using GLib;

namespace Mp3
{
	public class Library
	{	
		public ArrayList songList;

		public Library(){
			songList = new ArrayList();	
		}		
		
		// will search for a song based on a field
		public ArrayList Search(string search, string field){
			ArrayList returnList = new ArrayList();	
			//Regex reg = new Regex(".*"+search.ToLower()+".*");
			
			foreach (Song x in this.songList){
				if (x.partialSearch(search,field)==true){
					returnList.Add(x);
				}
			}			
			return returnList;
		}
		
		public void AddSong(Song song){
			this.songList.Add(song);			
		}
		
		// Checks that a given song exists, if it does then it removes the song
		// Removes a song that matches the filename
		public bool RemoveSong(string filename){
			Song tempSong = new Song(filename,"","", "","","","","");
			int index = songList.BinarySearch(tempSong);
			if (index == 0){
			//	Console.WriteLine("File exists.  Removing file");
				this.songList.RemoveAt(index);
				return true;
			}
			else
				return false;			
		}
		override public string ToString(){
			String returnValue = "";
			foreach (Song x in this.songList){
				returnValue+= x.ToString() + "\n";
			}
			return returnValue;
		}
	}
}
