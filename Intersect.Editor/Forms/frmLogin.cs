using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Eto.Drawing;
using Eto.Forms;
using Intersect.Configuration;
using Intersect.Editor.Content;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Framework.Core;
using Intersect.Network;
using Intersect.Utilities;
using Intersect.Web;
using Microsoft.Extensions.Logging;

namespace Intersect.Editor.Forms;


public partial class FrmLogin : Form
{

    //Cross Thread Delegates
    public delegate void BeginEditorLoop();

    public BeginEditorLoop EditorLoopDelegate;

    private readonly bool _authenticating;

    private bool _optionsLoaded;
    private string _savedPassword = string.Empty;
    private bool _loginPending;
    private TokenResultType? _tokenResultType;
    private TokenResponse? _tokenResponse;

    // Controls
    private TextBox txtUsername;
    private PasswordBox txtPassword;
    private CheckBox chkRemember;
    private Button btnLogin;
    private Label lblStatus;
    private Label lblVersion;
    private Label lblGettingStarted;
    private Label lblUsername;
    private Label lblPassword;
    private UITimer tmrSocket;

    public FrmLogin(bool authenticating)
    {
        _authenticating = authenticating;
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = "Login";
        MinimumSize = new Size(400, 480);
        Size = new Size(400, 480);
        Resizable = false;

        txtUsername = new TextBox();
        txtPassword = new PasswordBox();
        chkRemember = new CheckBox { Text = "" };
        btnLogin = new Button { Text = "Login" };
        lblStatus = new Label { Text = "" };
        lblVersion = new Label { Text = "" };
        lblGettingStarted = new Label { Text = "Getting Started" };
        lblUsername = new Label { Text = "Username" };
        lblPassword = new Label { Text = "Password" };
        tmrSocket = new UITimer { Interval = 0.05 };

        var layout = new DynamicLayout();
        layout.Padding = new Padding(20);
        layout.DefaultSpacing = new Size(5, 5);

        layout.Add(lblVersion);
        layout.Add(lblGettingStarted);
        layout.AddSpace();

        layout.Add(lblUsername);
        layout.Add(txtUsername);

        layout.Add(lblPassword);
        layout.Add(txtPassword);

        layout.Add(chkRemember);
        layout.AddSpace();

        layout.Add(btnLogin);
        layout.AddSpace();

        layout.Add(lblStatus);

        Content = layout;

        // Event handlers
        Load += frmLogin_Load;
        tmrSocket.Elapsed += tmrSocket_Tick;
        btnLogin.Click += btnLogin_Click;
        txtPassword.KeyDown += txtPassword_KeyDown;
        txtUsername.KeyDown += txtUsername_KeyDown;
        KeyDown += FrmLogin_KeyDown;

        Closed += OnFormClosed;
    }

    private void frmLogin_Load(object sender, EventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += Program.CurrentDomain_UnhandledException;
        try
        {
            Strings.Load();
        }
        catch (Exception exception)
        {
            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError(exception, "Error loading strings");
            throw;
        }

        InitLocalization();

        if (Preferences.LoadPreference("username").Trim().Length > 0)
        {
            txtUsername.Text = Preferences.LoadPreference("Username");
            txtPassword.Text = "*****";
            _savedPassword = Preferences.LoadPreference("Password");
            chkRemember.Checked = true;
        }

        lblStatus.Visible = !_authenticating;
        if (_authenticating)
        {
            return;
        }

        GameContentManager.CheckForResources();
        Database.LoadOptions();
        _optionsLoaded = true;
        EditorLoopDelegate = Main.StartLoop;

        Database.InitMapCache();

        tmrSocket.Start();
    }

    private void InitLocalization()
    {
        Title = Strings.Login.title;
        lblVersion.Text = Strings.Login.version.ToString("1.0.0");
        lblGettingStarted.Text = Strings.Login.gettingstarted;
        lblUsername.Text = Strings.Login.username;
        lblPassword.Text = Strings.Login.password;
        chkRemember.Text = Strings.Login.rememberme;
        btnLogin.Text = Strings.Login.login;
        lblStatus.Text = Strings.Login.connecting;
    }

    public long LastNetworkStatusChangeTime { get; private set; }

    private NetworkStatus _networkStatus;

    public void SetNetworkStatus(NetworkStatus networkStatus)
    {
        _networkStatus = networkStatus;
        LastNetworkStatusChangeTime = Timing.Global.MillisecondsUtc;
    }

