import clr
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Drawing")

from System.Text.RegularExpressions import *	
from System.Windows.Forms import *
from System.Drawing import *

class Dialog:
	def __init__(self):
		self._f = Form()
		pnlBottom = Panel()
		#pnlBottom.BackColor = Color.White
		pnlBottom.Dock = DockStyle.Bottom
		buttonOk = Button()
		buttonOk.Text = "OK"
		buttonOk.DialogResult = DialogResult.OK
		buttonCancel = Button()
		buttonCancel.Text = "Cancel"
		buttonCancel.DialogResult = DialogResult.Cancel
		pnlBottom.Controls.Add(buttonOk)
		pnlBottom.Controls.Add(buttonCancel)
		buttonOk.Left = pnlBottom.Right
		buttonCancel.Left = buttonOk.Left - buttonOk.Size.Width
		pnlBottom.Height = buttonOk.Size.Height		
		self._pnlContainer = Panel()
		self._f.Controls.Add(pnlBottom)
		self._f.Controls.Add(self._pnlContainer)
		self._pnlContainer.Dock = DockStyle.Fill
		buttonOk.Anchor = AnchorStyles.Right or AnchorStyles.Top
		buttonCancel.Anchor = AnchorStyles.Right or AnchorStyles.Top
		
		
	def GetContainer(self):
		return self._pnlContainer
		
	def ShowDialog(self):
		return self._f.ShowDialog()
		
		
#Dialog().ShowDialog()		