using System.Diagnostics;

namespace IE_NFOW
{
    public partial class Window : Form
    {
        private readonly FOWManager FoW = new("Baldur");

        public Window()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            if (FoW.IsEnabled)
            {
                button_Switch.Text = "ENABLE FoW";
            }
            else
            {
                button_Switch.Text = "DISABLE FoW";
            }
        }

        private void Label_Info_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://baldursgate.fandom.com/wiki/Fog_of_War",
                UseShellExecute = true
            });
        }

        private void Button_Switch_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;

            if (FoW.Switch())
            {
                btn.Text = "ENABLE FoW";
            }
            else
            {
                btn.Text = "DISABLE FoW";
            }
        }
    }
}