using System;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace FluentStorage.Git.Storage;

/// <summary>
/// Options controlling how a <see cref="GitStore"/> clones, reads, writes and commits a git repository.
/// </summary>
public class GitStorageOptions {

	/// <summary>
	/// URL of the remote git repository (HTTP/HTTPS or a local path). Required.
	/// </summary>
	public string Url { get; set; }

	/// <summary>
	/// Username used for HTTPS authentication.
	/// </summary>
	public string UserName { get; set; }

	/// <summary>
	/// Password used for HTTPS authentication.
	/// </summary>
	public string Password { get; set; }

	/// <summary>
	/// Personal access token used for HTTPS authentication. When set, it overrides <see cref="Password"/>.
	/// </summary>
	public string Token { get; set; }

	/// <summary>
	/// Branch to clone and work on. When null, the remote's default branch is used.
	/// </summary>
	public string Branch { get; set; }

	/// <summary>
	/// Root sub-folder within the repository that acts as the FluentStorage root. All paths resolve relative to it.
	/// When null or empty, the repository root is used.
	/// </summary>
	public string RootPath { get; set; }

	/// <summary>
	/// When true, each write operation (set/delete/move) automatically commits the changes.
	/// </summary>
	public bool AutoCommit { get; set; }

	/// <summary>
	/// When true, each write operation automatically commits and pushes the changes to the remote.
	/// Implies <see cref="AutoCommit"/>.
	/// </summary>
	public bool AutoPush { get; set; }

	/// <summary>
	/// When true, the repository is pulled from the remote before the first write and before each commit. Default: true.
	/// </summary>
	public bool PullBeforeWrite { get; set; } = true;

	/// <summary>
	/// Name of the commit author. Default: "FluentStorage".
	/// </summary>
	public string CommitAuthorName { get; set; } = "FluentStorage";

	/// <summary>
	/// Email of the commit author. Default: "fluentstorage@example.com".
	/// </summary>
	public string CommitAuthorEmail { get; set; } = "fluentstorage@example.com";

	/// <summary>
	/// Commit message used by automatic commits (see <see cref="AutoCommit"/>/<see cref="AutoPush"/>).
	/// Default: "FluentStorage automatic commit".
	/// </summary>
	public string DefaultCommitMessage { get; set; } = "FluentStorage automatic commit";

	/// <summary>
	/// Local directory used to store the working copy. When null, a temporary directory is created.
	/// When provided, an existing clone at this path is reused and updated with a pull.
	/// </summary>
	public string LocalWorkingDirectory { get; set; }

	/// <summary>
	/// When true and <see cref="LocalWorkingDirectory"/> is null, the temporary working directory is deleted on dispose.
	/// </summary>
	public bool DeleteLocalOnDispose { get; set; } = true;

	/// <summary>
	/// Optional custom <see cref="CloneOptions"/> used when cloning the repository.
	/// </summary>
	public CloneOptions CloneOptions { get; set; }

	/// <summary>
	/// Optional custom <see cref="FetchOptions"/> used when pulling/fetching.
	/// </summary>
	public FetchOptions FetchOptions { get; set; }

	/// <summary>
	/// Optional custom <see cref="PushOptions"/> used when pushing.
	/// </summary>
	public PushOptions PushOptions { get; set; }

	/// <summary>
	/// Optional handler to validate the server certificate. Defaults to LibGit2Sharp's validation.
	/// </summary>
	public CertificateCheckHandler CertificateCheck { get; set; }

	/// <summary>
	/// Optional pre-built LibGit2Sharp credentials. When null, credentials are built from <see cref="UserName"/> and
	/// <see cref="Password"/> or <see cref="Token"/>.
	/// </summary>
	public Credentials Credentials { get; set; }

	internal CredentialsHandler BuildCredentialsProvider() {
		Credentials creds = Credentials;

		if (creds == null) {
			string password = Token ?? Password;
			string userName = string.IsNullOrEmpty(UserName) ? (Token ?? "token") : UserName;

			creds = new UsernamePasswordCredentials {
				Username = userName,
				Password = password
			};
		}

		return (_, usernameFromUrl, types) => creds;
	}

	internal Signature BuildSignature() {
		return new Signature(CommitAuthorName, CommitAuthorEmail, DateTimeOffset.Now);
	}
}