from System.IO import *
from System.Text import *
from System.Text.RegularExpressions import *


class TextContext:
	def __init__(self, text, lineNumber):
		self.Text = text
		self.LineNumber = lineNumber
		self._absolute = False
        
	def SetAbsolute(self):
		self._absolute = True
	def IsAbsolute(self):
		return self._absolute        
        

class DocumentScanner:
    
    def __init__(self, text):
        self._text = text
        self._absoluteText = ""
        self._absoluteText = text#.strip("\n")
        # on demand mapping
        self._absoluteCharToLineMap = None 
        #for line in self.GetLines():
        #    self._absoluteText += line + "\n"
    
    def MapCharIndexToLineNumber(self, index):
        if not self._absoluteCharToLineMap:
            self._absoluteCharToLineMap = []
            for line in self.GetLines():
                lineLen = line.Length
                lineLen += 2
                self._absoluteCharToLineMap.append(lineLen)
                
        idx = 0
        lineNumber = 1
        for len in self._absoluteCharToLineMap:
            idx += len
            if idx >= index:
                return lineNumber
            lineNumber += 1
                
    def GetFirstDistinctLines(self, ctxList):
	    lookup = {}
	    for ctx in ctxList:
		    if not lookup.ContainsKey(ctx.Text.ToUpper()):
			    lookup[ctx.Text.ToUpper()] = ctx
			    yield ctx
		
    def SortList(self, list):
        sortedList = []
        #lookup = {}
        for ctx in list:
            sortedList.append((ctx.Text, ctx))
            #lookup[ctx.Text] = ctx
        sortedList.sort()
        for ctx in sortedList:
            yield ctx[1]
        """
        for table in sortedList:
        yield lookup[table]
        """

    def GetEvaluatorFirstGroup(self):
        return lambda text, match: match.Groups[1].ToString()
        
	
    def MatchLinesSorted(self, expr, matchEvaluator):
        for ctx in self.SortList(self.MatchLines(expr, matchEvaluator)):
            yield ctx
            
    def MatchLinesSortedDefault(self, expr):	
        return self.MatchLinesSorted(expr, self.GetEvaluatorFirstGroup())

    def GetFirstDistinctLinesSortedDefault(self, expr):
        for table in self.GetFirstDistinctLinesSorted(expr, self.GetEvaluatorFirstGroup()):
            yield table
            
    def GetRegions(self):
        return self.GetFirstDistinctLinesSorted("--[\s]+#region[\s]+?([\w\s]+)", lambda text, match: match.Groups[1].ToString())

    def GetVariables(self):
        return self.GetFirstDistinctLinesSorted("DECLARE[\s]+([@\w]+)", self.GetEvaluatorFirstGroup())
    
    def GetIndexes(self):
        return self.GetFirstDistinctLinesSorted("CREATE[\s\w]+INDEX[\s]+([\w]+)[\s]+ON[\s]+([#\w]+)", lambda text, match: "%s.%s" % (match.Groups[2].ToString(), match.Groups[1].ToString()))
    
    def GetLines(self):
	    ms = MemoryStream(Encoding.UTF8.GetBytes(self._text))
	    sr = StreamReader(ms)
	    while not sr.EndOfStream:
		    yield sr.ReadLine()
            
    def GetFirstDistinctLinesSorted(self, expr, matchEvaluator):
        
        for ctx in self.SortList(self.GetFirstDistinctLines(self.MatchLines(expr, matchEvaluator))):
            yield ctx
        """
	    sortedList = []
	    lookup = {}
	    for ctx in self.GetFirstDistinctLines(expr, matchEvaluator):
		    sortedList.append(ctx.Text)
		    lookup[ctx.Text] = ctx
	    sortedList.sort()
	    for table in sortedList:
		    yield lookup[table]
        """
            

    def MatchLines(self, expr, matchEvaluator):
        if not matchEvaluator:
            matchEvaluator = lambda text, match: text[match.Index:match.Index+match.Length]
        
        options = RegexOptions.IgnoreCase
        options |= RegexOptions.Singleline
        tempExpr = Regex(expr, options)            
        tempLineNumber = 1
	    #sortedList = []
                
        for line in self.GetLines():
		    m = tempExpr.Match(line)
		    if m.Success:
			    #textPortion = line[m.Index:m.Index+m.Length]
			    textPortion = matchEvaluator(line, m)
			    ctx = TextContext(textPortion, tempLineNumber)
			    yield ctx
		    tempLineNumber += 1 

    def MatchAbsolute(self, expr, matchEvaluator, indexEvaluator=None):
        if not matchEvaluator:
            matchEvaluator = lambda text, match: text[match.Index:match.Index+match.Length]
        if not indexEvaluator:
            indexEvaluator = lambda match: match.Groups[1].Index
            
        options = RegexOptions.IgnoreCase
        options |= RegexOptions.Singleline
        tempExpr = Regex(expr, options)            
        tempLineNumber = 1
        m = tempExpr.Match(self._absoluteText)        
        while m.Success:
            textPortion = matchEvaluator(self._absoluteText, m)
            
            #ctx = TextContext(textPortion, self.MapCharIndexToLineNumber(m.Groups[1].Index))
            ctx = TextContext(textPortion, self.MapCharIndexToLineNumber(indexEvaluator(m)))
            #ctx.SetAbsolute()
            
            m = m.NextMatch()
            yield ctx


    def GetTempTables(self):
        # self.GetFirstDistinctLinesSorted("#[\w]+", None)
        tables = []        
        
        
        for ctx in self.GetFirstDistinctLinesSorted("CREATE TABLE[\s]+?([#\w\[\]]+)", self.GetEvaluatorFirstGroup()):
            #print "found: " + ctx.Text
            tables.append(ctx)
        for ctx in self.SortList(self.GetFirstDistinctLines(self.MatchAbsolute("SELECT.*?INTO.*?([#\w]+).*?FROM", self.GetEvaluatorFirstGroup()))):
            #print "found into: " + ctx.Text
            tables.append(ctx)
            
        ctxList = self.GetFirstDistinctLines(tables)
        return self.SortList(ctxList)
