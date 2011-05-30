
import System
from SSMSCore import *
from SSMSUtilityFunctions import *
from System.Text.RegularExpressions import *
from SmartTreeView import *


try:
    from DocumentScanner import *
except System.Exception, e:
    print "Error importing DocumentScanner:" + repr(e)
    
from Bookmarks import *


    
class TreeContext:
    def __init__(self, window, textContext):
        self.Window = window
        self.TextContext = textContext

class CommandSQLNavigator(CommandHandler):
    def _smartTreeViewErrorHandler(self, message):
        self.ShowMessage(message)


    def populateGroup(self, generator, window, treeView, parentNode, imageIndex=-1):
        treeView.BeginPopulateNode(parentNode)
        try:
            for table in generator():
                nodeEvents = NodeEvents()
                nodeEvents.SetDoubleClickHandler(self.handleTreeViewNodeDoubleClick)
                nodeName = ""
                if table.IsAbsolute():
                    nodeName = "%s (#%d)" % (table.Text, table.LineNumber)
                else:
                    nodeName = "%s (%d)" % (table.Text, table.LineNumber)
                node = treeView.FindOrCreateNode(parentNode, nodeName, nodeEvents, TreeContext(window, table))
                node.SetImageIndex(imageIndex)
        finally:
            treeView.EndPopulateNode(parentNode)
        
    def populateTempTables(self, window, treeView, parentNode, text):
        
        self.populateGroup(DocumentScanner(text).GetTempTables, window, treeView, parentNode, self._iconSqlNavTempTable.ImageIndex)
            
    def populateRegions(self, window, treeView, parentNode, text):
        self.populateGroup(DocumentScanner(text).GetRegions, window, treeView, parentNode, self._iconSqlNavRegion.ImageIndex)
        
    def populateVariables(self, window, treeView, parentNode, text):
        self.populateGroup(DocumentScanner(text).GetVariables, window, treeView, parentNode, self._iconSqlNavVariable.ImageIndex)        

    def populateIndexes(self, window, treeView, parentNode, text):
        self.populateGroup(DocumentScanner(text).GetIndexes, window, treeView, parentNode, self._iconSqlNavIndex.ImageIndex)        
        
    
    def populateCustomGroups(self, parentNode, activeWindow, treeView, text):
        try:
            expressions = Path.Combine(self.GetWorkingDirectory(), "Scripts\Expressions.txt")
            re = Regex(r"([\w]+),(.*)")
            if File.Exists(expressions):
                for line in ReadLines(expressions):
                    m = re.Match(line)
                    if m.Success:
                        categoryNode = self.AddCategoryNode(parentNode, 
                            m.Groups[1].ToString(), activeWindow)
                        document = DocumentScanner(text)
                        self.populateGroup(lambda: document.MatchLinesSortedDefault(m.Groups[2].ToString()), 
                            activeWindow, treeView, categoryNode)
        except System.Exception, e:
            InspectWithPyPad(globals(), locals())
            self.ShowMessage("Error in populateCustomGroups: %s" % repr(e))            
                
                
    def AddCategoryNode(self, parentNode, text, activeWindow):
        nodeEvents = NodeEvents()
        nodeEvents.SetDoubleClickHandler(self.handleCategoryNodeDoubleClick)        
        node = self._treeView.FindOrCreateNode(parentNode, text, nodeEvents, TreeContext(activeWindow, None))
        node.SetImageIndex(self._iconSqlDefault.ImageIndex)
        return node
    
    def createActivatableNodeEvents(self):
        nodeEvents = NodeEvents()
        nodeEvents.SetDoubleClickHandler(self.handleCategoryNodeDoubleClick)
        return nodeEvents
        
    
        
    def populateWindow(self, parent, activeWindow):
        mainNode = self.CreateWindowNode(parent, activeWindow)
                    
        #tempTableNode = self._treeView.FindOrCreateNode(mainNode, "Temp Tables", NodeEvents(), TreeContext(activeWindow, None))
        #tempRegionNode = self._treeView.FindOrCreateNode(mainNode, "Regions", NodeEvents(), TreeContext(activeWindow, None))
        if self.isTextWindow(activeWindow):                    
            activeWindow.Selection.SelectAll()
            text = activeWindow.Selection.Text                    
            
            tempTableNode = self.AddCategoryNode(mainNode, "Temp Tables", activeWindow)
            #tempTableNode.SetImageIndex(self._iconSqlNavTempTable.ImageIndex)
            
            self.populateTempTables(activeWindow, 
                self._treeView, 
                tempTableNode,
                text)
            self.populateRegions(activeWindow, 
                self._treeView, 
                self.AddCategoryNode(mainNode, "Regions", activeWindow), 
                text)
            self.populateVariables(activeWindow, 
                self._treeView, 
                self.AddCategoryNode(mainNode, "Variables", activeWindow), 
                text)
            imageIndexNode = self.AddCategoryNode(mainNode, "Indexes", activeWindow)             
            #imageIndexNode.GetNode().ImageIndex = self._iconSqlNavIndex.ImageIndex
            self.populateIndexes(activeWindow, 
                self._treeView, 
                self.AddCategoryNode(mainNode, "Indexes", activeWindow), 
                text)
            self.populateCustomGroups(mainNode, activeWindow, self._treeView, text)
        #self.populateRegions(activeWindow, self._treeView, tempRegionNode, text)
        return mainNode
        
    def isTextWindow(self, window):
        return window and window.Selection and (self.getWindowDisplayName(window) <> "SQL Navigator")
    
    def getWindowDisplayName(self, window):
        if window.Document:
            return window.Document()
        else:
            return window.Caption
    
    def CreateWindowNode(self, parent, activeWindow):
        return self._treeView.FindOrCreateNode(parent, 
            self.getWindowDisplayName(activeWindow), 
            self.createActivatableNodeEvents(), 
            TreeContext(activeWindow, None))
            
    def CreateRootNode(self, text):
        return self._treeView.FindOrCreateNode(None, 
                            text, 
                            NodeEvents(), 
                            TreeContext(None, None))        
    
    def GetQueriesNode(self):
        return self.CreateRootNode("Queries")
    
    def AddBookmark(self, window, bookmarkName):
        nodeQueries = self.GetQueriesNode()
        nodeWindow = self.CreateWindowNode(nodeQueries, window)
        bkNode = self.AddCategoryNode(nodeWindow, "Bookmarks", window)
        textContext = TextContext(window.Selection.Text, window.Selection.CurrentLine)
        node = self._treeView.FindOrCreateNode(bkNode, bookmarkName, self.createActivatableNodeEvents(), TreeContext(window, textContext))
    
    def populateNavigator(self, activeWindow):
        try:
            self._treeView.BeginUpdate()
            try:                
                #self._treeView.Nodes.Clear()
                queryWindowsNode = self.CreateRootNode("Queries")
                otherWindowsNode = self.CreateRootNode("Other")
                
                self._treeView.BeginPopulateNode(queryWindowsNode)
                self._treeView.BeginPopulateNode(otherWindowsNode)                
                try:
                    sortList = []
                    lookup = {}
                    for window in self._DTE.Windows:
                        sortList.append(self.getWindowDisplayName(window))
                        lookup[self.getWindowDisplayName(window)] = window
                        #if self.isTextWindow(window):
                    sortList.sort()
                    for winCaption in sortList:
                        window = lookup[winCaption]
                        targetNode = None
                        if self.isTextWindow(window):
                            targetNode = queryWindowsNode
                        else:
                            targetNode = otherWindowsNode
                        childNode = self.populateWindow(targetNode, window)
                        if not self.isTextWindow(window):
                            childNode.SetImageIndex(self._iconSqlNavWindow.ImageIndex)
                finally:
                    self._treeView.EndPopulateNode(queryWindowsNode)
                    self._treeView.EndPopulateNode(otherWindowsNode)
                    pass
                otherWindowsNode.GetNode().Expand()
                queryWindowsNode.GetNode().Expand()
            finally:
                self._treeView.EndUpdate()
            #print "finished populating " + self.getWindowDisplayName(activeWindow)              
        except System.Exception, e:
            InspectWithPyPad(globals(), locals())
            self.ShowMessage("Error in populateNavigator: %s" % repr(e))
                   
    def handleCategoryNodeDoubleClick(self, sender, e, treeNodeData):
        try:
            #print "handleCategoryNodeDoubleClick"
            ctx = treeNodeData.GetDataContext()
            ctx.Window.Activate()
        except System.Exception, e:
            self.ShowMessage("Error in handleTreeViewNodeDoubleClick: %s" % repr(e))  


    def handleTreeViewNodeDoubleClick(self, sender, e, treeNodeData):
        try:
            
            ctx = treeNodeData.GetDataContext()
            ctx.Window.Activate()
            if ctx.TextContext.IsAbsolute():
                ctx.Window.Selection.MoveToAbsoluteOffset(ctx.TextContext.LineNumber)
            else:
                ctx.Window.Selection.GotoLine(ctx.TextContext.LineNumber)
        except System.Exception, e:
            self.ShowMessage("Error in handleTreeViewNodeDoubleClick: %s" % repr(e))  
        
    def RefreshNavigator(self):
        try:
            activeWindow = self._DTE.ActiveWindow
            self._activeWindow = activeWindow            
            self._Adapter.LockAround(self._lockObject, self.populateNavigator, activeWindow)
            #self._treeView.GetControl().ExpandAll()
        except System.Exception, e:
            self.ShowMessage("Error in CommandSQLNavigator.RefreshNavigator: %s" % repr(e))

    def GetIconFileName(self, fileName):
        return Path.Combine(self.GetWorkingDirectory(), "Icons\%s" % fileName)
    
    def CreateNavigator(self):
        try:
            activeWindow = self._DTE.ActiveWindow
            self._lockObject = self
            self._form = Panel()
            self._treeView = SmartTreeView()
            self._iconSqlDefault = self._treeView.AddIcon("SqlNavDefault", self.GetIconFileName("SqlNavDefault.ico"))
            self._iconSqlNavIndex = self._treeView.AddIcon("SqlNavIndex", self.GetIconFileName("SqlNavIndex.ico"))
            self._iconSqlNavTempTable = self._treeView.AddIcon("SqlNavTempTable", self.GetIconFileName("SqlNavTempTable.ico"))
            self._iconSqlNavRegion = self._treeView.AddIcon("SqlNavRegion", self.GetIconFileName("SqlNavRegion.ico"))
            self._iconSqlNavWindow = self._treeView.AddIcon("SqlNavWindow", self.GetIconFileName("SqlNavWindow.ico"))
            self._iconSqlNavVariable = self._treeView.AddIcon("SqlNavVariable", self.GetIconFileName("SqlNavVariable.ico"))

            
            self._treeView.SetErrorHandler(self._smartTreeViewErrorHandler)
            self._form.Controls.Add(self._treeView.GetControl())
            self._sqlAddin.CreateHostWindow(self._form, "SQL Navigator", "8F358257-0324-4F7B-9B07-A866FE33EEDB")            
            self.ShowMessage("Sql Navigator Created")
        except System.Exception, e:
            self.ShowMessage("Error in CommandSQLNavigator.InternalExecute: %s" % repr(e))

        
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        self.CreateNavigator()
        #self.RefreshNavigator()
             
        
