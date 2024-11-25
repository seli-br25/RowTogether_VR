using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

// Imported from Lexic Name Generator Asset from asset store.
// Tweaked with help of GPT to accomodate multiple name classes at once.

namespace Lexic
{
    public class NameGenerator : MonoBehaviour
    {
        public static string generatedName;

        public string[] nameSourceClasses; // List of name source classes
        public System.Random rng;

        private Dictionary<string, List<string>> classRulesMap; // Map of class names to rules
        private Regex ruleRegex = new Regex(@"^(?<token>(%([0-9]{1,2}|100))([a-z]+))+");
        private Regex tokenRegex = new Regex(@"^%([0-9]{1,2}|100)([a-z]+)");

        void Start()
        {
            if (rng == null)
                rng = new System.Random();

            // Initialize the rules map
            classRulesMap = new Dictionary<string, List<string>>();

            foreach (string sourceClass in nameSourceClasses)
            {
                System.Type t = System.Type.GetType(sourceClass);
                if (t == null || t.BaseType != typeof(BaseNames))
                {
                    throw new ArgumentException(sourceClass + " is not a derived class of BaseNames.");
                }

                MethodInfo method = t.GetMethod("GetRules", BindingFlags.Static | BindingFlags.Public);
                if (method == null)
                {
                    throw new MissingMethodException("Class " + sourceClass + " does not implement GetRules");
                }

                List<string> rules = (List<string>)method.Invoke(null, null);
                if (rules == null || rules.Count <= 0)
                {
                    throw new InvalidOperationException("Rule list for " + sourceClass + " is empty");
                }

                // Validate rules for this class
                ValidateRules(rules);

                // Store rules in the map
                classRulesMap[sourceClass] = rules;
            }
        }

        public bool ValidateRules(List<string> rules)
        {
            foreach (string rule in rules)
            {
                Match m = ruleRegex.Match(rule);
                if (!m.Success)
                {
                    throw new ArgumentException("Rule " + rule + " has incorrect format.");
                }
            }
            return true;
        }

        public void GenerateRandomName()
        {
            GetNextRandomName();
        }
        public string GetNextRandomName()
        {
            if (classRulesMap.Count == 0)
            {
                throw new InvalidOperationException("No name source classes loaded.");
            }

            // Randomly pick a name source class
            string selectedClass = nameSourceClasses[rng.Next(0, nameSourceClasses.Length)];
            List<string> rules = classRulesMap[selectedClass];

            // Generate the name using the selected class and its rules
            string result = "";

            string rule = rules[rng.Next(0, rules.Count)];
            Match rm = ruleRegex.Match(rule);

            CaptureCollection cc = rm.Groups["token"].Captures;

            System.Type t = System.Type.GetType(selectedClass);
            MethodInfo method = t.GetMethod("GetSyllableSet", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                throw new MissingMethodException("Class " + selectedClass + " does not implement GetSyllableSet");
            }

            for (int i = 0; i < cc.Count; i++)
            {
                Match tm = tokenRegex.Match(cc[i].Value);
                if (tm.Success)
                {
                    int chance = int.Parse(tm.Groups[1].Value);
                    string token = tm.Groups[2].Value;

                    if (rng.Next(0, 99) < chance)
                    {
                        List<string> syllables = (List<string>)method.Invoke(null, new object[] { token });
                        if (syllables == null || syllables.Count <= 0)
                        {
                            throw new InvalidOperationException("Syllable list for key:" + token + " in " + selectedClass + " is empty");
                        }
                        result += syllables[rng.Next(0, syllables.Count)];
                    }
                }
            }

            generatedName = result.Replace("_", " ");
            return generatedName;
        }
    }
}
