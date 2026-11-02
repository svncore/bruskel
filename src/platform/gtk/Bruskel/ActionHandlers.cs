//
// FileActionHandler.cs
//
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
//
// Copyright (c) 2010 Jonathan Pobst
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using Bruskel.Actions;
using Bruskel.Core;

namespace Bruskel;

public sealed class ActionHandlers
{
	private readonly List<IActionHandler> action_handlers;

	public ActionHandlers ()
	{
		ChromeManager chrome = BruskelCore.Chrome;
		WorkspaceManager workspace = BruskelCore.Workspace;
		ActionManager actions = BruskelCore.Actions;
		RecentFileManager recentFiles = BruskelCore.RecentFiles;
		ImageConverterManager imageFormats = BruskelCore.ImageFormats;
		SettingsManager settings = BruskelCore.Settings;
		SystemManager system = BruskelCore.System;
		ToolManager tools = BruskelCore.Tools;
		PaletteManager palette = BruskelCore.Palette;
		CanvasGridManager canvasGrid = BruskelCore.CanvasGrid;
		string applicationVersion = BruskelCore.ApplicationVersion;

		action_handlers = [
			// File
			new NewDocumentAction (actions, chrome, palette, settings, workspace),
			new NewScreenshotAction (system, chrome, workspace, actions),
			new OpenDocumentAction (actions.File, chrome, workspace, recentFiles, imageFormats),
			new SaveDocumentAction (actions.File, workspace),
			new SaveDocumentAsAction (actions.File, workspace),
			new SaveDocumentImplmentationAction (actions.File, actions.Image, chrome, imageFormats, recentFiles, tools),
			new ModifyCompressionAction (actions.File),
			//new PrintDocumentAction ();
			new CloseDocumentAction (actions, chrome, workspace, tools),
			new ExitProgramAction (actions, chrome, workspace),

			// Edit
			new OffsetSelectionAction (actions.Edit, chrome, workspace, tools),
			new PasteAction (chrome, actions, workspace, tools),
			new PasteIntoNewLayerAction (actions, chrome, workspace, tools),
			new PasteIntoNewImageAction (actions, chrome, workspace),
			new ResizePaletteAction (actions.Edit, chrome, palette),
			new AddinManagerAction (actions.Addins, chrome, system),
			new PreferencesDialogAction(actions.App, chrome, palette, settings),

			// Image
			new ResizeImageAction (actions.Image, chrome, workspace, settings),
			new ResizeCanvasAction (chrome, workspace, settings, actions),

			// Layers
			new LayerPropertiesAction (chrome, actions.Layers, workspace),
			new RotateZoomLayerAction (chrome, actions.Layers, workspace, tools),

			// View
			new ToolBarToggledAction (actions.View, chrome),
			new ImageTabsToggledAction (actions.View, chrome),
			new ToolWindowsToggledAction (actions.View, chrome),
			new StatusBarToggledAction (actions.View, chrome),
			new ToolBoxToggledAction (actions.View, chrome),
			new ColorSchemeChangedAction (settings),
			new EditCanvasGridAction (actions.View, chrome, canvasGrid),

			// Window
			new CloseAllDocumentsAction (actions, workspace),
			new SaveAllDocumentsAction (actions.Window, workspace),

			// Help
			new AboutDialogAction (actions.App, chrome, applicationVersion),
			new KeyboardShortcutsDialogAction (actions.App, actions, chrome, tools),
		];

		// Initialize each action handler
		foreach (var action in action_handlers)
			action.Initialize ();

		// We need to toggle actions active/inactive
		// when there isn't an open document
		BruskelCore.Workspace.DocumentActivated += Workspace_DocumentCreated;
		BruskelCore.Workspace.DocumentClosed += Workspace_DocumentClosed;

		// Initially, no documents are open.
		ToggleActions (false);
	}

	private void Workspace_DocumentClosed (object? sender, DocumentEventArgs e)
	{
		if (!BruskelCore.Workspace.HasOpenDocuments)
			ToggleActions (false);
	}

