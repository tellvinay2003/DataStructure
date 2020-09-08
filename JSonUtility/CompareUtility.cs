using Newtonsoft.Json;
// using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace JSonUtility
{
    public class CompareUtility
    {
        public static List<JProperty> PrepareComparisonReport(string newData, string previousData)
        {
            // delete this
            var existing = File.ReadAllText(@"D:\SVN_Marketplace\json files\PreviousJson.txt");
            var modified = File.ReadAllText(@"D:\SVN_Marketplace\json files\NewJson.txt");

            // convert JSON to object
            var actualJson = JToken.Parse(existing);
            var modifiedJson = JToken.Parse(modified);

            return DifferentiateJsons(modified, existing);



            //JToken compareResult = JToken.Parse("{}");
            //compareResult = Differentiate(modifiedJson, actualJson);
            //return compareResult;



        }


        private static JToken Differentiate(JToken modifiedJson, JToken actualJson)
        {
            // Check if both type r same
            if (actualJson != null && modifiedJson != null && actualJson?.GetType() != modifiedJson?.GetType())
                throw new InvalidOperationException("types must match; " + actualJson.GetType().Name + "' <> '" + modifiedJson.GetType().Name + "'");

            // Check if both are same
            if (JToken.DeepEquals(modifiedJson, actualJson))
                return (JToken)null;

            // Combine the JSons
            IEnumerable<JToken> jSonUnion = (modifiedJson != null ? modifiedJson.Children() : new JEnumerable<JToken>()).
                                          Union<JToken>((IEnumerable<JToken>)(actualJson != null ? actualJson.Children() : new JEnumerable<JToken>()));

            // use for unique Json string keys in both JSons
            IEnumerable<string> jsonStringsKeys;
            if (jSonUnion == null)
            {
                jsonStringsKeys = (IEnumerable<string>)null;
            }
            else
            {
                // Get all property names / string in union
                IEnumerable<string> source2 = jSonUnion.Select<JToken, string>((Func<JToken, string>)(_ => !(_ is JProperty jproperty) ? (string)null : jproperty.Name));

                // all distince JSon keys
                jsonStringsKeys = source2 != null ? source2.Distinct<string>() : (IEnumerable<string>)null;
            }


            IEnumerable<string> combinedWithDistinctKeys = jsonStringsKeys;
            if (!combinedWithDistinctKeys.Any<string>() && (actualJson is JValue || modifiedJson is JValue))
                return modifiedJson != null ? modifiedJson : actualJson;

            JToken compareResult = JToken.Parse("{}");

            // for all Keys in both JSon objects 
            foreach (string item in combinedWithDistinctKeys)
            {
                if (item == null)
                {
                    if (modifiedJson == null)
                        compareResult = actualJson;
                    else if (modifiedJson is JArray && modifiedJson.Children().All<JToken>((Func<JToken, bool>)(c => !(c is JValue))))
                    {
                        JArray jarray = new JArray();
                        int num = Math.Max(modifiedJson != null ? actualJson.Count<JToken>() : 0, actualJson != null ? actualJson.Count<JToken>() : 0);

                        for (int index = 0; index < num; ++index)
                        {
                            JToken jtoken2 = Differentiate(modifiedJson != null ? modifiedJson.ElementAtOrDefault<JToken>(index) :
                                              (JToken)null, actualJson != null ? actualJson.ElementAtOrDefault<JToken>(index) : (JToken)null);
                            if (jtoken2 != null)
                                jarray.Add(jtoken2);
                        }
                        if (jarray.HasValues)
                            compareResult = (JToken)jarray;
                    }
                    else
                        compareResult = actualJson;
                }

                else if (modifiedJson?[(object)item] == null)
                {
                    JProperty parent = actualJson?[(object)item]?.Parent as JProperty;
                    //compareResult[(object)("Deleted:--> ")] = string.Empty;
                    //compareResult[(object)(item)] = parent.Value;
                    compareResult[(object)("Change Detected:--> ")] = string.Empty;
                    compareResult[(object)("Deleted:--> " + item)] = parent.Value;
                }

                else if (actualJson?[(object)item] == null)
                {
                    JProperty parent = modifiedJson?[(object)item]?.Parent as JProperty;
                    // compareResult[(object)("NewlyAdded:--> " + item)] = parent.Value;
                    compareResult[(object)("Changed Detected:--> ")] = string.Empty;
                    compareResult[(object)("NewlyAdded:--> " + item)] = parent.Value;
                }
            }

            return compareResult;
        }


        public static List<JProperty> DifferentiateJsons(string strModifiedJson, string strActualJson)
        {
            // convert JSON to object
            JObject actualJson = JObject.Parse(strActualJson);
            JObject modifiedJson = JObject.Parse(strModifiedJson);

            // read properties and create list for further processing
            var modifiedProps = modifiedJson.Properties().ToList();
            var actProps = actualJson.Properties().ToList();

            List<JProperty> finalJson = new List<JProperty>();
            List<JProperty> childJson = new List<JProperty>();
            #region Linq region

            foreach (JProperty item in modifiedProps)
            {
                if (actProps.Any(x => x.Name.Equals(item.Name) && x.Value.Equals(item.Value)))
                {
                    finalJson.Add(item);
                }

                if (!actProps.Any(x => x.Name.Equals(item.Name)))
                {
                    JProperty jProperty = new JProperty("Change Detected", "Change Detected: New Item Added");
                    finalJson.Add(jProperty);
                    finalJson.Add(item);
                }

                if (actProps.Any(x => x.Name.Equals(item.Name) && !x.Value.Equals(item.Value)))
                {
                    JProperty changeDetected = new JProperty("Change Detected", item + "Changed");
                    finalJson.Add(changeDetected);
                    finalJson.Add(item);
                }

                if (actProps.Any(x => x.Name.Equals(item.Name) && item.Children().Children().Count() > 0))
                {
                    var check = actProps.Where(t => t.Name.Equals(item.Name)).ToList();
                    CompareChildKeyValues(check.Children().Children(), item.Children().Children());

                    //JProperty changeDetected = new JProperty("Change Detected", item + "Changed");
                    //finalJson.Add(changeDetected);
                    //finalJson.Add(item);
                }


            }

            foreach (JProperty item in actProps)
            {
                if (!modifiedProps.Any(x => x.Name.Equals(item.Name)))
                {
                    JProperty changeDetected = new JProperty("Change Detected", item + "Changed");
                    finalJson.Add(changeDetected);
                    finalJson.Add(item);
                }
            }


            #endregion



            #region for loop
            /*
            for (int i = 0; i < modifiedProps.Count() - 1; i++)
            {
                for (int j = 0; j < actProps.Count() - 1; j++)
                {
                    if (modifiedProps[i].Name.Equals(actProps[j].Name) && modifiedProps[i].Value.Equals(actProps[j].Value))
                    {
                        finalJson.Add(modifiedProps[i]);
                        break;
                    }
                    if (modifiedProps[i].Name.Equals(actProps[j].Name) && !modifiedProps[i].Value.Equals(actProps[j].Value))
                    {
                        finalJson.Add(modifiedProps[i]);
                        JProperty changeDetected = new JProperty("Change Detected", modifiedProps[i] + "Changed");
                        finalJson.Add(changeDetected);
                        break;
                    }
                    if (! modifiedProps[i].Name.Equals(actProps[j].Name))
                    {
                        finalJson.Add(modifiedProps[i]);
                        JProperty changeDetected = new JProperty("Change Detected", modifiedProps[i] + "Changed");
                        finalJson.Add(changeDetected);
                        break;
                    }

                }
            }
            */
            #endregion

            return finalJson;

        }

        private static List<JToken> CompareChildKeyValues(IJEnumerable<JToken> check, IJEnumerable<JToken> childValues)
        {
            var result = new List<JToken>();
            foreach (JToken item in childValues)
            {
                result = check.Where(x => !x.SequenceEqual(item)).ToList();
            }

            return result;
        }

        public static Result Compare(string newData, string previousData)
        {
            var result = GetJsonDiffInGroup(newData, previousData);
            return result;
        }


        #region Private Method
        private static Result GetJsonDiffInGroup(string existing, string modified)
        {
            List<JProperty> finalResult = new List<JProperty>();
            // delete this
            existing = File.ReadAllText(@"D:\SVN_Marketplace\json files\PreviousJson.txt");
            modified = File.ReadAllText(@"D:\SVN_Marketplace\json files\NewJson.txt");

            // convert JSON to object
            JObject actualJson = JObject.Parse(existing);
            JObject modifiedJson = JObject.Parse(modified);

            // read properties and create list for further processing
            var modifiedProps = modifiedJson.Properties().ToList();
            var actProps = actualJson.Properties().ToList();

            //
            // var Common = modifiedProps.Where(n => actProps.Any(o => o.Name.Equals(n.Name))).ToList();
            var Deleted = actProps.Where(o => !modifiedProps.Any(n => n.Name.Equals(o.Name))).ToList();
            var NewlyAdded = modifiedProps.Where(n => !actProps.Any(o => o.Name.Equals(n.Name))).ToList();

            // find differing properties
            var auditLog = (from existingProp in actProps
                            from modifiedProp in modifiedProps
                            where modifiedProp.Path.Equals(existingProp.Path) && !modifiedProp.Value.Equals(existingProp.Value)
                            // where !modifiedProp.Value.Equals(existingProp.Value)

                            select new CustomJProperty
                            {
                                Field = existingProp.Path,
                                OldValue = existingProp.Value,
                                NewValue = modifiedProp.Value,
                            }).ToList();


            Result result = new Result();
            result.Deleted = new JObject(Deleted);
            result.NewlyCreated = new JObject(NewlyAdded);
            result.Modified = new List<CustomJProperty>();
            result.Modified.AddRange(auditLog);

            return result;
        }
        #endregion
    }
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
    public List<CustomJProperty> Modified { get; set; }
}

