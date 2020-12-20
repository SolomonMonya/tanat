using System.Collections.Generic;

public interface IDownloaderCreator
{
	Downloader CreateDownloader(IEnumerable<Downloader.Task> _tasks);
}
