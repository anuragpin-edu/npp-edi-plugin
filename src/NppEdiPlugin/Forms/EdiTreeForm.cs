using System;
using System.Drawing;
using System.Windows.Forms;
using Edi.Core.Model;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace NppEdiPlugin.Forms
{
    public partial class EdiTreeForm : Form
    {
        private TreeView _treeView;
        private Label _statusLabel;
        private ScintillaGateway _scintilla;

        public EdiTreeForm(ScintillaGateway scintilla)
        {
            _scintilla = scintilla;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "EDI Tree";
            this.Size = new Size(300, 600);

            _statusLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "No document loaded",
                Padding = new Padding(5, 0, 0, 0)
            };

            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                HideSelection = false
            };
            _treeView.AfterSelect += TreeView_AfterSelect;

            this.Controls.Add(_treeView);
            this.Controls.Add(_statusLabel);
        }

        public void RenderDocument(EdiDocument document)
        {
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();

            if (document == null)
            {
                _statusLabel.Text = "Failed to parse document.";
                _treeView.EndUpdate();
                return;
            }

            string versionStr = !string.IsNullOrEmpty(document.Version) ? $" — {document.Version}" : "";
            _statusLabel.Text = $"Parsed {document.Standard}{versionStr} document with {document.Segments.Count} segments.";

            var rootNode = new TreeNode($"{document.Standard} Interchange{versionStr}")
            {
                Tag = new NodeOffsets { Start = 0, End = 0 }
            };

            foreach (var segment in document.Segments)
            {
                var segNode = new TreeNode(segment.Tag)
                {
                    Tag = new NodeOffsets { Start = segment.StartOffset, End = segment.EndOffset }
                };

                if (!string.IsNullOrEmpty(segment.Label))
                {
                    segNode.Text += $" - {segment.Label}";
                }

                foreach (var element in segment.Elements)
                {
                    var elNode = new TreeNode($"[{element.Position}] {element.RawValue}")
                    {
                        Tag = new NodeOffsets { Start = segment.StartOffset, End = segment.EndOffset }
                    };

                    if (!string.IsNullOrEmpty(element.Label))
                    {
                        elNode.Text += $" - {element.Label}";
                    }
                    if (!string.IsNullOrEmpty(element.ValueDescription))
                    {
                        elNode.Text += $" ({element.ValueDescription})";
                    }

                    foreach (var component in element.Components)
                    {
                        var compNode = new TreeNode($"[{component.Index}] {component.RawValue}")
                        {
                            Tag = new NodeOffsets { Start = segment.StartOffset, End = segment.EndOffset }
                        };

                        if (!string.IsNullOrEmpty(component.Label))
                        {
                            compNode.Text += $" - {component.Label}";
                        }
                        if (!string.IsNullOrEmpty(component.ValueDescription))
                        {
                            compNode.Text += $" ({component.ValueDescription})";
                        }

                        elNode.Nodes.Add(compNode);
                    }

                    segNode.Nodes.Add(elNode);
                }

                rootNode.Nodes.Add(segNode);
            }

            rootNode.Expand();
            _treeView.Nodes.Add(rootNode);

            _treeView.EndUpdate();
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is NodeOffsets offsets)
            {
                if (offsets.Start > 0 || offsets.End > 0)
                {
                    _scintilla.SetSelection(offsets.Start, offsets.End);
                    _scintilla.ScrollCaret();
                }
            }
        }

        private class NodeOffsets
        {
            public int Start { get; set; }
            public int End { get; set; }
        }
    }
}
