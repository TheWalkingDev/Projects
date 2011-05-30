from System import *
from System.IO import *
class Bookmarks:
	def __init__(self):
		self._bookmarks = {}
	def GetBookmarks(self):
		return self._bookmarks
		
	def LoadBookmarks(self, fileName):
			self._bookmarks.clear()
			f = FileStream(fileName, FileMode.Open)
			try:
				sw = StreamReader(f)
				while not sw.EndOfStream:
					line = sw.ReadLine()
					parts = line.split("|")
					self.AddBookmark(parts[0], parts[1], Int32.Parse(parts[2]))
				sw.Close()
			finally:
				f.Close()
			return self._bookmarks
			
	def SaveBookmarks(self, fileName):
			bookmarks = self._bookmarks
			f = FileStream(fileName, FileMode.Create)
			try:
				sw = StreamWriter(f)
				for bk in bookmarks:
					val = bookmarks[bk]
					line = "%s|%d" % (bk, val[2])
					#print line
					sw.WriteLine(line)
				sw.Close()
			finally:
				f.Close()
			return bookmarks
	def AddBookmark(self, windowName, bookmarkName, lineNumber):
		bookmarks = self._bookmarks
		bookmarkLookupName = "%s|%s" % (windowName, bookmarkName)
		bookmarks[bookmarkLookupName] = (windowName, bookmarkName, lineNumber)
