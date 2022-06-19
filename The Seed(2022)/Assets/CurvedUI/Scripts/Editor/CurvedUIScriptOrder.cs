using UnityEditor;

namespace CurvedUI
{
    /// <summary>
    /// This class changes the Execution Order of Scripts in CurvedUI package.
    /// Some of them must be executed before or after default time to work properly.
    /// </summary>
    [InitializeOnLoad]
    public class CurvedUIScriptOrder : Editor
    {

      

        static CurvedUIScriptOrder()
        {
            ChangeScriptOrder(typeof(CurvedUITMP).Name, 100, OrderMatch.GREATER_THAN);
        }




        static void ChangeScriptOrder(string scriptName, int order, OrderMatch match = OrderMatch.EXACT)
        {
            // Iterate through all scripts (Might be a better way to do this?)
            foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
            {
                // If found our script
                if (monoScript.name == scriptName)
                {
                    

                    if(match == OrderMatch.EXACT)
                    {
                        // And it's not at the execution time we want already
                        if (MonoImporter.GetExecutionOrder(monoScript) != order)
                        {
                            MonoImporter.SetExecutionOrder(monoScript, order);
                        }
                        break;
                    }

                    if (match == OrderMatch.LESSER_THAN)
                    {
                        // And it's not at the execution time we want already
                        if (MonoImporter.GetExecutionOrder(monoScript) > order)
                        {
                            MonoImporter.SetExecutionOrder(monoScript, order);
                        }
                        break;
                    }

                    if (match == OrderMatch.GREATER_THAN)
                    {
                        // And it's not at the execution time we want already
                        if (MonoImporter.GetExecutionOrder(monoScript) < order)
                        {
                            MonoImporter.SetExecutionOrder(monoScript, order);
                        }
                        break;
                    }
                }
            }
        }

        enum OrderMatch
        {
            EXACT = 0,
            GREATER_THAN = 1,
            LESSER_THAN = 2,
        }
    }
}

