namespace PMP.Extension
{
    using UnityEngine;
    using LitJson;
    using System.IO;
    using System.Text;

    public static class LitJsonExtensions
    {
        public static void SaveJsonData(this JsonData jsonData,string path){
            if(jsonData == null){
                Debug.LogError("JsonData is null");
                return;
            }
            var index = path.LastIndexOf("/");
            var directoryPath = path.Substring(0,index);
            if(!Directory.Exists(directoryPath)){
                Directory.CreateDirectory(directoryPath);
            }
            var sw = new StreamWriter(path, false, new UTF8Encoding(false));
			StringBuilder strBuilder = new StringBuilder();
			strBuilder.AppendLine(jsonData.ToJson());
            sw.Write(strBuilder);
			sw.Flush();
			sw.Close();
        }
    }
}