class CommandSQLNavigatorBase(CommandHandler):        
    def SetNavigator(self, navigator):
        self._navigator = navigator
        
class CommandSQLNavigatorRefresh(CommandSQLNavigatorBase):    
    def SetBookmarksCommand(self, command):
        self._CommandSQLNavigatorCreateBookmark = command
        
    def Refresh(self):
        self._navigator.RefreshNavigator()
        self._CommandSQLNavigatorCreateBookmark.populateAllWindowsBookmarks()
        
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        self.Refresh()
        
class CommandSQLNavigatorCreateBookmark(CommandSQLNavigatorBase):
    def promptBookmarkName(self):
        import clr
        clr.AddReference("Microsoft.VisualBasic")
        from Microsoft.VisualBasic import *
        bookmarkName = Interaction.InputBox("Enter bookmark Name:", 
	        "SQLAddin", 
	        "Bookmark", 0, 0)
        return bookmarkName
        
    def getConfigFile(self):
        configDir = Path.Combine(self.GetWorkingDirectory(), "Config")
        
        if not Directory.Exists(configDir):
            Directory.CreateDirectory(configDir)
        configFile = Path.Combine(self.GetWorkingDirectory(), "Config\Bookmarks.txt")        
        return configFile
    
    def loadBookmarks(self):
        
        bkMarks = Bookmarks()
        if File.Exists(self.getConfigFile()):
            bkMarks.LoadBookmarks(self.getConfigFile())        
        return bkMarks
        
    def saveBookmark(self, windowName, bookmarkName, lineNumber):
        bkMarks = self.loadBookmarks()
        bkMarks.AddBookmark(windowName, bookmarkName, lineNumber)
        bkMarks.SaveBookmarks(self.getConfigFile())
        
    def populateAllWindowsBookmarks(self):
        for window in self._DTE.Windows:
            self.populateBookmarks(window)
            
        
    def populateBookmarks(self, window):
        try:
            if self._navigator.isTextWindow(window):
                bookmarks = self.loadBookmarks().GetBookmarks()
                
                for bookmarkEntry in bookmarks:
                    bookmark = bookmarks[bookmarkEntry]
                    #print "compare %s = %s" % (bookmark[0], self._navigator.getWindowDisplayName(window))
                    if bookmark[0] == self._navigator.getWindowDisplayName(window):
                        self._navigator.AddBookmark(window, bookmark[1])
        except System.Exception, e:
            self.ShowMessage("Error in CommandSQLNavigatorCreateBookmark.populateBookmarks: %s" % repr(e))
            InspectWithPyPad(globals(), locals())
                
        
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):        
        activeWindow = self._DTE.ActiveWindow
        bookmarkName = self.promptBookmarkName()
        self._navigator.AddBookmark(activeWindow, bookmarkName)
        self.saveBookmark(self._navigator.getWindowDisplayName(activeWindow), bookmarkName, activeWindow.Selection.CurrentLine)
        
        
            
