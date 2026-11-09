#nullable disable

namespace Expenses.Tests.Scratch;

partial class HostedScalingScratchView
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _layout = new TableLayoutPanel();
        _group = new GroupBox();
        _fields = new TableLayoutPanel();
        _caption = new Label();
        _numberCaption = new Label();
        _dateCaption = new Label();
        _choiceCaption = new Label();
        _input = new TextBox();
        _number = new NumericUpDown();
        _date = new MaskedTextBox();
        _choice = new ComboBox();
        ((System.ComponentModel.ISupportInitialize)_number).BeginInit();
        _layout.SuspendLayout();
        _group.SuspendLayout();
        _fields.SuspendLayout();
        SuspendLayout();
        _layout.AutoSize = true;
        _layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layout.Dock = DockStyle.Fill;
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.RowCount = 1;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(_group, 0, 0);
        _layout.Name = "ViewLayout";
        _group.Text = "Hosted view";
        _group.AutoSize = true;
        _group.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _group.Dock = DockStyle.Fill;
        _group.Name = "InputGroup";
        _group.Controls.Add(_fields);
        _fields.AutoSize = true;
        _fields.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _fields.Dock = DockStyle.Fill;
        _fields.ColumnCount = 2;
        _fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _fields.RowCount = 4;
        _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _fields.Padding = new Padding(8);
        _fields.Name = "FieldsLayout";
        _fields.Controls.Add(_caption, 0, 0);
        _fields.Controls.Add(_input, 1, 0);
        _fields.Controls.Add(_numberCaption, 0, 1);
        _fields.Controls.Add(_number, 1, 1);
        _fields.Controls.Add(_dateCaption, 0, 2);
        _fields.Controls.Add(_date, 1, 2);
        _fields.Controls.Add(_choiceCaption, 0, 3);
        _fields.Controls.Add(_choice, 1, 3);
        _caption.AutoSize = true;
        _caption.Anchor = AnchorStyles.Left;
        _caption.Text = "&Name:";
        _caption.Name = "NameLabel";
        _numberCaption.AutoSize = true;
        _numberCaption.Anchor = AnchorStyles.Left;
        _numberCaption.Text = "&Quantity:";
        _numberCaption.Name = "QuantityLabel";
        _dateCaption.AutoSize = true;
        _dateCaption.Anchor = AnchorStyles.Left;
        _dateCaption.Text = "&Date:";
        _dateCaption.Name = "DateLabel";
        _choiceCaption.AutoSize = true;
        _choiceCaption.Anchor = AnchorStyles.Left;
        _choiceCaption.Text = "&Choice:";
        _choiceCaption.Name = "ChoiceLabel";
        _input.Dock = DockStyle.Fill;
        _input.MinimumSize = new Size(160, 0);
        _input.Name = "NameInput";
        _input.AccessibleName = "Name";
        _number.Dock = DockStyle.Fill;
        _number.Minimum = 1;
        _number.Maximum = 100;
        _number.MinimumSize = new Size(80, 0);
        _number.Name = "QuantityInput";
        _number.AccessibleName = "Quantity";
        _date.Dock = DockStyle.Fill;
        _date.Mask = "00/00/0000";
        _date.MinimumSize = new Size(150, 0);
        _date.Name = "DateInput";
        _date.AccessibleName = "Date";
        _choice.Dock = DockStyle.Fill;
        _choice.DropDownStyle = ComboBoxStyle.DropDownList;
        _choice.Items.Add("Short");
        _choice.Items.Add("A longer option text");
        _choice.MinimumSize = new Size(160, 0);
        _choice.Name = "ChoiceInput";
        _choice.AccessibleName = "Choice";
        AutoScaleMode = AutoScaleMode.Inherit;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Name = "HostedScalingScratchView";
        Size = new Size(600, 150);
        Controls.Add(_layout);
        ((System.ComponentModel.ISupportInitialize)_number).EndInit();
        _fields.ResumeLayout(false);
        _fields.PerformLayout();
        _group.ResumeLayout(false);
        _group.PerformLayout();
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _layout;
    private GroupBox _group;
    private TableLayoutPanel _fields;
    private Label _caption;
    private Label _numberCaption;
    private Label _dateCaption;
    private Label _choiceCaption;
    private TextBox _input;
    private NumericUpDown _number;
    private MaskedTextBox _date;
    private ComboBox _choice;
}
