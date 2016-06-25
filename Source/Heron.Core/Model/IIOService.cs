
namespace Heron.Core.Model {
    public interface IIOService {

        bool FolderExists(string folderPath);

        bool CreateFolder(string folderPath);

        string GetFolderPath();

        string[] GetFiles(string folderPath, string extensions);

        string GetFileName(string imagePath);
    }
}
