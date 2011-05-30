import clr
clr.AddReference("System.Windows.Forms")
from SSMSCore import *
from SimpleEditList import *
from Dialog import *
from System.Text.RegularExpressions import *	
from System.Windows.Forms import *

class CommandAddProcedureComment(CommandHandler):
    def getDescription(self, text):
        r = Regex("Description:([\s\w]+)History:")
        m = r.Match(text)
        if m.Success:	        
	        return m.Groups[1].ToString().Trim()
        return ""
    """
    def getHistory(self, text):
        r = Regex("([\w]{4,4}-[\w]{2,2}-[\w]{2,2})[\s]+?:[\s]+?([\s\w]+)\:([\s\w]+)[\r\n]")
        m = r.Match(text)
        while m.Success:
	        print "match:"
	        yield (m.Groups[1].ToString(), m.Groups[2].ToString(), m.Groups[3].ToString())
	        m = m.NextMatch()
    """
    def getHistory(self, text):
        r = Regex("History:([^\*]+)")
        m = r.Match(text)
        return m.Groups[1].ToString().Trim()
        
                
        
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        try:
            text = """
            /***************************************************************
            Description: 
            This stored Procedure performs xyz

            History:
            yyyy-MM-dd : Initials : Created 
            yyyy-MM-dd : Initials : Changed XYZ
            ***************************************************************/
            """
            text = self.GetActiveWindow().Selection.Text
            
            f = Dialog()
            editors = SimpleMultiEdit()
            controlHistory = RichTextBox()
            for history in self.getHistory(text):
                controlHistory.AppendText(history)
            controlDescription = TextBox()
            controlDescription.Text = self.getDescription(text)
            controlComment = TextBox()
            editors.AddEdit("Description", "Description", controlDescription.Text, controlDescription)
            editors.AddEdit("Comment", "Comment", "", controlComment)
            editors.AddEdit("History", "History", "", controlHistory)
            container = editors.CreateControls()
            container.Dock = DockStyle.Fill
            f.GetContainer(). Controls.Add(container)        
            
            if f.ShowDialog() == DialogResult.OK:
                self.GetActiveWindow().Selection.Delete()
                i = self.GetActiveWindow().Selection.Insert
                i("/***************************************************************\r\n")
                i("Description:\r\n")
                i(controlDescription.Text+"\r\n")
                i("History:\r\n")
                i(controlHistory.Text + "\r\n")
                i("%s: %s: %s%s" % (String.Format("{0:yyyy-MM-dd}", DateTime.Now), "AS", controlComment.Text, "\r\n"))
                i("***************************************************************/\r\n")
        except System.Exception, e:
            InspectWithPyPad(globals(), locals())
            self.ShowMessage("Error in CommandAddProcedureComment: %s" % repr(e))

class FormattingCommands:
    def __init__(self, SqlAddIn, ACommandBar):
        self._CommandAddProcedureComment = CommandAddProcedureComment(SqlAddIn, ACommandBar,  "IpyFmtAddProcComment", "Formatting: Add Procedure Comment", "Formatting: Add Procedure Comment", 13) 
