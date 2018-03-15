using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using Gst;

namespace Mp3
{
	public class Song
	{
		public string fileName;
		public string path;
		public string songName;
		public string artistName;
		public string albumName;
		public string trackNumber;
		public string songGenre;
		public string songYear;
		public string songComment;
		public bool hasTag;
		
		public Song(){
			this.fileName = "";
			this.songName = "";
			this.artistName = "";
			this.albumName = "";
			this.trackNumber = "";
			this.songGenre ="";
			this.songYear ="";
			this.songComment = "";
			this.path = "";
		}

		public Song(string file, string song, string artist, string album, string track, string genre, string year, string comment)
		{
			if (song.Equals("ErrorNoSongTitle")){
				hasTag=false;
				this.fileName = file;
				this.songName = "";
				this.artistName = "";
				this.albumName = "";
				this.trackNumber = "";
				this.songGenre ="";
				this.songYear ="";
				this.songComment = "";
				this.fileName = Path.GetFileName(file);	
				this.path = file;
				return;
			}
			else{
				this.fileName = Path.GetFileName(file);
				this.path = file;
				this.songName = song;
				this.artistName = artist;
				this.albumName = album;
				this.trackNumber = track;
				this.songGenre = genre;
				this.songYear = year;
				this.songComment = comment;
				this.hasTag = true;
			}
		}
		
		public bool CompareTo(object o){
			if(this.Equals(((Song)o).fileName))
				return true;
			else
				return false;
		}
		
		public bool partialSearch(string searchValue, string field){
			
			bool found = false;
			Regex reg = new Regex(searchValue.ToLower()+".*");
			if(field.Equals("Artist")){				
				found = reg.IsMatch(this.artistName.ToLower());
				}
			else if(field.Equals("Song")){
				found = reg.IsMatch(this.songName.ToLower());
				}
			else if(field.Equals("Track #")){
				found = reg.IsMatch(this.trackNumber.ToLower());
				}
			else if(field.Equals("Album")){
				found = reg.IsMatch(this.albumName.ToLower());
			}
			else if(field.Equals("Genre")){
				found = reg.IsMatch(this.songGenre.ToLower());
				}
			else if(field.Equals("Filename")){
				found = reg.IsMatch(this.fileName.ToLower());
			}
			else if(field.Equals("All Fields")){				
				found =(   reg.IsMatch(this.artistName.ToLower())
				        || reg.IsMatch(this.songName.ToLower())
				        || reg.IsMatch(this.trackNumber.ToLower())
				        || reg.IsMatch(this.albumName.ToLower())
				        || reg.IsMatch(this.songGenre.ToLower())
				        || reg.IsMatch(this.fileName.ToLower())
				       );
			}
			return found;
		}

		override public string ToString(){
			if (hasTag == false){
				return fileName;
			}			
			return this.artistName + " - " + this.songName;
		}
		
		
		
	}
}
