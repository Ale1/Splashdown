using System;
using System.Collections;
using System.Collections.Generic;
using Splashdown.Editor;
using UnityEngine;
using UnityEditor;

namespace Splashdown.Editor
{
    public class CommandLineInterpreter
    {
        //e.g // /Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ~/UnityProjects/MyProject -executeMethod MyEditorScript.PerformSetSplashWithOptions -name MySplashdown -l1 hello -l2 cruel -l3 world

        private static Options _options;


        public CommandLineInterpreter()
        {
            _options = null;
        }

        public static void PerformSetSplashWithOptions()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string name = null;
            string l1 = null;
            string l2 = null;
            string l3 = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-name") name = args[i + 1];
                else if (args[i] == "-l1") l1 = args[i + 1];
                else if (args[i] == "-l2") l2 = args[i + 1];
                else if (args[i] == "-l3") l3 = args[i + 1];
            }

            _options = new()
            {
                line1 = l1,
                line2 = l2,
                line3 = l3,
                textColor = Color.red
            };
            
            var splashdownData = SplashdownController.GetOptionsByName(name);
            splashdownData.line1 = l1 ?? splashdownData.line1;
            splashdownData.line1 = l2 ?? splashdownData.line2;
            splashdownData.line1 = l3 ?? splashdownData.line3;

            _options = splashdownData;
        }

        public static Options ProvideOptions()
        {
            return _options;
        }
    }
}
