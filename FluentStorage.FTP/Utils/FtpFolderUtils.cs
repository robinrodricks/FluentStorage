using System.Collections.Generic;
using FluentFTP;
using FluentStorage.Enums;
using FluentStorage.Model;

namespace FluentStorage.FTP.Utils;

public static class FtpFolderUtils {

	public static Dictionary<StorageExists, FtpRemoteExists> UploadFolderMap = new Dictionary<StorageExists, FtpRemoteExists> {
		[StorageExists.Skip] = FtpRemoteExists.Skip,
		[StorageExists.Overwrite] = FtpRemoteExists.Overwrite,
	};
	public static Dictionary<StorageExists, FtpLocalExists> DownloadFolderMap = new Dictionary<StorageExists, FtpLocalExists> {
		[StorageExists.Skip] = FtpLocalExists.Skip,
		[StorageExists.Overwrite] = FtpLocalExists.Overwrite,
	};

	public static StorageProgress ConvertProgress(FtpProgress progress) {
		return new StorageProgress {
			Progress = progress.Progress,
			TransferredBytes = progress.TransferredBytes,
			TransferSpeed = progress.TransferSpeed,
			ETA = progress.ETA,
			LocalPath = progress.LocalPath,
			RemotePath = progress.RemotePath,
			FileIndex = progress.FileIndex,
			FileCount = progress.FileCount
		};
	}


}