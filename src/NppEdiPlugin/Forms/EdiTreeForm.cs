using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Kbg.NppPluginNET.PluginInfrastructure;
using Edi.Core.Model;

namespace NppEdiPlugin.Forms
{
    public class EdiTreeForm : Form
    {
        private TreeView _treeView;
        private Label _statusLabel;
        private readonly IScintillaGateway _scintillaGateway;

        public EdiTreeForm(IScintillaGateway scintillaGateway)
        {
            _scintillaGateway = scintillaGateway;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "EDI Inspector";
            this.ClientSize = new Size(300, 500);

            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                HideSelection = false,
                FullRowSelect = true,
                ShowLines = true,
                Font = new Font("Consolas", 10F)
            };
            _treeView.NodeMouseDoubleClick += TreeView_NodeMouseDoubleClick;

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = SystemColors.ControlLight,
                Text = "Ready"
            };

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

            _statusLabel.Text = $"Parsed {document.Standard} document with {document.Segments.Count} segments.";

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
                        Tag = new NodeOffsets { Start = segment.StartOffset, End = segment.EndOffset } // We only highlight the segment for now
                    };

                    if (!string.IsNullOrEmpty(element.Label))
                    {
                        elNode.Text += $" - {element.Label}";
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

                        elNode.Nodes.Add(compNode);
                    }

                    segNode.Nodes.Add(elNode);
                }

                _treeView.Nodes.Add(segNode);
            }

            _treeView.EndUpdate();
        }

        private void TreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is NodeOffsets offsets)
            {
                _scintillaGateway.SetSel(offsets.Start, offsets.End);
                
                // Ensure visibility (scroll to selection)
                int currentPos = _scintillaGateway.GetCurrentPos();
                int line = _scintillaGateway.LineFromPosition(currentPos);
                _scintillaGateway.EnsureVisibleEnforcePolicy(line);
            }
        }

        private class NodeOffsets
        {
            public int Start { get; set; }
            public int End { get; set; }
        }
    }
}
