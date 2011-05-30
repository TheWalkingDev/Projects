import clr
clr.AddReference("System.Drawing")
import System
from System.Windows.Forms import *
from System.Drawing import *

from SSMSUtilityFunctions import *


class NodeEvents:
    def __init__(self):
        self.SetDoubleClickHandler(self._nullDoubleClickHandler)
        
    def _nullDoubleClickHandler(self, sender, e, treeNodeData):
        #print "_nullDoubleClickHandler"
        pass
        
    def SetDoubleClickHandler(self, doubleClickHandler):
        self.__doubleClickHandler = doubleClickHandler
        
    def RaiseDoubleClick(self, sender, e, treeNodeData):
        #print "Raising double click"
        self.__doubleClickHandler(sender, e, treeNodeData)
    
class TreeNodeData:
    def __init__(self, node, dataContext, nodeEvents):
        self.__node = node
        self.__childNodes = {}       
        self.__dataContext = dataContext
        self.__nodeEvents = nodeEvents
        self.__dirty = False
    
    def Remove(self, node):
        node.GetNode().Remove()
        self.__childNodes.Remove(node)
        
    def GetDataContext(self):
        return self.__dataContext
        
    def GetNodeEvents(self):
        return self.__nodeEvents
    def GetNode(self):
        return self.__node
    def GetChildNodes(self):
        return self.__childNodes
    def SetNotDirty(self):
        self.__dirty = False
    def SetDirty(self):
        self.__dirty = True
    def IsDirty(self):
        return self.__dirty
    def SetImageIndex(self, index):
        self.GetNode().ImageIndex = index
        self.GetNode().SelectedImageIndex = index
        
    def GetChildNodes(self):
        return self.__childNodes
        
    
class IconData:
    def __init__(self, name, fileName, imageIndex):
        self.Name = name
        self.FileName = fileName
        self.ImageIndex = imageIndex
        
class SmartTreeView:
    
    def __init__(self):
        self.__root = {}
        self._treeView = TreeView()
        self._treeView.Dock = DockStyle.Fill
        self._treeView.NodeMouseDoubleClick += self.handleTreeViewNodeDoubleClick
        self._errorHandler = self._nullErrorHandler
        self._imageList = ImageList()
        self._imageMap = {}
        self._treeView.ImageList = self._imageList    
    
    def AddIcon(self, name, fileName):
        ico = Icon(fileName)
        self._imageList.Images.Add(ico);
        iconData = IconData(name, fileName, self._imageList.Images.Count-1)
        self._imageMap[name] = iconData
        return iconData
        
    def GetImageData(self, name):
        return self._imageMap[name]
    
    def FindOrCreateNode(self, parent, text, nodeEvents, dataContext):
        try:
            childNodes = self.GetParentNodeChildNodes(parent)
            ctx = None
            if childNodes.ContainsKey(text):
                ctx = childNodes[text]
                #print "Set not dirty: %s" % text
                ctx.SetNotDirty()
            else:
                newNode = TreeNode(text)
                newNode.ImageIndex = -1
                ctx = TreeNodeData(newNode, dataContext, nodeEvents)    
                newNode.Tag = ctx                
                self.GetParentTreeNodeNodes(parent).Add(newNode)
                #print "Constructed: %s" % text
                childNodes[text] = ctx               
            return ctx
        except System.Exception, e:
            self.LogError("Error in FindOrCreateNode: %s" % repr(e), locals())  
                    
    def BeginUpdate(self):
        self._treeView.BeginUpdate()
    def EndUpdate(self):
        self._treeView.EndUpdate()
    
    def GetParentTreeNodeNodes(self, node):
        try:
            if node:
                return node.GetNode().Nodes
            else:
                return self._treeView.Nodes
        except System.Exception, e:
            self.LogError("Error in GetParentTreeNodeNodes: %s" % repr(e), locals())  
                
    def GetParentNodeChildNodes(self, node):    
        nodesDict = None
        if not node:
            nodesDict = self.__root
        else:
            nodesDict = node.GetChildNodes()        
        return nodesDict
        
        
    def BeginPopulateNode(self, node):
        try:
            #InspectWithPyPad(globals(), locals())
            childNodes = self.GetParentNodeChildNodes(node)
            for n in childNodes:
                childNode = childNodes[n]
                #print "%s: set dirty" % n
                childNode.SetDirty()
        except System.Exception, e:
            self.LogError("Error in BeginPopulateNode: %s" % repr(e), locals())                 
            
    def EndPopulateNode(self, node):
        try:
            childNodes = self.GetParentNodeChildNodes(node)
            removeList = []
            for n in childNodes:
                childNode = childNodes[n]
                if childNode.IsDirty():
                    parentTreeNode = self.GetParentTreeNodeNodes(node)
                    childTreeNode = childNode.GetNode()
                    #print "Removing %s.%s" % (repr(parentTreeNode), repr(childTreeNode))
                    #InspectWithPyPad(globals(), locals())
                    parentTreeNode.Remove(childTreeNode)
                    removeList.append(n)
            for n in removeList:
                childNodes.Remove(n)
        except System.Exception, e:
            self.LogError("Error in EndPopulateNode: %s" % repr(e),locals()) 
         
    def _nullErrorHandler(self, message):
        pass
    
    def SetErrorHandler(self, errorHandler):
        self._errorHandler = errorHandler
    
    def LogError(self, error, contextLocals):
        self._errorHandler(error)
        errorMessage = error
        InspectWithPyPad(globals(), contextLocals)
                    
    def handleTreeViewNodeDoubleClick(self, sender, e):
        try:
            treeNodeData = e.Node.Tag
            treeNodeData.GetNodeEvents().RaiseDoubleClick(sender, e, treeNodeData)            
        except System.Exception, e:
            self.LogError("Error in handleTreeViewNodeDoubleClick: %s" % repr(e), locals())  
        
    def GetControl(self):
        return self._treeView