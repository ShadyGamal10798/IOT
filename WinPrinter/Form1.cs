using System;
using System.Drawing;
using System.IO.Pipes;
using System.Windows.Forms;

namespace WinPrinter
{
    public partial class Form1 : Form
    {
        private TextBox textBoxDevice1;
        private TextBox textBoxDevice2;
        private Label labelDevice1;
        private Label labelDevice2;
        private Label labelCountDevice1;
        private Label labelCountDevice2;
        private int countDevice1 = 0;
        private int countDevice2 = 0;
        private Button buttonLockDevice1;
        private Button buttonUnlockDevice1;
        private Button buttonLockDevice2;
        private Button buttonUnlockDevice2;

        public Form1()
        {
            InitializeComponent();
            InitializeTextBoxes();
            InitializeLabels();
            InitializeButtons();
            StartListeningForMessages();
        }

        private void InitializeTextBoxes()
        {
            // Initialize and configure the TextBox for Device 1
            textBoxDevice1 = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(10, 50), // Adjusted vertical position
                Size = new Size(this.ClientSize.Width / 2 - 20, (int)(this.ClientSize.Height * 0.75)),
            };

            // Initialize and configure the TextBox for Device 2
            textBoxDevice2 = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(this.ClientSize.Width / 2 + 10, 50), // Adjusted vertical position
                Size = new Size(this.ClientSize.Width / 2 - 20, (int)(this.ClientSize.Height * 0.75)),
            };

            // Add the text boxes to the form
            this.Controls.Add(textBoxDevice1);
            this.Controls.Add(textBoxDevice2);
        }

        private void InitializeLabels()
        {
            // Initialize and configure the Label for Device 1
            labelDevice1 = new Label
            {
                Text = "Device 1",
                Location = new Point(10, 0), // Adjusted for visibility
                AutoSize = true
            };

            // Initialize and configure the Label for Device 2
            labelDevice2 = new Label
            {
                Text = "Device 2",
                Location = new Point(this.ClientSize.Width / 2 + 10, 0), // Adjusted for visibility
                AutoSize = true
            };
            // Initialize and configure the Label for the count of Device 1 messages
            labelCountDevice1 = new Label
            {
                Text = "Messages: 0",
                Location = new Point(10, 30), // Adjusted for visibility above the text box
                AutoSize = true
            };

            // Initialize and configure the Label for the count of Device 2 messages
            labelCountDevice2 = new Label
            {
                Text = "Messages: 0",
                Location = new Point(this.ClientSize.Width / 2 + 10, 30), // Adjusted for visibility above the text box
                AutoSize = true
            };



            // Add the labels to the form
            this.Controls.Add(labelDevice1);
            this.Controls.Add(labelDevice2);
            // Add the count labels to the form
            this.Controls.Add(labelCountDevice1);
            this.Controls.Add(labelCountDevice2);
        }
        private void InitializeButtons()
        {
            // Initialize and configure the Lock Button for Device 1
            buttonLockDevice1 = new Button
            {
                Text = "Lock Device 1",
                Location = new Point(10, textBoxDevice1.Bottom + 10),
                Size = new Size(100, 30),
            };
            buttonLockDevice1.Click += (sender, e) => SendCommand("863540062368775", "00000000000000140C01050000000C7365746469676F75742031300100002ED4"); // Replace with actual IMEI

            // Initialize and configure the Unlock Button for Device 1
            buttonUnlockDevice1 = new Button
            {
                Text = "Unlock Device 1",
                Location = new Point(120, textBoxDevice1.Bottom + 10),
                Size = new Size(100, 30),
            };
            buttonUnlockDevice1.Click += (sender, e) => SendCommand("863540062368775", "00000000000000140C01050000000C7365746469676F75742030310100007E84"); // Replace with actual IMEI

            // Initialize and configure the Lock Button for Device 2
            buttonLockDevice2 = new Button
            {
                Text = "Lock Device 2",
                Location = new Point(textBoxDevice2.Left, textBoxDevice2.Bottom + 10),
                Size = new Size(100, 30),
            };
            buttonLockDevice2.Click += (sender, e) => SendCommand("864636060709553", "00000000000000140C01050000000C7365746469676F75742031300100002ED4"); // Replace with actual IMEI

            // Initialize and configure the Unlock Button for Device 2
            buttonUnlockDevice2 = new Button
            {
                Text = "Unlock Device 2",
                Location = new Point(textBoxDevice2.Left + 110, textBoxDevice2.Bottom + 10),
                Size = new Size(100, 30),
            };
            buttonUnlockDevice2.Click += (sender, e) => SendCommand("864636060709553", "00000000000000140C01050000000C7365746469676F75742030310100007E84"); // Replace with actual IMEI

            // Add the buttons to the form
            this.Controls.AddRange(new Control[] { buttonLockDevice1, buttonUnlockDevice1, buttonLockDevice2, buttonUnlockDevice2 });
        }
        private async Task SendCommand(string imei, string command)
        {
            using (var client = new NamedPipeClientStream(".", "CommandPipe", PipeDirection.Out))
            {
                try
                {
                    MessageBox.Show($"Attempting to send {command} command to IMEI: {imei}"); // For testing
                    client.Connect(1000); // Timeout to avoid hanging
                    using (var writer = new StreamWriter(client))
                    {
                        writer.AutoFlush = true;
                        string formattedCommand = $"COMMAND:{imei}:{command}"; // Format the command
                        await writer.WriteAsync(formattedCommand); // Write the command to the pipe
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Failed to connect to the pipe server.");
                }
            }
        }

        // ... existing StartListeningForMessages and UpdateMessage methods ...
    


private void UpdateMessage(string colorCode, string message)
        {
            this.Invoke(new Action(() =>
            {
                if (colorCode == "green")
                {
                    textBoxDevice1.AppendText(message + Environment.NewLine);
                    countDevice1++;
                    labelCountDevice1.Text = $"Messages: {countDevice1}";
                }
                else if (colorCode == "blue")
                {
                    textBoxDevice2.AppendText(message + Environment.NewLine);
                    countDevice2++;
                    labelCountDevice2.Text = $"Messages: {countDevice2}";
                }
            }));
        }

        private async void StartListeningForMessages()
        {
            while (true)
            {
                using (var server = new NamedPipeServerStream("WinPrinterPipe", PipeDirection.In))
                {
                    await server.WaitForConnectionAsync();

                    using (var reader = new StreamReader(server))
                    {
                        string message = await reader.ReadToEndAsync();
                        var parts = message.Split(new[] { ':' }, 2);
                        if (parts.Length == 2)
                        {
                            UpdateMessage(parts[0], parts[1]);
                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
