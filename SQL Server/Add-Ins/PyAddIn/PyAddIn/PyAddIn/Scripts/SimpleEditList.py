import clr
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Drawing")
from System import *
from System.Windows.Forms import *
from System.Drawing import *

guid = Guid().NewGuid()

class LabelEdit:
	def __init__(self, editor):
		self._panel = Panel()		
		#self._panel.BackColor = Color.White
		self._label = Label()
		self._label.Dock = DockStyle.Top
		self._editorControl = editor
		self._editorControl.Dock = DockStyle.Bottom
		self._editorControl.Width = self._panel.Width
		self._panel.Controls.Add(self._label)
		self._panel.Controls.Add(self._editorControl)
		self._panel.Height = 22 + editor.Height
	def SetLabel(self, label):
		self._label.Text = label
	def GetEditorControl(self):
		return self._editorControl
	def GetControl(self):
		self._editorControl.Top = 26
		return self._panel

class SimpleMultiEdit:
	def __init__(self):
		self.__edits = []
		self.__container = Panel()
		self.__controls = {}
		
	def AddEdit(self, name, label, value, editor):
		self.__edits.append((name, label, value, editor))
		
	def GetControl(self, name):
		return self.__controls[name]
    
	def CreateControls(self):
		self.__edits.reverse()
		for edit in self.__edits:
			t = LabelEdit(edit[3])
			t.SetLabel(edit[0])
			t.GetControl().Dock = DockStyle.Top
			self.__container.Controls.Add(t.GetControl())
			self.__controls[edit[0]] = t
		return self.__container
	
""" 
def GetProcedureComment():
    f = Form()
    editors = SimpleMultiEdit()
    editors.AddEdit("Description", "Description", "some description",  TextBox())
    textBox = RichTextBox()
    textBox.Text = "History:"
    editors.AddEdit("History", "History", "history", textBox)
    container = editors.CreateControls()
    container.Dock = DockStyle.Fill
    f.Controls.Add(container)
    return (f, editors)

try:
	edit = GetProcedureComment()
	
	edit[0].ShowDialog()
	print edit[1].GetControl("name").GetEditorControl().Text
except Exception, e:
	print repr(e)
"""