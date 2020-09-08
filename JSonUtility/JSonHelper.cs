using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSonUtility
{
    public class JSonHelper
    {
        public static List<JValue> DifferentiateJsonLists(string strModifiedJson, string strActualJson)
        {
            // convert JSON to object
            JObject actualJson = JObject.Parse(strActualJson);
            JObject modifiedJson = JObject.Parse(strModifiedJson);

            // read properties and create list for further processing
            var modifiedJValues = JValue.Parse(strModifiedJson).ToList();
            var modifiedProps = modifiedJson.Properties().ToList();
            var actProps = actualJson.Properties().ToList();

            List<JValue> finalJson = new List<JValue>();

            #region comparison region

            // Matching
            var resultMatching = (from l1 in modifiedProps
                                  join l2 in actProps
                                on l1.Name equals l2.Name
                                  where l1.Value.Children().Children().SequenceEqual(l2.Value.Children().Children())
                                  select  // l1
                                   new JValue("NoChange")
                                   {
                                       Value = "" + l1
                                   }
                                   ).ToList();

            // Deleted
            var resultDeletedItems = (from l2 in actProps
                                      where !(from l1 in modifiedProps
                                              select l1.Name).Contains(l2.Name)
                                      select // l2
                                      new JValue("Deleted")
                                      {
                                          Value = "Change Detected : " + l2 + "   Deleted"

                                      }
                                 ).ToList();

            // Newly added
            var resultNewItems = (from l1 in modifiedProps
                                  where !(from l2 in actProps
                                          select l2.Name).Contains(l1.Name)
                                  select  //l1
                                  new JValue("NewlyAdded")
                                  {
                                      Value = "Change Detected : " + l1 + "   Newly added"
                                  }
                                 ).ToList();


            // Changed
            var resultChanged = (
                                 from l1 in modifiedProps.Children().Children()
                                 join l2 in actProps.Children().Children()
                                 on l1.Path equals l2.Path
                                 where (!l1.Children().SequenceEqual(l2.Children()))
                                 select
                                       new JValue("ChangeDetected")
                                       {
                                           Value = "Change Detected : " + l1.Path + " -->NewValue:" + l1 + ", Old Value:" + l2
                                       }
                                         ).ToList();

            finalJson.AddRange(resultMatching);
            finalJson.AddRange(resultDeletedItems);
            finalJson.AddRange(resultNewItems);
            finalJson.AddRange(resultChanged);

            finalJson.Union(modifiedJValues);
            #endregion
            return finalJson;

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
                    JProperty changeDetected = new JProperty("Change Detected", "Change Detected:Data Changed");
                    finalJson.Add(changeDetected);
                    finalJson.Add(item);
                }

                if (actProps.Any(x => x.Name.Equals(item.Name) && item.Children().Children().Count() > 0))
                {
                    var check = actProps.Where(t => t.Name.Equals(item.Name)).ToList();
                    List<object> customJProperties = CompareChildKeyValues(check.Children().Children(), item.Children().Children());

                    JProperty changeDetected = new JProperty("Change Detected", item + "Changed");
                    finalJson.Add(changeDetected);
                    // finalJson.AddRange(customJProperties);
                }


            }

            foreach (JProperty item in actProps)
            {
                if (!modifiedProps.Any(x => x.Name.Equals(item.Name)))
                {
                    JProperty changeDetected = new JProperty("Change Detected", "Change Detected: Data Deleted");
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

        public static List<JProperty> DifferentiateJsonsInSequence(string strModifiedJson, string strActualJson)
        {
            // convert JSON to object
            JObject actualJson = JObject.Parse(strActualJson);
            JObject modifiedJson = JObject.Parse(strModifiedJson);

            // read properties and create list for further processing
            var modifiedProps = modifiedJson.Properties().ToList();
            var actProps = actualJson.Properties().ToList();

            List<JProperty> finalJson = new List<JProperty>();
            List<JProperty> childJson = new List<JProperty>();
            int countAdded = 1;
            int countChanged = 1;
            int countDeleted = 1;

            #region Linq region
            foreach (JProperty item in modifiedProps)
            {
                int indexOfPropFromNewJson = modifiedProps.IndexOf(item);
                var oldPropertyOnSameIndex = indexOfPropFromNewJson >= actProps.Count() ? null : actProps[indexOfPropFromNewJson];

                // All unchchanged
                if (oldPropertyOnSameIndex != null)
                {
                    if (actProps.Any(x => x.Name.Equals(item.Name) && x.Value.Children().Children().SequenceEqual(item.Value.Children().Children()))
                                             && (modifiedProps[indexOfPropFromNewJson].Name.Equals(oldPropertyOnSameIndex.Name)))
                    {
                        finalJson.Add(item);
                    }
                    else if (actProps.Any(x => x.Name.Equals(item.Name) && x.Value.Children().Children().SequenceEqual(item.Value.Children().Children()))
                                             && (!modifiedProps[indexOfPropFromNewJson].Name.Equals(oldPropertyOnSameIndex.Name)))
                    {
                        finalJson.Add(item);
                        if (!modifiedProps.Any(t => t.Name.Equals(oldPropertyOnSameIndex.Name)))
                        {
                            JObject deleted = new JObject();
                            deleted.Add(oldPropertyOnSameIndex.Name, oldPropertyOnSameIndex.Value);
                            JProperty jDelProperty = new JProperty("ChangeDetected: Deleted:" + countDeleted, deleted);
                            finalJson.Add(jDelProperty);
                            countDeleted += 1;
                        }

                    }

                    else if (!actProps.Any(x => x.Name.Equals(item.Name)))
                    {
                        finalJson.Add(item);
                        if (!modifiedProps.Any(x => x.Name.Equals(oldPropertyOnSameIndex.Name)))
                        {
                            JObject deleted = new JObject();
                            deleted.Add(oldPropertyOnSameIndex.Name, oldPropertyOnSameIndex.Value);
                            JProperty jDelProperty = new JProperty("ChangeDetected: Deleted:" + countDeleted, deleted);
                            finalJson.Add(jDelProperty);
                            countDeleted += 1;
                        }
                        JObject newlyAdded = new JObject();
                        newlyAdded.Add(item.Name, item.Value);
                        JProperty jProperty = new JProperty("ChangeDetected: NewlyAdded:" + countAdded, newlyAdded);
                        finalJson.Add(jProperty);
                        countAdded += 1;
                    }
                    else if (actProps.Any(x => x.Name.Equals(item.Name) && item.Children().Children().Count<JToken>() == 0 && !x.Value.Equals(item.Value)))
                    {
                        finalJson.Add(item);
                        JObject changedData = new JObject();
                        JObject oldData = new JObject();
                        changedData.Add(item.Name, item.Value);
                        JProperty jProperty = new JProperty("ChangeDetected", changedData);

                        JProperty changeDetected = new JProperty("Change Detected: Data Changed:" + countChanged, changedData);
                        finalJson.Add(changeDetected);
                        countChanged += 1;
                    }

                    else if (actProps.Any(x => x.Name.Equals(item.Name) && item.Children().Children().Count() > 0))
                    {
                        foreach (JProperty jitem in actProps.Where(j => j.Children().Children().Count() > 0).ToList())
                        {
                            if (jitem.Name.Equals(item.Name) && !jitem.Children().Children().SequenceEqual(item.Children().Children()))
                            {
                                // start
                                foreach (var childItem in item.Children().Children().Children())
                                { 
                                  // if(jitem.Children().Where(x => x.))
                                }
                                // end
                                finalJson.Add(item);
                                JObject oldValue = new JObject();
                                oldValue.Add(jitem.Name, jitem.Value);
                                JProperty jProperty = new JProperty("ChangeDetected: DataChanged:" + countChanged, oldValue);

                                finalJson.Add(jProperty);
                                countChanged += 1;
                            }
                        }
                    }
                }
                else
                {
                    if (actProps.Any(x => x.Name.Equals(item.Name) && x.Value.Children().Children().SequenceEqual(item.Value.Children().Children())))
                    {
                        finalJson.Add(item);
                    }
                    else if (!actProps.Any(x => x.Name.Equals(item.Name)))
                    {
                        finalJson.Add(item);
                        JObject newlyAdded = new JObject();
                        newlyAdded.Add(item.Name, item.Value);
                        JProperty jProperty = new JProperty("ChangeDetected: NewlyAdded:" + countAdded, newlyAdded);
                        finalJson.Add(jProperty);
                        countAdded += 1;
                    }

                    else if (actProps.Any(x => x.Name.Equals(item.Name) && item.Children().Children().Count<JToken>() == 0 && !x.Value.Equals(item.Value)))
                    {
                        finalJson.Add(item);
                        JObject changedData = new JObject();
                        JObject oldData = new JObject();
                        changedData.Add(item.Name, item.Value);
                        JProperty jProperty = new JProperty("ChangeDetected", changedData);

                        JProperty changeDetected = new JProperty("Change Detected: Data Changed:" + countChanged, changedData);
                        finalJson.Add(changeDetected);
                        countChanged += 1;
                    }

                    else if (actProps.Any(x => x.Name.Equals(item.Name) && item.Children().Children().Count() > 0))
                    {
                        foreach (JProperty jitem in actProps.Where(j => j.Children().Children().Count() > 0).ToList())
                        {
                            if (jitem.Name.Equals(item.Name) && !jitem.Children().Children().SequenceEqual(item.Children().Children()))
                            {
                                finalJson.Add(item);
                                JObject oldValue = new JObject();
                                oldValue.Add(jitem.Name, jitem.Value);
                                JProperty jProperty = new JProperty("ChangeDetected: DataChanged:" + countChanged, oldValue);

                                finalJson.Add(jProperty);
                                countChanged += 1;
                            }

                        }
                    }
                }

            }

            #endregion
            return finalJson;

        }


        private static List<object> CompareChildKeyValues(IJEnumerable<JToken> check, IJEnumerable<JToken> childValues)
        {
            var result = new List<object>();

            foreach (JProperty item in childValues.Children())
            {
                var t = check.Where(x => x.Path.Equals(item.Path) && !x.Children().Equals(item.Children())).ToList();
            }

            //result = from x in childValues
            //                 from y in check
            //                 where x.Path.Equals(y.Path) && !x.Value.Equals(y.Value)


            //foreach (JToken item in childValues)
            //{
            //    if (check.Any(x => x.Path.Equals(item.Path) && !x.SequenceEqual(item)))
            //    {
            //        JProperty jProperty = new JProperty(item.Path, item);
            //        result.Add(jProperty);
            //    }
            //}
            return result;
        }

    }

    public class JItem
    {
        public JToken OriginalData { get; set; }
        public JValue ChangedData { get; set; }
    }
    public class JSonUtilities
    {
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
                    JProperty changeDetected = new JProperty("Change Detected", "Change Detected:Data Changed");
                    finalJson.Add(changeDetected);
                    finalJson.Add(item);
                }

                if (actProps.Any(x => x.Name.Equals(item.Name) && item.Children().Children().Count() > 0))
                {
                    var check = actProps.Where(t => t.Name.Equals(item.Name)).ToList();
                    foreach (JToken jItem in actProps.Children().Children())
                    {
                        if (!item.Children().Children().SequenceEqual(jItem))
                        {

                        }
                    }

                    JProperty changeDetected = new JProperty("Change Detected", item + "Changed");
                    finalJson.Add(changeDetected);
                    // finalJson.AddRange(customJProperties);
                }
            }

            foreach (JProperty item in actProps)
            {
                if (!modifiedProps.Any(x => x.Name.Equals(item.Name)))
                {
                    JProperty changeDetected = new JProperty("Change Detected", "Change Detected: Data Deleted");
                    finalJson.Add(changeDetected);
                    finalJson.Add(item);
                }
            }


            #endregion
            return finalJson;

        }

    }
}
