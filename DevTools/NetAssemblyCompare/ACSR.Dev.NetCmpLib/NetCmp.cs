using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;

namespace ACSR.Dev.NetCmpLib
{
    public class NetCmp
    {
        private dynamic _ILFilter;
        public dynamic ILFilter
        {
            get {
                return _ILFilter;
            }
            set {
                _ILFilter = value;
            }
        }
        public string Md5ToString(byte[] md5)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte hex in md5)
                sb.Append(hex.ToString("x2"));
            string md5sum = sb.ToString();
            return md5sum;
        }

        public string GetMD5HashFromFile(string file_name)
        {
            byte[] retVal;
            using (FileStream file = new FileStream(file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                retVal = md5.ComputeHash(file);
            }
            return Md5ToString(retVal);
        }

        bool MD5Compare(string Left, string Right)
        {           
            return ((new FileInfo(Left).Length == new FileInfo(Right).Length) && (string.Compare(GetMD5HashFromFile(Left), GetMD5HashFromFile(Right)) == 0));
        }

        public bool CompareMD5(string Left, string Right)
        {
            return MD5Compare(Left, Right);
        }

        void DeleteIfExist(string FileName)
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
        }
        string GetMD5Resources(string TempDir)
        {
            var resourceFiles = Directory.GetFiles(TempDir, "*.resources");
            var l = new List<string>(resourceFiles);
            l.Sort();
            using (Stream ss = new MemoryStream())
            {
                using (var sr = new StreamWriter(ss))
                {
                    
                    foreach (var s in l)
                    {

                        var md5 =  GetMD5HashFromFile(s);
                        sr.Write(md5);
                    }
                    ss.Position = 0;
                    return Md5ToString(new MD5CryptoServiceProvider().ComputeHash(ss));
                }
            }
        }
        void DeleteResources(string TempDir)
        {
            var resourceFiles = Directory.GetFiles(TempDir, "*.resources");
            foreach (var file in resourceFiles)
            {
                File.Delete(file);
            }
        }

        List<string> LoadDissasembly(string FileName)
        {
            var results = new List<string>();
            try

            {
                
                var exprs = new List<Regex>();
                exprs.Add(new Regex(@"Time-date stamp:[\s]+?[\d]x"));
                //exprs.Add(new Regex(@"MVID:[\s]+?{[\w\-]+?}"));
                exprs.Add(new Regex(@"MVID:"));

                exprs.Add(new Regex(@"Image base:[\s]+?[\d]+?x"));
                exprs.Add(new Regex(@"^//.*?Virtual Size"));
                exprs.Add(new Regex(@"^//.*?Import Name Table"));
                exprs.Add(new Regex(@"^//.*?Offset"));
                exprs.Add(new Regex(@"^//.*?address"));
                exprs.Add(new Regex(@"^//.*?Addr. of entry"));
                exprs.Add(new Regex(@"^//.*?Method begins at"));
                exprs.Add(new Regex(@"^//.*?SIG:"));
                exprs.Add(new Regex(@"^//.*PE"));
                exprs.Add(new Regex(@"<PrivateImplementationDetails>{[A-F-0-9]{36,36}}"));
                
                
                exprs.Add(new Regex(@"^//"));
                //   exprs.Add(new Regex(@"^//.*HEX"));
                //exprs.Add(new Regex(@"^//.*?Unaccounted"));

                exprs.Add(new Regex(@"\.ver"));
                var reError = new Regex(@"^error\s:\s");

                var trimmer = new List<Regex>();
                //trimmer.Add(new Regex(@"(/\*[\s]?[A-Z0-9]*[\s]?\*/)"));
                trimmer.Add(new Regex(@"(/\*.*?\*/)"));
                trimmer.Add(new Regex(@"('\$\$method0x[a-f0-9-]*')"));
                trimmer.Add(new Regex(@"('\$\$method0x[a-f0-9-]*')"));
                trimmer.Add(new Regex(@"mscorlib_([\d]{1,2})"));
                // PADDING

                trimmer.Add(new Regex(@"(\.data cil I_[A-F0-9]+?\s=\sint[\d]{1,2}\[[\d]{1,2}\])")); /// NB THIS LINE MUST BE BEFORE THE NEXT LINE
                trimmer.Add(new Regex(@"\.data cil (I_[A-F0-9]*)"));
                // END OF PADDING
                
                

                using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader SR = new StreamReader(FS))
                    {
                        while (!SR.EndOfStream)
                        {
                            string Line = SR.ReadLine().Trim();
                            bool acceptable = true;

                            if (reError.Match(Line).Success)
                            {
                                return null;
                            }
                            foreach (var expr in exprs)
                            {

                                if (expr.Match(Line).Success)
                                {
                                    acceptable = false;
                                    break;

                                }
                                else
                                {
                                    //  results.Add(string.Format("IGNORED: {0}", expr));
                                }
                            }
                            if (acceptable)
                            {
                                foreach (var t in trimmer)
                                {
                                    StringBuilder output = new StringBuilder();
                                    int start = 0;
                                    MatchCollection matches = t.Matches(Line, 0);
                                    if (matches != null)
                                    {
                                        foreach (Match match in matches)
                                        {
                                            output.Append(Line.Substring(start, match.Index - start));
                                            start = match.Index + match.Length;
                                            //output.Append(Line.Substring(match.Index, match.Length));
                                            //start += match.Length;
                                        }
                                    }
                                    output.Append(Line.Substring(start, Line.Length - start));
                                    Line = output.ToString();
                                }
                                if (!string.IsNullOrEmpty(Line))
                                    results.Add(Line);
                            }

                        }
                    }
                }
                results.Sort();
            }
            catch (Exception e)
            {
                throw new Exception("LoadDissasembly: " + e.Message);
            }
            return results;
        }

        void ListToFile(List<string> List, string FileName)
        {
            using (FileStream FS = new FileStream(FileName, FileMode.Create)) 
            {
                using (StreamWriter SW = new StreamWriter(FS))
                {
                    foreach (var s in List)
                    {
                        SW.WriteLine(s);
                    }
                }
            }

        }

        bool CompareFilesWithSort(string Left, string Right, string originalFileName, string  TempDir)
        {
            bool result = true;
            try
            {
                var LeftList = LoadDissasembly(Left);
                var RightList = LoadDissasembly(Right);
                if (LeftList == null || RightList == null)
                {
                    //Console.WriteLine(string.Format("Failed to compare {0} -> {1}", Left, Right));
                    return false;
                }
                if (LeftList.Count != RightList.Count)
                {
                   // Console.WriteLine(string.Format("File mismatch {0}->{1}", Left, Right));
                    result = false;
                }
                for (int i = 0; i < LeftList.Count; i++)
                {
                    if (string.Compare(LeftList[i], RightList[i], false) != 0)
                    {
                        result = false;
                        break;

                        //return false;
                    }
                }
                if (!result)
                {
                    ListToFile(LeftList, TempDir + "\\mod_srt_" + Path.GetFileName(originalFileName) + ".1");
                    ListToFile(RightList, TempDir + "\\mod_srt_" + Path.GetFileName(originalFileName) + ".2");
                }                
            }
            catch (Exception e)
            {
                throw new Exception("CompareFilesWithSort: " + e.Message);
            }
            if (result)
            {
             //   Console.WriteLine(string.Format("File match {0}->{1}", Left, Right));
            }
            return result;
        }

        public bool Compare(string Left, string Right, string TempDir, string IlDasmPath)
        {
            var manual = false;
            if (ILFilter != null)
            {
                manual = true;
                foreach (string line in _ILFilter)
                {
                    var re = new Regex(line);
                    if (re.Match(Left).Success && re.Match(Right).Success)
                    {

                        //Console.WriteLine(string.Format("Not doing manual compare for file: {0}", Left));
                        manual = false;
                        break;
                    }
                }
            }
            if (manual)
                return MD5Compare(Left, Right);

            string output = TempDir + "\\cmp_out.dmp";
            string tLeft = TempDir + "\\cmp_left.dmp";
            string tRight = TempDir + "\\cmp_right.dmp";
            string fileName = TempDir + "\\cmp_assembly";
            

            try
            {
                DeleteIfExist(output);
                DeleteIfExist(tLeft);
                DeleteIfExist(tRight);
                DeleteIfExist(fileName);
                if (File.Exists(output) || File.Exists(tLeft) || File.Exists(tRight) || File.Exists(fileName))
                {
                    throw new Exception(string.Format("Could not clear temp files, please manually clear temp."));
                }
                if (!File.Exists(IlDasmPath))
                {
                    throw new Exception("Path to ILDasm doesnt exist");
                }
                var p = new Process();

                p.StartInfo.FileName = IlDasmPath;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                // comment v3

                
                //p.StartInfo.Arguments = string.Format("/BYTES /STATS /CLASSLIST /TOKENS /SOURCE \"{0}\" /OUT=\"{1}\"", fileName, output);
                p.StartInfo.Arguments = string.Format("/ALL /TYPELIST \"{0}\" /OUT=\"{1}\"", fileName, output);

                DeleteResources(TempDir);
                File.Copy(Left, fileName, true);                
                p.Start();
                p.WaitForExit();
               /* if (p.ExitCode != 0)
                {
                    throw new Exception("ILDasm has a non zero exit code");
                }*/
                File.Move(output, tLeft);
                if (!File.Exists(fileName) || !File.Exists(tLeft))
                {
                    manual = true;
                    throw new Exception("ILDasm malfunction Output file doesnt exist");
                }
                var leftRMD5 = GetMD5Resources(TempDir);

                DeleteResources(TempDir);
                File.Copy(Right, fileName, true);
                p.Start();
                p.WaitForExit();
                File.Move(output, tRight);
                if (!File.Exists(fileName) || !File.Exists(tRight))
                {
                    manual = true;
                    throw new Exception("ILDasm malfunction Output file doesnt exist");
                }
                var rightRMD5 = GetMD5Resources(TempDir);
                if (string.Compare(leftRMD5, rightRMD5) != 0)
                {
                    return false;
                }

                
                manual = CompareFilesWithSort(tLeft, tRight, Left, TempDir);
                if (!manual)
                {
                    File.Copy(tLeft, TempDir + "\\mod_" + Path.GetFileName(Left) + ".1", true);
                    File.Copy(tRight, TempDir + "\\mod_" + Path.GetFileName(Left) + ".2", true);
                }
                else
                {
                    return true;
                }
                
            }
            catch (Exception e)
            {
                if (!manual)
                {
                    throw e;
                }
                throw e;
                
            }
            
            return MD5Compare(Left, Right);
                


        }


    }
}
