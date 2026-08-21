namespace FluentStorage.Azure.Blobs.DataLake.Model;

class PathList {
	public Gen2Path[] Paths { get; set; }

	public override string ToString() => $"{Paths?.Length ?? 0}";
}