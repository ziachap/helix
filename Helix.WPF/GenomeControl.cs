// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.

using Helix.Genetics;
using System.Drawing.Drawing2D;
using Helix.Utilities;

namespace Helix.Forms;

public partial class GenomeControl : UserControl
{
    /// <summary>
    /// Constructs a new instance of <see cref="GenomeControl"/>.
    /// </summary>
    public GenomeControl()
    {
        InitializeComponent();
    }

    private const int NodeDiameter = 10;
    private const int LayerDistance = 60;

    private Dictionary<int, int> _nodeLayers = new Dictionary<int, int>();

    private Genome? _genome;
    public Genome? Genome
    {
        get => _genome;
        set
        {
            _genome = value;
            Invalidate();
            Update();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (Genome == null) return;

        try
        {
            // Sample data
            Node[] inputs = Genome.Inputs
                .Select(x => new Node(x.Id, HasConnections(x.Id))
                {
                    TopLabel = x.Label
                })
                .ToArray();

            Node[] hidden = Genome.HiddenNeurons
                .Select(x => new Node(x.Id, HasConnections(x.Id))
                {
                    AggregationFunction = x.AggregationFunction.ToShortString(),
                    ActivationFunction = x.ActivationFunction.ToShortString()
                })
                .ToArray();

            Node[] outputs = Genome.OutputNeurons
                .Select(x => new Node(x.Id, HasConnections(x.Id))
                {
                    BottomLabel = x.Label,
                    AggregationFunction = x.AggregationFunction.ToShortString(),
                    ActivationFunction = x.ActivationFunction.ToShortString()
                })
                .ToArray();

            Connection[] connections = Genome.Connections.Select(x => new Connection()
            {
                SourceId = x.SourceId,
                DestinationId = x.DestinationId,
                Weight = x.Weight,
                Integrator = x.Integrator,
            }).ToArray();

            List<List<Node>> hiddenLayers = CreateHiddenLayers(inputs, hidden, outputs, connections);

            foreach (var connection in connections)
            {
                connection.IsRecurrent = Influences(connection.SourceId, connection.DestinationId);
            }

            Draw(e.Graphics, inputs, hiddenLayers, outputs, connections);

            bool HasConnections(int id)
            {
                return Genome.Connections.Any(x => x.SourceId == id || x.DestinationId == id);
            }

            bool Influences(int sourceId, int targetId, HashSet<int> visitedIds = null)
            {
                if (visitedIds == null) visitedIds = new HashSet<int>();

                // if node has already been visited, return false
                if (!visitedIds.Add(sourceId)) return false;

                foreach (var conn in Genome.Connections.Where(c => c.SourceId == sourceId))
                {
                    // checks the source is in the same or an earlier layer
                    if (_nodeLayers[conn.SourceId] >= _nodeLayers[targetId]) return true;
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    List<List<Node>> CreateHiddenLayers(Node[] inputs, Node[] allHidden, Node[] outputs, Connection[] connections)
    {
        _nodeLayers.Clear();

        // Note: Input nodes are on layer 0 and output nodes are on the highest layer.
        foreach (var node in inputs) _nodeLayers[node.Id] = 0;

        List<List<Node>> hiddenLayers = new List<List<Node>>();

        foreach (var node in allHidden)
        {
            // Find the layer where this node should be placed
            int layerIndex = connections.Where(c => c.DestinationId == node.Id)
                .Select(c =>
                {
                    if (inputs.Any(x => x.Id == c.SourceId)) return 0;
                    if (allHidden.Any(x => x.Id == c.SourceId)) return hiddenLayers.FindIndex(layer => layer.Any(n => n.Id == c.SourceId)) + 1;
                    return -1;
                })
                .DefaultIfEmpty(-1)
                .Max() + 1;

            // Ensure there are enough layers
            while (hiddenLayers.Count <= layerIndex)
            {
                hiddenLayers.Add(new List<Node>());
            }

            // Add the node to the appropriate layer
            hiddenLayers[layerIndex].Add(node);
            _nodeLayers[node.Id] = layerIndex;
        }

        foreach (var node in outputs) _nodeLayers[node.Id] = hiddenLayers.Count;

        hiddenLayers = hiddenLayers.Where(x => x.Any()).ToList();

        return hiddenLayers;
    }

    private void Draw(Graphics g, Node[] inputs, List<List<Node>> hiddenLayers, Node[] outputs, Connection[] connections)
    {
        Dictionary<int, Point> nodeLocations = new Dictionary<int, Point>();

        var totalLayers = hiddenLayers.Count + 2;

        // Draw the nodes
        DrawLayer(g, 0, inputs, nodeLocations, totalLayers);
        for (int i = 0; i < hiddenLayers.Count; i++)
        {
            DrawLayer(g, i + 1, hiddenLayers[i].ToArray(), nodeLocations, totalLayers);
        }
        DrawLayer(g, hiddenLayers.Count + 1, outputs, nodeLocations, totalLayers);

        // Draw the connections
        DrawConnections(g, connections, nodeLocations);

        // Draw the nodes again
        DrawLayer(g, 0, inputs, nodeLocations, totalLayers);
        for (int i = 0; i < hiddenLayers.Count; i++)
        {
            DrawLayer(g, i + 1, hiddenLayers[i].ToArray(), nodeLocations, totalLayers);
        }
        DrawLayer(g, hiddenLayers.Count + 1, outputs, nodeLocations, totalLayers);
    }

    private void DrawConnections(Graphics g, Connection[] connections, Dictionary<int, Point> nodeLocations)
    {
        foreach (var connection in connections.OrderBy(x => x.IsRecurrent).ThenByDescending(x => x.Weight))
        {
            Point sourceLocation = nodeLocations[connection.SourceId];
            Point destinationLocation = nodeLocations[connection.DestinationId];

            var pen = connection.Weight > 0 ? new Pen(Color.Blue) : new Pen(Color.Red);

            if (connection.Integrator == ConnectionIntegrator.Modulate)
            {
                pen = new Pen(Color.MediumPurple)
                {
                    DashStyle = DashStyle.Dot,
                };
            }

            pen.Width = (float)Math.Min(3, Math.Max(1, Math.Abs(connection.Weight)));

            if (connection.IsRecurrent)
            {
                var horizontalDistance = destinationLocation.X - sourceLocation.X;
                var isHorizontallyAligned = Math.Abs(horizontalDistance) < NodeDiameter;

                var arcSize = 16;

                if (isHorizontallyAligned)
                {
                    if (connection.SourceId == connection.DestinationId) arcSize = 12;

                    Point[] points = new Point[]
                    {
                            sourceLocation with
                            {
                                Y = sourceLocation.Y + NodeDiameter / 2
                            },
                            sourceLocation with
                            {
                                X = sourceLocation.X - arcSize,
                                Y = sourceLocation.Y + arcSize
                            },
                            destinationLocation with
                            {
                                X = destinationLocation.X - arcSize,
                                Y = destinationLocation.Y - arcSize
                            },
                            destinationLocation with
                            {
                                Y = destinationLocation.Y - NodeDiameter / 2
                            },
                    };

                    g.DrawCurve(pen, points);
                }
                else if (horizontalDistance >= 0)
                {
                    Point[] points = new Point[]
                    {
                            sourceLocation with
                            {
                                Y = sourceLocation.Y + NodeDiameter / 2
                            },
                            sourceLocation with
                            {
                                X = sourceLocation.X + arcSize * 2,
                                Y = sourceLocation.Y + arcSize
                            },
                            destinationLocation with
                            {
                                X = destinationLocation.X - arcSize * 2,
                                Y = destinationLocation.Y - arcSize
                            },
                            destinationLocation with
                            {
                                Y = destinationLocation.Y - NodeDiameter / 2
                            },
                    };

                    g.DrawCurve(pen, points);
                }
                else
                {
                    Point[] points = new Point[]
                    {
                            sourceLocation with
                            {
                                Y = sourceLocation.Y + NodeDiameter / 2
                            },
                            sourceLocation with
                            {
                                X = sourceLocation.X - arcSize * 2,
                                Y = sourceLocation.Y + arcSize
                            },
                            destinationLocation with
                            {
                                X = destinationLocation.X + arcSize * 2,
                                Y = destinationLocation.Y - arcSize
                            },
                            destinationLocation with
                            {
                                Y = destinationLocation.Y - NodeDiameter / 2
                            },
                    };

                    g.DrawCurve(pen, points);
                }
            }
            else
            {
                var src = nodeLocations[connection.SourceId];
                var dst = nodeLocations[connection.DestinationId];
                g.DrawLine(pen,
                    src with { Y = src.Y + NodeDiameter / 2 },
                    dst with { Y = dst.Y - NodeDiameter / 2 });
            }
        }
    }

    private void DrawLayer(Graphics g, int layer, Node[] nodes, Dictionary<int, Point> nodeLocations, int totalLayers)
    {
        int totalHeight = this.Height;
        int totalWidth = this.Width;

        int padding = totalHeight / (totalLayers + 1); // Adjust the padding based on the total number of layers
        int LayerDistance = (totalHeight - 2 * padding) / totalLayers; // Redistribute the LayerDistance based on the new padding

        int nodeDistance = totalWidth / (nodes.Length + 1); // distribute nodes evenly across the viewport
        int startX = nodeDistance; // start from the first gap
        int y = layer * LayerDistance + padding; // y-location based on layer

        // Create a new semi-bold font
        Font f = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular);
        Font fb = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold);

        foreach (Node node in nodes)
        {
            Point location = new Point(startX, y);

            g.FillRectangle(Brushes.White, new Rectangle(location, new Size(NodeDiameter, NodeDiameter)));
            g.DrawRectangle(Pens.Black, new Rectangle(location, new Size(NodeDiameter, NodeDiameter)));

            nodeLocations[node.Id] = new Point(startX + NodeDiameter / 2, y + NodeDiameter / 2); // Save the center of the node

            // Calculate the position for the top label
            PointF labelPosition = new PointF(startX - NodeDiameter, y + NodeDiameter / 2);

            var brush = node.HasConnections ? Brushes.Black : Brushes.LightGray;

            // Translate to side label position
            g.TranslateTransform(labelPosition.X, labelPosition.Y);
            g.DrawString(node.Id.ToString(), f, brush, new PointF(-18, -9));
            g.ResetTransform();

            if (!string.IsNullOrEmpty(node.TopLabel))
            {
                // Translate to top label position and rotate
                g.TranslateTransform(labelPosition.X, labelPosition.Y);
                g.RotateTransform(-90);
                g.DrawString(node.TopLabel, f, brush, new PointF(10, 6));
                g.ResetTransform();
            }

            if (!string.IsNullOrEmpty(node.AggregationFunction))
            {
                // Translate to side label position
                g.TranslateTransform(labelPosition.X, labelPosition.Y);
                g.DrawString(node.AggregationFunction, f, brush, new PointF(24, -16));
                g.ResetTransform();
            }

            if (!string.IsNullOrEmpty(node.ActivationFunction))
            {
                // Translate to side label position
                g.TranslateTransform(labelPosition.X, labelPosition.Y);
                g.DrawString(node.ActivationFunction, fb, brush, new PointF(24, -2));
                g.ResetTransform();
            }

            if (!string.IsNullOrEmpty(node.BottomLabel))
            {
                // Translate to bottom label position and rotate
                g.TranslateTransform(labelPosition.X, labelPosition.Y);
                g.RotateTransform(90);
                g.DrawString(node.BottomLabel, f, brush, new PointF(10, -24));
                g.ResetTransform();
            }

            startX += nodeDistance; // Move x-location for the next node
        }
    }

    internal class Node
    {
        public Node(int id, bool hasConnections)
        {
            Id = id;
            HasConnections = hasConnections;
        }

        public int Id { get; }
        public bool HasConnections { get; }
        public string TopLabel { get; set; }
        public string AggregationFunction { get; set; }
        public string ActivationFunction { get; set; }
        public string BottomLabel { get; set; }
    }

    internal class Connection
    {
        public bool IsRecurrent { get; set; }

        public int SourceId { get; set; }

        public int DestinationId { get; set; }

        public double Weight { get; set; }

        /// <summary>
        /// Describes how this connection is applied to its destination.
        /// </summary>
        public ConnectionIntegrator Integrator { get; set; }
    }
}
