using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class JsonComparer
{
    public static Result PrepareComparisonReport(string newData, string previousData)
    {
        Result finalResult = new Result();
        // delete this
        var existing = File.ReadAllText(@"D:\SVN_Marketplace\json files\PreviousJson.txt");
        var modified = File.ReadAllText(@"D:\SVN_Marketplace\json files\NewJson.txt");



        // convert JSON to object
        JObject actualJson = JObject.Parse(existing);
        JObject modifiedJson = JObject.Parse(modified);

        // read properties and create list for further processing
        var modifiedProps = modifiedJson.Properties().ToList();
        var actProps = actualJson.Properties().ToList();

        var newItemChildProp = modifiedProps.Where(x => ((JProperty)x).Path!= null).ToList();
        var actualItemChildProp = actProps.Where(x => x.Children().Count() > 1).ToList();


        return finalResult;
    }



    public class CustomJProperty
    {
        public JToken Field { get; set; }
        public JToken OldValue { get; set; }
        public JToken NewValue { get; set; }
    }


    public class Result
    {
        public JObject Deleted { get; set; }
        public JObject NewlyCreated { get; set; }

        public List<JProperty> UnChanged { get; set; }
        public List<CustomJProperty> Modified { get; set; }
    }
}