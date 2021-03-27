using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EVLCPlaylistMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private List<string> fileTypes;
		private IEnumerable<string> ds;
		
		public MainWindow()
		{
			InitializeFileTypes();
			InitializeComponent();

			string[] s = Environment.GetCommandLineArgs();
			string path = "";
			if (s.Length > 1)
			{
				path = s[1];
			}
			else
			{
				FolderBrowserDialog fbd = new FolderBrowserDialog();
				fbd.ShowDialog();
				path = fbd.SelectedPath;

				if (path == "")
					Close();
			}

			Dictionary<string, List<string>> files = GetFiles(path);
			SaveFile(path, files);
			Close();
		}
		public void InitializeFileTypes()
		{
			fileTypes = new List<string>();
			fileTypes.Add(".mp4");
			fileTypes.Add(".mp3");
			fileTypes.Add(".m4v");
			fileTypes.Add(".mkv");
			fileTypes.Add(".webm");
		}
		public void SaveFile(string path, Dictionary<string, List<string>> files)
		{
			StreamWriter masterStream = new StreamWriter(File.Open(path + "\\All.xspf", FileMode.Create));
			WriteHeader(masterStream, "All");

			foreach (KeyValuePair<string, List<string>> kvp in files)
			{
				if (kvp.Value.Count == 0)
					continue;
				string title = kvp.Key.Remove(0, path.Length);
				if (title.Length > 0)
					title = title.Remove(0, 1);
				if (title.Length == 0)
					title = "Root";
				StreamWriter stream = new StreamWriter(File.Open(path + "\\" + title + ".xspf", FileMode.Create));
				WriteHeader(stream, title);

				foreach (string s in kvp.Value)
				{
					WriteTrack(masterStream, s);
					WriteTrack(stream, s);
				}

				WriteFooter(stream);
				stream.Close();
			}
			WriteFooter(masterStream);
			masterStream.Close();
		}
		public void WriteHeader(StreamWriter stream, string title)
		{
			stream.WriteLine("<playlist>");
			stream.WriteLine("\t<title>" + title + "</title>");
			stream.WriteLine("\t<trackList>");
		}
		public void WriteFooter(StreamWriter stream)
		{
			stream.WriteLine("\t</trackList>");
			stream.WriteLine("</playlist>");
		}
		public void WriteTrack(StreamWriter stream, string filePath)
		{
			stream.WriteLine("\t\t<track>");
			stream.WriteLine("\t\t\t<location>file:///" + ConvertFilePath(filePath) + "</location>");
			stream.WriteLine("\t\t</track>");
		}
		public string ConvertFilePath(string filePath)
		{
			string result = "";
			foreach (char c in filePath)
			{
				if (c == ' ')
					result += "%20";
				else if (c == '\\')
					result += '/';
				else if (c == '&')
					result += "%26";
				else
					result += c;
			}
			return result;
		}
		public Dictionary<string, List<string>> GetFiles(string path)
		{
			Dictionary<string, List<string>> result = new Dictionary<string,List<string>>();
			List<string> tempD = new List<string>();
			ds = Directory.EnumerateDirectories(path);
			foreach (string s in ds)
				tempD.Add(s.Remove(0, path.Length + 1));

			string[] files = Directory.GetFiles(path);
			List<string> orderedFiles = OrderFiles(files, path, path);
			result.Add(path, orderedFiles);
			
			foreach (string d in ds)
			{
//				List<string> filePaths = new List<string>();
				files = Directory.GetFiles(d);
				orderedFiles = OrderFiles(files, d, path);
				
				result.Add(d, orderedFiles);
			}
			return result;
		}
		public List<string> OrderFiles(string[] unordered, string directory, string path)
		{
			int l = directory.Length;
			string rel = directory.Remove(0, path.Length);
			if (rel.Length > 0)
			{
				rel = rel.Remove(0, 1);	// remove the / if its there
				rel += "/";
			}
			
			List<string> result = new List<string>();
			List<string> unorderedList = new List<string>();

			foreach (string s in unordered)
			{
				foreach(string type in fileTypes)
				{
					if (!s.Contains(type))
						continue;
					string fileName = s.Remove(0, l + 1);
					unorderedList.Add(fileName);
					break;
				}
			}
			Dictionary<int, string> dict = new Dictionary<int, string>();
			List<string> list = new List<string>();
			foreach (string s in unorderedList)
			{
				string[] sa = s.Split('.');
				int num = -1;
				if (int.TryParse(sa[0], out num))
				{
					while (dict.ContainsKey(num)) num++;
					dict.Add(num, s);
				}
				else
					list.Add(s);
			}
			var test = dict.OrderBy(i => i.Key);
			foreach (KeyValuePair<int, string> kvp in test)
				result.Add(rel + kvp.Value);
			foreach (string s in list)
				result.Add(rel + s);

			return result;
		}
	}
}