	private void Workspace_DocumentCreated (object? sender, DocumentEventArgs e)
	{
		ToggleActions (true);
	}

	private static void ToggleActions (bool enable)
	{
		BruskelCore.Actions.File.Close.Sensitive = enable;
		BruskelCore.Actions.File.Save.Sensitive = enable;
		BruskelCore.Actions.File.SaveAs.Sensitive = enable;
		BruskelCore.Actions.File.Print.Sensitive = enable;
		BruskelCore.Actions.Edit.Copy.Sensitive = enable;
		BruskelCore.Actions.Edit.CopyMerged.Sensitive = enable;
		BruskelCore.Actions.Edit.Cut.Sensitive = enable;
		BruskelCore.Actions.Edit.PasteIntoNewLayer.Sensitive = enable;
		BruskelCore.Actions.Edit.EraseSelection.Sensitive = enable;
		BruskelCore.Actions.Edit.FillSelection.Sensitive = enable;
		BruskelCore.Actions.Edit.InvertSelection.Sensitive = enable;
		BruskelCore.Actions.Edit.OffsetSelection.Sensitive = enable;
		BruskelCore.Actions.Edit.SelectAll.Sensitive = enable;
		BruskelCore.Actions.Edit.Deselect.Sensitive = enable;

		BruskelCore.Actions.View.ActualSize.Sensitive = enable;
		BruskelCore.Actions.View.ZoomIn.Sensitive = enable;
		BruskelCore.Actions.View.ZoomOut.Sensitive = enable;
		BruskelCore.Actions.View.ZoomToSelection.Sensitive = enable;
		BruskelCore.Actions.View.ZoomToWindow.Sensitive = enable;
		BruskelCore.Actions.View.ZoomComboBox.Sensitive = enable;

		BruskelCore.Actions.Image.CropToSelection.Sensitive = enable;
		BruskelCore.Actions.Image.AutoCrop.Sensitive = enable;
		BruskelCore.Actions.Image.CanvasSize.Sensitive = enable;
		BruskelCore.Actions.Image.Resize.Sensitive = enable;
		BruskelCore.Actions.Image.FlipHorizontal.Sensitive = enable;
		BruskelCore.Actions.Image.FlipVertical.Sensitive = enable;
		BruskelCore.Actions.Image.Rotate180.Sensitive = enable;
		BruskelCore.Actions.Image.RotateCCW.Sensitive = enable;
		BruskelCore.Actions.Image.RotateCW.Sensitive = enable;
		BruskelCore.Actions.Image.Flatten.Sensitive = enable;

		BruskelCore.Actions.Layers.AddNewLayer.Sensitive = enable;
		BruskelCore.Actions.Layers.DeleteLayer.Sensitive = enable;
		BruskelCore.Actions.Layers.DuplicateLayer.Sensitive = enable;
		BruskelCore.Actions.Layers.MergeLayerDown.Sensitive = enable;
		BruskelCore.Actions.Layers.ImportFromFile.Sensitive = enable;
		BruskelCore.Actions.Layers.FlipHorizontal.Sensitive = enable;
		BruskelCore.Actions.Layers.FlipVertical.Sensitive = enable;
		BruskelCore.Actions.Layers.RotateZoom.Sensitive = enable;
		BruskelCore.Actions.Layers.MoveLayerUp.Sensitive = enable;
		BruskelCore.Actions.Layers.MoveLayerDown.Sensitive = enable;
		BruskelCore.Actions.Layers.Properties.Sensitive = enable;

		BruskelCore.Actions.Adjustments.ToggleActionsSensitive (enable);
		BruskelCore.Actions.Effects.ToggleActionsSensitive (enable);

		BruskelCore.Actions.Window.SaveAll.Sensitive = enable;
		BruskelCore.Actions.Window.CloseAll.Sensitive = enable;
	}
}

