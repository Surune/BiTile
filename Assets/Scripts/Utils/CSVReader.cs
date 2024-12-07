using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*  사용 방법
        1-1. Resources 폴더 내에 csv 파일이 들어있어야 합니다. (skillinfo.csv가 들어있다고 가정)
        1-2. csv 파일의 1열에는 각 column의 이름이 들어있어야 합니다.
        2.   var data = CSVReader.Read('skillinfo');와 같이 파일명을 통해 파일을 불러올 수 있습니다. 
             파일명은 확장자(.csv)를 제외하고 넣습니다.
        3.   data[num]["LEVEL"]과 같이 사용하면, num 열에 해당하는 데이터의 "LEVEL" 행의 값을 불러올 수 있습니다. 
            이때 가져오는 값은 object이므로 적절한 parsing(tostring, int.parse 등)으로 원하는 값으로 바꿔주면 됩니다.

    Sample Code
        var data = CSVReader.Read("skillinfo")
        var row = CSVReader.FindRowWithNum(skillInfo, skillNum);
        skillNameText.text = row["NAME"].ToString();
        skillCooltimeText.text = row["COOLTIME"].ToString();
        skillDescriptionText.text = row["DESCRIPTION"].ToString();
 * */

public class CSVReader {
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static List<Dictionary<string, object>> Read(string file) {
        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load(file) as TextAsset;

        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++) {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++) {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n)) {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f)) {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }

    public static Dictionary<string, object> FindRowWithColumnName(List<Dictionary<string, object>> dataList, string columnName, int numValue)
    {
        foreach (var row in dataList)
        {
            if (row.ContainsKey(columnName) && row[columnName].Equals(numValue))
            {
                return row;
            }
        }

        Debug.Log("No row with" + columnName + " == " + numValue + " found.");
        return null;
    }
}
