using System.Net;
using System.IO;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;


public class FTPUtils
{
    private string host = null;
    private string user = null;
    private string pass = null;
    private FtpWebRequest ftpRequest = null;
    private FtpWebResponse ftpResponse = null;
    private Stream ftpStream = null;
        
    /* Construct Object */

    public FTPUtils(string hostIP, string userName, string password) { host = hostIP; user = userName; pass = password; }

    private FtpWebRequest CreateWebRequest(string url,string method)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
        request.Credentials = new NetworkCredential(user, pass);
        request.Method = method;
        return request;
    }

    /* Delete File */
    public void DeleteFolder(string deleteFile)
    {
        try
        {
            var folderUrl =$"{host}/{deleteFile}";
            Debug.Log(folderUrl);
            FtpWebRequest ftpRequest = CreateWebRequest(folderUrl,WebRequestMethods.Ftp.ListDirectory);
            var response = ftpRequest.GetResponse();
            var ftpresponse =  (FtpWebResponse)response;

            var listFiles = new List<string>();
            using (var stream =ftpresponse.GetResponseStream())
            {
                using (var reader = new StreamReader(stream, true))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        listFiles.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }

            foreach(var file in listFiles)
            {
                var dUrl =$"{host}/cdn/{file}";
                FtpWebRequest delRequest = CreateWebRequest(dUrl,WebRequestMethods.Ftp.DeleteFile);
                var resp =  delRequest.GetResponse();
                resp.Close();
            }
            CreateWebRequest(folderUrl,WebRequestMethods.Ftp.RemoveDirectory).GetResponse();
        }
        catch
        {

        }
        Debug.Log("Delete ready");
    }

    /* Rename File */
    public void rename(string currentFileNameAndPath, string newFileName)
    {
        try
        {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentFileNameAndPath);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.Rename;
            /* Rename the File */
            ftpRequest.RenameTo = newFileName;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Resource Cleanup */
            ftpResponse.Close();
            ftpRequest = null;
        }
        catch (Exception ex) { Debug.Log(ex.ToString()); }
        return;
    }

    /* Create a New Directory on the FTP Server */
    public void createDirectory(string newDirectory)
    {
        try
        {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Resource Cleanup */
            ftpResponse.Close();
            ftpRequest = null;
        }
        catch (Exception ex) { Debug.Log(ex.ToString()); }
        return;
    }

    /* Get the Date/Time a File was Created */
    public string getFileCreatedDateTime(string fileName)
    {
        try
        {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);
            /* Store the Raw Response */
            string fileInfo = null;
            /* Read the Full Response Stream */
            try { fileInfo = ftpReader.ReadToEnd(); }
            catch (Exception ex) { Debug.Log(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return File Created Date Time */
            return fileInfo;
        }
        catch (Exception ex) { Debug.Log(ex.ToString()); }
        /* Return an Empty string Array if an Exception Occurs */
        return "";
    }

    /* Get the Size of a File */
    public string getFileSize(string fileName)
    {
        try
        {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);
            /* Store the Raw Response */
            string fileInfo = null;
            /* Read the Full Response Stream */
            try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
            catch (Exception ex) { Debug.Log(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return File Size */
            return fileInfo;
        }
        catch (Exception ex) { Debug.Log(ex.ToString()); }
        /* Return an Empty string Array if an Exception Occurs */
        return "";
    }

    /* List Directory Contents File/Folder Name Only */
    public string[] directoryListSimple(string directory)
    {
        try
        {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);
            /* Store the Raw Response */
            string directoryRaw = null;
            /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
            try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
            catch (Exception ex) { Debug.Log(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
            try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
            catch (Exception ex) { Debug.Log(ex.ToString()); }
        }
        catch (Exception ex) { Debug.Log(ex.ToString()); }
        /* Return an Empty string Array if an Exception Occurs */
        return new string[] { "" };
    }

    /* List Directory Contents in Detail (Name, Size, Created, etc.) */
    public string[] directoryListDetailed(string directory)
    {
        try
        {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);
            /* Store the Raw Response */
            string directoryRaw = null;
            /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
            try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
            catch (Exception ex) { Debug.Log(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
            try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
            catch (Exception ex) { Debug.Log(ex.ToString()); }
        }
        catch (Exception ex) { Debug.Log(ex.ToString()); }
        /* Return an Empty string Array if an Exception Occurs */
        return new string[] { "" };
    }

#region UPLOADING
    public async Task UploadDirectory(string dirPath, string uploadRelativePath)
    {
        string[] files = Directory.GetFiles(dirPath, "*.*");
        string[] subDirs = Directory.GetDirectories(dirPath);

        foreach (string localFile in files)
        {
            var remotePath = uploadRelativePath + "/" + Path.GetFileName(localFile);
            await UploadFile(localFile,remotePath);
        }

        foreach (string subDir in subDirs)
        {
            createDirectory(uploadRelativePath + "/" + Path.GetFileName(subDir));
            await UploadDirectory(subDir, uploadRelativePath + "/" + Path.GetFileName(subDir));
        }
    }

    public async Task UploadFile(string localPath, string UploadRelativePath)
    {
        var fileSize = new FileInfo(localPath).Length;
        string From = localPath;
        string To = $"{host}/{UploadRelativePath}";
        bool isDone = false;
        using (WebClient client = new WebClient())
        {
            client.Credentials = new NetworkCredential(user, pass);
            var uri = new Uri(To);
            client.UploadFileAsync(uri, From);
            client.UploadProgressChanged += (s, e) => {
                var perc = (float)e.BytesSent/(float)fileSize;
                #if UNITY_EDITOR
                    UnityEditor.EditorUtility.DisplayProgressBar("Uploading",$"{From}",perc);
                #endif
            };
            client.UploadFileCompleted += (s, e) => {
                Task.Factory.StartNew(client.Dispose);
                isDone = true;
            };

            while(!isDone)
            {
                await Task.Yield();
            }
            #if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
            #endif
        }
    }
    #endregion

} 