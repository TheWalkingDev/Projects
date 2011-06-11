from SSMSCore import *
from SSMSUtilityFunctions import *
from System.Windows.Forms import *

class SampleCommand(CommandHandler):
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        print "MyCommand Executed"
        MessageBox.Show("My Command Executed") 
        self._DTE.ExecuteCommand("File.NewQuery") # opens a new query
        self._DTE.ActiveWindow.Selection.Insert("SELECT GETDATE()")
        self._DTE.ExecuteCommand("Query.Execute")