class SqlNavigator:
    def __init__(self, SqlAddIn, ACommandBar):
        self.CreateNavigator = CommandSQLNavigator(SqlAddIn, ACommandBar,  "SqlNavigatorCreate", "SQL Navigator: Create Window", "Sql Navigator Tooltip", 13) 
        self.CreateNavigator.GetCommandInstance().Bindings = "Global::ctrl+F2";        
        
        
        self._CommandSQLNavigatorCreateBookmark = CommandSQLNavigatorCreateBookmark(SqlAddIn, ACommandBar,  "SqlNavigatorCreateBookmark", "SQL Navigator: Create Bookmark", "Sql Navigator Create Bookmark", 13)
        #self._CommandSQLNavigatorCreateBookmark.GetCommandInstance().Bindings = "Global::ctrl+Shift+F5";
        self._CommandSQLNavigatorCreateBookmark.SetNavigator(self.CreateNavigator)

        self.CreateNavigatorRefresh = CommandSQLNavigatorRefresh(SqlAddIn, ACommandBar,  "SqlNavigatorRefresh", "SQL Navigator: Refresh Active Window", "Sql Navigator Refresh", 13)
        self.CreateNavigatorRefresh.GetCommandInstance().Bindings = "Global::ctrl+Shift+F5";
        self.CreateNavigatorRefresh.SetNavigator(self.CreateNavigator) 
        self.CreateNavigatorRefresh.SetBookmarksCommand(self._CommandSQLNavigatorCreateBookmark) 
        
        
        