    private void tmrSocket_Tick(object sender, EventArgs e)
    {
        if (_authenticating)
        {
            if (_tokenResultType == TokenResultType.TokenReceived && _tokenResponse != default)
            {
                tmrSocket.Stop();
                Visible = false;
                Globals.UpdateForm.ShowWithToken(_tokenResponse);
            }

            return;
        }

        if (!_optionsLoaded)
        {
            return;
        }

        Networking.Network.Update();
        btnLogin.Enabled = _networkStatus == NetworkStatus.Online || Networking.Network.Connected;

        string statusString = _networkStatus switch
        {
            NetworkStatus.Unknown => Strings.Login.Denied,
            NetworkStatus.Connecting => Strings.Login.connecting,
            NetworkStatus.Online => Strings.Login.connected,
            NetworkStatus.Offline => Strings.Login.failedtoconnect.ToString(((Globals.NextServerStatusPing - Timing.Global.MillisecondsUtc) / 1000).ToString("0")),
            NetworkStatus.Failed => Strings.Login.Denied,
            NetworkStatus.VersionMismatch => Strings.Login.Denied,
            NetworkStatus.ServerFull => Strings.Login.Denied,
            NetworkStatus.HandshakeFailure => Strings.Login.Denied,
            NetworkStatus.Quitting => Strings.Login.Denied,
            _ => throw new UnreachableException(),
        };

        var hasRaptr = Process.GetProcesses()
            .ToArray()
            .Any(process => process.ProcessName.Contains("raptr"));
        if (hasRaptr)
        {
            statusString = Strings.Login.raptr;
            btnLogin.Enabled = false;
        }

        Globals.LoginForm.lblStatus.Text = statusString;

        if (_loginPending && Networking.Network.Connected)
        {
            _loginPending = false;
            if (txtUsername.Text.Trim().Length > 0 && txtPassword.Text.Trim().Length > 0)
            {
                using (var sha = new SHA256Managed())
                {
                    if (_savedPassword != "")
                    {
                        PacketSender.SendLogin(txtUsername.Text.Trim(), _savedPassword);
                    }
                    else
                    {
                        PacketSender.SendLogin(
                            txtUsername.Text.Trim(),
                            BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(txtPassword.Text.Trim())))
                                .Replace("-", "")
                        );
                    }
                }
            }
        }
    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        if (_authenticating)
        {
            _loginPending = true;
            btnLogin.Enabled = false;
            Task.Run(() =>
            {
                using IntersectHttpClient httpClient = new(ClientConfiguration.Instance.UpdateUrl);
                var hashed = !string.IsNullOrWhiteSpace(_savedPassword);
                _tokenResultType = httpClient.TryRequestToken(
                    txtUsername.Text,
                    hashed ? _savedPassword : txtPassword.Text,
                    out _tokenResponse,
                    hashed: hashed
                );
                if (_tokenResultType != TokenResultType.TokenReceived || _tokenResponse == default)
                {
                    _loginPending = false;
                    btnLogin.Enabled = true;
                }
            });
            return;
        }

        // Create the main editor form if not already created
        Console.WriteLine($"frmLogin: About to check MainForm (current: {(Globals.MainForm == null ? "null" : "exists")})");
        if (Globals.MainForm == null)
        {
            Console.WriteLine("frmLogin: Creating new FrmMain");
            Globals.MainForm = new FrmMain();
            Console.WriteLine("frmLogin: FrmMain created");
        }

        if (!Networking.Network.Connected)
        {
            Networking.Network.Connect();
        }

        _loginPending = true;
        btnLogin.Enabled = false;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Keys.Enter)
        {
            e.Handled = true;
            btnLogin_Click(null, null);
        }
    }

    private void OnFormClosed(object sender, EventArgs e)
    {
        if (_authenticating)
        {
            return;
        }

        Networking.Network.EditorLidgrenNetwork?.Disconnect(NetworkStatus.Quitting.ToString());
        Application.Instance.Quit();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
    }

    public void TryRemembering()
    {
        using (var sha = new SHA256Managed())
        {
            if (chkRemember.Checked == true)
            {
                Preferences.SavePreference("Username", txtUsername.Text);
                if (_savedPassword != "")
                {
                    Preferences.SavePreference("Password", _savedPassword);
                }
                else
                {
                    Preferences.SavePreference(
                        "Password",
                        BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(txtPassword.Text.Trim())))
                            .Replace("-", "")
                    );
                }
            }
            else
            {
                Preferences.SavePreference("Username", "");
                Preferences.SavePreference("Password", "");
            }
        }
    }

    private void txtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Enter)
        {
            return;
        }

        if (_savedPassword != "")
        {
            _savedPassword = string.Empty;
            txtPassword.Text = string.Empty;
            chkRemember.Checked = false;
        }
    }

    private void txtUsername_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Enter)
        {
            return;
        }

        if (_savedPassword != "")
        {
            _savedPassword = string.Empty;
            txtUsername.Text = string.Empty;
            txtPassword.Text = string.Empty;
            chkRemember.Checked = false;
        }
    }

    private void FrmLogin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Keys.F1)
        {
            var optionsForm = new FrmOptions();
            optionsForm.Show();
        }
    }

    public void HideSafe()
    {
        ShowSafe(false);
    }

    public void ShowSafe(bool show = true)
    {
        Application.Instance.Invoke(() =>
        {
            if (show)
            {
                Show();
            }
            else
            {
                Visible = false;
            }
        });
    }

}
