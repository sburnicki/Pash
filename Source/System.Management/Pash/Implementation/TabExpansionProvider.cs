using System;
using System.Linq;
using System.Collections.Generic;
using System.Management;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Pash.Implementation
{
    internal class TabExpansionProvider
    {
        private LocalRunspace _runspace;
        private char[] _quoteChars = new char[] {'\'', '"'};
        private PathIntrinsics _pathIntrinsics;

        public TabExpansionProvider(LocalRunspace runspace)
        {
            _runspace = runspace;
            if (_runspace != null)
            {
                _pathIntrinsics = new PathIntrinsics(_runspace.ExecutionContext.SessionState);
            }
        }

        public string[] GetAllExpansions(string cmdStart, string replacableEnd)
        {
            var expansions = new List<string>();
            // check if we're inside a cmdlet invocation command
            var cmdlet = CheckForCommandWithCmdlet(cmdStart);
            // either complete the current cmdlet command or show available commands
            if (cmdlet != null)
            {
                expansions.AddRange(GetCmdletParameterExpansions(cmdlet, replacableEnd));
            }
            else
            {
                // doing expansion for commands and functions only makes sense if we're not "inside" a cmdlet
                expansions.AddRange(GetCommandExpansions(cmdStart, replacableEnd));
                expansions.AddRange(GetFunctionExpansions(cmdStart, replacableEnd));
            }

            // provide expansion for files
            expansions.AddRange(GetProviderPathExpansions(cmdStart, replacableEnd));

            // last but not least, provide extension for variables
            expansions.AddRange(GetVariableExpansions(cmdStart, replacableEnd));
            var expArray = expansions.ToArray();
            return expArray;
        }

        public CmdletInfo CheckForCommandWithCmdlet(string cmdStart)
        {
            if (_runspace == null)
            {
                return null;
            }
            // first find out by checking cmdStart if we're in a cmdlet by scanning from right to left for a cmdlet name
            var cmdRest = cmdStart.Trim();
            while (cmdRest.Length > 0)
            {
                int pos = cmdRest.LastUnquotedIndexOf(' ') + 1;
                var rest = cmdRest.Substring(pos).Trim();
                // check if we reached the end of the possible cmd, e.g. in pipeline, parenthesis, multiple cmdlets
                if (rest.StartsWith("|") || rest.StartsWith("(") || rest.EndsWith(";"))
                {
                    return null;
                }
                // only check the current term if it has the correct form
                if (Regex.IsMatch(rest, @"^\w+[\w-]+$"))
                {
                    try
                    {
                        var cmd = _runspace.CommandManager.FindCommand(rest);
                        return cmd as CmdletInfo; // either null or the cmdletInfo
                    }
                    catch (CommandNotFoundException)
                    {
                        // do nothing, check next term (could be a parameter for example)
                    }
                }
                if (pos == 0)
                {
                    break;
                }
                // check rest for cmdlet terms
                cmdRest = cmdRest.Substring(0, pos -1);
            }
            return null;
        }

        public IEnumerable<string> GetCmdletParameterExpansions(CmdletInfo info, string prefix)
        {

            if (prefix.StartsWith("-"))
            {
                // get only a specific
                prefix = prefix.Substring(1);
            }
            else if (prefix.Length > 0)
            {
                // we only deal with empty prefix or those that began with "-"
                return Enumerable.Empty<string>();
            }
            var pattern = new WildcardPattern(prefix + "*", WildcardOptions.IgnoreCase);
            return from key in info.ParameterInfoLookupTable.Keys where pattern.IsMatch(key)
                orderby key ascending select "-" + key;
        }

        public IEnumerable<string> GetCommandExpansions(string cmdStart, string replacableEnd)
        {
            if (_runspace == null)
            {
                return Enumerable.Empty<string>();
            }
            return from cmdletPair in _runspace.ExecutionContext.SessionState.Cmdlet.Find(replacableEnd + "*")
                orderby cmdletPair.Key ascending select cmdletPair.Key + " ";
        }

        public IEnumerable<string> GetVariableExpansions(string cmdStart, string replacableEnd)
        {
            if (_runspace == null)
            {
                return Enumerable.Empty<string>();
            }
            // check if it's the beginning of a variable or we should provide all variables
            // everything else doesn't make sense
            if (replacableEnd.StartsWith("$"))
            {
                replacableEnd = replacableEnd.Substring(1);
            }
            else if (replacableEnd.Length > 0)
            {
                return Enumerable.Empty<string>();
            }

            var vars = _runspace.ExecutionContext.SessionState.PSVariable.Find(replacableEnd + "*");
            return from varPair in vars orderby varPair.Key ascending select "$" + varPair.Key;
        }

        public IEnumerable<string> GetFunctionExpansions(string cmdStart, string replacableEnd)
        {
            if (_runspace == null)
            {
                return Enumerable.Empty<string>();
            }
            var funs = _runspace.ExecutionContext.SessionState.Function.Find(replacableEnd + "*");
            return from funPair in funs orderby funPair.Key ascending select funPair.Key;
        }

        private string MakeRelativePath(string path, string basePath)
        {
            if (!path.StartsWith(basePath))
            {
                return path;
            }
            return _pathIntrinsics.Combine("." + PathIntrinsics.CorrectSlash, path.Substring(basePath.Length));
        }

        public IEnumerable<string> GetProviderPathExpansions(string cmdStart, string replacableEnd)
        {
            if (_pathIntrinsics == null)
            {
                return Enumerable.Empty<string>();
            }
            replacableEnd = StripQuotes(replacableEnd);
            var pathIntrinsics = new PathIntrinsics(_runspace.ExecutionContext.SessionState);
            var globbed = pathIntrinsics.GetResolvedPSPathFromPSPath(replacableEnd + "*");
            var curPath = pathIntrinsics.CurrentLocation.Path;
            var relatives = from p in globbed select MakeRelativePath(p.Path, curPath);
            return from p in relatives orderby p ascending select QuoteIfNecessary(p);
        }

        private string QuoteIfNecessary(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return "";
            }
            bool isQuoted = _quoteChars.Contains(str[0]);
            return str.Contains(' ') && !isQuoted ? String.Format("'{0}'", str) : str;
        }

        private string StripQuotes(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return "";
            }
            if (_quoteChars.Contains(str[0]))
            {
                str = str.Substring(1);
            }
            if (str.Length == 0)
            {
                return "";
            }
            if (_quoteChars.Contains(str[str.Length - 1]))
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
    }
}

