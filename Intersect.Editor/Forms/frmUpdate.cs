using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Intersect.Configuration;
using Intersect.Editor.Content;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Framework;
using Intersect.Framework.Core.AssetManagement;
using Intersect.Framework.Utilities;
using Intersect.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ApplicationContext = Intersect.Core.ApplicationContext;

namespace Intersect.Editor.Forms;


public partial class FrmUpdate : Form
{

    private readonly object _manifestTaskLock = new();

    private long _nextUpdateAttempt;
    private Task? _pendingManifestTask;
    private TokenResponse? _tokenResponse;
    private Updater? _updater;
    private UpdaterStatus? _updaterStatus;

    // Controls
    private Label lblVersion;
    private Label lblStatus;
    private Label lblFiles;
    private Label lblSize;
    private ProgressBar progressBar;
    private UITimer tmrUpdate;

    public FrmUpdate()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = "Update";
        MinimumSize = new Size(500, 300);
        Size = new Size(500, 300);
        Resizable = false;

        lblVersion = new Label { Text = "" };
        lblStatus = new Label { Text = "" };
        lblFiles = new Label { Text = "", Visible = false };
        lblSize = new Label { Text = "", Visible = false };
        progressBar = new ProgressBar();
        tmrUpdate = new UITimer { Interval = 0.05 };

        var layout = new DynamicLayout();
        layout.Padding = new Padding(20);
        layout.DefaultSpacing = new Size(5, 5);

        layout.Add(lblVersion);
        layout.AddSpace();
        layout.Add(lblStatus);
        layout.Add(lblFiles);
        layout.Add(lblSize);
        layout.Add(progressBar);
        layout.AddSpace();

        Content = layout;

        // Event handlers
        Load += frmUpdate_Load;
        tmrUpdate.Elapsed += tmrUpdate_Tick;
        Shown += OnFormShown;
        Closed += OnFormClosed;
    }

    private void frmUpdate_Load(object sender, EventArgs e)
    {
        try
        {
            Strings.Load();
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, "Error loading strings");
            throw;
        }

        GameContentManager.CheckForResources();
        Database.LoadOptions();
        InitLocalization();


        if (ClientConfiguration.Instance.UpdateUrl is not { } updateUrl || string.IsNullOrWhiteSpace(updateUrl))
        {
            return;
        }

        _updater = new Updater(
            updateUrl,
            "editor/update.json",
            "version.editor.json",
            7
        );

        var rawTokenResponse = Preferences.LoadPreference(nameof(TokenResponse));
        if (string.IsNullOrWhiteSpace(rawTokenResponse))
        {
            return;
        }

        try
        {
            _tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(rawTokenResponse);
            _updater.SetAuthorizationData(_tokenResponse);
        }
        catch (Exception exception)
        {
            ApplicationContext.CurrentContext.Logger.LogError(
                exception,
                "Failed to deserialize token on disk, re-authentication will be necessary"
            );
        }
    }

    private void InitLocalization()
    {
        Title = Strings.Update.Title;
        lblVersion.Text = Strings.Login.version.ToString("1.0.0");
        lblStatus.Text = Strings.Update.Checking;
    }

    private void OnFormClosed(object sender, EventArgs e)
    {
        _updater?.Stop();
        Application.Instance.Quit();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
    }

    private void OnFormShown(object sender, EventArgs e)
    {
        if (_updater is null)
        {
            SwitchToLogin(requiresAuthentication: false, deferHide: true);
        }
        else
        {
            tmrUpdate.Start();
        }
    }

    private void SwitchToLogin(bool requiresAuthentication, bool deferHide = false)
    {
        lblFiles.Visible = false;
        lblSize.Visible = false;
        tmrUpdate.Stop();

        var loginForm = Globals.LoginForm ??= new FrmLogin(requiresAuthentication);

        _pendingManifestTask = null;

        try
        {
            loginForm.Show();

            Visible = false;
        }
        catch
        {
            throw;
        }
        finally
        {
        }
    }

    private void CheckForUpdate()
    {
        lock (_manifestTaskLock)
        {
            if (_pendingManifestTask != null)
            {
                return;
            }

            _pendingManifestTask = Task.Run(
                () =>
                {
                    _updaterStatus = _updater?.TryGetManifest(out _, force: _tokenResponse != null);
                    if (_updaterStatus == UpdaterStatus.Offline)
                    {
                        _nextUpdateAttempt = Environment.TickCount64 + 10_000;
                    }

                    _pendingManifestTask = null;
                }
            );
        }
    }

    internal void ShowWithToken(TokenResponse tokenResponse)
    {
        _tokenResponse = tokenResponse ?? throw new ArgumentNullException(nameof(tokenResponse));

        Preferences.SavePreference(nameof(TokenResponse), JsonConvert.SerializeObject(_tokenResponse));
        _updater?.SetAuthorizationData(_tokenResponse);

        _updaterStatus = null;

        lblFiles.Visible = true;
        lblSize.Visible = true;
        tmrUpdate.Start();

        Show();

        Globals.LoginForm?.Close();
        Globals.LoginForm = null;
    }

    private void tmrUpdate_Tick(object sender, EventArgs e)
    {
        if (_updater == null)
        {
            return;
        }

        switch (_updaterStatus)
        {
            case UpdaterStatus.NoUpdateNeeded:
                SwitchToLogin(false);
                return;
            case UpdaterStatus.NeedsAuthentication:
                SwitchToLogin(true);
                return;
            case UpdaterStatus.Ready:
                _nextUpdateAttempt = long.MinValue;
                _updaterStatus = null;
                _updater.Start();
                break;
            case UpdaterStatus.Offline:
                break;
            default:
                throw Exceptions.UnreachableInvalidEnum(_updaterStatus ?? default);
        }

        if (_nextUpdateAttempt != long.MinValue)
        {
            var now = Environment.TickCount64;
            if (now < _nextUpdateAttempt)
            {
                return;
            }

            _nextUpdateAttempt = now + 10_000;
            CheckForUpdate();
            return;
        }

        switch (_updater.Status)
        {
            case UpdateStatus.DownloadingManifest:
                lblStatus.Text = Strings.Update.Checking;
                break;
            case UpdateStatus.UpdateInProgress:
                lblFiles.Visible = true;
                lblSize.Visible = true;
                lblFiles.Text = Strings.Update.Files.ToString(_updater.FilesRemaining);
                lblSize.Text = Strings.Update.Size.ToString(Updater.GetHumanReadableFileSize(_updater.SizeRemaining));
                lblStatus.Text = Strings.Update.Updating.ToString((int)_updater.Progress);
                progressBar.Value = Math.Min(100, (int)_updater.Progress);
                break;
            case UpdateStatus.Restart:
                lblFiles.Visible = false;
                lblSize.Visible = false;
                progressBar.Value = 100;
                lblStatus.Text = Strings.Update.Restart.ToString();
                tmrUpdate.Stop();

                if (!ProcessHelper.TryRelaunch())
                {
                    ApplicationContext.CurrentContext.Logger.LogWarning("Failed to restart automatically");
                }

                Close();

                break;
            case UpdateStatus.UpdateCompleted:
                progressBar.Value = 100;
                lblStatus.Text = Strings.Update.Done;
                SwitchToLogin(false);
                break;
            case UpdateStatus.Error:
                lblFiles.Visible = false;
                lblSize.Visible = false;
                progressBar.Value = 100;
                lblStatus.Text = Strings.Update.Error.ToString(_updater.Exception?.Message ?? "");
                break;
            case UpdateStatus.None:
                SwitchToLogin(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
