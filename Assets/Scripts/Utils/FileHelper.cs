using System.IO;

public class FileHelper
{
    public string Load(string path)
    {
        string content = File.ReadAllText(path);

        return content;
    }
    
    public void Save(string path, string content)
    {
        File.WriteAllText(path, content);
    }
}
